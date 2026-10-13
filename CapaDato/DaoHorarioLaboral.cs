using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace CapaDato
{
    /// <summary>
    /// Catalogo de horarios laborales: crear, editar, prender/apagar un perfil,
    /// y el horario propio de una persona.
    /// La asignacion de un perfil a un usuario sigue viviendo en DaoUsuarioHorario.
    /// </summary>
    public class DaoHorarioLaboral
    {
        /// <summary>
        /// Lista los perfiles del catalogo con su resumen de dias y cuanta gente los usa.
        /// </summary>
        public static List<EntHorarioLaboral> ListarHorarios(string filtro, bool incluirInactivos, bool incluirPropios)
        {
            List<EntHorarioLaboral> lista = new List<EntHorarioLaboral>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarHorarios", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                cmd.Parameters.Add("@IncluirInactivos", SqlDbType.Bit).Value = incluirInactivos;
                cmd.Parameters.Add("@IncluirPropios", SqlDbType.Bit).Value = incluirPropios;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntHorarioLaboral()
                        {
                            IdHorarioLaboral = Convert.ToInt32(dr["IdHorarioLaboral"].ToString()),
                            Codigo = dr["Codigo"].ToString(),
                            Nombre = dr["Nombre"].ToString(),
                            EsPredeterminado = Convert.ToInt32(dr["EsPredeterminado"].ToString()),
                            Activo = Convert.ToInt32(dr["Activo"].ToString()),
                            EsPropio = Convert.ToInt32(dr["EsPropio"].ToString()),
                            CodigoDueno = dr["CodigoDueno"].ToString(),
                            NombreDueno = dr["NombreDueno"].ToString(),
                            Resumen = dr["Resumen"].ToString(),
                            UsuariosAsignados = Convert.ToInt32(dr["UsuariosAsignados"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Los siete dias de un perfil. Siempre devuelve siete, aunque en la tabla falte alguno.
        /// </summary>
        public static List<EntHorarioLaboralDetalle> ObtenerHorario(int idHorarioLaboral)
        {
            List<EntHorarioLaboralDetalle> lista = new List<EntHorarioLaboralDetalle>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ObtenerHorario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdHorarioLaboral", SqlDbType.Int).Value = idHorarioLaboral;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntHorarioLaboralDetalle()
                        {
                            DiaSemana = Convert.ToInt32(dr["DiaSemana"].ToString()),
                            NombreDia = dr["NombreDia"].ToString(),
                            EsLaborable = Convert.ToInt32(dr["EsLaborable"].ToString()),
                            HoraInicio = dr["HoraInicio"].ToString(),
                            HoraFin = dr["HoraFin"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Crea o actualiza un perfil compartido junto con sus siete dias.
        /// Cuando el perfil tiene gente asignada y cambian las horas, el SP devuelve
        /// -5 sin guardar nada: la pantalla pregunta y reintenta con confirmaSobrescribir.
        /// </summary>
        public static EntRespuesta GuardarHorario(EntHorarioLaboral horario,
                                                  List<EntHorarioLaboralDetalle> dias,
                                                  string usuarioRegistro,
                                                  bool confirmaSobrescribir)
        {
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarHorario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdHorarioLaboral", SqlDbType.Int).Value = horario.IdHorarioLaboral;
                cmd.Parameters.Add("@Codigo", SqlDbType.VarChar, 30).Value = horario.Codigo ?? string.Empty;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 120).Value = horario.Nombre ?? string.Empty;
                cmd.Parameters.Add("@EsPredeterminado", SqlDbType.Bit).Value = (horario.EsPredeterminado == 1);
                cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = (horario.Activo == 1);
                cmd.Parameters.Add("@Detalle", SqlDbType.Xml).Value = ArmarXmlDias(dias);
                cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 100).Value = usuarioRegistro ?? string.Empty;
                cmd.Parameters.Add("@ConfirmaSobrescribir", SqlDbType.Bit).Value = confirmaSobrescribir;

                return EjecutarYLeerRespuesta(cnx, cmd, "No se pudo guardar el horario.");
            }
        }

        /// <summary>
        /// Prende o apaga un perfil desde la grilla.
        /// </summary>
        public static EntRespuesta CambiarEstadoHorario(int idHorarioLaboral, bool activo, string usuarioRegistro)
        {
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_CambiarEstadoHorario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdHorarioLaboral", SqlDbType.Int).Value = idHorarioLaboral;
                cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = activo;
                cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 100).Value = usuarioRegistro ?? string.Empty;

                return EjecutarYLeerRespuesta(cnx, cmd, "No se pudo cambiar el estado del horario.");
            }
        }

        /// <summary>
        /// Guarda el horario que es de una sola persona y lo deja asignado, todo en
        /// la misma transaccion del SP.
        /// </summary>
        public static EntRespuesta GuardarHorarioPropio(string idResponsable,
                                                        List<EntHorarioLaboralDetalle> dias,
                                                        string fechaDesde,
                                                        string usuarioRegistro)
        {
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarHorarioPropio", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id_Responsable", SqlDbType.VarChar, 20).Value = idResponsable ?? string.Empty;
                cmd.Parameters.Add("@Detalle", SqlDbType.Xml).Value = ArmarXmlDias(dias);
                cmd.Parameters.Add("@FechaDesde", SqlDbType.VarChar, 10).Value = (object)fechaDesde ?? DBNull.Value;
                cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 100).Value = usuarioRegistro ?? string.Empty;

                return EjecutarYLeerRespuesta(cnx, cmd, "No se pudo guardar el horario propio.");
            }
        }

        /// <summary>
        /// Los siete dias como XML. Son siete filas: veintiun parametros sueltos no
        /// valia la pena.
        ///   <dias><dia ds="1" lab="1" ini="08:30" fin="17:30" /> ... </dias>
        /// </summary>
        private static string ArmarXmlDias(List<EntHorarioLaboralDetalle> dias)
        {
            StringBuilder xml = new StringBuilder();
            xml.Append("<dias>");

            if (dias != null)
            {
                foreach (EntHorarioLaboralDetalle dia in dias)
                {
                    xml.Append("<dia ds=\"");
                    xml.Append(dia.DiaSemana);
                    xml.Append("\" lab=\"");
                    xml.Append(dia.EsLaborable == 1 ? "1" : "0");
                    xml.Append("\" ini=\"");
                    xml.Append(LimpiarHora(dia.HoraInicio));
                    xml.Append("\" fin=\"");
                    xml.Append(LimpiarHora(dia.HoraFin));
                    xml.Append("\" />");
                }
            }

            xml.Append("</dias>");
            return xml.ToString();
        }

        /// <summary>
        /// Deja pasar solo digitos y ':'. Lo que llegue raro se convierte en vacio y
        /// el SP lo rechaza con -4, en vez de romper el XML.
        /// </summary>
        private static string LimpiarHora(string hora)
        {
            if (string.IsNullOrEmpty(hora)) return string.Empty;

            StringBuilder limpia = new StringBuilder();

            foreach (char c in hora)
            {
                if (char.IsDigit(c) || c == ':')
                {
                    limpia.Append(c);
                }
            }

            return limpia.Length > 8 ? limpia.ToString(0, 8) : limpia.ToString();
        }

        /// <summary>
        /// Todos los SPs de esta pantalla devuelven la misma fila Respuestas/Mensaje.
        /// Respuestas > 0 es exito; los negativos son los codigos documentados en el script.
        /// </summary>
        private static EntRespuesta EjecutarYLeerRespuesta(SqlConnection cnx, SqlCommand cmd, string mensajeError)
        {
            EntRespuesta respuesta = new EntRespuesta()
            {
                estado = "0",
                resultado = "0",
                tipoMensaje = "danger",
                mensaje = ""
            };

            try
            {
                cnx.Open();

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
            catch (Exception ex)
            {
                respuesta.estado = "0";
                respuesta.resultado = "0";
                respuesta.tipoMensaje = "danger";
                respuesta.mensaje = mensajeError + " Detalle: " + ex.Message;
            }

            return respuesta;
        }
    }
}
