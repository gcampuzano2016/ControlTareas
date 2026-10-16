using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace CapaDato
{
    /// <summary>
    /// Llamadas a los Sp_RTA_Agenda* (docs/sql/2026-10-15-agenda-esquema.sql).
    /// No decide nada. Las consultas de actividades devuelven dos resultados;
    /// si falta el de entregables se lanza: un "sin entregables" silencioso
    /// mostraria completas cosas que no lo estan.
    /// </summary>
    public class DaoAgenda
    {
        private static SqlConnection Conexion()
        {
            return new DaoReporTareaAranda().conectar();
        }

        private static SqlCommand Comando(SqlConnection cnx, string procedimiento)
        {
            SqlCommand cmd = new SqlCommand(procedimiento, cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            return cmd;
        }

        private static string Texto(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? "" : Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
        }

        private static bool Bit(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v != DBNull.Value && Convert.ToBoolean(v, CultureInfo.InvariantCulture);
        }

        public static List<EntAgendaPersona> PersonasActivas()
        {
            List<EntAgendaPersona> lista = new List<EntAgendaPersona>();
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaPersonasActivas"))
            {
                cnx.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntAgendaPersona
                        {
                            CodUsuario = Texto(dr, "Cod_Usuario"),
                            Nombre = Texto(dr, "Nombre"),
                            Correo = Texto(dr, "Correo"),
                            CodJefe = Texto(dr, "CodJefe"),
                            IdArea = Convert.ToInt32(dr["IdArea"], CultureInfo.InvariantCulture),
                            Area = Texto(dr, "Area"),
                            Color = Texto(dr, "Color"),
                            SinArea = Bit(dr, "EsSinArea")
                        });
                    }
                }
            }
            return lista;
        }

        public static List<EntAgendaArea> Areas()
        {
            List<EntAgendaArea> lista = new List<EntAgendaArea>();
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaAreas"))
            {
                cnx.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntAgendaArea
                        {
                            IdArea = Convert.ToInt32(dr["IdArea"], CultureInfo.InvariantCulture),
                            Nombre = Texto(dr, "Nombre"),
                            Color = Texto(dr, "Color"),
                            Activa = Bit(dr, "Activa"),
                            EsSinArea = Bit(dr, "EsSinArea")
                        });
                    }
                }
            }
            return lista;
        }

        public static int GuardarArea(int idArea, string nombre, string color, bool activa, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaGuardarArea"))
            {
                cmd.Parameters.Add("@IdArea", SqlDbType.Int).Value = idArea;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = nombre;
                cmd.Parameters.Add("@Color", SqlDbType.Char, 7).Value = color;
                cmd.Parameters.Add("@Activa", SqlDbType.Bit).Value = activa;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        public static void AsignarArea(string codUsuario, int idArea, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaAsignarArea"))
            {
                cmd.Parameters.Add("@CodUsuario", SqlDbType.VarChar, 20).Value = codUsuario;
                cmd.Parameters.Add("@IdArea", SqlDbType.Int).Value = idArea;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static List<EntAgendaActividad> Actividades(DateTime desde, DateTime hasta, DateTime hoy)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaActividades"))
            {
                cmd.Parameters.Add("@Desde", SqlDbType.Date).Value = desde.Date;
                cmd.Parameters.Add("@Hasta", SqlDbType.Date).Value = hasta.Date;
                cmd.Parameters.Add("@Hoy", SqlDbType.Date).Value = hoy.Date;
                cnx.Open();
                return LeerActividades(cmd, "Sp_RTA_AgendaActividades");
            }
        }

        public static EntAgendaActividad Actividad(int idActividad)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaActividad"))
            {
                cmd.Parameters.Add("@IdActividad", SqlDbType.Int).Value = idActividad;
                cnx.Open();
                return LeerActividades(cmd, "Sp_RTA_AgendaActividad").FirstOrDefault();
            }
        }

        private static List<EntAgendaActividad> LeerActividades(SqlCommand cmd, string procedimiento)
        {
            List<EntAgendaActividad> lista = new List<EntAgendaActividad>();
            Dictionary<int, EntAgendaActividad> porId = new Dictionary<int, EntAgendaActividad>();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    EntAgendaActividad a = new EntAgendaActividad
                    {
                        IdActividad = Convert.ToInt32(dr["IdActividad"], CultureInfo.InvariantCulture),
                        Titulo = Texto(dr, "Titulo"),
                        CodResponsable = Texto(dr, "Cod_Responsable"),
                        CodAsignadoPor = Texto(dr, "Cod_AsignadoPor"),
                        FechaEntrega = Convert.ToDateTime(dr["FechaEntrega"], CultureInfo.InvariantCulture),
                        CompletadaManual = Bit(dr, "CompletadaManual"),
                        FechaCompletada = dr["FechaCompletada"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(dr["FechaCompletada"], CultureInfo.InvariantCulture),
                        Anulada = Bit(dr, "Anulada")
                    };
                    lista.Add(a);
                    porId[a.IdActividad] = a;
                }
                if (!dr.NextResult())
                {
                    throw new InvalidOperationException(procedimiento + " no devolvió los entregables.");
                }
                while (dr.Read())
                {
                    EntAgendaActividad a;
                    if (!porId.TryGetValue(Convert.ToInt32(dr["IdActividad"], CultureInfo.InvariantCulture), out a)) { continue; }
                    a.Entregables.Add(new EntAgendaEntregable
                    {
                        IdEntregable = Convert.ToInt32(dr["IdEntregable"], CultureInfo.InvariantCulture),
                        IdActividad = a.IdActividad,
                        Descripcion = Texto(dr, "Descripcion"),
                        Orden = Convert.ToInt32(dr["Orden"], CultureInfo.InvariantCulture),
                        Hecho = Bit(dr, "Hecho")
                    });
                }
            }
            return lista;
        }

        public static int GuardarActividad(int idActividad, string titulo, string codResponsable, string codAsignadoPor,
            DateTime fechaEntrega, IList<EntAgendaEntregable> entregables, string usuario)
        {
            XElement xml = new XElement("es",
                (entregables ?? new List<EntAgendaEntregable>()).Select(e => new XElement("e",
                    new XAttribute("id", e.IdEntregable),
                    new XAttribute("o", e.Orden),
                    e.Descripcion ?? "")));

            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaGuardarActividad"))
            {
                cmd.Parameters.Add("@IdActividad", SqlDbType.Int).Value = idActividad;
                cmd.Parameters.Add("@Titulo", SqlDbType.NVarChar, 200).Value = titulo;
                cmd.Parameters.Add("@CodResponsable", SqlDbType.VarChar, 20).Value = codResponsable;
                cmd.Parameters.Add("@CodAsignadoPor", SqlDbType.VarChar, 20).Value = codAsignadoPor;
                cmd.Parameters.Add("@FechaEntrega", SqlDbType.Date).Value = fechaEntrega.Date;
                cmd.Parameters.Add("@Entregables", SqlDbType.Xml).Value = xml.ToString(SaveOptions.DisableFormatting);
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        public static void MarcarEntregable(int idEntregable, bool hecho, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaMarcarEntregable"))
            {
                cmd.Parameters.Add("@IdEntregable", SqlDbType.Int).Value = idEntregable;
                cmd.Parameters.Add("@Hecho", SqlDbType.Bit).Value = hecho;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void MarcarCompleta(int idActividad, bool completa, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaMarcarCompleta"))
            {
                cmd.Parameters.Add("@IdActividad", SqlDbType.Int).Value = idActividad;
                cmd.Parameters.Add("@Completa", SqlDbType.Bit).Value = completa;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void Anular(int idActividad, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaAnular"))
            {
                cmd.Parameters.Add("@IdActividad", SqlDbType.Int).Value = idActividad;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void RegistrarCorreo(int idActividad, string destinatario, string asunto, bool enviado, string error)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaRegistrarCorreo"))
            {
                cmd.Parameters.Add("@IdActividad", SqlDbType.Int).Value = idActividad;
                cmd.Parameters.Add("@Destinatario", SqlDbType.VarChar, 200).Value = (object)destinatario ?? DBNull.Value;
                cmd.Parameters.Add("@Asunto", SqlDbType.NVarChar, 300).Value = asunto ?? "";
                cmd.Parameters.Add("@Enviado", SqlDbType.Bit).Value = enviado;
                cmd.Parameters.Add("@Error", SqlDbType.NVarChar, 1000).Value = (object)error ?? DBNull.Value;
                cnx.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
