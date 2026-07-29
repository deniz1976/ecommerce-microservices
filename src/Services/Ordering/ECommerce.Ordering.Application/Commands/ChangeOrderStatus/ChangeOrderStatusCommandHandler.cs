using ECommerce.BuildingBlocks.Contracts.Cqrs;
using ECommerce.Ordering.Application.Orders;

namespace ECommerce.Ordering.Application.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusCommandHandler
    : ICommandHandler<ChangeOrderStatusCommand>
{
    private readonly OrderStatusService orderStatusService;

    public ChangeOrderStatusCommandHandler(OrderStatusService orderStatusService)
    {
        this.orderStatusService = orderStatusService;
    }

    public Task HandleAsync(
        ChangeOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        return command.Change switch
        {
            OrderStatusChange.Confirm => orderStatusService.ConfirmAsync(
                command.OrderId,
                command.CustomerId,
                cancellationToken),
            OrderStatusChange.Cancel => orderStatusService.CancelAsync(
                command.OrderId,
                command.CustomerId,
                command.ReasonCode,
                cancellationToken),
            OrderStatusChange.InventoryReserved => orderStatusService.InventoryReservedAsync(
                command.OrderId,
                command.CustomerId,
                cancellationToken),
            OrderStatusChange.PaymentAuthorized => orderStatusService.PaymentAuthorizedAsync(
                command.OrderId,
                command.CustomerId,
                cancellationToken),
            OrderStatusChange.ShipmentCreated => orderStatusService.ShipmentCreatedAsync(
                command.OrderId,
                command.CustomerId,
                cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(command),
                command.Change,
                "Unsupported order status change.")
        };
    }
}
