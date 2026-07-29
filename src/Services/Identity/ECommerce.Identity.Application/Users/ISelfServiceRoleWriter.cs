namespace ECommerce.Identity.Application.Users;

public interface ISelfServiceRoleWriter
{
    Task<Domain.User?> ReplaceAsync(
        string provider,
        string subject,
        string role,
        CancellationToken cancellationToken);
}
