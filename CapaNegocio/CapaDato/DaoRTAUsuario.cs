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
    public class DaoRTAUsuario
    {

        public static string RTA_verificaUsuario(string IdUsuario)
        {
            EntError objError = new EntError();
            string resultado = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAVerificaUsuario", cnx);
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
        public static int RTA_IngresaUsuario(EntUsuario objUsuario)
        {
            int Respuesta = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAInsertaUsuario", cnx);

                //cmd.Parameters.AddWithValue("Id_Usuario", objUsuario.Id_Usuario);
                cmd.Parameters.AddWithValue("@Cod_Usuario", objUsuario.Cod_Usuario);
                cmd.Parameters.AddWithValue("@Nom_Usuario", objUsuario.Nom_Usuario);
                cmd.Parameters.AddWithValue("@Log_Usuario", objUsuario.Log_Usuario);
                cmd.Parameters.AddWithValue("@Pass_Usuario", objUsuario.Pass_Usuario);
                cmd.Parameters.AddWithValue("@Cod_Perfil", objUsuario.Cod_Perfil);
                cmd.Parameters.AddWithValue("@E_Mail", objUsuario.E_Mail);
                cmd.Parameters.AddWithValue("@Rol_Usuario", objUsuario.Rol_Usuario);
                cmd.Parameters.AddWithValue("@Fec_Creacion", objUsuario.Fec_Creacion);
                cmd.Parameters.AddWithValue("@Usuario_Estado", objUsuario.Usuario_Estado);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                dr.Read();
                Respuesta = Convert.ToInt16(dr["Respuestas"].ToString());
            }
            catch (Exception ex)
            {
                Respuesta = 0;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return Respuesta;

        }
        public static int RTA_InsertaCodigoSeguridad(EntUsuario objUsuario)
        {
            int Respuesta = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAInsertaCodigoSeguridad", cnx);
                cmd.Parameters.AddWithValue("@CodigoReset", objUsuario.CodigoReset);
                cmd.Parameters.AddWithValue("@Log_Usuario", objUsuario.Log_Usuario);
                
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                dr.Read();
                Respuesta = Convert.ToInt16(dr["Respuestas"].ToString());
            }
            catch (Exception ex)
            {
                Respuesta = 0;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return Respuesta;

        }
        public static EntUsuario RTA_ConsultarCodigoSeguridad(string IdUsuario)
        {

            EntUsuario objUsuario = null;// new entUsuario();
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAConsultarCodigoSeguridad", cnx);
                cmd.Parameters.AddWithValue("@Log_Usuario", IdUsuario);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                objUsuario = new EntUsuario();
                dr.Read();
                objUsuario.CodigoReset = dr["CodigoReset"].ToString();
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
        public static string RTA_AutenticaUsuario(string IdUsuario, string Passw)
        {
            EntError objError = new EntError();
            string resultado = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAAutenticaUsuario", cnx);
                cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);
                cmd.Parameters.AddWithValue("@Passw", Passw);
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
        public static EntUsuario RTA_ConsultaUsuarioRTA(string CodId)
        {

            EntUsuario objUsuario = null;// new entUsuario();
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAConsultaUsuario", cnx);
                cmd.Parameters.AddWithValue("@CodId", CodId);
                cmd.CommandType = CommandType.StoredProcedure;
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
                objUsuario.Id_Perfil = Convert.ToInt32(dr["Id_Perfil"].ToString());
                objUsuario.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
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
        public static EntUsuario RTAConsultaUsuarioPorCodigo(string IdUsuario)
        {

            EntUsuario objUsuario = null;// new entUsuario();
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAConsultaUsuarioPorCodigo", cnx);
                cmd.Parameters.AddWithValue("@Cod_Usuario", IdUsuario);
                cmd.CommandType = CommandType.StoredProcedure;
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
        public static bool RTA_ConsultaUsuarioEsJefe(string IdUsuario)
        {

            bool usuarioEsJefe = false;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            string resultado = "";

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("RTA_ConsultaUsuarioEsJefe", cnx);
                cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                resultado = dr["usuarioEsJefe"].ToString();
                if(resultado == "true")
                {
                    usuarioEsJefe = true;
                }
                else
                {
                    usuarioEsJefe = false;
                }

            }
            catch (Exception ex)
            {
                usuarioEsJefe = false;

            }
            finally
            {
                cmd.Connection.Close();
            }
            return usuarioEsJefe;
        }
        public static string RTA_CorreoJefeInmediato(string IdUsuario)
        {

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            string resultado = "";

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTACorreoJefeInmediato", cnx);
                cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                resultado = dr["Respuesta"].ToString();

            }
            catch (Exception ex)
            {
                resultado = "";

            }
            finally
            {
                cmd.Connection.Close();
            }
            return resultado;
        }        
        public static string RTA_CorreoUsuario(string IdUsuario)
        {

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            string resultado = "";

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTACorreoUsuario", cnx);
                cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                resultado = dr["Respuesta"].ToString();

            }
            catch (Exception ex)
            {
                resultado = "";

            }
            finally
            {
                cmd.Connection.Close();
            }
            return resultado;
        }
        public static List<EntCombo> ListaUsuariosCombo(string Idusuario)
        {
            List<EntCombo> cmbUsuarios = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAConsultaUsuarioPorJefe", cnx);
                cmd.Parameters.AddWithValue("@Cod_Jefe_Inm", Idusuario);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                cmbUsuarios = new List<EntCombo>();
                while (dr.Read())
                {
                    EntCombo usuario = new EntCombo();

                    usuario.Id = dr["Cod_Usuario"].ToString();
                    usuario.Valor = dr["Nom_Usuario"].ToString();

                    cmbUsuarios.Add(usuario);
                }
                
            }
            catch (Exception ex)
            {
                cmbUsuarios = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return cmbUsuarios;
        }
        public static List<EntCombo> ConsultarDatosEmpleado(string Cod_Usuario,int Tipo)
        {
            List<EntCombo> cmbUsuarios = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAConsultaDatosEmpleados", cnx);
                cmd.Parameters.AddWithValue("@Cod_Usuario", Cod_Usuario);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                cmbUsuarios = new List<EntCombo>();
                while (dr.Read())
                {
                    EntCombo usuario = new EntCombo();

                    usuario.Id = dr["Cod_Usuario"].ToString();
                    usuario.Valor = dr["Nom_Usuario"].ToString();

                    cmbUsuarios.Add(usuario);
                }

            }
            catch (Exception ex)
            {
                cmbUsuarios = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return cmbUsuarios;
        }
        public static List<EntUsuario> ConsultarDatosReemplazo(string Cod_Usuario, int Tipo)
        {
            List<EntUsuario> cmbUsuarios = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("Sp_RTAConsultaDatosEmpleados", cnx);
                cmd.Parameters.AddWithValue("@Cod_Usuario", Cod_Usuario);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                cmbUsuarios = new List<EntUsuario>();
                while (dr.Read())
                {
                    EntUsuario usuario = new EntUsuario();

                    usuario.Cod_Usuario = dr["Cod_Usuario"].ToString();
                    usuario.Cedula = dr["Cedula"].ToString();
                    usuario.Departamento = dr["Departamento"].ToString();
                    usuario.Cod_Jefe_Inm = dr["Cod_Jefe_Inm"].ToString();
                    usuario.Nom_Usuario = dr["Nom_Usuario"].ToString();
                    usuario.JefeInmediato = dr["JefeInmediato"].ToString();

                    cmbUsuarios.Add(usuario);
                }

            }
            catch (Exception ex)
            {
                cmbUsuarios = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return cmbUsuarios;
        }

        public static string RTACorreoUsuarioPlanificacionVac(string Cod_Jefe_Inm)
        {

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            string resultado = "";

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTACorreoUsuarioPlanificacionVac", cnx);
                cmd.Parameters.AddWithValue("@Cod_Jefe_Inm", Cod_Jefe_Inm);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                resultado = dr["Respuesta"].ToString();

            }
            catch (Exception ex)
            {
                resultado = "";

            }
            finally
            {
                cmd.Connection.Close();
            }
            return resultado;
        }

    }
}
