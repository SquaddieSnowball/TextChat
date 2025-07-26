using System.Net;
using System.Net.Sockets;
using TextChat.Application.Services.Abstractions;
using TextChat.Domain.Entities;
using TextChat.Domain.Primitives;

namespace TextChat.Application.Services;

public class ChatClient : IChatClient
{
	private readonly TcpClient _client = new();
	private bool _disposed;
	private StreamReader? _streamReader;
	private StreamWriter? _streamWriter;

	private readonly IChatMessageParser _chatMessageParser;
	private readonly IChatMessageBuilder _chatMessageBuilder;

	public bool Connected { get; private set; }

	public IPEndPoint? ConnectionEndpoint { get; private set; }

	public event EventHandler<IPEndPoint>? ClientConnected;

	public event EventHandler<IPEndPoint>? ClientDisconnected;

	public event EventHandler<ServerChatMessage>? MessageReceived;

	public event EventHandler<Error>? ErrorReceivingMessage;

	public ChatClient(IChatMessageParser chatMessageParser, IChatMessageBuilder chatMessageBuilder) =>
		(_chatMessageParser, _chatMessageBuilder) = (chatMessageParser, chatMessageBuilder);

	public async Task<Result> Connect(string ipAddress, int port)
	{
		if (Connected)
			return Result.Success();

		try
		{
			ConnectionEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

			await _client.ConnectAsync(ConnectionEndpoint);

			_streamReader = new StreamReader(_client.GetStream());
			_streamWriter = new StreamWriter(_client.GetStream());
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
		Connected = true;

		ClientConnected?.Invoke(this, ConnectionEndpoint);

		StartReceivingMessages();

		return Result.Success();
	}

	public void Disconnect()
	{
		if (!Connected)
			return;

		Dispose();

		ClientDisconnected?.Invoke(this, ConnectionEndpoint!);
	}

	public async Task<Result> SendMessage(string message)
	{
		if (!Connected)
			return new Error("Code", "Description"); // TODO

		ClientChatMessage clientChatMessage = new(DateTime.Now, message);
		Result<string> buildResult = await _chatMessageBuilder.BuildClientMessage(clientChatMessage);

		if (buildResult.IsFailure)
			return buildResult.Error;

		try
		{
			await _streamWriter!.WriteAsync(buildResult.Value);
			await _streamWriter.FlushAsync();
		}
		catch (IOException)
		{
			return new Error("Code", "Description"); // TODO
		}

		return Result.Success();
	}

	private async Task<Result<ServerChatMessage>> ReceiveMessage()
	{
		string? message;

		try
		{
			message = await _streamReader!.ReadLineAsync();
		}
		catch (IOException)
		{
			return new Error("Code", "Description"); // TODO
		}

		if (message is null)
			return new Error("Code", "Description"); // TODO

		Result<ServerChatMessage> parseResult = await _chatMessageParser.ParseServerMessage(message);

		return parseResult.IsSuccess
			? parseResult
			: new Error("Code", "Description"); // TODO
	}

	private void StartReceivingMessages()
	{
		Task.Run(async () =>
		{
			while (Connected)
			{
				Result<ServerChatMessage> receiveMessageResult = await ReceiveMessage();

				if (receiveMessageResult.IsSuccess)
					MessageReceived?.Invoke(this, receiveMessageResult.Value);
				else
					ErrorReceivingMessage?.Invoke(this, receiveMessageResult.Error);
			}
		});
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
			_client.Close();
			_streamReader?.Close();
			_streamWriter?.Close();

			Connected = false;
			ConnectionEndpoint = default;
		}

		_disposed = true;
	}
}