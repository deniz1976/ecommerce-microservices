namespace ECommerce.Ordering.Api.Orders;

public static class CorrelationReader
{
    public static Guid Read(HttpContext httpContext)
    {
        string? value = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault();
        return Guid.TryParse(value, out Guid correlationId) ? correlationId : Guid.NewGuid();
    }
}
