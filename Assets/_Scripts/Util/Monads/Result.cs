using System;

public interface IResult
{
    public string ErrorMessage { get; }
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
}

public class Result<TSomeType> : IResult
{
    private readonly TSomeType _value;
    private readonly string _errorMessage;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private Result(TSomeType value, string errorMessage, bool isSuccess)
    {
        _value = value;
        _errorMessage = errorMessage;
        IsSuccess = isSuccess;
    }

    public static Result<TSomeType> Ok(TSomeType value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        return new Result<TSomeType>(value, null, true);
    }

    public static Result<TSomeType> Error(string error)
    {
        if (error == null)
            throw new ArgumentException("Error message must be provided");

        return new Result<TSomeType>(default, error, false);
    }

    public TSomeType Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    public string ErrorMessage => IsFailure ? _errorMessage : null;

    // Chain: performs an action on the given value if possible. returns the same result
    public Result<TSomeType> Chain(Action<TSomeType> action)
    {
        if (IsSuccess)
            action(_value);

        return this;
    }

    // Map: transforms the value if successful, keeps error otherwise
    public Result<TOtherType> Map<TOtherType>(Func<TSomeType, TOtherType> func)
    {
        if (IsSuccess)
            return Result<TOtherType>.Ok(func(_value));

        return Result<TOtherType>.Error(_errorMessage);
    }

    // Bind: chains another operation that returns a Result<U>
    public Result<TOtherType> Bind<TOtherType>(Func<TSomeType, Result<TOtherType>> func)
    {
        return IsSuccess ? func(_value) : Result<TOtherType>.Error(_errorMessage);
    }

    public Result<TSomeType> Match(Action<TSomeType> successAction, Action failureAction)
    {
        if (IsSuccess)
            successAction(_value);

        if (IsFailure)
            failureAction();

        return this;
    }

    public Result<TSomeType> Match(Action<TSomeType> successAction)
    {
        if (IsSuccess)
            successAction(_value);

        return this;
    }

    public TOtherType Switch<TOtherType>(Func<TSomeType, TOtherType> successFunc, Func<TOtherType> failureFunc)
    {
        if (IsSuccess)
            return successFunc(_value);

        return failureFunc();
    }

    public TSomeType Switch()
    {
        return Switch(success => success, () => default);
    }

    public Result<TSomeType> Check(Func<TSomeType, bool> func, string errorMessage = "Check failed.")
    {
        if (IsSuccess && !func(_value))
            return Error(errorMessage);

        return this;
    }

    public Result<TSomeType> ReadError(Action<string> func)
    {
        if (IsFailure)
            func(_errorMessage);
        
        return this;
    }

    public static Result<TSomeType> BoolToResult(TSomeType value, Func<TSomeType, bool> func)
    {
        // If the function is not successful, return an error
        if (!func(value))
            return Error($"Value cannot be null!");

        return Ok(value);
    }

    public static explicit operator Result<TSomeType>(TSomeType value)
    {
        // If the value is null, return an error
        if (value == null)
            return Error($"Could not convert NULL to a Result<{typeof(TSomeType)}>");

        // If the value is ok, return an ok
        return Ok(value);
    }
}