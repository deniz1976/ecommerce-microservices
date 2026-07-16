namespace ECommerce.Identity.Application.Users;

public sealed record RegisterUserRequest(
    string Email,
    string DisplayName,
    string Password,
    string? Role = null);
