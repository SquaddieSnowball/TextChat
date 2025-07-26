namespace TextChat.Domain.Primitives;

public class Result
{
	public bool IsSuccess { get; }

	public bool IsFailure => !IsSuccess;

	public Error Error { get; }

	private Result(bool isSuccess, Error error) => (IsSuccess, Error) = (isSuccess, error);

	public static Result Success() => new(true, Error.None);

	public static Result Failure(Error error) => new(false, error);

	public static implicit operator Result(Error error) => Failure(error);
}