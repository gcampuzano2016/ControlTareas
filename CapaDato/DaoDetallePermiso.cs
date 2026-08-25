using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Tipo de permiso, tratamiento del excedente y detalle de teletrabajo.
    /// </summary>
    public class DaoDetallePermiso
    {
        public static EntRespuesta Guardar(EntDetallePermiso detalle)
        {
            EntRespuesta respuesta = new EntRespuesta();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarDetallePermiso", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdVacaciones", SqlDbType.BigInt).Value = detalle.IdVacaciones;
                cmd.Parameters.Add("@TipoPermiso", SqlDbType.VarChar, 20).Value = detalle.TipoPermiso ?? string.Empty;
                cmd.Parameters.Add("@TratamientoExcedente", SqlDbType.VarChar, 20).Value = detalle.TratamientoExcedente ?? string.Empty;
                cmd.Parameters.Add("@Modalidad", SqlDbType.VarChar, 20).Value = detalle.Modalidad ?? string.Empty;
                cmd.Parameters.Add("@HoraDesde", SqlDbType.VarChar, 10).Value = detalle.HoraDesde ?? string.Empty;
                cmd.Parameters.Add("@HoraHasta", SqlDbType.VarChar, 10).Value = detalle.HoraHasta ?? string.Empty;
                cmd.Parameters.Add("@Lugar", SqlDbType.VarChar, 200).Value = detalle.Lugar ?? string.Empty;
                cmd.Parameters.Add("@MediosContacto", SqlDbType.VarChar, 200).Value = detalle.MediosContacto ?? string.Empty;
                cmd.Parameters.Add("@MotivoGeneral", SqlDbType.VarChar, 500).Value = detalle.MotivoGeneral ?? string.Empty;
                cmd.Parameters.Add("@Actividades", SqlDbType.VarChar, 1000).Value = detalle.Actividades ?? string.Empty;
                cmd.Parameters.Add("@Entregables", SqlDbType.VarChar, 1000).Value = detalle.Entregables ?? string.Empty;
                cmd.Parameters.Add("@ConfirmaConectividad", SqlDbType.Bit).Value = detalle.ConfirmaConectividad;

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

            respuesta.tipoMensaje = respuesta.estado == "1" ? "success" : "danger";
            return respuesta;
        }

        /// <summary>
        /// El detalle de una solicitud. Devuelve null solo si la solicitud no
        /// existe: una sin detalle cargado responde con los campos vacíos, para
        /// que la pantalla no tenga que distinguir esos dos casos.
        /// </summary>
        public static EntDetallePermiso Obtener(long idVacaciones)
        {
            EntDetallePermiso detalle = null;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ObtenerDetallePermiso", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdVacaciones", SqlDbType.BigInt).Value = idVacaciones;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        detalle = new EntDetallePermiso()
                        {
                            IdVacaciones = Convert.ToInt64(dr["IdVacaciones"]),
                            TipoPermiso = dr["TipoPermiso"].ToString(),
                            TratamientoExcedente = dr["TratamientoExcedente"].ToString(),
                            Actividad = dr["Actividad"].ToString(),
                            EsTeletrabajo = Convert.ToInt32(dr["EsTeletrabajo"]) == 1,
                            Modalidad = dr["Modalidad"].ToString(),
                            HoraDesde = dr["HoraDesde"].ToString(),
                            HoraHasta = dr["HoraHasta"].ToString(),
                            Lugar = dr["Lugar"].ToString(),
                            MediosContacto = dr["MediosContacto"].ToString(),
                            MotivoGeneral = dr["MotivoGeneral"].ToString(),
                            Actividades = dr["Actividades"].ToString(),
                            Entregables = dr["Entregables"].ToString(),
                            ConfirmaConectividad = Convert.ToBoolean(dr["ConfirmaConectividad"])
                        };
                    }
                }
            }

            return detalle;
        }
    }
}
