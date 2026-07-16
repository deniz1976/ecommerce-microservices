using ECommerce.BuildingBlocks.Contracts.Errors;

namespace ECommerce.BuildingBlocks.Contracts.Results;

public sealed class Result<T> : Result
{
    private Result(T? value, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(value, true, null);

    public static new Result<T> Failure(Error error) => new(default, false, error);
}
