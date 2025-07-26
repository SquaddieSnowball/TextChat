namespace TextChat.Domain.Primitives;

public class Result<TResult>
{
	private readonly TResult _value;

	public bool IsSuccess { get; }

	public bool IsFailure => !IsSuccess;

	public TResult Value => IsSuccess
		? _value
		: throw new InvalidOperationException("The value cannot be retrieved because the result is a failure");

	public Error Error { get; }

	private Result(bool isSuccess, TResult value, Error error) => (IsSuccess, _value, Error) = (isSuccess, value, error);

	public static Result<TResult> Success(TResult value) => new(true, value, Error.None);

	public static Result<TResult> Failure(Error error) => new(false, default!, error);

	public static implicit operator Result<TResult>(TResult value) => Success(value);

	public static implicit operator Result<TResult>(Error error) => Failure(error);
}