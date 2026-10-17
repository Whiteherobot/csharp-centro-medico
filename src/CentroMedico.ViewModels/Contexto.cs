using CentroMedico.Core.Repositorios;
using CentroMedico.Core.Servicios;
using CentroMedico.Datos.Memoria;
using CentroMedico.Datos.Oracle;

namespace CentroMedico.ViewModels;

/// <summary>Repositorios y servicios de la aplicación, armados para Oracle o para el modo demo.</summary>
public class Contexto
{
    public required IPacienteRepository Pacientes { get; init; }
    public required IEmpleadoRepository Empleados { get; init; }
    public required IUsuarioRepository Usuarios { get; init; }
    public required IServicioRepository Servicios { get; init; }
    public required ICitaRepository Citas { get; init; }
    public required IFacturaRepository Facturas { get; init; }
    public required TimeProvider Reloj { get; init; }
    public required IDialogos Dialogos { get; init; }
    public bool ModoDemo { get; init; }

    public AgendaService Agenda => new(Citas, Pacientes, Empleados, Servicios, Reloj);
    public FacturacionService Facturacion => new(Facturas, Citas, Pacientes, Servicios, Reloj);
    public AutenticacionService Autenticacion => new(Usuarios, Empleados);
    public RegistroService Registro => new(Pacientes, Empleados, Servicios);

    public DateTime Ahora => Reloj.GetLocalNow().DateTime;
    public DateOnly Hoy => DateOnly.FromDateTime(Ahora);

    public static Contexto Oracle(string cadena, TimeProvider reloj, IDialogos dialogos)
    {
        var db = new ConexionOracle(cadena);
        return new Contexto
        {
            Pacientes = new PacienteRepositoryOracle(db),
            Empleados = new EmpleadoRepositoryOracle(db),
            Usuarios = new UsuarioRepositoryOracle(db),
            Servicios = new ServicioRepositoryOracle(db),
            Citas = new CitaRepositoryOracle(db),
            Facturas = new FacturaRepositoryOracle(db),
            Reloj = reloj,
            Dialogos = dialogos
        };
    }

    public static Contexto Demo(TimeProvider reloj, IDialogos dialogos)
    {
        var db = new AlmacenMemoria();
        var contexto = new Contexto
        {
            Pacientes = new PacienteRepositoryMemoria(db),
            Empleados = new EmpleadoRepositoryMemoria(db),
            Usuarios = new UsuarioRepositoryMemoria(db),
            Servicios = new ServicioRepositoryMemoria(db),
            Citas = new CitaRepositoryMemoria(db),
            Facturas = new FacturaRepositoryMemoria(db),
            Reloj = reloj,
            Dialogos = dialogos,
            ModoDemo = true
        };
        DatosDemo.Cargar(contexto);
        return contexto;
    }
}
