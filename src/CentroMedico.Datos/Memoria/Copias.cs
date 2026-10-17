using CentroMedico.Core.Modelos;

namespace CentroMedico.Datos.Memoria;

internal static class Copias
{
    public static Paciente Copiar(this Paciente p) => CopiarPersona(p, new Paciente
    {
        FechaNacimiento = p.FechaNacimiento
    });

    public static Empleado Copiar(this Empleado e) => CopiarPersona(e, new Empleado
    {
        Cargo = e.Cargo, Especialidad = e.Especialidad
    });

    private static T CopiarPersona<T>(Persona origen, T destino) where T : Persona
    {
        destino.Id = origen.Id;
        destino.Cedula = origen.Cedula;
        destino.Nombres = origen.Nombres;
        destino.Apellidos = origen.Apellidos;
        destino.Direccion = origen.Direccion;
        destino.Telefono = origen.Telefono;
        destino.Email = origen.Email;
        destino.Activo = origen.Activo;
        return destino;
    }

    public static Usuario Copiar(this Usuario u) => new()
    {
        Id = u.Id, NombreUsuario = u.NombreUsuario, HashContrasena = u.HashContrasena,
        Rol = u.Rol, Activo = u.Activo, EmpleadoId = u.EmpleadoId
    };

    public static Servicio Copiar(this Servicio s) => new()
    {
        Id = s.Id, Nombre = s.Nombre, Precio = s.Precio, GravaIva = s.GravaIva,
        DuracionMinutos = s.DuracionMinutos, Activo = s.Activo
    };

    public static Cita Copiar(this Cita c) => new()
    {
        Id = c.Id, PacienteId = c.PacienteId, MedicoId = c.MedicoId, ServicioId = c.ServicioId,
        FechaHora = c.FechaHora, Estado = c.Estado, FacturaId = c.FacturaId
    };

    public static Factura Copiar(this Factura f) => new()
    {
        Id = f.Id, Numero = f.Numero, Fecha = f.Fecha, PacienteId = f.PacienteId,
        SubtotalTarifa0 = f.SubtotalTarifa0, SubtotalGravado = f.SubtotalGravado,
        Iva = f.Iva, Total = f.Total,
        Detalles = f.Detalles.Select(d => new DetalleFactura
        {
            ServicioId = d.ServicioId, Descripcion = d.Descripcion, Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario, GravaIva = d.GravaIva, CitaId = d.CitaId
        }).ToList()
    };
}
