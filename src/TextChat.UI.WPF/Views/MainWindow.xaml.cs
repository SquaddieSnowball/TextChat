using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TextChat.UI.WPF.ViewModels;

namespace TextChat.UI.WPF.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		DataContext = App.AppHost.Services.GetRequiredService<MainViewModel>();
	}
}