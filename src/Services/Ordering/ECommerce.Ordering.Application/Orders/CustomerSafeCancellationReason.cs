using ECommerce.BuildingBlocks.Contracts.Errors;

namespace ECommerce.Ordering.Application.Orders;

internal static class CustomerSafeCancellationReason
{
    public static string Normalize(string? reasonCode)
    {
        return reasonCode switch
        {
            ErrorCodes.InsufficientStock => ErrorCodes.InsufficientStock,
            ErrorCodes.PaymentFailed => ErrorCodes.PaymentFailed,
            ErrorCodes.ShipmentFailed => ErrorCodes.ShipmentFailed,
            _ => ErrorCodes.UnexpectedError
        };
    }
}
