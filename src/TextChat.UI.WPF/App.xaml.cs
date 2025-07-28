using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;
using TextChat.Application.Services;
using TextChat.Application.Services.Abstractions;
using TextChat.UI.WPF.ViewModels;
using TextChat.UI.WPF.Views;
using WpfApplication = System.Windows.Application;

namespace TextChat.UI.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : WpfApplication
{
	public static IHost AppHost { get; private set; }

	static App()
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
					.AddSingleton<MainWindow>()
					.AddTransient<MainViewModel>();
			});

		AppHost = builder.Build();
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		MainWindow mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
		mainWindow.Show();

		base.OnStartup(e);
	}

	protected override void OnExit(ExitEventArgs e)
	{
		AppHost.Dispose();

		base.OnExit(e);
	}
}