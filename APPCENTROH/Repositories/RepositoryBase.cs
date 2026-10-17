using Oracle.ManagedDataAccess.Client;

namespace APPCENTROH.Repositories
{
    public abstract class RepositoryBase
    {
        private readonly string _connectionString;

        public RepositoryBase()
        {
            _connectionString = "Data Source=localhost:1521/XE; User ID=CentroMedico; Password=12345E;";
        }

        protected OracleConnection GetConnection()
        {
            return new OracleConnection(_connectionString);
        }
    }
}
