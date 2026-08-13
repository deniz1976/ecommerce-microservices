using ECommerce.BuildingBlocks.Contracts.Errors;

namespace ECommerce.BuildingBlocks.Localization;

public sealed class ErrorMessageLocalizer : IErrorMessageLocalizer
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Messages =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            [SupportedCultures.English] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [ErrorCodes.ProductNotFound] = "Product was not found.",
                [ErrorCodes.BasketNotFound] = "Basket was not found.",
                [ErrorCodes.OrderNotFound] = "Order was not found.",
                [ErrorCodes.OrderNotCancellable] = "This order can no longer be cancelled.",
                [ErrorCodes.OrderCancelledByCustomer] = "The customer cancelled the order.",
                [ErrorCodes.InventoryTimeout] = "Inventory processing timed out.",
                [ErrorCodes.PaymentTimeout] = "Payment processing timed out.",
                [ErrorCodes.ShipmentTimeout] = "Shipment processing timed out.",
                [ErrorCodes.PaymentNotFound] = "Payment was not found.",
                [ErrorCodes.ShipmentNotFound] = "Shipment was not found.",
                [ErrorCodes.NotificationNotFound] = "Notification was not found.",
                [ErrorCodes.InsufficientStock] = "Insufficient stock.",
                [ErrorCodes.StockBelowReserved] = "Stock cannot be lower than the reserved quantity.",
                [ErrorCodes.PaymentFailed] = "Payment failed.",
                [ErrorCodes.ShipmentFailed] = "Shipment failed.",
                [ErrorCodes.ValidationFailed] = "Validation failed.",
                [ErrorCodes.AuthenticationRequired] = "Authentication is required.",
                [ErrorCodes.AccessDenied] = "Access to the requested resource is denied.",
                [ErrorCodes.RateLimitExceeded] = "Too many requests. Please try again later.",
                [ErrorCodes.DependencyUnavailable] = "A required service is temporarily unavailable.",
                [ErrorCodes.UnexpectedError] = "An unexpected error occurred."
            },
            [SupportedCultures.Turkish] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [ErrorCodes.ProductNotFound] = "Ürün bulunamadı.",
                [ErrorCodes.BasketNotFound] = "Sepet bulunamadı.",
                [ErrorCodes.OrderNotFound] = "Sipariş bulunamadı.",
                [ErrorCodes.OrderNotCancellable] = "Bu sipariş artık iptal edilemez.",
                [ErrorCodes.OrderCancelledByCustomer] = "Sipariş müşteri tarafından iptal edildi.",
                [ErrorCodes.InventoryTimeout] = "Stok işlemi zaman aşımına uğradı.",
                [ErrorCodes.PaymentTimeout] = "Ödeme işlemi zaman aşımına uğradı.",
                [ErrorCodes.ShipmentTimeout] = "Kargo işlemi zaman aşımına uğradı.",
                [ErrorCodes.PaymentNotFound] = "Ödeme bulunamadı.",
                [ErrorCodes.ShipmentNotFound] = "Kargo kaydı bulunamadı.",
                [ErrorCodes.NotificationNotFound] = "Bildirim bulunamadı.",
                [ErrorCodes.InsufficientStock] = "Yetersiz stok.",
                [ErrorCodes.StockBelowReserved] = "Stok, rezerve edilen miktardan düşük olamaz.",
                [ErrorCodes.PaymentFailed] = "Ödeme başarısız oldu.",
                [ErrorCodes.ShipmentFailed] = "Kargo işlemi başarısız oldu.",
                [ErrorCodes.ValidationFailed] = "Doğrulama başarısız oldu.",
                [ErrorCodes.AuthenticationRequired] = "Kimlik doğrulaması gereklidir.",
                [ErrorCodes.AccessDenied] = "İstenen kaynağa erişim reddedildi.",
                [ErrorCodes.RateLimitExceeded] = "Çok fazla istek gönderildi. Lütfen daha sonra tekrar deneyin.",
                [ErrorCodes.DependencyUnavailable] = "Gerekli bir hizmete geçici olarak ulaşılamıyor.",
                [ErrorCodes.UnexpectedError] = "Beklenmeyen bir hata oluştu."
            }
        };

    public string GetMessage(string code, string? culture)
    {
        string normalizedCulture = SupportedCultures.Normalize(culture);

        if (Messages.TryGetValue(normalizedCulture, out IReadOnlyDictionary<string, string>? cultureMessages) &&
            cultureMessages.TryGetValue(code, out string? message))
        {
            return message;
        }

        return Messages[SupportedCultures.Default].TryGetValue(code, out string? fallbackMessage)
            ? fallbackMessage
            : Messages[SupportedCultures.Default][ErrorCodes.UnexpectedError];
    }
}
