using Gtk;
using UIObject = Gtk.Builder.ObjectAttribute;

namespace TextChat.UI.GTK.Views;

internal class MainWindow : Window
{
	[UIObject] private readonly Label _label = default!;
	[UIObject] private readonly Button _button = default!;

	private int _counter;

	public MainWindow() : this(new Builder("MainWindow.glade")) { }

	private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
	{
		builder.Autoconnect(this);

		DeleteEvent += WindowOnDeleteEvent;

		_button.Clicked += ButtonOnClicked;
	}

	private void WindowOnDeleteEvent(object sender, DeleteEventArgs a) => Application.Quit();

	private void ButtonOnClicked(object? sender, EventArgs a)
	{
		_counter++;
		_label.Text = $"Hello World! This button has been clicked {_counter} time(s).";
	}
}