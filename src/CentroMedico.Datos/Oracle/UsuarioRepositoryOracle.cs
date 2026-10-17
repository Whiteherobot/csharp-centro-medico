using System.Data;
using CentroMedico.Core.Modelos;
using CentroMedico.Core.Repositorios;

namespace CentroMedico.Datos.Oracle;

public class UsuarioRepositoryOracle(ConexionOracle db) : IUsuarioRepository
{
    private const string Select = """
        SELECT usu_id, usu_usuario, usu_contrasena, usu_permiso, usu_activo, idm_empleados_emp_id
          FROM idm_usuarios
        """;

    public Usuario? ObtenerPorNombre(string nombreUsuario) =>
        Uno("usu_usuario = LOWER(:valor)", nombreUsuario);

    public Usuario? ObtenerPorEmpleado(int empleadoId) =>
        Uno("idm_empleados_emp_id = :valor", empleadoId);

    public void Agregar(Usuario u)
    {
        using var cx = db.Abrir();
        ErroresOracle.Traducir(() =>
        {
            using var c = ConexionOracle.Comando(cx, """
                INSERT INTO idm_usuarios (usu_usuario, usu_contrasena, usu_permiso, usu_activo, idm_empleados_emp_id)
                VALUES (:usuario, :hash, :permiso, :activo, :empleado)
                RETURNING usu_id INTO :id
                """)
                .Con("usuario", u.NombreUsuario).Con("hash", u.HashContrasena).Con("permiso", u.Rol)
                .Con("activo", u.Activo).Con("empleado", u.EmpleadoId);
            u.Id = c.InsertarRetornandoId();
        });
    }

    public void Actualizar(Usuario u)
    {
        using var cx = db.Abrir();
        using var c = ConexionOracle.Comando(cx, """
            UPDATE idm_usuarios
               SET usu_contrasena = :hash, usu_permiso = :permiso, usu_activo = :activo
             WHERE usu_id = :id
            """).Con("hash", u.HashContrasena).Con("permiso", u.Rol).Con("activo", u.Activo).Con("id", u.Id);
        if (c.ExecuteNonQuery() == 0) throw new KeyNotFoundException("No existe el usuario.");
    }

    private Usuario? Uno(string condicion, object valor)
    {
        using var cx = db.Abrir();
        using var c = ConexionOracle.Comando(cx, $"{Select} WHERE {condicion}").Con("valor", valor);
        return c.Leer(Mapear).FirstOrDefault();
    }

    private static Usuario Mapear(IDataRecord r) => new()
    {
        Id = r.Entero("usu_id"),
        NombreUsuario = r.Texto("usu_usuario"),
        HashContrasena = r.Texto("usu_contrasena"),
        Rol = r.Enum<Rol>("usu_permiso"),
        Activo = r.Booleano("usu_activo"),
        EmpleadoId = r.Entero("idm_empleados_emp_id")
    };
}
