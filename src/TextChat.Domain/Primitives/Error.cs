namespace TextChat.Domain.Primitives;

public record class Error(string Code, string Description)
{
	public static readonly Error None = new(string.Empty, string.Empty);
}