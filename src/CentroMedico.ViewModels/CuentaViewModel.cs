using System.Windows.Input;
using CentroMedico.Core.Servicios;

namespace CentroMedico.ViewModels;

public class CuentaViewModel : ViewModelBase
{
    private readonly Contexto _contexto;
    private string _actual = "";
    private string _nueva = "";
    private string _confirmacion = "";

    public CuentaViewModel(Contexto contexto, Sesion sesion)
    {
        _contexto = contexto;
        Sesion = sesion;
        CambiarCommand = Comando(Cambiar, () => Actual.Length > 0 && Nueva.Length > 0 && Confirmacion.Length > 0);
    }

    /// <summary>La vista limpia los PasswordBox cuando se dispara.</summary>
    public event Action? ContrasenaCambiada;

    public Sesion Sesion { get; }
    public string Usuario => Sesion.Usuario.NombreUsuario;
    public string Nombre => Sesion.Empleado.NombreCompleto;
    public string Rol => Sesion.EsAdministrador ? "Administrador" : "Recepción";
    public string Cargo => Sesion.Empleado.Cargo switch
    {
        Core.Modelos.Cargo.Medico => $"Médico ({Sesion.Empleado.Especialidad})",
        Core.Modelos.Cargo.Administracion => "Administración",
        _ => "Atención al cliente"
    };

    public string Actual { get => _actual; set => Set(ref _actual, value); }
    public string Nueva { get => _nueva; set => Set(ref _nueva, value); }
    public string Confirmacion { get => _confirmacion; set => Set(ref _confirmacion, value); }

    public ICommand CambiarCommand { get; }

    private void Cambiar()
    {
        bool listo = Intentar(() =>
        {
            if (Nueva != Confirmacion)
                throw new ReglaNegocioException("La confirmación no coincide con la contraseña nueva.");
            _contexto.Autenticacion.CambiarContrasena(Sesion, Actual, Nueva);
        }, "Contraseña actualizada.");

        if (listo)
        {
            Actual = Nueva = Confirmacion = "";
            ContrasenaCambiada?.Invoke();
        }
    }
}
