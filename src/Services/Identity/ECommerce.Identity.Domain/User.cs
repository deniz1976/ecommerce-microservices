namespace ECommerce.Identity.Domain;

public sealed class User
{
    private readonly List<UserRole> roles = [];

    private User()
    {
        Email = string.Empty;
        DisplayName = string.Empty;
        PasswordHash = string.Empty;
    }

    public User(Guid id, string email, string displayName, string passwordHash)
        : this(id, email, displayName, passwordHash, null, null)
    {
    }

    public User(Guid id, string email, string displayName, string passwordHash, string? externalProvider, string? externalSubject)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        ExternalProvider = externalProvider;
        ExternalSubject = externalSubject;
        Status = UserStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string DisplayName { get; private set; }

    public string PasswordHash { get; private set; }

    public string? ExternalProvider { get; private set; }

    public string? ExternalSubject { get; private set; }

    public UserStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? OnboardingCompletedAt { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => roles;

    public void AddRole(string role)
    {
        if (roles.Any(x => x.Role == role))
        {
            return;
        }

        roles.Add(new UserRole(Id, role));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetSelfServiceRole(string role)
    {
        string normalizedRole = UserRoleNames.NormalizeSelfServiceRole(role);

        if (roles.Count(x => x.Role == UserRoleNames.Customer || x.Role == UserRoleNames.Seller) == 1 &&
            roles.Any(x => x.Role == normalizedRole))
        {
            return;
        }

        roles.RemoveAll(x => x.Role == UserRoleNames.Customer || x.Role == UserRoleNames.Seller);
        roles.Add(new UserRole(Id, normalizedRole));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void LinkExternalIdentity(string provider, string subject)
    {
        if (ExternalProvider == provider && ExternalSubject == subject)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(ExternalProvider) || !string.IsNullOrWhiteSpace(ExternalSubject))
        {
            return;
        }

        ExternalProvider = provider;
        ExternalSubject = subject;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void CompleteOnboarding()
    {
        OnboardingCompletedAt ??= DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
