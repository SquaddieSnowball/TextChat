using System.Net;
using TextChat.Application.Services.Abstractions;
using TextChat.Domain.Primitives;
using TextChat.UI.WPF.Commands.Base;
using TextChat.UI.WPF.ViewModels.Base;

namespace TextChat.UI.WPF.ViewModels;

internal class MainViewModel : ViewModel
{
	private const int Port = 12345;

	private readonly IChatClient _chatClient;
	private readonly IChatServer _chatServer;

	private string _ipAddress = "127.0.0.1";
	private bool _serverMode;
	private string _connect = "Connect";
	private string _chatHistory = string.Empty;
	private string _message = string.Empty;

	private bool _connected;

	public string IPAddress
	{
		get => _ipAddress;
		set => Set(ref _ipAddress, value);
	}

	public bool ServerMode
	{
		get => _serverMode;
		set => Set(ref _serverMode, value);
	}

	public string Connect
	{
		get => _connect;
		set => Set(ref _connect, value);
	}

	public string ChatHistory
	{
		get => _chatHistory;
		set => Set(ref _chatHistory, value);
	}

	public string Message
	{
		get => _message;
		set => Set(ref _message, value);
	}

	public bool Connected
	{
		get => _connected;
		set => Set(ref _connected, value);
	}

	public RelayCommand ConnectCommand { get; private set; }

	public RelayCommand SendCommand { get; private set; }

	public MainViewModel(IChatClient chatClient, IChatServer chatServer)
	{
		(_chatClient, _chatServer) = (chatClient, chatServer);

		ConnectCommand = new RelayCommand(OnConnectCommandExecute, CanConnectCommandExecute);
		SendCommand = new RelayCommand(OnSendCommandExecute, CanSendCommandExecute);
	}

	private async void OnConnectCommandExecute()
	{
		if (!CanConnectCommandExecute())
			return;

		if (!ServerMode)
			await ClientConnect();
		else
			ServerConnect();
	}

	private bool CanConnectCommandExecute() => true;

	private async void OnSendCommandExecute()
	{
		if (!CanSendCommandExecute())
			return;

		if (!ServerMode)
			await _chatClient.SendMessage(Message);
		else
			await _chatServer.BroadcastMessage(Message);

		Message = string.Empty;
	}

	private bool CanSendCommandExecute() => true;

	private async Task ClientConnect()
	{
		if (!Connected)
		{
			SubscribeToClientChatEvents();

			Result connectResult = await _chatClient.Connect(IPAddress, Port);

			if (connectResult.IsSuccess)
			{
				Connect = "Disconnect";
				Connected = true;
			}
			else
				UnsubscribeFromClientChatEvents();
		}
		else
		{
			_chatClient.Disconnect();

			Connect = "Connect";
			Connected = false;

			UnsubscribeFromClientChatEvents();
		}
	}

	private void ServerConnect()
	{
		if (!Connected)
		{
			SubscribeToServerChatEvents();

			Result connectResult = _chatServer.Start(IPAddress, Port);

			if (connectResult.IsSuccess)
			{
				Connect = "Disconnect";
				Connected = true;
			}
			else
				UnsubscribeFromServerChatEvents();
		}
		else
		{
			_chatServer.Stop();

			Connect = "Connect";
			Connected = false;

			UnsubscribeFromServerChatEvents();
		}
	}

	private void SubscribeToClientChatEvents()
	{
		_chatClient.ClientConnected += ChatClientOnClientConnected;
		_chatClient.ClientDisconnected += ChatClientOnClientDisconnected;
		_chatClient.MessageReceived += ChatOnMessage;
	}

	private void SubscribeToServerChatEvents()
	{
		_chatServer.ServerStarted += ChatServerOnServerStarted;
		_chatServer.ServerStopped += ChatServerOnServerStopped;
		_chatServer.ClientConnected += ChatServerOnClientConnected;
		_chatServer.ClientDisconnected += ChatServerOnClientDisconnected;
		_chatServer.MessageBroadcasted += ChatOnMessage;
	}

	private void UnsubscribeFromClientChatEvents()
	{
		_chatClient.ClientConnected -= ChatClientOnClientConnected;
		_chatClient.ClientDisconnected -= ChatClientOnClientDisconnected;
		_chatClient.MessageReceived -= ChatOnMessage;
	}

	private void UnsubscribeFromServerChatEvents()
	{
		_chatServer.ServerStarted -= ChatServerOnServerStarted;
		_chatServer.ServerStopped -= ChatServerOnServerStopped;
		_chatServer.ClientConnected -= ChatServerOnClientConnected;
		_chatServer.ClientDisconnected -= ChatServerOnClientDisconnected;
		_chatServer.MessageBroadcasted -= ChatOnMessage;
	}

	private void ChatOnMessage(object? sender, Domain.Entities.ServerChatMessage e) =>
		ChatHistory += $"{e.ClientIPAddress}: {e.Body}{Environment.NewLine}";

	private void ChatClientOnClientConnected(object? sender, IPEndPoint e) =>
		ChatHistory += $"You have joined the chat{Environment.NewLine}";

	private void ChatClientOnClientDisconnected(object? sender, IPEndPoint e)
	{
		ChatHistory += $"You have left the chat{Environment.NewLine}";

		Connect = "Connect";
		Connected = false;

		UnsubscribeFromClientChatEvents();
	}

	private void ChatServerOnServerStarted(object? sender, IPEndPoint e) =>
		ChatHistory += $"Server started at {e.Address}:{e.Port}{Environment.NewLine}";

	private void ChatServerOnServerStopped(object? sender, IPEndPoint e) =>
		ChatHistory += $"Server stopped at {e.Address}:{e.Port}{Environment.NewLine}";

	private void ChatServerOnClientConnected(object? sender, IPEndPoint e) =>
		ChatHistory += $"{e.Address} joined{Environment.NewLine}";

	private void ChatServerOnClientDisconnected(object? sender, IPEndPoint e) =>
		ChatHistory += $"{e.Address} left{Environment.NewLine}";
}