using System.Net;
using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services.Abstractions;

public interface IChatServer : IDisposable
{
	bool Started { get; }

	IPEndPoint? ServerEndpoint { get; }

	IEnumerable<IPEndPoint> ConnectedClientEndpoints { get; }

	event EventHandler<IPEndPoint>? ServerStarted;

	event EventHandler<IPEndPoint>? ServerStopped;

	event EventHandler<IPEndPoint>? ClientConnected;

	event EventHandler<IPEndPoint>? ClientDisconnected;

	event EventHandler<Error>? ErrorAcceptingClient;

	event EventHandler<ClientChatMessage>? MessageReceived;

	event EventHandler<Error>? ErrorReceivingMessage;

	event EventHandler<ServerChatMessage>? MessageBroadcasted;

	event EventHandler<Error>? ErrorBroadcastingClientMessage;

	Result Start(string ipAddress, int port);

	void Stop();

	Task<Result> BroadcastMessage(string message);
}