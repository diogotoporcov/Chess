using System.Windows.Input;

namespace Chess.Desktop.Infrastructure.Commands;

public sealed class RelayCommand<T> : ICommand where T : class
{
    private readonly Action<T> _execute;
    private readonly Predicate<T>? _canExecute;

    public RelayCommand(
        Action<T> execute,
        Predicate<T>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(
        object? parameter)
    {
        return parameter is T value && (_canExecute?.Invoke(value) ?? true);
    }

    public void Execute(
        object? parameter)
    {
        if (parameter is not T value)
        {
            throw new ArgumentException(
                $"Command parameter must be of type {typeof(T).Name}.",
                nameof(parameter));
        }

        _execute(value);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
