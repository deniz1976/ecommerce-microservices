namespace ECommerce.Identity.Domain;

public sealed class UserRole
{
    private UserRole()
    {
        Role = string.Empty;
    }

    public UserRole(Guid userId, string role)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Role = role;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Role { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
