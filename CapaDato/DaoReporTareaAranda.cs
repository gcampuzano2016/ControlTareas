using System.Configuration;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoReporTareaAranda
    {

        /// <summary>
        /// Clase que quenera la linea d conexion con la base de datos
        /// Las cadenas viven en connections.config, fuera del repositorio.
        /// </summary>
        /// <returns>Retorna un String de Conexion</returns>
        /// 
        #region conectar
        public SqlConnection conectar()
        {
            SqlConnection cn = new SqlConnection();
            cn.ConnectionString = ConfigurationManager.ConnectionStrings["ReporTarea"].ConnectionString;
            return cn;
        }
        #endregion

        #region conectarSAP
        public SqlConnection conectarSAP()
        {
            SqlConnection cn = new SqlConnection();
            cn.ConnectionString = ConfigurationManager.ConnectionStrings["Sap"].ConnectionString;
            return cn;
        }
        #endregion


    }
}
