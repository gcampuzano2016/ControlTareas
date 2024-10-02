using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace CapaDato
{
    class DaoConexion
    {

        /// <summary>
        /// Clase que quenera la linea d conexion con la base de datos
        /// /* Se debe tener en cuenta en utilizar un metodo de encriptacion para la linea de conexion */
        /// </summary>
        /// <returns>Retorna un String de Conexion</returns>
        public SqlConnection conectar()
        {
            SqlConnection cn = new SqlConnection();
            cn.ConnectionString = "Data Source=; Initial Catalog=; User Id=; Password=";
            //cn.ConnectionString = "Data Source=CARLOS-PC; Initial Catalog=FBillWeb; User Id=BillWeb; Password=BillWeb";
            return cn;
        }

    }
}
