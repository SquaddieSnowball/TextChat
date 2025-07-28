using Gtk;
using System.Net;
using TextChat.Application.Services.Abstractions;
using TextChat.Domain.Primitives;
using GtkApplication = Gtk.Application;
using UIObject = Gtk.Builder.ObjectAttribute;

namespace TextChat.UI.GTK.Views;

internal class MainWindow : Window
{
	private const int Port = 12345;

	private readonly IChatClient _chatClient;
	private readonly IChatServer _chatServer;

	[UIObject] private readonly Entry _ipAddress = default!;
	[UIObject] private readonly CheckButton _serverMode = default!;
	[UIObject] private readonly Button _connect = default!;
	[UIObject] private readonly TextView _chatHistory = default!;
	[UIObject] private readonly Entry _message = default!;
	[UIObject] private readonly Button _send = default!;

	private bool _connected;

	public MainWindow(IChatClient chatClient, IChatServer chatServer)
		: this(new Builder("MainWindow.glade"), chatClient, chatServer) { }

	private MainWindow(Builder builder, IChatClient chatClient, IChatServer chatServer)
		: base(builder.GetRawOwnedObject("MainWindow"))
	{
		builder.Autoconnect(this);

		(_chatClient, _chatServer) = (chatClient, chatServer);

		_connect.Clicked += ConnectOnClicked;
		_send.Clicked += SendOnClicked;

		DeleteEvent += WindowOnDeleteEvent;
	}

	private async void ConnectOnClicked(object? sender, EventArgs e)
	{
		if (!_serverMode.Active)
			await ClientConnect();
		else
			ServerConnect();
	}

	private async void SendOnClicked(object? sender, EventArgs e)
	{
		if (!_serverMode.Active)
			await _chatClient.SendMessage(_message.Text);
		else
			await _chatServer.BroadcastMessage(_message.Text);

		_message.Text = string.Empty;
	}

	private async Task ClientConnect()
	{
		if (!_connected)
		{
			SubscribeToClientChatEvents();

			Result connectResult = await _chatClient.Connect(_ipAddress.Text, Port);

			if (connectResult.IsSuccess)
			{
				SwitchConnectControls(true);

				_connected = true;
			}
			else
				UnsubscribeFromClientChatEvents();
		}
		else
		{
			_chatClient.Disconnect();

			SwitchConnectControls(false);

			_connected = false;

			UnsubscribeFromClientChatEvents();
		}
	}

	private void ServerConnect()
	{
		if (!_connected)
		{
			SubscribeToServerChatEvents();

			Result connectResult = _chatServer.Start(_ipAddress.Text, Port);

			if (connectResult.IsSuccess)
			{
				SwitchConnectControls(true);

				_connected = true;
			}
			else
				UnsubscribeFromServerChatEvents();
		}
		else
		{
			_chatServer.Stop();

			SwitchConnectControls(false);

			_connected = false;

			UnsubscribeFromServerChatEvents();
		}
	}

	private void SwitchConnectControls(bool connected)
	{
		if (connected)
		{
			_ipAddress.Sensitive = false;
			_serverMode.Sensitive = false;
			_connect.Label = "Disconnect";
			_message.Sensitive = true;
			_send.Sensitive = true;
		}
		else
		{
			_ipAddress.Sensitive = true;
			_serverMode.Sensitive = true;
			_connect.Label = "Connect";
			_message.Sensitive = false;
			_send.Sensitive = false;
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
		_chatHistory.Buffer.Text += $"{e.ClientIPAddress}: {e.Body}{Environment.NewLine}";

	private void ChatClientOnClientConnected(object? sender, IPEndPoint e) =>
		_chatHistory.Buffer.Text += $"You have joined the chat{Environment.NewLine}";

	private void ChatClientOnClientDisconnected(object? sender, IPEndPoint e)
	{
		_chatHistory.Buffer.Text += $"You have left the chat{Environment.NewLine}";

		SwitchConnectControls(false);

		_connected = false;

		UnsubscribeFromClientChatEvents();
	}

	private void ChatServerOnServerStarted(object? sender, IPEndPoint e) =>
		_chatHistory.Buffer.Text += $"Server started at {e.Address}:{e.Port}{Environment.NewLine}";

	private void ChatServerOnServerStopped(object? sender, IPEndPoint e) =>
		_chatHistory.Buffer.Text += $"Server stopped at {e.Address}:{e.Port}{Environment.NewLine}";

	private void ChatServerOnClientConnected(object? sender, IPEndPoint e) =>
		_chatHistory.Buffer.Text += $"{e.Address} joined{Environment.NewLine}";

	private void ChatServerOnClientDisconnected(object? sender, IPEndPoint e) =>
		_chatHistory.Buffer.Text += $"{e.Address} left{Environment.NewLine}";

	private void WindowOnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Program.AppHost.Dispose();

		GtkApplication.Quit();
	}
}