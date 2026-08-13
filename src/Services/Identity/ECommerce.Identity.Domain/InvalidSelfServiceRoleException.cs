namespace ECommerce.Identity.Domain;

public sealed class InvalidSelfServiceRoleException(string role)
    : Exception($"Role '{role}' is not available for self-service registration.");
