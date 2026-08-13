using ECommerce.Shipping.Domain;

namespace ECommerce.Shipping.UnitTests;

public sealed class ShipmentAddressTests
{
    [Fact]
    public void TryCreateNormalizesValidAddress()
    {
        bool succeeded = ShipmentAddress.TryCreate(
            "  Test Customer  ",
            "  Runtime Avenue 1  ",
            "  Istanbul  ",
            " tr ",
            " 34000 ",
            out ShipmentAddress? address);

        Assert.True(succeeded);
        Assert.NotNull(address);
        Assert.Equal("Test Customer", address.RecipientName);
        Assert.Equal("Runtime Avenue 1", address.AddressLine);
        Assert.Equal("Istanbul", address.City);
        Assert.Equal("TR", address.CountryCode);
        Assert.Equal("34000", address.PostalCode);
    }

    [Theory]
    [InlineData("", "Runtime Avenue 1", "Istanbul", "TR", "34000")]
    [InlineData("Test Customer", "", "Istanbul", "TR", "34000")]
    [InlineData("Test Customer", "Runtime Avenue 1", "", "TR", "34000")]
    [InlineData("Test Customer", "Runtime Avenue 1", "Istanbul", "TUR", "34000")]
    [InlineData("Test Customer", "Runtime Avenue 1", "Istanbul", "TR", "")]
    public void TryCreateRejectsInvalidAddress(
        string recipientName,
        string addressLine,
        string city,
        string countryCode,
        string postalCode)
    {
        bool succeeded = ShipmentAddress.TryCreate(
            recipientName,
            addressLine,
            city,
            countryCode,
            postalCode,
            out ShipmentAddress? address);

        Assert.False(succeeded);
        Assert.Null(address);
    }
}
