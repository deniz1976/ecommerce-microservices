using ECommerce.Identity.Application.Users;

namespace ECommerce.ContractTests;

internal sealed class TrackingSelfServiceRoleWriter : ISelfServiceRoleWriter
{
    public bool ReplaceCalled { get; private set; }

    public Exception? ExceptionToThrow { get; init; }

    public Task<Identity.Domain.User?> ReplaceAsync(
        string provider,
        string subject,
        string role,
        CancellationToken cancellationToken)
    {
        ReplaceCalled = true;
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return Task.FromResult<Identity.Domain.User?>(null);
    }
}
