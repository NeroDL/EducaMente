using System.Data.SqlClient;

namespace EducaMente.AccessData
{
    public class AccesoData
    {
        private readonly string cadConeSQL;

        public AccesoData(string conexionSql)
        {
            cadConeSQL = conexionSql;
        }

        //Devuelve una conexión abierta
        public SqlConnection GetConnection()
        {
            var connection = new SqlConnection(cadConeSQL);
            connection.Open();
            return connection;
        }

        //devuelve la conexión SIN abrirla (ideal para scoped, controlado manualmente)
        public SqlConnection GetUnopenedConnection()
        {
            return new SqlConnection(cadConeSQL);
        }
    }
}
