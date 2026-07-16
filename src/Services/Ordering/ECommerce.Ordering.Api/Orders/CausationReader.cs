namespace ECommerce.Ordering.Api.Orders;

public static class CausationReader
{
    public static Guid? Read(HttpContext httpContext)
    {
        string? value = httpContext.Request.Headers["X-Causation-Id"].FirstOrDefault();
        return Guid.TryParse(value, out Guid causationId) ? causationId : null;
    }
}
