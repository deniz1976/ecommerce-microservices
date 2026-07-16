namespace ECommerce.Basket.Domain;

public sealed class BasketItem
{
    private BasketItem()
    {
        ProductName = string.Empty;
        Currency = string.Empty;
    }

    public BasketItem(Guid productId, string productName, int quantity, decimal unitPrice, string currency)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Currency = currency;
    }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal TotalPrice => Quantity * UnitPrice;

    public string Currency { get; private set; }

    public void Update(string productName, int quantity, decimal unitPrice, string currency)
    {
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Currency = currency;
    }
}
