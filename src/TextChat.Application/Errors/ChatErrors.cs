using TextChat.Domain.Primitives;

namespace TextChat.Application.Errors;

internal static class ChatErrors
{
	public static readonly Error WrongIP = new(
		nameof(WrongIP),
		"IP address does not match the format");

	public static readonly Error WrongPort = new(
		nameof(WrongPort),
		"The port must be in the range from 0 to 65535");

	public static readonly Error ServerUnavailable = new(
		nameof(ServerUnavailable),
		"Unable to connect to server");

	public static readonly Error NotConnected = new(
		nameof(NotConnected),
		"No connection to the server");

	public static readonly Error Disconnected = new(
		nameof(Disconnected),
		"Disconnected from server");

	public static readonly Error UnableToStart = new(
		nameof(UnableToStart),
		"Unable to start server");

	public static readonly Error NotRunning = new(
		nameof(NotRunning),
		"The server is not running");

	public static readonly Error ClientDisconnected = new(
		nameof(ClientDisconnected),
		"The client has disconnected from the server");

	public static readonly Error CanNotAcceptClient = new(
		nameof(CanNotAcceptClient),
		"Can not accept a client");

	public static readonly Error WrongMessage = new(
		nameof(WrongMessage),
		"The message does not match the format");
}