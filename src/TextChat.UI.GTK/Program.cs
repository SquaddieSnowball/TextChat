using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TextChat.Application.Services;
using TextChat.Application.Services.Abstractions;
using TextChat.UI.GTK.Views;
using GtkApplication = Gtk.Application;

namespace TextChat.UI.GTK;

internal class Program
{
	public static IHost AppHost { get; private set; }

	static Program()
	{
		IHostBuilder builder = Host.CreateDefaultBuilder();

		builder
			.ConfigureLogging(logging =>
			{
				logging
					.ClearProviders();
			})
			.ConfigureServices((context, services) =>
			{
				services
					.AddTransient<IChatClient, ChatClient>()
					.AddTransient<IChatServer, ChatServer>()
					.AddTransient<IChatMessageParser, ChatMessageParser>()
					.AddTransient<IChatMessageBuilder, ChatMessageBuilder>()
					.AddSingleton<MainWindow>();
			});

		AppHost = builder.Build();
	}

	[STAThread]
	public static void Main()
	{
		GtkApplication.Init();

		MainWindow mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
		mainWindow.Show();

		GtkApplication.Run();
	}
}