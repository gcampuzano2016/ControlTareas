using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Plan de recuperación de un permiso y su cierre.
    /// </summary>
    public class DaoPlanRecuperacion
    {
        public static EntRespuesta Guardar(EntPlanRecuperacion plan)
        {
            EntRespuesta respuesta = new EntRespuesta();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarPlanRecuperacion", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdVacaciones", SqlDbType.BigInt).Value = plan.IdVacaciones;
                cmd.Parameters.Add("@FechaPropuesta", SqlDbType.Date).Value = plan.FechaPropuesta.Date;
                cmd.Parameters.Add("@HorarioPropuesto", SqlDbType.VarChar, 100).Value = plan.HorarioPropuesto ?? string.Empty;
                cmd.Parameters.Add("@Actividades", SqlDbType.VarChar, 1000).Value = plan.Actividades ?? string.Empty;
                cmd.Parameters.Add("@Entregables", SqlDbType.VarChar, 1000).Value = plan.Entregables ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        respuesta.estado = dr["Respuestas"].ToString();
                        respuesta.mensaje = dr["Mensaje"].ToString();
                    }
                }
            }

            respuesta.tipoMensaje = respuesta.estado == "1" ? "success" : "warning";
            return respuesta;
        }

        /// <summary>
        /// El cierre que hace el jefe pasada la fecha máxima. El procedimiento
        /// rechaza si todavía no vence o si dice que no se recuperó sin explicar.
        /// </summary>
        public static EntRespuesta Confirmar(long idVacaciones, bool seRecupero, string observacion, string codUsuario)
        {
            EntRespuesta respuesta = new EntRespuesta();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ConfirmarRecuperacion", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdVacaciones", SqlDbType.BigInt).Value = idVacaciones;
                cmd.Parameters.Add("@SeRecupero", SqlDbType.Bit).Value = seRecupero;
                cmd.Parameters.Add("@Observacion", SqlDbType.VarChar, 500).Value = observacion ?? string.Empty;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 64).Value = codUsuario ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        respuesta.estado = dr["Respuestas"].ToString();
                        respuesta.mensaje = dr["Mensaje"].ToString();
                    }
                }
            }

            respuesta.tipoMensaje = respuesta.estado == "1" ? "success" : "warning";
            return respuesta;
        }

        /// <summary>Las recuperaciones cuyo plazo ya venció y nadie cerró.</summary>
        public static List<EntPlanRecuperacion> ListarPendientes()
        {
            List<EntPlanRecuperacion> lista = new List<EntPlanRecuperacion>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarRecuperacionesPendientes", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntPlanRecuperacion()
                        {
                            IdVacaciones = Convert.ToInt64(dr["IdVacaciones"]),
                            Colaborador = dr["Colaborador"].ToString(),
                            Cedula = dr["Cedula"].ToString(),
                            JefeInmediato = dr["JefeInmediato"].ToString(),
                            FechaPermiso = Convert.ToDateTime(dr["FechaPermiso"]),
                            Horas = dr["Horas"].ToString(),
                            FechaPropuesta = Convert.ToDateTime(dr["FechaPropuesta"]),
                            HorarioPropuesto = dr["HorarioPropuesto"].ToString(),
                            Actividades = dr["Actividades"].ToString(),
                            Entregables = dr["Entregables"].ToString(),
                            FechaMaximaCierre = Convert.ToDateTime(dr["FechaMaximaCierre"]),
                            DiasVencido = Convert.ToInt32(dr["DiasVencido"])
                        });
                    }
                }
            }

            return lista;
        }
    }
}
