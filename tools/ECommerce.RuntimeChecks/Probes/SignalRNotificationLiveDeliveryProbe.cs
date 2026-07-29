using System.Threading.Channels;
using ECommerce.RuntimeChecks.Configuration;
using ECommerce.RuntimeChecks.Models;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace ECommerce.RuntimeChecks.Probes;

internal sealed class SignalRNotificationLiveDeliveryProbe : INotificationLiveDeliveryProbe
{
    private readonly RuntimeCheckOptions options;

    public SignalRNotificationLiveDeliveryProbe(RuntimeCheckOptions options)
    {
        this.options = options;
    }

    public async Task<RuntimeNotificationMessage> CaptureOrderNotificationAsync(
        Guid customerId,
        Func<CancellationToken, Task<OrderResponse>> trigger,
        DateTimeOffset deadline,
        CancellationToken cancellationToken)
    {
        Channel<RuntimeNotificationMessage> notifications =
            Channel.CreateUnbounded<RuntimeNotificationMessage>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });

        await using HubConnection connection = CreateConnection();
        using IDisposable subscription = connection.On<RuntimeNotificationMessage>(
            "notificationReceived",
            notification => notifications.Writer.TryWrite(notification));

        connection.Reconnected += async _ =>
        {
            await connection.InvokeAsync(
                "JoinCustomerGroup",
                customerId.ToString(),
                cancellationToken);
        };

        await connection.StartAsync(cancellationToken);
        await connection.InvokeAsync(
            "JoinCustomerGroup",
            customerId.ToString(),
            cancellationToken);

        OrderResponse order = await trigger(cancellationToken);

        while (DateTimeOffset.UtcNow <= deadline)
        {
            TimeSpan remaining = deadline - DateTimeOffset.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                break;
            }

            using CancellationTokenSource timeoutSource =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(remaining);

            try
            {
                RuntimeNotificationMessage notification =
                    await notifications.Reader.ReadAsync(timeoutSource.Token);
                if (notification.CustomerId == customerId &&
                    notification.OrderId == order.Id)
                {
                    return notification;
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        throw new TimeoutException(
            $"SignalR did not deliver an order notification for order {order.Id} before the configured timeout.");
    }

    private HubConnection CreateConnection()
    {
        Uri hubUri = new(options.GatewayBaseUri, "/gateway/hubs/notifications");
        return new HubConnectionBuilder()
            .WithUrl(
                hubUri,
                connectionOptions =>
                {
                    connectionOptions.AccessTokenProvider =
                        () => Task.FromResult<string?>(options.AccessToken);
                    connectionOptions.Transports =
                        HttpTransportType.WebSockets |
                        HttpTransportType.ServerSentEvents |
                        HttpTransportType.LongPolling;
                })
            .WithAutomaticReconnect()
            .Build();
    }
}
