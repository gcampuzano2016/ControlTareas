using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoUsuarioHorario
    {
        /// <summary>
        /// Lista los perfiles de horario activos para combos (Id / Valor).
        /// </summary>
        public static List<EntCombo> ListarPerfiles()
        {
            List<EntCombo> lista = new List<EntCombo>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarPerfilesHorario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntCombo()
                        {
                            Id = dr["Id"].ToString(),
                            Valor = dr["Valor"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Lista los usuarios activos con el horario que tienen vigente.
        /// </summary>
        public static List<EntUsuarioHorario> ListarUsuarios(string filtro)
        {
            List<EntUsuarioHorario> lista = new List<EntUsuarioHorario>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarUsuariosConHorario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntUsuarioHorario()
                        {
                            Cod_Usuario = dr["Cod_Usuario"].ToString(),
                            Nom_Usuario = dr["Nom_Usuario"].ToString(),
                            Cedula = dr["Cedula"].ToString(),
                            Departamento = dr["Departamento"].ToString(),
                            Empresa = dr["Empresa"].ToString(),
                            IdHorarioLaboral = Convert.ToInt32(dr["IdHorarioLaboral"].ToString()),
                            CodigoHorario = dr["CodigoHorario"].ToString(),
                            NombreHorario = dr["NombreHorario"].ToString(),
                            EsPredeterminado = Convert.ToInt32(dr["EsPredeterminado"].ToString()),
                            FechaDesde = dr["FechaDesde"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Asigna (o cambia) el horario de un usuario conservando el historial.
        /// </summary>
        public static EntRespuesta AsignarHorario(string idResponsable, int idHorarioLaboral, string fechaDesde, string usuarioRegistro)
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
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_AsignarHorarioUsuario", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id_Responsable", SqlDbType.VarChar, 20).Value = idResponsable ?? string.Empty;
                    cmd.Parameters.Add("@IdHorarioLaboral", SqlDbType.Int).Value = idHorarioLaboral;
                    cmd.Parameters.Add("@FechaDesde", SqlDbType.VarChar, 10).Value = (object)fechaDesde ?? DBNull.Value;
                    cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 100).Value = usuarioRegistro ?? string.Empty;

                    cnx.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            int respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());
                            string mensajeSP = dr["Mensaje"].ToString();

                            respuesta.resultado = respuestaSP.ToString();
                            respuesta.mensaje = mensajeSP;

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
            catch (Exception ex)
            {
                respuesta.estado = "0";
                respuesta.resultado = "0";
                respuesta.tipoMensaje = "danger";
                respuesta.mensaje = "Ocurrió un error al asignar el horario. Detalle: " + ex.Message;
            }

            return respuesta;
        }
    }
}
