using ECommerce.Identity.Application.Users;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Identity.Infrastructure.Security;

public sealed class PasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<Domain.User> passwordHasher = new();

    public string HashPassword(Domain.User user, string password)
    {
        return passwordHasher.HashPassword(user, password);
    }
}
