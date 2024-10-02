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
    public class DaoTransaccion
    {

        //public static List<EntTransaccion> ListaTransaccion(Int64 idPerfil, int idModulo , int IdPais )
        public static List<EntTransaccion> ListaTransaccion(int idModulo)
        {
            List<EntTransaccion> listaTransaccion = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAListaTramsaccion", cnx);
                //cmd.Parameters.AddWithValue("@IdPerfil", idPerfil);
                cmd.Parameters.AddWithValue("@IdModulo", idModulo);
                //cmd.Parameters.AddWithValue("@IdPais", IdPais);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                listaTransaccion = new List<EntTransaccion>();
                while (dr.Read())
                {
                    EntTransaccion transaccion = new EntTransaccion();
                    transaccion.IdTransaccion = Convert.ToInt32(dr["IdTransaccion"].ToString());
                    transaccion.IdModulo = Convert.ToInt32(dr["IdModulo"].ToString());
                    transaccion.NombrePresenta = dr["NombrePresenta"].ToString();
                    transaccion.NombrePantalla = dr["NombrePantalla"].ToString();
                    transaccion.Descripcion = dr["Descripcion"].ToString();
                    transaccion.FecCrea = Convert.ToDateTime(dr["FecCrea"].ToString());
                    transaccion.UserCrea = Convert.ToInt32(dr["UserCrea"].ToString());
                    transaccion.FecModifica = Convert.ToDateTime(dr["FecModifica"].ToString());
                    transaccion.UserModifica = Convert.ToInt32(dr["UserModifica"].ToString());
                    transaccion.Estado = dr["Estado"].ToString();
                    listaTransaccion.Add(transaccion);

                }

            }
            catch (Exception ex)
            {
                listaTransaccion = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return listaTransaccion;
        }

    }


}
