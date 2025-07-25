using Gtk;
using TextChat.UI.GTK.Views;

namespace TextChat.UI.GTK;

internal class Program
{
	[STAThread]
	public static void Main()
	{
		Application.Init();

		Application application = new("ru.Trackmark.TextChat", GLib.ApplicationFlags.None);
		application.Register(GLib.Cancellable.Current);

		MainWindow mainWindow = new();
		application.AddWindow(mainWindow);
		mainWindow.Show();

		Application.Run();
	}
}