namespace ECommerce.Identity.Application.Users;

public sealed record ExternalUserProfile(
    string Provider,
    string Subject,
    string Email,
    string DisplayName);
