using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using TextChat.Application.Errors;
using TextChat.Application.Services.Abstractions;
using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services;

public class ChatServer : IChatServer
{
	private TcpListener? _server;
	private bool _disposed;
	private readonly ConcurrentDictionary<Guid, ServerClient> _connectedClients = new();

	private readonly IChatMessageParser _chatMessageParser;
	private readonly IChatMessageBuilder _chatMessageBuilder;

	public bool Started { get; private set; }

	public IPEndPoint? ServerEndpoint { get; private set; }

	public IEnumerable<IPEndPoint> ConnectedClientEndpoints =>
		_connectedClients.Values.Select(c => c.Endpoint);

	public event EventHandler<IPEndPoint>? ServerStarted;

	public event EventHandler<IPEndPoint>? ServerStopped;

	public event EventHandler<IPEndPoint>? ClientConnected;

	public event EventHandler<IPEndPoint>? ClientDisconnected;

	public event EventHandler<Error>? ErrorAcceptingClient;

	public event EventHandler<ClientChatMessage>? MessageReceived;

	public event EventHandler<Error>? ErrorReceivingMessage;

	public event EventHandler<ServerChatMessage>? MessageBroadcasted;

	public event EventHandler<Error>? ErrorBroadcastingClientMessage;

	public ChatServer(IChatMessageParser chatMessageParser, IChatMessageBuilder chatMessageBuilder) =>
		(_chatMessageParser, _chatMessageBuilder) = (chatMessageParser, chatMessageBuilder);

	public Result Start(string ipAddress, int port)
	{
		if (Started)
			return Result.Success();

		try
		{
			ServerEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

			_server = new TcpListener(ServerEndpoint);
			_server.Start();
		}
		catch (FormatException)
		{
			return ChatErrors.WrongIP;
		}
		catch (ArgumentOutOfRangeException)
		{
			return ChatErrors.WrongPort;
		}
		catch (SocketException)
		{
			return ChatErrors.UnableToStart;
		}

		_disposed = false;
		Started = true;

		ServerStarted?.Invoke(this, ServerEndpoint);

		AcceptClient();

		return Result.Success();
	}

	public void Stop()
	{
		if (!Started)
			return;

		Dispose();
	}

	public Task<Result> BroadcastMessage(string message) => BroadcastMessage(message, default);

	private async Task<Result> BroadcastMessage(string message, ServerClient? broadcastingServerClient = default)
	{
		if (!Started)
			return ChatErrors.NotRunning;

		ServerChatMessage serverChatMessage = new(
			broadcastingServerClient is not null
				? broadcastingServerClient.Endpoint.Address.ToString()
				: ServerEndpoint!.Address.ToString(),
			DateTime.Now,
			message);

		Result<string> buildResult = await _chatMessageBuilder.BuildServerMessage(serverChatMessage);

		if (buildResult.IsFailure)
			return buildResult.Error;

		foreach (ServerClient serverClient in _connectedClients.Values)
		{
			try
			{
				await serverClient.StreamWriter.WriteLineAsync(buildResult.Value);
				await serverClient.StreamWriter.FlushAsync();
			}
			catch (IOException)
			{
				RemoveClient(serverClient);

				if (!Started)
					break;
			}
		}

		MessageBroadcasted?.Invoke(this, serverChatMessage);

		return Result.Success();
	}

	private async Task<Result<ClientChatMessage>> ReceiveMessage(ServerClient serverClient)
	{
		string? message;

		try
		{
			message = await serverClient.StreamReader.ReadLineAsync();
		}
		catch (IOException)
		{
			return ChatErrors.ClientDisconnected;
		}

		if (message is null)
			return ChatErrors.ClientDisconnected;

		Result<ClientChatMessage> parseResult = await _chatMessageParser.ParseClientMessage(message);

		return parseResult;
	}

	private void AcceptClient()
	{
		Task.Run(async () =>
		{
			if (!Started)
				return;

			TcpClient client;

			try
			{
				client = await _server!.AcceptTcpClientAsync();
			}
			catch (SocketException)
			{
				ErrorAcceptingClient?.Invoke(this, ChatErrors.CanNotAcceptClient);

				return;
			}
			catch (NullReferenceException)
			{
				return;
			}
			finally
			{
				AcceptClient();
			}

			ServerClient serverClient = new(
				Guid.NewGuid(),
				client,
				(IPEndPoint)client.Client.RemoteEndPoint!,
				new StreamReader(client.GetStream()),
				new StreamWriter(client.GetStream()));

			_connectedClients.TryAdd(serverClient.Id, serverClient);

			ClientConnected?.Invoke(this, serverClient.Endpoint);

			await StartReceivingMessages(serverClient);
		});
	}

	private async Task StartReceivingMessages(ServerClient serverClient)
	{
		while (Started)
		{
			Result<ClientChatMessage> receiveMessageResult = await ReceiveMessage(serverClient);

			if (receiveMessageResult.IsSuccess)
			{
				MessageReceived?.Invoke(this, receiveMessageResult.Value);

				Result broadcastMessageResult = await BroadcastMessage(receiveMessageResult.Value.Body, serverClient);

				if (broadcastMessageResult.IsFailure)
					ErrorBroadcastingClientMessage?.Invoke(this, broadcastMessageResult.Error);
			}
			else
			{
				ErrorReceivingMessage?.Invoke(this, receiveMessageResult.Error);

				RemoveClient(serverClient);

				break;
			}
		}
	}

	private void RemoveClient(ServerClient serverClient)
	{
		serverClient.TcpClient.Close();
		serverClient.StreamReader.Close();
		serverClient.StreamWriter.Close();

		_connectedClients.TryRemove(serverClient.Id, out _);

		if (Started)
			ClientDisconnected?.Invoke(this, serverClient.Endpoint);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			Started = false;

			_server?.Stop();
			_server = default;

			foreach (ServerClient serverClient in _connectedClients.Values)
			{
				serverClient.TcpClient.Close();
				serverClient.StreamReader.Close();
				serverClient.StreamWriter.Close();
			}

			_connectedClients.Clear();

			ServerStopped?.Invoke(this, ServerEndpoint!);

			ServerEndpoint = default;
		}

		_disposed = true;
	}
}