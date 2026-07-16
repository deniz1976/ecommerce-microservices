namespace ECommerce.Identity.Application.Users;

public static class UserMapper
{
    public static UserResponse ToResponse(this Domain.User user)
    {
        return new UserResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Roles.Select(x => x.Role).Order(StringComparer.OrdinalIgnoreCase).ToArray(),
            user.OnboardingCompletedAt is not null);
    }
}
