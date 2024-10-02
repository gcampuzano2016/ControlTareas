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
    public class DaoInfoAdicionalHisClinica
    {
        public static EntRespuesta Consulta_Sp_InsUpdInfoAdicional1(EntInfoAdicionalHisClinica objInfo)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            Int32 respuestaSP = 0;
            string respuestaSP1 = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            int o = objInfo.opci;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAInsUpdInfoAdicionalHisClinica", cnx);

                cmd.Parameters.AddWithValue("@IdInfo", objInfo.IdInfo);
                cmd.Parameters.AddWithValue("@Cedula", objInfo.IdEmpleado);
                cmd.Parameters.AddWithValue("@Lentes", objInfo.Lentes);
                cmd.Parameters.AddWithValue("@GruposVulnerables", objInfo.GruposVulnerables);
                cmd.Parameters.AddWithValue("@RegistraAlergias", objInfo.RegistraAlergias);
                cmd.Parameters.AddWithValue("@NombreAlergia", objInfo.NombreAlergia);
                cmd.Parameters.AddWithValue("@TipoAlergia", objInfo.TipoAlergia);
                cmd.Parameters.AddWithValue("@ReaccionesAlergia", objInfo.ReaccionesAlergia);
                //cmd.Parameters.AddWithValue("@TipoOperacion", 2);
                //cmd.Parameters.AddWithValue("@Fec_Modificacion", "");
                //cmd.Parameters.AddWithValue("@Usu_Modificacion", "");

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                respuestaSP1 = dr["Respuestas"].ToString();
                if (respuestaSP1 == "")
                {
                    respuestaSP = 1;
                }
                else
                {
                    respuestaSP = Convert.ToInt32(respuestaSP1);
                }

                if (respuestaSP >= 1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "1";
                    Respuesta.mensaje = "Datos Guardados con Exito.";
                    Respuesta.tipoMensaje = "success";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else if (respuestaSP == -1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "-1";
                    Respuesta.mensaje = "Por favor validar que la informacion este correcto";
                    Respuesta.tipoMensaje = "danger";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Ocurrio un error al guardar los datos.";
                    Respuesta.tipoMensaje = "danger";
                }
            }
            catch (Exception ex)
            {
                Respuesta.estado = "0";
                Respuesta.mensaje = ex.Message.ToString();
                Respuesta.tipoMensaje = "danger";
            }
            finally
            {
                cmd.Connection.Close();
            }

            return Respuesta;

        }
        public static EntRespuesta Consulta_Sp_InsUpdInfoAdicional2(EntInfoAdicionalHisClinica objInfo)
        {     
            EntRespuesta Respuesta = new EntRespuesta();
            Int32 respuestaSP = 0;
            string respuestaSP1 = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAInsUpdInfoAdicionalHisClinica", cnx);

                cmd.Parameters.AddWithValue("@IdInfo", objInfo.IdInfo);
                cmd.Parameters.AddWithValue("@Cedula", objInfo.IdEmpleado);
                cmd.Parameters.AddWithValue("@NomContactoEmergencia", objInfo.NomContactoEmergencia);
                cmd.Parameters.AddWithValue("@TelfContactoEmergencia", objInfo.TelfContactoEmergencia);
                cmd.Parameters.AddWithValue("@ParentescoContacto", objInfo.ParentescoContacto);
                //cmd.Parameters.AddWithValue("@TipoOperacion", 1);
                //cmd.Parameters.AddWithValue("@Fec_Modificacion", "");
                //cmd.Parameters.AddWithValue("@Usu_Modificacion", "");

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                respuestaSP1 = dr["Respuestas"].ToString();
                if (respuestaSP1 == "")
                {
                    respuestaSP = 1;
                }
                else
                {
                    respuestaSP = Convert.ToInt32(respuestaSP1);
                }

                if (respuestaSP >= 1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "1";
                    Respuesta.mensaje = "Datos Guardados con Exito.";
                    Respuesta.tipoMensaje = "success";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else if (respuestaSP == -1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "-1";
                    Respuesta.mensaje = "Por favor validar que la informacion este correcto";
                    Respuesta.tipoMensaje = "danger";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Ocurrio un error al guardar los datos.";
                    Respuesta.tipoMensaje = "danger";
                }
            }
            catch (Exception ex)
            {
                Respuesta.estado = "0";
                Respuesta.mensaje = ex.Message.ToString();
                Respuesta.tipoMensaje = "danger";
            }
            finally
            {
                cmd.Connection.Close();
            }

            return Respuesta;

        }

        public static List<EntInfoAdicionalHisClinica> Consulta_Sp_RTAConsultarInfoPorId(string IdEmpleado)
        {
            List<EntInfoAdicionalHisClinica> listaTareas = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {                
                EntInfoAdicionalHisClinica objInfo = new EntInfoAdicionalHisClinica();

                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAConsultarContactoEmpleadoPorId", cnx);
                cmd.Parameters.AddWithValue("@CedulaEmp", IdEmpleado);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntInfoAdicionalHisClinica>();

                while (dr.Read())
                {
                    listaTareas = new List<EntInfoAdicionalHisClinica>();
                    objInfo.IdEmpleado = dr["CEDULA"].ToString();
                    objInfo.Lentes = dr["LENTES"].ToString();
                    objInfo.GruposVulnerables = dr["GRUPOSVULNERABLES"].ToString();
                    objInfo.NomContactoEmergencia = dr["NOMCONTACTOEMERGENCIA"].ToString();
                    objInfo.TelfContactoEmergencia = dr["TELFCONTACTOEMERGENCIA"].ToString();
                    objInfo.ParentescoContacto = dr["PARENTESCOCONTACTO"].ToString();
                    objInfo.RegistraAlergias = dr["REGISTRAALERGIAS"].ToString();
                    objInfo.NombreAlergia = dr["NOMBREALERGIA"].ToString();
                    objInfo.TipoAlergia = dr["TIPOALERGIA"].ToString();
                    objInfo.ReaccionesAlergia = dr["REACCIONESALERGIA"].ToString();
                    listaTareas.Add(objInfo);
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
