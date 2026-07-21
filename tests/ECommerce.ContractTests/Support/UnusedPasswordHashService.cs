using ECommerce.Identity.Application.Users;
using ECommerce.Identity.Domain;

namespace ECommerce.ContractTests;

internal sealed class UnusedPasswordHashService : IPasswordHashService
{
    public string HashPassword(User user, string password) => throw new InvalidOperationException();
}
