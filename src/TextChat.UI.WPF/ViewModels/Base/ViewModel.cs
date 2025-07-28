using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TextChat.UI.WPF.ViewModels.Base;

internal abstract class ViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual bool Set<T>(
		ref T field,
		T value,
		[CallerMemberName] string? propertyName = default)
	{
		if (Equals(field, value))
			return false;

		field = value;
		OnPropertyChanged(propertyName);

		return true;
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = default) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}