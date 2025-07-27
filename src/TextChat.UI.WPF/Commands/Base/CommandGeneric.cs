using System.Windows.Input;

namespace TextChat.UI.WPF.Commands.Base;

internal abstract class Command<T> : ICommand
{
	private readonly Type _parameterType = typeof(T);

	public event EventHandler? CanExecuteChanged;

	bool ICommand.CanExecute(object? parameter)
	{
		if (parameter?.GetType().Equals(_parameterType) is false)
			throw new ArgumentException("The parameter type must match the command parameter type.", nameof(parameter));

		return CanExecute(parameter);
	}

	void ICommand.Execute(object? parameter)
	{
		if (parameter?.GetType().Equals(_parameterType) is false)
			throw new ArgumentException("The parameter type must match the command parameter type.", nameof(parameter));

		if (((ICommand)this).CanExecute(parameter) is true)
			Execute(parameter);
	}

	protected abstract bool CanExecute(object? parameter);

	protected abstract void Execute(object? parameter);

	public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}