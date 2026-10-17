using CentroMedico.Core.Modelos;
using CentroMedico.Datos.Memoria;

namespace CentroMedico.Tests.Datos;

public class RepositoriosMemoriaTests
{
    private readonly AlmacenMemoria _db = new();

    [Fact]
    public void ModificarElObjetoLeidoNoCambiaElDatoGuardado()
    {
        var repo = new PacienteRepositoryMemoria(_db);
        repo.Agregar(new Paciente { Cedula = "0104567896", Nombres = "Lucía", Apellidos = "Peralta" });

        var leido = repo.ObtenerPorCedula("0104567896")!;
        leido.Nombres = "Otro nombre";

        Assert.Equal("Lucía", repo.ObtenerPorId(leido.Id)!.Nombres);

        repo.Actualizar(leido);
        Assert.Equal("Otro nombre", repo.ObtenerPorId(leido.Id)!.Nombres);
    }

    [Fact]
    public void FiltraPorCedulaONombreSinImportarMayusculas()
    {
        var repo = new PacienteRepositoryMemoria(_db);
        repo.Agregar(new Paciente { Cedula = "0104567896", Nombres = "Lucía", Apellidos = "Peralta" });
        repo.Agregar(new Paciente { Cedula = "1710034065", Nombres = "Diego", Apellidos = "Ochoa" });

        Assert.Single(repo.Listar("peral"));
        Assert.Single(repo.Listar("17100"));
        Assert.Equal(2, repo.Listar().Count);
    }

    [Fact]
    public void SoloListaMedicosActivos()
    {
        var repo = new EmpleadoRepositoryMemoria(_db);
        repo.Agregar(new Empleado { Nombres = "Ana", Apellidos = "Medina", Cargo = Cargo.Medico, Especialidad = "Pediatría" });
        repo.Agregar(new Empleado { Nombres = "Luis", Apellidos = "Zhunio", Cargo = Cargo.Medico, Especialidad = "Cardiología", Activo = false });
        repo.Agregar(new Empleado { Nombres = "Rosa", Apellidos = "Cabrera", Cargo = Cargo.Atencion });

        var medicos = repo.ListarMedicos();

        Assert.Equal("Ana", Assert.Single(medicos).Nombres);
    }

    [Fact]
    public void AlGuardarFacturaMarcaSusCitasComoFacturadas()
    {
        var citas = new CitaRepositoryMemoria(_db);
        var facturas = new FacturaRepositoryMemoria(_db);
        var cita = new Cita { PacienteId = 1, MedicoId = 2, ServicioId = 3, Estado = EstadoCita.Atendida };
        citas.Agregar(cita);

        facturas.Agregar(new Factura { Detalles = [new DetalleFactura { CitaId = cita.Id, Cantidad = 1 }] });

        Assert.NotNull(citas.ObtenerPorId(cita.Id)!.FacturaId);
        var otra = new Factura { Detalles = [new DetalleFactura { CitaId = cita.Id, Cantidad = 1 }] };
        Assert.Throws<InvalidOperationException>(() => facturas.Agregar(otra));
        Assert.Single(facturas.Listar());
    }
}
