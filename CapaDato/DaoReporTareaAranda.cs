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
    public class DaoReporTareaAranda
    {

        /// <summary>
        /// Clase que quenera la linea d conexion con la base de datos
        /// /* Se debe tener en cuenta en utilizar un metodo de encriptacion para la linea de conexion */
        /// </summary>
        /// <returns>Retorna un String de Conexion</returns>
        /// 
        #region conectar
        public SqlConnection conectar()
        {
            SqlConnection cn = new SqlConnection();

            // DESARROLLO
            //cn.ConnectionString = "Data Source=GCAMPUZANO;Initial Catalog=ReporTarea; User Id=sa; Password=CAfKsUBnD0s";

            //PRODUCCION
            cn.ConnectionString = "Data Source=192.168.11.14; Initial Catalog=ReporTarea; User Id=sa; Password=CAfKsUBnD0s";

            //DYNATRACE
            //cn.ConnectionString = "Data Source=VMmarietta2\\SQLEXPRESS; Initial Catalog=ReporTarea; User Id=sa; Password=CAfKsUBnD0s";

            //PREPRODUCCION
            //cn.ConnectionString = "Data Source=192.168.11.14; Initial Catalog=ReporTareaPreProd; User Id=sa; Password=CAfKsUBnD0s";
            //cn.ConnectionString = "Data Source=SRV-CBS-ERP\\SQLEXPRESS; Initial Catalog=ReporTarea; User Id=sa; Password=CAfKsUBnD0s";
            // TEST
            //cn.ConnectionString = "Data Source=192.168.11.14; Initial Catalog=ReporTareaTest; User Id=sa; Password=CAfKsUBnD0s";
            //cn.ConnectionString = "Data Source=GUILLERMO; Initial Catalog=ReporTarea; User Id=sa; Password=Sql$erver2014";
            //cn.ConnectionString = "Data Source=CARLOS-PC; Initial Catalog=FBillWeb; User Id=BillWeb; Password=BillWeb";
            return cn;
        }
        #endregion

        #region conectarSAP
        public SqlConnection conectarSAP()
        {
            SqlConnection cn = new SqlConnection();

            // DESARROLLO
            //cn.ConnectionString = "Data Source=GUILLERMO; Initial Catalog=AD; User Id=sa; Password=Sql$erver2014";

            // PRODUCCION
            cn.ConnectionString = "Data Source=192.168.11.8//DCP; Initial Catalog=DCP; User Id=sap; Password=Sql$erver2014";

            return cn;
        }
        #endregion


    }
}
