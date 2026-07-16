namespace ECommerce.Identity.Application.Users;

public interface IPasswordHashService
{
    string HashPassword(Domain.User user, string password);
}
