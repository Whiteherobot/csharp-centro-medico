using CentroMedico.App.Models;
using CentroMedico.App.ViewModels;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Data;
using System.Net;
using System.Windows;

namespace CentroMedico.App.Repositories
{
    public class UserRepository : RepositoryBase, IUserRepository
    {

        public void Add(UserModel userModel)
        {
            throw new NotImplementedException();
        }

        public bool AuthenticateUser(NetworkCredential credential)
        {
            bool validUser;
            using (var connection = GetConnection())
            using (var command = new OracleCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "SELECT * FROM IDM_USUARIOS WHERE USU_USUARIO = :username AND USU_CONTRASENA = :password";   // Asegúrate de usar comillas dobles para los nombres de tabla y columna
                command.Parameters.Add(":username", OracleDbType.NVarchar2).Value = credential.UserName;
                command.Parameters.Add(":password", OracleDbType.NVarchar2).Value = credential.Password;
                validUser = command.ExecuteScalar() == null ? false : true;
            }
            return validUser;
        }
        public UserModel GetByUsername(string username)
        {
            UserModel user = null;
            using (var connection = GetConnection())
            using (var command = new OracleCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = @"
                                        SELECT
                                            U.IDM_EMPLEADOS_EMP_ID,
                                            P.per_cedula,
                                            P.per_nombre AS Nombres,
                                            P.per_apellido AS Apellidos
                                        FROM
                                            idm_usuarios U
                                        LEFT JOIN
                                            idm_empleados E ON U.IDM_EMPLEADOS_EMP_ID = E.emp_id
                                        LEFT JOIN
                                            idm_personas P ON E.IDM_PERSONAS_PER_ID = P.per_id
                                        WHERE
                                            U.USU_USUARIO = :username";

                command.Parameters.Add(":username", OracleDbType.NVarchar2).Value = username;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new UserModel()
                        {
                            Id = reader["IDM_EMPLEADOS_EMP_ID"].ToString(),
                            Username = username,
                            Password = string.Empty,
                            Name = reader["Nombres"].ToString(),
                            LastName = reader["Apellidos"].ToString(),
                        };
                    }
                }
            }
            return user;
        }

        public void InsertData(RegisterViewModel registerViewModel, string username, string password)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Inserción en idm_personas
                        string queryInsertPersona = "INSERT INTO idm_personas (per_id, per_cedula, per_nombre, per_apellido, per_direccion, per_telefono, per_email, per_activo) " +
                                                    "VALUES (per_id.NEXTVAL, :cedula, :nombre, :apellido, :direccion, :telefono, :email, '1') " +
                                                    "RETURNING per_id INTO :v_per_id";

                        using (var command = new OracleCommand(queryInsertPersona, connection))
                        {
                            command.Parameters.Add(":cedula", OracleDbType.Varchar2).Value = registerViewModel.cedula;
                            command.Parameters.Add(":nombre", OracleDbType.Varchar2).Value = registerViewModel.nombre;
                            command.Parameters.Add(":apellido", OracleDbType.Varchar2).Value = registerViewModel.apellido;
                            command.Parameters.Add(":direccion", OracleDbType.Varchar2).Value = registerViewModel.direccion;
                            command.Parameters.Add(":telefono", OracleDbType.Varchar2).Value = registerViewModel.telefono;
                            command.Parameters.Add(":email", OracleDbType.Varchar2).Value = registerViewModel.Email;
                            command.Parameters.Add(":v_per_id", OracleDbType.Decimal).Direction = ParameterDirection.Output;

                            command.ExecuteNonQuery();
                            decimal v_per_id = ((OracleDecimal)command.Parameters[":v_per_id"].Value).Value;

                            // Inserción en idm_empleados
                            string queryInsertEmpleado = "INSERT INTO idm_empleados (emp_id, emp_tipo, IDM_PERSONAS_PER_ID) " +
                                                         "VALUES (emp_id.NEXTVAL, 'Atencion', :v_per_id) " +
                                                         "RETURNING emp_id INTO :v_emp_id";

                            command.CommandText = queryInsertEmpleado;
                            command.Parameters.Clear();
                            command.Parameters.Add(":v_per_id", OracleDbType.Decimal).Value = v_per_id;
                            command.Parameters.Add(":v_emp_id", OracleDbType.Decimal).Direction = ParameterDirection.Output;

                            command.ExecuteNonQuery();
                            decimal v_emp_id = ((OracleDecimal)command.Parameters[":v_emp_id"].Value).Value;

                            // Inserción en idm_usuarios
                            string queryInsertUsuario = "INSERT INTO idm_usuarios (usu_id, usu_usuario, usu_contrasena, usu_permiso, usu_activo, IDM_EMPLEADOS_EMP_ID) " +
                                                        "VALUES (usu_id.NEXTVAL, :usuario, :contrasena, 'Administrador', '1', :v_emp_id)";

                            command.CommandText = queryInsertUsuario;
                            command.Parameters.Clear();
                            command.Parameters.Add(":usuario", OracleDbType.Varchar2).Value = username;
                            command.Parameters.Add(":contrasena", OracleDbType.Varchar2).Value = password;
                            command.Parameters.Add(":v_emp_id", OracleDbType.Decimal).Value = v_emp_id;

                            command.ExecuteNonQuery();

                            // Commit de la transacción
                            transaction.Commit();
                            MessageBox.Show("Registro exitoso");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error al registrar datos: " + ex.Message);
                    }
                }
            }
        }
    }
}
