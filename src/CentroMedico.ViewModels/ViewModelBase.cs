using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CentroMedico.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    private readonly List<ViewModelCommand> _comandos = [];
    private string? _mensaje;
    private string? _error;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Aviso de que una operación salió bien (se muestra en verde).</summary>
    public string? Mensaje
    {
        get => _mensaje;
        protected set => Set(ref _mensaje, value);
    }

    /// <summary>Error de validación o de negocio (se muestra en rojo).</summary>
    public string? Error
    {
        get => _error;
        protected set => Set(ref _error, value);
    }

    protected bool Set<T>(ref T campo, T valor, [CallerMemberName] string? propiedad = null)
    {
        if (EqualityComparer<T>.Default.Equals(campo, valor)) return false;
        campo = valor;
        OnPropertyChanged(propiedad);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propiedad = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));
        // Cualquier cambio puede habilitar o deshabilitar un botón.
        foreach (var comando in _comandos) comando.NotificarCambio();
    }

    protected ViewModelCommand Comando(Action ejecutar, Func<bool>? puede = null)
    {
        var comando = new ViewModelCommand(_ => ejecutar(), puede is null ? null : _ => puede());
        _comandos.Add(comando);
        return comando;
    }

    protected ViewModelCommand Comando<T>(Action<T> ejecutar, Func<T, bool>? puede = null)
    {
        var comando = new ViewModelCommand(
            p => { if (p is T valor) ejecutar(valor); },
            puede is null ? null : p => p is T valor && puede(valor));
        _comandos.Add(comando);
        return comando;
    }

    /// <summary>
    /// Ejecuta una acción del usuario y muestra el resultado en Mensaje o Error.
    /// Solo se atrapan las reglas de negocio; un error inesperado debe verse.
    /// </summary>
    protected bool Intentar(Action accion, string? exito = null)
    {
        Error = null;
        Mensaje = null;
        try
        {
            accion();
            Mensaje = exito;
            return true;
        }
        catch (Core.Servicios.ReglaNegocioException ex)
        {
            Error = ex.Message;
            return false;
        }
    }
}
