using System.Configuration;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoArandaDb
    {

        /// <summary>
        /// Clase que quenera la linea d conexion con la base de datos.
        /// La cadena vive en connections.config, fuera del repositorio.
        /// </summary>
        /// <returns>Retorna un String de Conexion</returns>
        public SqlConnection conectar()
        {
            SqlConnection cn = new SqlConnection();
            cn.ConnectionString = ConfigurationManager.ConnectionStrings["ArandaDb"].ConnectionString;
            return cn;
        }

    }
}
