using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace CapaDato
{
    /// <summary>
    /// Sp_RTA_TableroCostos devuelve tres resultados, en este orden: actividades,
    /// tarifas y parametros. Si falta uno, se lanza una excepcion en vez de
    /// seguir: sin tarifas todo saldria valorado en $0, sin ningun error visible.
    /// </summary>
    public class DaoTableroCostos
    {
        public static EntTcDatos Cargar(DateTime desde, DateTime hasta)
        {
            EntTcDatos d = new EntTcDatos();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_TableroCostos", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.Add("@Desde", SqlDbType.Date).Value = desde.Date;
                cmd.Parameters.Add("@Hasta", SqlDbType.Date).Value = hasta.Date;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        d.Actividades.Add(new EntTcActividad
                        {
                            IdDetalle = Convert.ToInt64(dr["Id_RegDetTareas"], CultureInfo.InvariantCulture),
                            FechaInicio = Convert.ToDateTime(dr["Det_Fch_RegDetalleIni"], CultureInfo.InvariantCulture),
                            FechaFin = FechaNula(dr, "Det_Fch_RegDetalleFin"),
                            Tiempo = Texto(dr, "Det_Tiempo"),
                            CodResponsable = Texto(dr, "Id_Responsable"),
                            NomResponsable = Texto(dr, "Nom_Usuario"),
                            Empresa = Texto(dr, "Det_Nom_Empresa"),
                            Contrato = Texto(dr, "NombreCliente"),
                            Ticket = Texto(dr, "Det_Id_CompAranda"),
                            OrdenServicio = Texto(dr, "Det_Num_OrdenServicio"),
                            Tarea = Texto(dr, "Det_Det_Tarea"),
                            Estado = Texto(dr, "Det_Estado"),
                            IdEstadoIni = EnteroNulo(dr, "IdDet_EstadoIni"),
                            TipoHE = EnteroNulo(dr, "Det_Horas_Extras_Tipo") ?? 0,
                            EstadoHE = EnteroNulo(dr, "Det_Horas_Extras_Estado") ?? 0
                        });
                    }

                    if (!dr.NextResult())
                    {
                        throw new InvalidOperationException("Sp_RTA_TableroCostos no devolvió las tarifas.");
                    }
                    while (dr.Read())
                    {
                        d.Tarifas.Add(new EntTcTarifa
                        {
                            CodUsuario = Texto(dr, "Cod_Usuario"),
                            Cargo = Texto(dr, "Cargo"),
                            JornadaHorasDia = EnteroNulo(dr, "JornadaHorasDia"),
                            AplicaHE = dr["AplicaHE"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(dr["AplicaHE"], CultureInfo.InvariantCulture),
                            Salario = DecimalNulo(dr, "Monto")
                        });
                    }

                    if (!dr.NextResult())
                    {
                        throw new InvalidOperationException("Sp_RTA_TableroCostos no devolvió los parámetros.");
                    }
                    while (dr.Read())
                    {
                        /* Vienen ordenados por vigencia: la mas reciente pisa a la anterior. */
                        d.Parametros[Texto(dr, "Clave")] = Convert.ToDecimal(dr["Valor"], CultureInfo.InvariantCulture);
                    }
                }
            }
            return d;
        }

        /// <summary>Devuelve el IdAuditoria insertado, o 0 si la base no devolvio nada.</summary>
        public static int Auditar(string codUsuario, string accion, string filtros)
        {
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_TableroCostosAuditar", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CodUsuario", SqlDbType.VarChar, 20).Value = codUsuario ?? "";
                cmd.Parameters.Add("@Accion", SqlDbType.VarChar, 20).Value = accion ?? "";
                cmd.Parameters.Add("@Filtros", SqlDbType.NVarChar, 1000).Value = (object)filtros ?? DBNull.Value;
                cnx.Open();

                object v = cmd.ExecuteScalar();
                return v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v, CultureInfo.InvariantCulture);
            }
        }

        private static string Texto(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? "" : Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
        }

        private static int? EnteroNulo(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (int?)null : Convert.ToInt32(v, CultureInfo.InvariantCulture);
        }

        private static decimal? DecimalNulo(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (decimal?)null : Convert.ToDecimal(v, CultureInfo.InvariantCulture);
        }

        private static DateTime? FechaNula(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(v, CultureInfo.InvariantCulture);
        }
    }
}
