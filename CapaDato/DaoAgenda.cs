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

        private static bool TieneColumna(IDataRecord dr, string columna)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (string.Equals(dr.GetName(i), columna, StringComparison.OrdinalIgnoreCase)) { return true; }
            }
            return false;
        }

        /// <summary>Columna que los procedimientos de la Entrega 1 no traen: si falta, "".</summary>
        private static string TextoOpcional(IDataRecord dr, string columna)
        {
            return TieneColumna(dr, columna) ? Texto(dr, columna) : "";
        }

        /// <summary>Columna que puede faltar o venir NULL: null en ambos casos.</summary>
        private static int? EnteroNulo(IDataRecord dr, string columna)
        {
            if (!TieneColumna(dr, columna)) { return null; }
            object v = dr[columna];
            return v == DBNull.Value ? (int?)null : Convert.ToInt32(v, CultureInfo.InvariantCulture);
        }

        private static DateTime? FechaNula(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(v, CultureInfo.InvariantCulture);
        }

        private static string XmlPasos(IEnumerable<XElement> pasos)
        {
            return new XElement("ps", pasos).ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>Un solo resultado con una columna entera (Resultado).</summary>
        private static int Resultado(SqlCommand cmd, string columna)
        {
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (!dr.Read()) { throw new InvalidOperationException(cmd.CommandText + " no devolvió " + columna + "."); }
                return Convert.ToInt32(dr[columna], CultureInfo.InvariantCulture);
            }
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
                        Anulada = Bit(dr, "Anulada"),
                        IdProcesoPaso = EnteroNulo(dr, "IdProcesoPaso"),
                        IdProceso = EnteroNulo(dr, "IdProceso"),
                        NombreProceso = TextoOpcional(dr, "NombreProceso"),
                        EstadoPaso = TextoOpcional(dr, "EstadoPaso")
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

        public static EntAgendaAvance MarcarEntregable(int idEntregable, bool hecho, string usuario, DateTime? fechaSiguiente = null)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaMarcarEntregable"))
            {
                cmd.Parameters.Add("@IdEntregable", SqlDbType.Int).Value = idEntregable;
                cmd.Parameters.Add("@Hecho", SqlDbType.Bit).Value = hecho;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                if (fechaSiguiente.HasValue)
                {
                    cmd.Parameters.Add("@FechaEntregaSiguiente", SqlDbType.Date).Value = fechaSiguiente.Value.Date;
                }
                cnx.Open();
                return LeerAvance(cmd);
            }
        }

        public static EntAgendaAvance MarcarCompleta(int idActividad, bool completa, string usuario, DateTime? fechaSiguiente = null)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaMarcarCompleta"))
            {
                cmd.Parameters.Add("@IdActividad", SqlDbType.Int).Value = idActividad;
                cmd.Parameters.Add("@Completa", SqlDbType.Bit).Value = completa;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                if (fechaSiguiente.HasValue)
                {
                    cmd.Parameters.Add("@FechaEntregaSiguiente", SqlDbType.Date).Value = fechaSiguiente.Value.Date;
                }
                cnx.Open();
                return LeerAvance(cmd);
            }
        }

        /// <summary>
        /// Fila (Respuestas, IdProcesoPasoActivado, IdProcesoTerminado). Contra el procedimiento
        /// anterior (sin resultado) devuelve Respuestas 0 y sin avance.
        /// </summary>
        private static EntAgendaAvance LeerAvance(SqlCommand cmd)
        {
            EntAgendaAvance av = new EntAgendaAvance();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    av.Respuestas = TieneColumna(dr, "Respuestas") && dr["Respuestas"] != DBNull.Value
                        ? Convert.ToInt32(dr["Respuestas"], CultureInfo.InvariantCulture) : 0;
                    av.IdProcesoPasoActivado = EnteroNulo(dr, "IdProcesoPasoActivado");
                    av.IdProcesoTerminado = EnteroNulo(dr, "IdProcesoTerminado");
                }
            }
            return av;
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

        public static void RegistrarCorreo(int? idActividad, string destinatario, string asunto, bool enviado, string error, int? idProceso = null)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaRegistrarCorreo"))
            {
                cmd.Parameters.Add("@IdActividad", SqlDbType.Int).Value = (object)idActividad ?? DBNull.Value;
                cmd.Parameters.Add("@Destinatario", SqlDbType.VarChar, 200).Value = (object)destinatario ?? DBNull.Value;
                cmd.Parameters.Add("@Asunto", SqlDbType.NVarChar, 300).Value = asunto ?? "";
                cmd.Parameters.Add("@Enviado", SqlDbType.Bit).Value = enviado;
                cmd.Parameters.Add("@Error", SqlDbType.NVarChar, 1000).Value = (object)error ?? DBNull.Value;
                cmd.Parameters.Add("@IdProceso", SqlDbType.Int).Value = (object)idProceso ?? DBNull.Value;
                cnx.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static HashSet<DateTime> Feriados()
        {
            HashSet<DateTime> fechas = new HashSet<DateTime>();
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaFeriados"))
            {
                cnx.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        fechas.Add(Convert.ToDateTime(dr["Fecha"], CultureInfo.InvariantCulture).Date);
                    }
                }
            }
            return fechas;
        }

        public static List<EntAgendaPlantilla> Plantillas()
        {
            List<EntAgendaPlantilla> lista = new List<EntAgendaPlantilla>();
            Dictionary<int, EntAgendaPlantilla> porId = new Dictionary<int, EntAgendaPlantilla>();
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaPlantillas"))
            {
                cnx.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        EntAgendaPlantilla t = new EntAgendaPlantilla
                        {
                            IdPlantilla = Convert.ToInt32(dr["IdPlantilla"], CultureInfo.InvariantCulture),
                            Nombre = Texto(dr, "Nombre"),
                            Descripcion = Texto(dr, "Descripcion"),
                            Activa = Bit(dr, "Activa"),
                            TieneProcesos = Bit(dr, "TieneProcesos")
                        };
                        lista.Add(t);
                        porId[t.IdPlantilla] = t;
                    }
                    if (!dr.NextResult())
                    {
                        throw new InvalidOperationException("Sp_RTA_AgendaPlantillas no devolvió los pasos.");
                    }
                    while (dr.Read())
                    {
                        EntAgendaPlantilla t;
                        if (!porId.TryGetValue(Convert.ToInt32(dr["IdPlantilla"], CultureInfo.InvariantCulture), out t)) { continue; }
                        t.Pasos.Add(new EntAgendaPlantillaPaso
                        {
                            IdPlantillaPaso = Convert.ToInt32(dr["IdPlantillaPaso"], CultureInfo.InvariantCulture),
                            Orden = Convert.ToInt32(dr["Orden"], CultureInfo.InvariantCulture),
                            IdArea = Convert.ToInt32(dr["IdArea"], CultureInfo.InvariantCulture),
                            Area = Texto(dr, "Area"),
                            Titulo = Texto(dr, "Titulo"),
                            PlazoDias = Convert.ToInt32(dr["PlazoDias"], CultureInfo.InvariantCulture),
                            Entregables = Texto(dr, "Entregables")
                        });
                    }
                }
            }
            return lista;
        }

        /// <summary>Resultado = IdPlantilla, o -1 nombre repetido, -2 datos invalidos, -3 no existe.</summary>
        public static int GuardarPlantilla(EntAgendaPlantilla t, string usuario)
        {
            string xml = XmlPasos((t.Pasos ?? new List<EntAgendaPlantillaPaso>()).Select(p => new XElement("p",
                new XAttribute("o", p.Orden),
                new XAttribute("a", p.IdArea),
                new XAttribute("t", p.Titulo ?? ""),
                new XAttribute("d", p.PlazoDias),
                new XAttribute("e", p.Entregables ?? ""))));
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaGuardarPlantilla"))
            {
                cmd.Parameters.Add("@IdPlantilla", SqlDbType.Int).Value = t.IdPlantilla;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 150).Value = t.Nombre ?? "";
                cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value = t.Descripcion ?? "";
                cmd.Parameters.Add("@Activa", SqlDbType.Bit).Value = t.Activa;
                cmd.Parameters.Add("@Pasos", SqlDbType.Xml).Value = xml;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                return Resultado(cmd, "Resultado");
            }
        }

        public static List<EntAgendaProceso> Procesos()
        {
            List<EntAgendaProceso> lista = new List<EntAgendaProceso>();
            Dictionary<int, EntAgendaProceso> porId = new Dictionary<int, EntAgendaProceso>();
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaProcesos"))
            {
                cnx.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        EntAgendaProceso p = new EntAgendaProceso
                        {
                            IdProceso = Convert.ToInt32(dr["IdProceso"], CultureInfo.InvariantCulture),
                            Nombre = Texto(dr, "Nombre"),
                            IdPlantilla = Convert.ToInt32(dr["IdPlantilla"], CultureInfo.InvariantCulture),
                            Plantilla = Texto(dr, "Plantilla"),
                            CodIniciadoPor = Texto(dr, "CodIniciadoPor"),
                            Estado = Texto(dr, "Estado"),
                            FechaInicio = Convert.ToDateTime(dr["FechaInicio"], CultureInfo.InvariantCulture),
                            FechaFin = FechaNula(dr, "FechaFin"),
                            MotivoCancelacion = Texto(dr, "MotivoCancelacion")
                        };
                        lista.Add(p);
                        porId[p.IdProceso] = p;
                    }
                    if (!dr.NextResult())
                    {
                        throw new InvalidOperationException("Sp_RTA_AgendaProcesos no devolvió los pasos.");
                    }
                    while (dr.Read())
                    {
                        EntAgendaProceso p;
                        if (!porId.TryGetValue(Convert.ToInt32(dr["IdProceso"], CultureInfo.InvariantCulture), out p)) { continue; }
                        p.Pasos.Add(new EntAgendaProcesoPaso
                        {
                            IdProcesoPaso = Convert.ToInt32(dr["IdProcesoPaso"], CultureInfo.InvariantCulture),
                            IdProceso = p.IdProceso,
                            Orden = Convert.ToInt32(dr["Orden"], CultureInfo.InvariantCulture),
                            IdArea = Convert.ToInt32(dr["IdArea"], CultureInfo.InvariantCulture),
                            Area = Texto(dr, "Area"),
                            Color = Texto(dr, "Color"),
                            Titulo = Texto(dr, "Titulo"),
                            PlazoDias = Convert.ToInt32(dr["PlazoDias"], CultureInfo.InvariantCulture),
                            Entregables = Texto(dr, "Entregables"),
                            CodResponsable = Texto(dr, "CodResponsable"),
                            Estado = Texto(dr, "Estado"),
                            FechaActivacion = FechaNula(dr, "FechaActivacion"),
                            FechaCompletado = FechaNula(dr, "FechaCompletado"),
                            IdActividad = EnteroNulo(dr, "IdActividad"),
                            FechaEntrega = FechaNula(dr, "FechaEntrega"),
                            ActividadCompleta = Bit(dr, "ActividadCompleta")
                        });
                    }
                }
            }
            return lista;
        }

        /// <summary>
        /// Devuelve IdProceso (o -2 datos invalidos, -3 plantilla inexistente o inactiva);
        /// idPasoActivado es el paso 1 activado, o null si no se inicio.
        /// </summary>
        public static int IniciarProceso(string nombre, int idPlantilla, IList<EntAgendaProcesoPaso> pasos,
            DateTime fechaPrimero, string usuario, out int? idPasoActivado)
        {
            string xml = XmlPasos((pasos ?? new List<EntAgendaProcesoPaso>()).Select(p => new XElement("p",
                new XAttribute("o", p.Orden),
                new XAttribute("a", p.IdArea),
                new XAttribute("t", p.Titulo ?? ""),
                new XAttribute("d", p.PlazoDias),
                new XAttribute("e", p.Entregables ?? ""),
                new XAttribute("r", p.CodResponsable ?? ""))));
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaIniciarProceso"))
            {
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 200).Value = nombre ?? "";
                cmd.Parameters.Add("@IdPlantilla", SqlDbType.Int).Value = idPlantilla;
                cmd.Parameters.Add("@Pasos", SqlDbType.Xml).Value = xml;
                cmd.Parameters.Add("@FechaEntregaPrimero", SqlDbType.Date).Value = fechaPrimero.Date;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) { throw new InvalidOperationException("Sp_RTA_AgendaIniciarProceso no devolvió IdProceso."); }
                    idPasoActivado = EnteroNulo(dr, "IdProcesoPasoActivado");
                    return Convert.ToInt32(dr["IdProceso"], CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>0, -1 el proceso o el paso cambio, -2 sin responsable.</summary>
        public static int ReasignarPaso(int idPaso, string estadoEsperado, string codResponsable, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaReasignarPaso"))
            {
                cmd.Parameters.Add("@IdProcesoPaso", SqlDbType.Int).Value = idPaso;
                cmd.Parameters.Add("@EstadoEsperado", SqlDbType.VarChar, 20).Value = estadoEsperado ?? "";
                cmd.Parameters.Add("@CodResponsable", SqlDbType.VarChar, 20).Value = codResponsable ?? "";
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                return Resultado(cmd, "Resultado");
            }
        }

        /// <summary>0, -1 el paso ya no esta Pendiente, -2 datos invalidos.</summary>
        public static int CambiarPasoPendiente(EntAgendaProcesoPaso paso, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaCambiarPasoPendiente"))
            {
                cmd.Parameters.Add("@IdProcesoPaso", SqlDbType.Int).Value = paso.IdProcesoPaso;
                cmd.Parameters.Add("@Titulo", SqlDbType.NVarChar, 200).Value = paso.Titulo ?? "";
                cmd.Parameters.Add("@PlazoDias", SqlDbType.Int).Value = paso.PlazoDias;
                cmd.Parameters.Add("@Entregables", SqlDbType.NVarChar, 2000).Value = paso.Entregables ?? "";
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                return Resultado(cmd, "Resultado");
            }
        }

        /// <summary>0, -1 el paso ya no esta EnCurso, -2 sin fecha.</summary>
        public static int CambiarFechaPaso(int idPaso, DateTime fecha, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaCambiarFechaPaso"))
            {
                cmd.Parameters.Add("@IdProcesoPaso", SqlDbType.Int).Value = idPaso;
                cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = fecha.Date;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                return Resultado(cmd, "Resultado");
            }
        }

        /// <summary>0, -1 el proceso no esta EnCurso.</summary>
        public static int CancelarProceso(int idProceso, string motivo, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_AgendaCancelarProceso"))
            {
                cmd.Parameters.Add("@IdProceso", SqlDbType.Int).Value = idProceso;
                cmd.Parameters.Add("@Motivo", SqlDbType.NVarChar, 400).Value = motivo ?? "";
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario;
                cnx.Open();
                return Resultado(cmd, "Resultado");
            }
        }
    }
}
