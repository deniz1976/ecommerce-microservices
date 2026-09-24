namespace ECommerce.ContractTests;

public sealed record RetryProbe(Guid Id, int FailuresBeforeSuccess, bool ConcurrencyFailure);
