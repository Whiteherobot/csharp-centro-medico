using CentroMedico.Core.Modelos;
using CentroMedico.Core.Repositorios;

namespace CentroMedico.Datos.Memoria;

internal static class Filtros
{
    public static bool Coincide(Persona p, string? filtro) =>
        string.IsNullOrWhiteSpace(filtro)
        || p.Cedula.Contains(filtro.Trim())
        || p.NombreCompleto.Contains(filtro.Trim(), StringComparison.CurrentCultureIgnoreCase);

    public static void Reemplazar<T>(List<T> lista, Func<T, bool> mismo, T nuevo, string entidad)
    {
        int i = lista.FindIndex(x => mismo(x));
        if (i < 0) throw new KeyNotFoundException($"No existe el {entidad}.");
        lista[i] = nuevo;
    }
}

public class PacienteRepositoryMemoria(AlmacenMemoria db) : IPacienteRepository
{
    public IReadOnlyList<Paciente> Listar(string? filtro = null)
    {
        lock (db.Candado)
            return db.Pacientes.Where(p => Filtros.Coincide(p, filtro))
                .OrderBy(p => p.Apellidos).ThenBy(p => p.Nombres)
                .Select(p => p.Copiar()).ToList();
    }

    public Paciente? ObtenerPorId(int id)
    {
        lock (db.Candado) return db.Pacientes.FirstOrDefault(p => p.Id == id)?.Copiar();
    }

    public Paciente? ObtenerPorCedula(string cedula)
    {
        lock (db.Candado) return db.Pacientes.FirstOrDefault(p => p.Cedula == cedula)?.Copiar();
    }

    public void Agregar(Paciente paciente)
    {
        lock (db.Candado)
        {
            paciente.Id = db.NuevoId();
            db.Pacientes.Add(paciente.Copiar());
        }
    }

    public void Actualizar(Paciente paciente)
    {
        lock (db.Candado)
            Filtros.Reemplazar(db.Pacientes, p => p.Id == paciente.Id, paciente.Copiar(), "paciente");
    }
}

public class EmpleadoRepositoryMemoria(AlmacenMemoria db) : IEmpleadoRepository
{
    public IReadOnlyList<Empleado> Listar(string? filtro = null)
    {
        lock (db.Candado)
            return db.Empleados.Where(e => Filtros.Coincide(e, filtro))
                .OrderBy(e => e.Apellidos).ThenBy(e => e.Nombres)
                .Select(e => e.Copiar()).ToList();
    }

    public IReadOnlyList<Empleado> ListarMedicos()
    {
        lock (db.Candado)
            return db.Empleados.Where(e => e.EsMedico && e.Activo)
                .OrderBy(e => e.Apellidos).Select(e => e.Copiar()).ToList();
    }

    public Empleado? ObtenerPorId(int id)
    {
        lock (db.Candado) return db.Empleados.FirstOrDefault(e => e.Id == id)?.Copiar();
    }

    public Empleado? ObtenerPorCedula(string cedula)
    {
        lock (db.Candado) return db.Empleados.FirstOrDefault(e => e.Cedula == cedula)?.Copiar();
    }

    public void Agregar(Empleado empleado)
    {
        lock (db.Candado)
        {
            empleado.Id = db.NuevoId();
            db.Empleados.Add(empleado.Copiar());
        }
    }

    public void Actualizar(Empleado empleado)
    {
        lock (db.Candado)
            Filtros.Reemplazar(db.Empleados, e => e.Id == empleado.Id, empleado.Copiar(), "empleado");
    }
}

public class UsuarioRepositoryMemoria(AlmacenMemoria db) : IUsuarioRepository
{
    public Usuario? ObtenerPorNombre(string nombreUsuario)
    {
        lock (db.Candado)
            return db.Usuarios.FirstOrDefault(u =>
                string.Equals(u.NombreUsuario, nombreUsuario, StringComparison.OrdinalIgnoreCase))?.Copiar();
    }

    public Usuario? ObtenerPorEmpleado(int empleadoId)
    {
        lock (db.Candado) return db.Usuarios.FirstOrDefault(u => u.EmpleadoId == empleadoId)?.Copiar();
    }

    public void Agregar(Usuario usuario)
    {
        lock (db.Candado)
        {
            usuario.Id = db.NuevoId();
            db.Usuarios.Add(usuario.Copiar());
        }
    }

    public void Actualizar(Usuario usuario)
    {
        lock (db.Candado)
            Filtros.Reemplazar(db.Usuarios, u => u.Id == usuario.Id, usuario.Copiar(), "usuario");
    }
}

public class ServicioRepositoryMemoria(AlmacenMemoria db) : IServicioRepository
{
    public IReadOnlyList<Servicio> Listar(bool soloActivos = false)
    {
        lock (db.Candado)
            return db.Servicios.Where(s => !soloActivos || s.Activo)
                .OrderBy(s => s.Nombre).Select(s => s.Copiar()).ToList();
    }

    public Servicio? ObtenerPorId(int id)
    {
        lock (db.Candado) return db.Servicios.FirstOrDefault(s => s.Id == id)?.Copiar();
    }

    public void Agregar(Servicio servicio)
    {
        lock (db.Candado)
        {
            servicio.Id = db.NuevoId();
            db.Servicios.Add(servicio.Copiar());
        }
    }

    public void Actualizar(Servicio servicio)
    {
        lock (db.Candado)
            Filtros.Reemplazar(db.Servicios, s => s.Id == servicio.Id, servicio.Copiar(), "servicio");
    }
}

public class CitaRepositoryMemoria(AlmacenMemoria db) : ICitaRepository
{
    public IReadOnlyList<Cita> ListarPorMedicoYFecha(int medicoId, DateOnly fecha) =>
        Buscar(c => c.MedicoId == medicoId && DateOnly.FromDateTime(c.FechaHora) == fecha);

    public IReadOnlyList<Cita> ListarPorPaciente(int pacienteId) =>
        Buscar(c => c.PacienteId == pacienteId);

    public IReadOnlyList<Cita> ListarPorFecha(DateOnly fecha) =>
        Buscar(c => DateOnly.FromDateTime(c.FechaHora) == fecha);

    public Cita? ObtenerPorId(int id)
    {
        lock (db.Candado) return db.Citas.FirstOrDefault(c => c.Id == id)?.Copiar();
    }

    public void Agregar(Cita cita)
    {
        lock (db.Candado)
        {
            cita.Id = db.NuevoId();
            db.Citas.Add(cita.Copiar());
        }
    }

    public void Actualizar(Cita cita)
    {
        lock (db.Candado)
            Filtros.Reemplazar(db.Citas, c => c.Id == cita.Id, cita.Copiar(), "cita");
    }

    private List<Cita> Buscar(Func<Cita, bool> condicion)
    {
        lock (db.Candado)
            return db.Citas.Where(condicion).OrderBy(c => c.FechaHora).Select(c => c.Copiar()).ToList();
    }
}

public class FacturaRepositoryMemoria(AlmacenMemoria db) : IFacturaRepository
{
    public IReadOnlyList<Factura> Listar()
    {
        lock (db.Candado)
            return db.Facturas.OrderByDescending(f => f.Fecha).Select(f => f.Copiar()).ToList();
    }

    public Factura? ObtenerPorId(int id)
    {
        lock (db.Candado) return db.Facturas.FirstOrDefault(f => f.Id == id)?.Copiar();
    }

    public int SiguienteSecuencial()
    {
        lock (db.Candado) return db.Facturas.Count + 1;
    }

    public void Agregar(Factura factura)
    {
        lock (db.Candado)
        {
            var citas = factura.Detalles.Where(d => d.CitaId is not null)
                .Select(d => db.Citas.FirstOrDefault(c => c.Id == d.CitaId)
                    ?? throw new KeyNotFoundException($"No existe la cita {d.CitaId}."))
                .ToList();
            if (citas.Any(c => c.FacturaId is not null))
                throw new InvalidOperationException("Una de las citas ya fue facturada.");

            factura.Id = db.NuevoId();
            db.Facturas.Add(factura.Copiar());
            foreach (var cita in citas)
                cita.FacturaId = factura.Id;
        }
    }
}
