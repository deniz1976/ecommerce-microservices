using ECommerce.Identity.Domain;

namespace ECommerce.Identity.Application.AdminUsers;

public sealed record AdminUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> Roles,
    UserStatus Status,
    bool IsOnboardingComplete,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
