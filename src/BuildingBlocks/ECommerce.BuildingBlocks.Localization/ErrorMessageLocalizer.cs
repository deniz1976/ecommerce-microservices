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
                [ErrorCodes.InsufficientStock] = "Insufficient stock.",
                [ErrorCodes.PaymentFailed] = "Payment failed.",
                [ErrorCodes.ShipmentFailed] = "Shipment failed.",
                [ErrorCodes.ValidationFailed] = "Validation failed.",
                [ErrorCodes.UnexpectedError] = "An unexpected error occurred."
            },
            [SupportedCultures.Turkish] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [ErrorCodes.ProductNotFound] = "Ürün bulunamadı.",
                [ErrorCodes.BasketNotFound] = "Sepet bulunamadı.",
                [ErrorCodes.OrderNotFound] = "Sipariş bulunamadı.",
                [ErrorCodes.InsufficientStock] = "Yetersiz stok.",
                [ErrorCodes.PaymentFailed] = "Ödeme başarısız oldu.",
                [ErrorCodes.ShipmentFailed] = "Kargo işlemi başarısız oldu.",
                [ErrorCodes.ValidationFailed] = "Doğrulama başarısız oldu.",
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
