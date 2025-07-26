using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using TextChat.Application.Services.Abstractions;
using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services;

public class ChatServer : IChatServer
{
	private TcpListener? _server;
	private bool _disposed;
	private readonly ConcurrentDictionary<Guid, ServerClient> _connectedClients = new();

	private readonly IClientChatMessageParser _clientMessageParser;
	private readonly IServerChatMessageBuilder _serverMessageBuilder;

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

	public ChatServer(
		IClientChatMessageParser clientMessageParser,
		IServerChatMessageBuilder serverMessageBuilder) =>
		(_clientMessageParser, _serverMessageBuilder) =
		(clientMessageParser, serverMessageBuilder);

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
			return new Error("Code", "Description"); // TODO
		}
		catch (ArgumentOutOfRangeException)
		{
			return new Error("Code", "Description"); // TODO
		}
		catch (SocketException)
		{
			return new Error("Code", "Description"); // TODO
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

		ServerStopped?.Invoke(this, ServerEndpoint!);
	}

	public Task<Result> BroadcastMessage(string message) => BroadcastMessage(message);

	private async Task<Result> BroadcastMessage(string message, ServerClient? broadcastingServerClient = default)
	{
		if (!Started)
			return new Error("Code", "Description"); // TODO

		ServerChatMessage serverChatMessage = new(
			broadcastingServerClient is not null
				? broadcastingServerClient.Endpoint.Address
				: ServerEndpoint!.Address,
			DateTime.Now,
			message);

		Result<string> buildResult = await _serverMessageBuilder.Build(serverChatMessage);

		if (buildResult.IsFailure)
			return buildResult.Error;

		foreach (ServerClient serverClient in _connectedClients.Values)
		{
			try
			{
				await serverClient.StreamWriter.WriteAsync(buildResult.Value);
				await serverClient.StreamWriter.FlushAsync();
			}
			catch (IOException)
			{
				return new Error("Code", "Description"); // TODO
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
			return new Error("Code", "Description"); // TODO
		}

		if (message is null)
			return new Error("Code", "Description"); // TODO

		Result<ClientChatMessage> parseResult = await _clientMessageParser.Parse(message);

		return parseResult.IsSuccess
			? parseResult
			: new Error("Code", "Description"); // TODO
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
				ErrorAcceptingClient?.Invoke(this, new Error("Code", "Description")); // TODO

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

				serverClient.TcpClient.Close();
				serverClient.StreamReader.Close();
				serverClient.StreamWriter.Close();

				_connectedClients.TryRemove(serverClient.Id, out _);

				break;
			}
		}
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
			_server?.Stop();
			_server = default;

			foreach (ServerClient serverClient in _connectedClients.Values)
			{
				serverClient.TcpClient.Close();
				serverClient.StreamReader.Close();
				serverClient.StreamWriter.Close();

				ClientDisconnected?.Invoke(this, serverClient.Endpoint);
			}

			_connectedClients.Clear();

			Started = false;
			ServerEndpoint = default;
		}

		_disposed = true;
	}
}