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
    public class DaoADBUsuario
    {

        public static string ADB_verificaUsuario(string IdUsuario)
        {
            EntError objError = new EntError();
            string resultado = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoArandaDb cn = new DaoArandaDb();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_ADBVerificaUsuario", cnx);
                cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                dr.Read();
                resultado = dr["Respuesta"].ToString();
            }
            catch (Exception ex)
            {
                resultado = "";
                objError.CodError = ex;
                objError.MenssageError = ex.Message;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return resultado;
        }


        public static EntUsuario ADB_ConsultaUsuarioADB(string CodId)
        {

            EntUsuario objUsuario = null;// new entUsuario();
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoArandaDb cn = new DaoArandaDb();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_ADBConsultaUsuarioCrea", cnx);
                cmd.Parameters.AddWithValue("@CodId", CodId);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                objUsuario = new EntUsuario();
                dr.Read();

                //objUsuario.Id_Usuario = Convert.ToInt32(dr["Id_Usuario"].ToString());
                objUsuario.Cod_Usuario = dr["Cod_Usuario"].ToString();
                objUsuario.Nom_Usuario = dr["Nom_Usuario"].ToString();
                objUsuario.Log_Usuario = dr["Log_Usuario"].ToString();
                //objUsuario.Pass_Usuario = dr["Pass_Usuario"].ToString();
                objUsuario.Cod_Perfil = Convert.ToInt32(dr["Cod_Perfil"].ToString());
                objUsuario.E_Mail = dr["E_Mail"].ToString();
                objUsuario.Rol_Usuario = Convert.ToInt32(dr["Rol_Usuario"].ToString());
                objUsuario.Cod_Jefe_Inm = dr["Cod_Jefe_Inm"].ToString();
                objUsuario.MailCodJefeInm = dr["MailCodJefeInm"].ToString();
                //objUsuario.Fec_Creacion = dr["Fec_Creacion"].ToString();
                //objUsuario.Usuario_Estado = dr["Usuario_Estado"].ToString();

                //resultado = dr["Respuesta"].ToString();
            }
            catch (Exception ex)
            {
                objUsuario = null;

            }
            finally
            {
                cmd.Connection.Close();
            }
            return objUsuario;
        }


        public static List<EntUsuario>  RTA_ConsultaLike(int tipo,string Descripcion)
        {
            List<EntUsuario> objUsuario = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("RTA_ConsultaLike", cnx);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.Parameters.AddWithValue("@Descripcion", Descripcion);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                objUsuario = new List<EntUsuario>();
                while (dr.Read())
                {
                    EntUsuario Tarea = new EntUsuario();
                    Tarea.ID = dr["ID"].ToString();
                    Tarea.NOMBRE = dr["NOMBRE"].ToString();
                    objUsuario.Add(Tarea);
                }
            }
            catch (Exception ex)
            {
                objUsuario = null;

            }
            finally
            {
                cmd.Connection.Close();
            }
            return objUsuario;
        }



    }
}
