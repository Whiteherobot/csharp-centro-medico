using System.Windows.Input;

namespace CentroMedico.ViewModels;

public class ViewModelCommand(Action<object?> ejecutar, Predicate<object?>? puede = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parametro) => puede?.Invoke(parametro) ?? true;

    public void Execute(object? parametro)
    {
        if (CanExecute(parametro)) ejecutar(parametro);
    }

    public void NotificarCambio() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
