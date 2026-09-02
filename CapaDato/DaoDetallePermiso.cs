using CapaEntidad;
using System;
using System.Collections.Generic;
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
                cmd.Parameters.Add("@UsaPermisoMensual", SqlDbType.Bit).Value = detalle.UsaPermisoMensual;
                cmd.Parameters.Add("@SaldoMensualTexto", SqlDbType.VarChar, 200).Value = detalle.SaldoMensualTexto ?? string.Empty;
                cmd.Parameters.Add("@RespaldoAdjunto", SqlDbType.VarChar, 20).Value = detalle.RespaldoAdjunto ?? string.Empty;

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
        ///
        /// Las columnas se leen por presencia y no a ciegas. Sp_RTA_ObtenerDetallePermiso
        /// crecio en varias entregas -el saldo mensual, el respaldo adjunto y el
        /// plan de recuperacion se agregaron despues-, asi que una base que quedo
        /// con la version anterior no devuelve todas. Pedir una columna que no vino
        /// lanza IndexOutOfRangeException, y esa excepcion no se queda aca: sube
        /// hasta PDFs.CargarDatos y deja al permiso sin documento y sin el correo a
        /// Talento Humano, que es quien pone la tercera firma. Vacaciones nunca se
        /// entera, porque no pasa por este procedimiento.
        ///
        /// Asi una base a medio actualizar degrada a "detalle vacio" -el documento
        /// pierde las secciones del permiso- en vez de tumbar el tramite entero. Lo
        /// que falte se arregla corriendo los scripts que quedaron pendientes.
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
                        HashSet<string> columnas = ColumnasDe(dr);

                        detalle = new EntDetallePermiso()
                        {
                            /* El del parametro: el procedimiento filtra por el, asi
                               que la fila no puede ser de otra solicitud. */
                            IdVacaciones = idVacaciones,
                            TipoPermiso = Texto(dr, columnas, "TipoPermiso"),
                            TratamientoExcedente = Texto(dr, columnas, "TratamientoExcedente"),
                            Actividad = Texto(dr, columnas, "Actividad"),
                            EsTeletrabajo = Bandera(dr, columnas, "EsTeletrabajo"),
                            Modalidad = Texto(dr, columnas, "Modalidad"),
                            HoraDesde = Texto(dr, columnas, "HoraDesde"),
                            HoraHasta = Texto(dr, columnas, "HoraHasta"),
                            Lugar = Texto(dr, columnas, "Lugar"),
                            MediosContacto = Texto(dr, columnas, "MediosContacto"),
                            MotivoGeneral = Texto(dr, columnas, "MotivoGeneral"),
                            Actividades = Texto(dr, columnas, "Actividades"),
                            Entregables = Texto(dr, columnas, "Entregables"),
                            ConfirmaConectividad = Bandera(dr, columnas, "ConfirmaConectividad"),
                            UsaPermisoMensual = Bandera(dr, columnas, "UsaPermisoMensual"),
                            SaldoMensualTexto = Texto(dr, columnas, "SaldoMensualTexto"),
                            RespaldoAdjunto = Texto(dr, columnas, "RespaldoAdjunto"),
                            TieneRecuperacion = Bandera(dr, columnas, "TieneRecuperacion"),
                            /* Las fechas del plan vienen como texto ya formateado
                               para el documento; si no hay plan quedan vacías. */
                            RecFechaPropuesta = FechaCorta(dr, columnas, "RecFechaPropuesta"),
                            RecHorario = Texto(dr, columnas, "RecHorario"),
                            RecActividades = Texto(dr, columnas, "RecActividades"),
                            RecEntregables = Texto(dr, columnas, "RecEntregables"),
                            RecFechaMaxima = FechaCorta(dr, columnas, "RecFechaMaxima")
                        };
                    }
                }
            }

            return detalle;
        }

        /// <summary>
        /// Los nombres de columna que trae el lector, sin distinguir mayusculas:
        /// es como los compara SQL Server y como los pide el indexador del lector.
        /// </summary>
        private static HashSet<string> ColumnasDe(SqlDataReader dr)
        {
            HashSet<string> nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < dr.FieldCount; i++)
            {
                nombres.Add(dr.GetName(i));
            }

            return nombres;
        }

        /// <summary>Una columna de texto, o vacio si no vino o es NULL.</summary>
        private static string Texto(SqlDataReader dr, HashSet<string> columnas, string nombre)
        {
            if (!columnas.Contains(nombre)) { return string.Empty; }

            object valor = dr[nombre];
            return valor == null || valor == DBNull.Value ? string.Empty : valor.ToString();
        }

        /// <summary>
        /// Una columna de si/no, o false si no vino o es NULL. Vale para las BIT y
        /// para el 0/1 que devuelven los CASE de EsTeletrabajo y TieneRecuperacion.
        /// </summary>
        private static bool Bandera(SqlDataReader dr, HashSet<string> columnas, string nombre)
        {
            if (!columnas.Contains(nombre)) { return false; }

            object valor = dr[nombre];
            if (valor == null || valor == DBNull.Value) { return false; }

            return Convert.ToBoolean(valor);
        }

        /// <summary>
        /// Una columna de fecha como dd/MM/yyyy, o vacio si no vino o es NULL.
        /// </summary>
        private static string FechaCorta(SqlDataReader dr, HashSet<string> columnas, string nombre)
        {
            if (!columnas.Contains(nombre)) { return string.Empty; }

            object valor = dr[nombre];
            if (valor == null || valor == DBNull.Value) { return string.Empty; }

            return Convert.ToDateTime(valor).ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Cuánto le queda a alguien del permiso mensual de 3 horas en el mes de
        /// la fecha indicada.
        /// </summary>
        /// <param name="idVacaciones">
        /// La solicitud que se está editando, para que no se cuente a sí misma.
        /// Cero cuando es una solicitud nueva.
        /// </param>
        public static EntSaldoPermisoMensual SaldoMensual(string codUsuario, DateTime fecha, long idVacaciones)
        {
            EntSaldoPermisoMensual saldo = null;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_SaldoPermisoMensual", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 64).Value = codUsuario ?? string.Empty;
                cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = fecha.Date;
                cmd.Parameters.Add("@IdVacaciones", SqlDbType.BigInt).Value = idVacaciones;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        saldo = new EntSaldoPermisoMensual()
                        {
                            MinutosAsignados = Convert.ToInt32(dr["MinutosAsignados"]),
                            MinutosUsados = Convert.ToInt32(dr["MinutosUsados"]),
                            MinutosDisponibles = Convert.ToInt32(dr["MinutosDisponibles"]),
                            VigenteHasta = Convert.ToDateTime(dr["VigenteHasta"]),
                            Mensaje = dr["Mensaje"].ToString()
                        };
                    }
                }
            }

            return saldo;
        }
    }
}
