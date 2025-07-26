using GtkApplication = Gtk.Application;
using TextChat.UI.GTK.Views;

namespace TextChat.UI.GTK;

internal class Program
{
	[STAThread]
	public static void Main()
	{
		GtkApplication.Init();

		GtkApplication application = new("ru.Trackmark.TextChat", GLib.ApplicationFlags.None);
		application.Register(GLib.Cancellable.Current);

		MainWindow mainWindow = new();
		application.AddWindow(mainWindow);
		mainWindow.Show();

		GtkApplication.Run();
	}
}