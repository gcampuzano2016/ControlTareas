using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using CapaEntidad;
using System.Globalization;

namespace CapaDato
{
    public class DaoRegistroTrabajo
    {
        public static List<ERegistroTrabajo> ConsultSp_RTAListaReporteHorario(string FchIni, string FchFin,int Id_Usuario)
        {
            List<ERegistroTrabajo> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaReporteHorario", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@Id_Usuario", Id_Usuario);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<ERegistroTrabajo>();

                while (dr.Read())
                {
                    ERegistroTrabajo Tarea = new ERegistroTrabajo();
                    Tarea.Id_Usuario = Convert.ToInt32(dr["Id_Usuario"].ToString());
                    Tarea.Nom_Usuario = dr["Nom_Usuario"].ToString();
                    Tarea.HoraEntrada = Convert.ToDateTime( dr["HoraEntrada"].ToString());
                    Tarea.HoraAlmuerzoSale = Convert.ToDateTime(dr["HoraAlmuerzoSale"].ToString());
                    Tarea.HoraAlmuerzoEntra = Convert.ToDateTime(dr["HoraAlmuerzoEntra"].ToString());
                    Tarea.HoraSalida = Convert.ToDateTime(dr["HoraSalida"].ToString());
                    listaTareas.Add(Tarea);
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
