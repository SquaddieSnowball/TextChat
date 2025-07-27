using System.Windows.Input;

namespace TextChat.UI.WPF.Commands.Base;

internal abstract class Command : ICommand
{
	public event EventHandler? CanExecuteChanged;

	bool ICommand.CanExecute(object? parameter) => CanExecute(parameter);

	void ICommand.Execute(object? parameter)
	{
		if (((ICommand)this).CanExecute(parameter) is true)
			Execute(parameter);
	}

	protected abstract bool CanExecute(object? parameter);

	protected abstract void Execute(object? parameter);

	public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}