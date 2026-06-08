using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDato
{
    public class DaoCodigoCIE
    {
        public static List<EntCodigoCIE> Consulta_Sp_RTAConsultarCodigoCIE(string descCodigo)
        {
            List<EntCodigoCIE> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAConsultarCodigoCIE", cnx);
                cmd.Parameters.AddWithValue("@DescCodigo", descCodigo);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntCodigoCIE>();
                while (dr.Read())
                {
                    EntCodigoCIE objCodigo = new EntCodigoCIE();
                    objCodigo.IdCod = dr["IdCod"].ToString();
                    objCodigo.DescCategoria = dr["DescCategoria"].ToString();
                    objCodigo.Codigo = dr["Codigo"].ToString();
                    objCodigo.DescCodigo = dr["DescCodigo"].ToString();
                    listaTareas.Add(objCodigo);
                }

            }
            catch (Exception ex)
            {
                listaTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return listaTareas;
        }
    }
}
