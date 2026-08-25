using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoUsuarioAdmin
    {
        /// <summary>Lista usuarios (activos e inactivos) con su perfil.</summary>
        public static List<EntUsuarioAdmin> ListarUsuarios(string filtro)
        {
            List<EntUsuarioAdmin> lista = new List<EntUsuarioAdmin>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarUsuariosAdmin", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntUsuarioAdmin()
                        {
                            Id_Usuario = Convert.ToDecimal(dr["Id_Usuario"]),
                            Cod_Usuario = dr["Cod_Usuario"].ToString(),
                            Nom_Usuario = dr["Nom_Usuario"].ToString(),
                            Log_Usuario = dr["Log_Usuario"].ToString(),
                            E_Mail = dr["E_Mail"].ToString(),
                            Cedula = dr["Cedula"].ToString(),
                            Departamento = dr["Departamento"].ToString(),
                            Empresa = dr["Empresa"].ToString(),
                            Cod_Sap = dr["Cod_Sap"].ToString(),
                            Cod_Jefe_Inm = dr["Cod_Jefe_Inm"].ToString(),
                            MailCodJefeInm = dr["MailCodJefeInm"].ToString(),
                            TelefonosEmergencia = dr["TelefonosEmergencia"].ToString(),
                            Id_Perfil = Convert.ToInt64(dr["Id_Perfil"]),
                            NombrePerfil = dr["NombrePerfil"].ToString(),
                            Usuario_Estado = dr["Usuario_Estado"].ToString(),
                            EstadoUsuario = dr["EstadoUsuario"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Departamentos que existen hoy en los usuarios, para el combo de la
        /// pantalla. No hay tabla catalogo: la lista sale de los propios datos.
        /// </summary>
        public static List<string> ListarDepartamentos()
        {
            List<string> lista = new List<string>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarDepartamentos", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(dr["Departamento"].ToString());
                    }
                }
            }

            return lista;
        }

        /// <summary>Historial de cambios de un usuario.</summary>
        public static List<EntUsuarioBitacora> ListarBitacora(decimal idUsuario)
        {
            List<EntUsuarioBitacora> lista = new List<EntUsuarioBitacora>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarBitacoraUsuario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id_Usuario", SqlDbType.Decimal).Value = idUsuario;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntUsuarioBitacora()
                        {
                            Accion = dr["Accion"].ToString(),
                            Detalle = dr["Detalle"].ToString(),
                            Usuario_Registro = dr["Usuario_Registro"].ToString(),
                            Fecha_Registro = dr["Fecha_Registro"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Actualiza los ocho campos editables y deja rastro en la bitácora.</summary>
        public static EntRespuesta ActualizarUsuario(EntUsuarioAdmin u, string usuarioRegistro)
        {
            EntRespuesta respuesta = NuevaRespuesta();

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_ActualizarUsuario", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id_Usuario", SqlDbType.Decimal).Value = u.Id_Usuario;
                    cmd.Parameters.Add("@Nom_Usuario", SqlDbType.VarChar, 300).Value = u.Nom_Usuario ?? string.Empty;
                    cmd.Parameters.Add("@E_Mail", SqlDbType.VarChar, 300).Value = u.E_Mail ?? string.Empty;
                    cmd.Parameters.Add("@Cedula", SqlDbType.VarChar, 300).Value = u.Cedula ?? string.Empty;
                    cmd.Parameters.Add("@Departamento", SqlDbType.VarChar, 300).Value = u.Departamento ?? string.Empty;
                    cmd.Parameters.Add("@Empresa", SqlDbType.VarChar, 300).Value = u.Empresa ?? string.Empty;
                    cmd.Parameters.Add("@Cod_Sap", SqlDbType.VarChar, 300).Value = u.Cod_Sap ?? string.Empty;
                    cmd.Parameters.Add("@Cod_Jefe_Inm", SqlDbType.VarChar, 300).Value = u.Cod_Jefe_Inm ?? string.Empty;
                    cmd.Parameters.Add("@MailCodJefeInm", SqlDbType.VarChar, 300).Value = u.MailCodJefeInm ?? string.Empty;
                    cmd.Parameters.Add("@TelefonosEmergencia", SqlDbType.VarChar, 300).Value = u.TelefonosEmergencia ?? string.Empty;
                    cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 50).Value = usuarioRegistro ?? "SISTEMA";

                    cnx.Open();
                    LeerRespuesta(cmd, respuesta);
                }
            }
            catch (Exception ex)
            {
                respuesta.estado = "0";
                respuesta.resultado = "0";
                respuesta.tipoMensaje = "danger";
                respuesta.mensaje = "Ocurrió un error al guardar los datos del usuario. Detalle: " + ex.Message;
            }

            return respuesta;
        }

        /// <summary>
        /// Inactiva (EstadoUsuario = 0) o activa (EstadoUsuario = NULL) al usuario.
        /// Controla si aparece en selectores de jefe, autocompletado y solicitudes;
        /// NO controla el inicio de sesión, que va contra Active Directory.
        /// </summary>
        public static EntRespuesta CambiarEstado(decimal idUsuario, bool inactivar, string usuarioRegistro)
        {
            EntRespuesta respuesta = NuevaRespuesta();

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_CambiarEstadoUsuario", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id_Usuario", SqlDbType.Decimal).Value = idUsuario;
                    cmd.Parameters.Add("@Inactivar", SqlDbType.Bit).Value = inactivar;
                    cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 50).Value = usuarioRegistro ?? "SISTEMA";

                    cnx.Open();
                    LeerRespuesta(cmd, respuesta);
                }
            }
            catch (Exception ex)
            {
                respuesta.estado = "0";
                respuesta.resultado = "0";
                respuesta.tipoMensaje = "danger";
                respuesta.mensaje = "Ocurrió un error al cambiar el estado del usuario. Detalle: " + ex.Message;
            }

            return respuesta;
        }

        private static EntRespuesta NuevaRespuesta()
        {
            return new EntRespuesta()
            {
                estado = "0",
                resultado = "0",
                tipoMensaje = "danger",
                mensaje = ""
            };
        }

        /// <summary>Traduce el contrato Respuestas/Mensaje de los SPs a EntRespuesta.</summary>
        private static void LeerRespuesta(SqlCommand cmd, EntRespuesta respuesta)
        {
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());
                    respuesta.resultado = respuestaSP.ToString();
                    respuesta.mensaje = dr["Mensaje"].ToString();

                    if (respuestaSP > 0)
                    {
                        respuesta.estado = "1";
                        respuesta.tipoMensaje = "success";
                    }
                    else
                    {
                        respuesta.estado = "0";
                        respuesta.tipoMensaje = "warning";
                    }
                }
                else
                {
                    respuesta.mensaje = "El procedimiento no devolvió información.";
                }
            }
        }
    }
}
