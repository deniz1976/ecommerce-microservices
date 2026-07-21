using MassTransit;

namespace ECommerce.ContractTests;

public sealed class EndpointNameFormatterTests
{
    [Fact]
    public void ServicePrefixesGiveSameNamedConsumersDifferentQueues()
    {
        KebabCaseEndpointNameFormatter sagaFormatter = new("ordering-saga", includeNamespace: false);
        KebabCaseEndpointNameFormatter notificationFormatter = new("notification", includeNamespace: false);

        Assert.Equal("ordering-saga-order-submitted", sagaFormatter.Consumer<OrderSubmittedConsumer>());
        Assert.Equal("notification-order-submitted", notificationFormatter.Consumer<OrderSubmittedConsumer>());
    }

}
