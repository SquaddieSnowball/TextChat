using System.Net;
using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services.Abstractions;

public interface IChatClient : IDisposable
{
	bool Connected { get; }

	IPEndPoint? ConnectionEndpoint { get; }

	event EventHandler<IPEndPoint>? ClientConnected;

	event EventHandler<IPEndPoint>? ClientDisconnected;

	event EventHandler<ServerChatMessage>? MessageReceived;

	event EventHandler<Error>? ErrorReceivingMessage;

	Task<Result> Connect(string ipAddress, int port);

	void Disconnect();

	Task<Result> SendMessage(string message);
}