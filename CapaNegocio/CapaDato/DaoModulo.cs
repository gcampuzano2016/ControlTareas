using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using CapaEntidad;

namespace CapaDato
{
    public class DaoModulo
    {


        public static List<EntModulo> Modulo()
        {
            List<EntModulo> listaModulo = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAListaModulo", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                listaModulo = new List<EntModulo>();
                while (dr.Read())
                {
                    EntModulo Modu = new EntModulo();
                    Modu.IdModulo = Convert.ToInt32(dr["IdModulo"].ToString());
                    Modu.Nombre = dr["Nombre"].ToString();
                    Modu.Descripcion = dr["Descripcion"].ToString();
                    Modu.FecCrea = Convert.ToDateTime(dr["FecCrea"].ToString());
                    Modu.UserCrea = Convert.ToInt32(dr["UserCrea"].ToString());
                    Modu.FecModifica = Convert.ToDateTime(dr["FecModifica"].ToString());
                    Modu.UserModifica = Convert.ToInt32(dr["UserModifica"].ToString());
                    Modu.Grafica = dr["Grafica"].ToString();
                    Modu.Estado = dr["Estado"].ToString();
                    listaModulo.Add(Modu);
                }

            }
            catch (Exception ex)
            {
                listaModulo = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return listaModulo;
        }


    }
}
