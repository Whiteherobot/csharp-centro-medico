using System.Windows.Input;
using CentroMedico.Core.Servicios;

namespace CentroMedico.ViewModels;

/// <summary>Entrada del menú lateral. Icono es el nombre de un icono de Font Awesome.</summary>
public record OpcionMenu(string Titulo, string Icono, Func<ViewModelBase> Crear);

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _vistaActual = null!;
    private OpcionMenu _opcionActual = null!;

    public MainViewModel(Contexto contexto, Sesion sesion)
    {
        Sesion = sesion;
        ModoDemo = contexto.ModoDemo;

        var opciones = new List<OpcionMenu>
        {
            new("Inicio", "House", () => new InicioViewModel(contexto)),
            new("Pacientes", "HospitalUser", () => new PacientesViewModel(contexto)),
        };
        if (sesion.EsAdministrador)
            opciones.Add(new("Personal", "UserDoctor", () => new PersonalViewModel(contexto, sesion)));
        opciones.Add(new("Mi cuenta", "UserGear", () => new CuentaViewModel(contexto, sesion)));
        Opciones = opciones;

        IrACommand = Comando<OpcionMenu>(IrA);
        CerrarSesionCommand = Comando(() => SesionCerrada?.Invoke());
        IrA(opciones[0]);
    }

    public event Action? SesionCerrada;

    public Sesion Sesion { get; }
    public bool ModoDemo { get; }
    public IReadOnlyList<OpcionMenu> Opciones { get; }

    public string NombreUsuario => Sesion.Empleado.NombreCompleto;
    public string Rol => Sesion.EsAdministrador ? "Administrador" : "Recepción";

    public ViewModelBase VistaActual
    {
        get => _vistaActual;
        private set => Set(ref _vistaActual, value);
    }

    public OpcionMenu OpcionActual
    {
        get => _opcionActual;
        private set => Set(ref _opcionActual, value);
    }

    public ICommand IrACommand { get; }
    public ICommand CerrarSesionCommand { get; }

    public void IrA(OpcionMenu opcion)
    {
        OpcionActual = opcion;
        VistaActual = opcion.Crear();
    }
}
