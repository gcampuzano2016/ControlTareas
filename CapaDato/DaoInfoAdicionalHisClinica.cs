using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

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

        public static EntRespuesta Consulta_Sp_InsUpdInfoRelevante(List<EntDatosMed> objInfo)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            bool hasErrors = false;

            try
            {
                // Crear conexión
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                using (SqlConnection cnx = cn.conectar())
                {
                    cnx.Open();

                    foreach (var info in objInfo)
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand("Sp_RTAInsMedInfoRelevante", cnx))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                // Formar el nombre completo
                                string nombreCompleto = $"{info.apellido1} {info.apellido2} {info.nombre1} {info.nombre2}";

                                // Agregar parámetros al procedimiento almacenado
                                cmd.Parameters.AddWithValue("@Nombre", nombreCompleto);
                                cmd.Parameters.AddWithValue("@Empresa", info.empresa ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Sexo", info.sexo ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Fecha", info.fecha ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Diagnostico1", info.diagnostico1 ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Diagnostico2", info.diagnostico2 ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Diagnostico3", info.diagnostico3 ?? (object)DBNull.Value);

                                // Ejecutar el procedimiento almacenado
                                int result = cmd.ExecuteNonQuery();

                                // Validar el resultado de la ejecución
                                if (result <= 0)
                                {
                                    hasErrors = true; // Marcar que hubo un error
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            hasErrors = true; // Marcar que hubo un error
                                              // Log o registro del error específico (opcional)
                            Console.WriteLine($"Error procesando elemento: {ex.Message}");
                        }
                    }
                }

                // Configurar la respuesta basada en los errores
                if (!hasErrors)
                {
                    Respuesta.estado = "1";
                    Respuesta.mensaje = "Todos los datos se guardaron con éxito.";
                    Respuesta.tipoMensaje = "success";
                }
                else
                {
                    Respuesta.estado = "0";
                    Respuesta.mensaje = "Datos cargados sin embargo algunos datos no pudieron guardarse.";
                    Respuesta.tipoMensaje = "warning";
                }
            }
            catch (Exception ex)
            {
                Respuesta.estado = "0";
                Respuesta.mensaje = $"Error: {ex.Message}";
                Respuesta.tipoMensaje = "danger";
            }

            return Respuesta;
        }



    }
}
