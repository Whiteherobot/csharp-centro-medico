using CentroMedico.Core.Modelos;
using CentroMedico.Core.Servicios;

namespace CentroMedico.ViewModels;

/// <summary>
/// Los mismos datos de db/02_datos_ejemplo.sql más algunas citas alrededor de hoy,
/// para que el modo demo tenga agenda y algo que facturar.
/// </summary>
public static class DatosDemo
{
    public const string UsuarioAdmin = "admin";
    public const string ClaveAdmin = "Admin2026";
    public const string UsuarioRecepcion = "rcabrera";
    public const string ClaveRecepcion = "Recepcion2026";

    public static void Cargar(Contexto c)
    {
        var admin = Empleado("0102030400", "Paola", "Andrade Mora", "Av. Huayna Cápac 1-45, Cuenca", "0987001122", "pandrade@centromedico.ec", Cargo.Administracion);
        var general = Empleado("1710034065", "Andrés", "Vintimilla Ríos", "Calle Larga 7-20, Cuenca", "0991234567", "avintimilla@centromedico.ec", Cargo.Medico, "Medicina general");
        var pediatra = Empleado("0301122339", "Ana Lucía", "Medina Coronel", "Av. Solano 3-12, Cuenca", "0984455667", "amedina@centromedico.ec", Cargo.Medico, "Pediatría");
        var recepcion = Empleado("1103456123", "Rosa", "Cabrera Loja", "Av. de las Américas 5-60, Cuenca", "0978899001", "rcabrera@centromedico.ec", Cargo.Atencion);
        foreach (var e in new[] { admin, general, pediatra, recepcion }) c.Empleados.Agregar(e);

        var auth = c.Autenticacion;
        auth.CrearCuenta(admin.Id, UsuarioAdmin, ClaveAdmin, Rol.Administrador);
        auth.CrearCuenta(recepcion.Id, UsuarioRecepcion, ClaveRecepcion, Rol.Recepcion);

        var consulta = Servicio("Consulta de medicina general", 25m);
        var pediatrica = Servicio("Consulta pediátrica", 30m);
        var control = Servicio("Control de presión y glucosa", 8m);
        foreach (var s in new[] { consulta, pediatrica, control, Servicio("Curación con insumos", 12.50m, true), Servicio("Certificado médico", 10m, true) })
            c.Servicios.Agregar(s);

        var lucia = Paciente("0104567896", "Lucía", "Peralta Sigüenza", "Av. 12 de Abril 2-30, Cuenca", "0987654321", "lperalta@correo.ec", new DateOnly(1990, 8, 20));
        var diego = Paciente("0923456784", "Diego", "Ochoa Pesántez", "Remigio Crespo 4-18, Cuenca", "072845123", "", new DateOnly(2015, 3, 2));
        var carmen = Paciente("1309873212", "Carmen", "Zambrano Vélez", "Av. Ordóñez Lasso 8-90, Cuenca", "0962233445", "czambrano@correo.ec", new DateOnly(1958, 11, 5));
        var jorge = Paciente("0607891231", "Jorge", "Guamán Tenesaca", "Sector Ricaurte, Cuenca", "0955566778", "", new DateOnly(1983, 1, 14));
        foreach (var p in new[] { lucia, diego, carmen, jorge }) c.Pacientes.Agregar(p);

        // Una cita atendida el último día hábil, pendiente de facturar.
        var anterior = DiaHabil(c.Hoy.AddDays(-1), -1);
        c.Citas.Agregar(new Cita
        {
            PacienteId = carmen.Id, MedicoId = general.Id, ServicioId = control.Id,
            FechaHora = anterior.ToDateTime(new TimeOnly(9, 0)), Estado = EstadoCita.Atendida
        });

        // Agenda del próximo día con turnos libres; se reserva con el servicio para respetar las reglas.
        var agenda = c.Agenda;
        var proximo = c.Hoy;
        while (agenda.HorariosDisponibles(general.Id, proximo).Count < 6) proximo = proximo.AddDays(1);
        var reservas = new (Paciente Paciente, Empleado Medico, Servicio Servicio)[]
        {
            (lucia, general, consulta), (diego, pediatra, pediatrica), (jorge, general, consulta)
        };
        foreach (var (paciente, medico, servicio) in reservas)
        {
            var libre = agenda.HorariosDisponibles(medico.Id, proximo).Skip(2).FirstOrDefault();
            if (libre != default)
                agenda.Reservar(paciente.Id, medico.Id, servicio.Id, proximo.ToDateTime(libre));
        }
    }

    /// <summary>El primer día desde <paramref name="desde"/> (en la dirección dada) con turnos libres.</summary>
    private static DateOnly DiaHabil(DateOnly desde, int paso)
    {
        var dia = desde;
        while (!AgendaService.Turnos(dia).Any()) dia = dia.AddDays(paso);
        return dia;
    }

    private static Empleado Empleado(string cedula, string nombres, string apellidos, string direccion,
        string telefono, string email, Cargo cargo, string? especialidad = null) => new()
    {
        Cedula = cedula, Nombres = nombres, Apellidos = apellidos, Direccion = direccion,
        Telefono = telefono, Email = email, Cargo = cargo, Especialidad = especialidad
    };

    private static Paciente Paciente(string cedula, string nombres, string apellidos, string direccion,
        string telefono, string email, DateOnly nacimiento) => new()
    {
        Cedula = cedula, Nombres = nombres, Apellidos = apellidos, Direccion = direccion,
        Telefono = telefono, Email = email, FechaNacimiento = nacimiento
    };

    private static Servicio Servicio(string nombre, decimal precio, bool gravaIva = false) =>
        new() { Nombre = nombre, Precio = precio, GravaIva = gravaIva };
}
