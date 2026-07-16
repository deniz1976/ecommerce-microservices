namespace ECommerce.Identity.Application.Users;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> Roles,
    bool IsOnboardingComplete);
