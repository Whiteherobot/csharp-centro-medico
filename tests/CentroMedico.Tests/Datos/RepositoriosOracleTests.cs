using CentroMedico.Core.Modelos;
using CentroMedico.Core.Servicios;
using CentroMedico.Datos.Oracle;

namespace CentroMedico.Tests.Datos;

[Collection("Oracle")]
public class RepositoriosOracleTests
{
    private readonly ConexionOracle _db;
    private readonly PacienteRepositoryOracle _pacientes;
    private readonly EmpleadoRepositoryOracle _empleados;
    private readonly UsuarioRepositoryOracle _usuarios;
    private readonly ServicioRepositoryOracle _servicios;
    private readonly CitaRepositoryOracle _citas;
    private readonly FacturaRepositoryOracle _facturas;

    private static readonly DateTime Turno = new(2026, 10, 19, 9, 0, 0);

    public RepositoriosOracleTests()
    {
        _db = new ConexionOracle(OracleFactAttribute.Cadena ?? "");
        _pacientes = new(_db);
        _empleados = new(_db);
        _usuarios = new(_db);
        _servicios = new(_db);
        _citas = new(_db);
        _facturas = new(_db);
        if (OracleFactAttribute.Cadena is not null) Vaciar();
    }

    private void Vaciar()
    {
        using var cx = _db.Abrir();
        foreach (var tabla in new[]
        {
            "idm_detalles_factura", "idm_citas", "idm_facturas", "idm_usuarios",
            "idm_servicios", "idm_pacientes", "idm_empleados", "idm_personas"
        })
        {
            using var c = ConexionOracle.Comando(cx, $"DELETE FROM {tabla}");
            c.ExecuteNonQuery();
        }
    }

    private static Paciente NuevoPaciente(string cedula = "0104567896") => new()
    {
        Cedula = cedula, Nombres = "Lucía", Apellidos = "Peralta Sigüenza", Direccion = "Av. 12 de Abril",
        Telefono = "0987654321", Email = "lperalta@correo.ec", FechaNacimiento = new DateOnly(1990, 8, 20)
    };

    private static Empleado NuevoMedico(string cedula = "1710034065") => new()
    {
        Cedula = cedula, Nombres = "Andrés", Apellidos = "Vintimilla", Telefono = "0991234567",
        Cargo = Cargo.Medico, Especialidad = "Medicina general"
    };

    private (Paciente, Empleado, Servicio) Basicos()
    {
        var paciente = NuevoPaciente();
        var medico = NuevoMedico();
        var consulta = new Servicio { Nombre = "Consulta general", Precio = 25m };
        _pacientes.Agregar(paciente);
        _empleados.Agregar(medico);
        _servicios.Agregar(consulta);
        return (paciente, medico, consulta);
    }

    [OracleFact]
    public void GuardaYLeeUnPacienteCompleto()
    {
        var paciente = NuevoPaciente();
        _pacientes.Agregar(paciente);

        var leido = _pacientes.ObtenerPorCedula("0104567896")!;

        Assert.Equal(paciente.Id, leido.Id);
        Assert.Equal("Peralta Sigüenza", leido.Apellidos);
        Assert.Equal(new DateOnly(1990, 8, 20), leido.FechaNacimiento);
        Assert.True(leido.Activo);
        Assert.Single(_pacientes.Listar("sigü"));
        Assert.Single(_pacientes.Listar("01045"));
    }

    [OracleFact]
    public void ActualizarCambiaPersonaYEstado()
    {
        var paciente = NuevoPaciente();
        _pacientes.Agregar(paciente);

        paciente.Telefono = "072845123";
        paciente.Email = "";
        paciente.Activo = false;
        _pacientes.Actualizar(paciente);

        var leido = _pacientes.ObtenerPorId(paciente.Id)!;
        Assert.Equal("072845123", leido.Telefono);
        Assert.Equal("", leido.Email);
        Assert.False(leido.Activo);
    }

    [OracleFact]
    public void UnEmpleadoQueSeVuelvePacienteReusaSusDatos()
    {
        var medico = NuevoMedico();
        _empleados.Agregar(medico);

        var comoPaciente = NuevoPaciente(medico.Cedula);
        _pacientes.Agregar(comoPaciente);

        using var cx = _db.Abrir();
        using var c = ConexionOracle.Comando(cx, "SELECT COUNT(*) FROM idm_personas");
        Assert.Equal(1, Convert.ToInt32(c.ExecuteScalar()));
        Assert.Equal("Lucía", _empleados.ObtenerPorId(medico.Id)!.Nombres);
    }

    [OracleFact]
    public void TraduceLasRestriccionesUnicas()
    {
        _pacientes.Agregar(NuevoPaciente());

        var error = Assert.Throws<ReglaNegocioException>(() => _pacientes.Agregar(NuevoPaciente()));
        Assert.Equal("Esa persona ya está registrada como paciente.", error.Message);
    }

    [OracleFact]
    public void SoloListaMedicosActivos()
    {
        var medico = NuevoMedico();
        _empleados.Agregar(medico);
        _empleados.Agregar(new Empleado
        {
            Cedula = "1103456123", Nombres = "Rosa", Apellidos = "Cabrera", Telefono = "0978899001", Cargo = Cargo.Atencion
        });

        Assert.Equal(medico.Id, Assert.Single(_empleados.ListarMedicos()).Id);

        medico.Activo = false;
        _empleados.Actualizar(medico);
        Assert.Empty(_empleados.ListarMedicos());
    }

    [OracleFact]
    public void BuscaUsuariosSinImportarMayusculas()
    {
        var medico = NuevoMedico();
        _empleados.Agregar(medico);
        _usuarios.Agregar(new Usuario { NombreUsuario = "avintimilla", HashContrasena = "x", EmpleadoId = medico.Id });

        Assert.NotNull(_usuarios.ObtenerPorNombre("AVintimilla"));
        Assert.Equal("avintimilla", _usuarios.ObtenerPorEmpleado(medico.Id)!.NombreUsuario);
    }

    [OracleFact]
    public void ElIndiceImpideDosCitasVigentesEnElMismoTurno()
    {
        var (paciente, medico, consulta) = Basicos();
        var cita = new Cita { PacienteId = paciente.Id, MedicoId = medico.Id, ServicioId = consulta.Id, FechaHora = Turno };
        _citas.Agregar(cita);

        var otra = new Cita { PacienteId = paciente.Id, MedicoId = medico.Id, ServicioId = consulta.Id, FechaHora = Turno };
        Assert.Throws<ReglaNegocioException>(() => _citas.Agregar(otra));

        cita.Estado = EstadoCita.Cancelada;
        _citas.Actualizar(cita);
        _citas.Agregar(otra);

        Assert.Equal(2, _citas.ListarPorMedicoYFecha(medico.Id, DateOnly.FromDateTime(Turno)).Count);
        Assert.Empty(_citas.ListarPorFecha(DateOnly.FromDateTime(Turno).AddDays(1)));
    }

    [OracleFact]
    public void LaFacturaGuardaDetallesYMarcaLaCita()
    {
        var (paciente, medico, consulta) = Basicos();
        var cita = new Cita
        {
            PacienteId = paciente.Id, MedicoId = medico.Id, ServicioId = consulta.Id,
            FechaHora = Turno, Estado = EstadoCita.Atendida
        };
        _citas.Agregar(cita);

        var factura = new Factura
        {
            Numero = "001-001-000000001", Fecha = Turno.AddHours(1), PacienteId = paciente.Id,
            SubtotalTarifa0 = 25m, Total = 25m,
            Detalles = [new DetalleFactura
            {
                ServicioId = consulta.Id, Descripcion = consulta.Nombre, Cantidad = 1, PrecioUnitario = 25m, CitaId = cita.Id
            }]
        };
        _facturas.Agregar(factura);

        var leida = _facturas.ObtenerPorId(factura.Id)!;
        Assert.Equal(25m, leida.Total);
        Assert.Equal(cita.Id, Assert.Single(leida.Detalles).CitaId);
        Assert.Equal(factura.Id, _citas.ObtenerPorId(cita.Id)!.FacturaId);
        Assert.Equal(2, _facturas.SiguienteSecuencial());
    }

    [OracleFact]
    public void SiLaCitaYaSeCobroNoQuedaNadaAMedias()
    {
        var (paciente, medico, consulta) = Basicos();
        var cita = new Cita
        {
            PacienteId = paciente.Id, MedicoId = medico.Id, ServicioId = consulta.Id,
            FechaHora = Turno, Estado = EstadoCita.Atendida
        };
        _citas.Agregar(cita);

        Factura Nueva(string numero) => new()
        {
            Numero = numero, Fecha = Turno, PacienteId = paciente.Id, SubtotalTarifa0 = 25m, Total = 25m,
            Detalles = [new DetalleFactura
            {
                ServicioId = consulta.Id, Descripcion = consulta.Nombre, Cantidad = 1, PrecioUnitario = 25m, CitaId = cita.Id
            }]
        };

        _facturas.Agregar(Nueva("001-001-000000001"));
        Assert.Throws<ReglaNegocioException>(() => _facturas.Agregar(Nueva("001-001-000000002")));

        Assert.Single(_facturas.Listar());
    }
}
