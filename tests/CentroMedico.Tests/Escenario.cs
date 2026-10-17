using CentroMedico.Core.Modelos;
using CentroMedico.Datos.Memoria;

namespace CentroMedico.Tests;

/// <summary>Reloj fijo en hora local de Ecuador (UTC-5) para que las pruebas no dependan del día.</summary>
public class RelojFijo(DateTime ahoraLocal) : TimeProvider
{
    private static readonly TimeZoneInfo Ecuador =
        TimeZoneInfo.CreateCustomTimeZone("ECT", TimeSpan.FromHours(-5), "Ecuador", "Ecuador");

    public DateTime Ahora { get; set; } = ahoraLocal;

    public override TimeZoneInfo LocalTimeZone => Ecuador;

    public override DateTimeOffset GetUtcNow() =>
        new DateTimeOffset(Ahora, TimeSpan.FromHours(-5)).ToUniversalTime();
}

/// <summary>Datos mínimos que comparten las pruebas de servicios.</summary>
public class Escenario
{
    // Lunes 19 de octubre de 2026, 07:00.
    public static readonly DateTime Lunes = new(2026, 10, 19, 7, 0, 0);

    public AlmacenMemoria Db { get; } = new();
    public RelojFijo Reloj { get; } = new(Lunes);

    public PacienteRepositoryMemoria Pacientes { get; }
    public EmpleadoRepositoryMemoria Empleados { get; }
    public ServicioRepositoryMemoria Servicios { get; }
    public CitaRepositoryMemoria Citas { get; }
    public FacturaRepositoryMemoria Facturas { get; }
    public UsuarioRepositoryMemoria Usuarios { get; }

    public Paciente Paciente { get; } = new() { Cedula = "0104567896", Nombres = "Lucía", Apellidos = "Peralta" };
    public Empleado Medico { get; } = new() { Nombres = "Andrés", Apellidos = "Vintimilla", Cargo = Cargo.Medico, Especialidad = "Medicina general" };
    public Empleado Recepcionista { get; } = new() { Nombres = "Rosa", Apellidos = "Cabrera", Cargo = Cargo.Atencion };
    public Servicio Consulta { get; } = new() { Nombre = "Consulta general", Precio = 25m };
    public Servicio Certificado { get; } = new() { Nombre = "Certificado médico", Precio = 10m, GravaIva = true };

    public Escenario()
    {
        Pacientes = new(Db);
        Empleados = new(Db);
        Servicios = new(Db);
        Citas = new(Db);
        Facturas = new(Db);
        Usuarios = new(Db);

        Pacientes.Agregar(Paciente);
        Empleados.Agregar(Medico);
        Empleados.Agregar(Recepcionista);
        Servicios.Agregar(Consulta);
        Servicios.Agregar(Certificado);
    }
}
