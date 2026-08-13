namespace ECommerce.Shipping.Domain;

public sealed record ShipmentAddress
{
    private ShipmentAddress(
        string recipientName,
        string addressLine,
        string city,
        string countryCode,
        string postalCode)
    {
        RecipientName = recipientName;
        AddressLine = addressLine;
        City = city;
        CountryCode = countryCode;
        PostalCode = postalCode;
    }

    public string RecipientName { get; }

    public string AddressLine { get; }

    public string City { get; }

    public string CountryCode { get; }

    public string PostalCode { get; }

    public static bool TryCreate(
        string recipientName,
        string addressLine,
        string city,
        string countryCode,
        string postalCode,
        out ShipmentAddress? address)
    {
        string normalizedRecipientName = recipientName.Trim();
        string normalizedAddressLine = addressLine.Trim();
        string normalizedCity = city.Trim();
        string normalizedCountryCode = countryCode.Trim().ToUpperInvariant();
        string normalizedPostalCode = postalCode.Trim();

        if (string.IsNullOrWhiteSpace(normalizedRecipientName) ||
            string.IsNullOrWhiteSpace(normalizedAddressLine) ||
            string.IsNullOrWhiteSpace(normalizedCity) ||
            normalizedCountryCode.Length != 2 ||
            string.IsNullOrWhiteSpace(normalizedPostalCode))
        {
            address = null;
            return false;
        }

        address = new ShipmentAddress(
            normalizedRecipientName,
            normalizedAddressLine,
            normalizedCity,
            normalizedCountryCode,
            normalizedPostalCode);
        return true;
    }
}
