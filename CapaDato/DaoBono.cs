using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace CapaDato
{
    /// <summary>
    /// Llamadas a los Sp_RTA_Bono* (docs/sql/2026-10-22-bonos-esquema.sql).
    /// No decide nada: los codigos de Respuestas se devuelven tal cual. Las
    /// consultas de bonos devuelven dos resultados; si falta el de los lunes se
    /// lanza, porque un bono "sin semanas" escondería un doble pago.
    /// </summary>
    public class DaoBono
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

        private static int Entero(IDataRecord dr, string columna)
        {
            return Convert.ToInt32(dr[columna], CultureInfo.InvariantCulture);
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

        private static object Nulo(string s)
        {
            return s == null ? (object)DBNull.Value : s;
        }

        private static object Nulo(decimal? d)
        {
            return d.HasValue ? (object)d.Value : DBNull.Value;
        }

        private static int Respuesta(SqlCommand cmd)
        {
            return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
        }

        public static List<EntBonoPersona> PersonasActivas()
        {
            List<EntBonoPersona> lista = new List<EntBonoPersona>();
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_BonoPersonasActivas"))
            {
                cnx.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntBonoPersona
                        {
                            CodUsuario = Texto(dr, "CodUsuario"),
                            Nombre = Texto(dr, "Nombre"),
                            Correo = Texto(dr, "Correo"),
                            CodJefe = Texto(dr, "CodJefe"),
                            Empresa = Texto(dr, "Empresa"),
                            CodSap = Texto(dr, "CodSap"),
                            Cedula = Texto(dr, "Cedula"),
                            IdPerfil = EnteroNulo(dr, "IdPerfil") ?? 0
                        });
                    }
                }
            }
            return lista;
        }

        /// <summary>Todo el catalogo. Las claves de Conceptos van normalizadas (DOS / AGILITY).</summary>
        public static List<EntBonoTipo> Tipos()
        {
            List<EntBonoTipo> lista = new List<EntBonoTipo>();
            Dictionary<int, EntBonoTipo> porId = new Dictionary<int, EntBonoTipo>();
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_BonoTipos"))
            {
                cnx.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        EntBonoTipo t = new EntBonoTipo
                        {
                            IdTipo = Entero(dr, "IdTipo"),
                            Nombre = Texto(dr, "Nombre"),
                            Familia = Texto(dr, "Familia"),
                            ValorSugerido = DecimalNulo(dr, "ValorSugerido"),
                            PorSemana = Bit(dr, "PorSemana"),
                            Activo = Bit(dr, "Activo")
                        };
                        lista.Add(t);
                        porId[t.IdTipo] = t;
                    }
                    if (!dr.NextResult())
                    {
                        throw new InvalidOperationException("Sp_RTA_BonoTipos no devolvió los conceptos SAP.");
                    }
                    while (dr.Read())
                    {
                        EntBonoTipo t;
                        if (!porId.TryGetValue(Entero(dr, "IdTipo"), out t)) { continue; }
                        // Misma normalizacion que NegBonoReglas.EmpresaNormalizada (CapaDato no llama a CapaNegocio).
                        string empresa = Texto(dr, "Empresa").ToUpperInvariant();
                        string codigo = Texto(dr, "CodigoConcepto");
                        if (empresa.Length > 0 && codigo.Length > 0) { t.Conceptos[empresa] = codigo; }
                    }
                }
            }
            return lista;
        }

        /// <summary>Respuestas = IdTipo, -1 nombre repetido, -2 el tipo no existe.</summary>
        public static int GuardarTipo(int idTipo, string nombre, string familia, decimal? valorSugerido, bool porSemana,
            bool activo, string conceptoDos, string conceptoAgility, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_BonoGuardarTipo"))
            {
                cmd.Parameters.Add("@IdTipo", SqlDbType.Int).Value = idTipo;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 150).Value = Nulo(nombre);
                cmd.Parameters.Add("@Familia", SqlDbType.VarChar, 20).Value = Nulo(familia);
                SqlParameter sugerido = cmd.Parameters.Add("@ValorSugerido", SqlDbType.Decimal);
                sugerido.Precision = 9; sugerido.Scale = 2; sugerido.Value = Nulo(valorSugerido);
                cmd.Parameters.Add("@PorSemana", SqlDbType.Bit).Value = porSemana;
                cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = activo;
                cmd.Parameters.Add("@ConceptoDos", SqlDbType.VarChar, 10).Value = Nulo(conceptoDos);
                cmd.Parameters.Add("@ConceptoAgility", SqlDbType.VarChar, 10).Value = Nulo(conceptoAgility);
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = Nulo(usuario);
                cnx.Open();
                return Respuesta(cmd);
            }
        }

        /// <summary>Todos los periodos, del mas reciente al mas viejo.</summary>
        public static List<EntBonoPeriodo> Periodos()
        {
            List<EntBonoPeriodo> lista = new List<EntBonoPeriodo>();
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_BonoPeriodos"))
            {
                cnx.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntBonoPeriodo
                        {
                            IdPeriodo = Entero(dr, "IdPeriodo"),
                            Anio = Entero(dr, "Anio"),
                            Mes = Entero(dr, "Mes"),
                            Estado = Texto(dr, "Estado"),
                            TotalHistorico = DecimalNulo(dr, "TotalHistorico"),
                            FechaApertura = FechaNula(dr, "FechaApertura"),
                            FechaCierre = FechaNula(dr, "FechaCierre"),
                            TotalAprobado = DecimalNulo(dr, "TotalAprobado") ?? 0m
                        });
                    }
                }
            }
            return lista;
        }

        /// <summary>Respuestas = IdPeriodo nuevo, -1 ya hay uno abierto.</summary>
        public static int AbrirPeriodo(string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_BonoAbrirPeriodo"))
            {
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = Nulo(usuario);
                cnx.Open();
                return Respuesta(cmd);
            }
        }

        /// <summary>Respuestas = 0 cerrado, -1 no esta abierto, -2 falta algun concepto SAP.</summary>
        public static int CerrarPeriodo(int idPeriodo, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_BonoCerrarPeriodo"))
            {
                cmd.Parameters.Add("@IdPeriodo", SqlDbType.Int).Value = idPeriodo;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = Nulo(usuario);
                cnx.Open();
                return Respuesta(cmd);
            }
        }

        /// <summary>null = el periodo abierto (si hay) mas los pendientes sin periodo.</summary>
        public static List<EntBono> Detalles(int? idPeriodo)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_BonoDetalles"))
            {
                cmd.Parameters.Add("@IdPeriodo", SqlDbType.Int).Value = idPeriodo.HasValue ? (object)idPeriodo.Value : DBNull.Value;
                cnx.Open();
                return LeerBonos(cmd, "Sp_RTA_BonoDetalles");
            }
        }

        /// <summary>Los bonos no rechazados de la persona, de cualquier periodo.</summary>
        public static List<EntBono> DePersona(string codUsuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_BonoDePersona"))
            {
                cmd.Parameters.Add("@CodUsuario", SqlDbType.VarChar, 50).Value = Nulo(codUsuario);
                cnx.Open();
                return LeerBonos(cmd, "Sp_RTA_BonoDePersona");
            }
        }

        private static List<EntBono> LeerBonos(SqlCommand cmd, string procedimiento)
        {
            List<EntBono> lista = new List<EntBono>();
            Dictionary<int, EntBono> porId = new Dictionary<int, EntBono>();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    EntBono b = new EntBono
                    {
                        IdBono = Entero(dr, "IdBono"),
                        IdPeriodo = EnteroNulo(dr, "IdPeriodo"),
                        CodUsuario = Texto(dr, "CodUsuario"),
                        Empresa = Texto(dr, "Empresa"),
                        Nombre = Texto(dr, "Nombre"),
                        Identificacion = Texto(dr, "Identificacion"),
                        Lugar = Texto(dr, "Lugar"),
                        IdTipo = Entero(dr, "IdTipo"),
                        Semanas = Entero(dr, "Semanas"),
                        ValorSugerido = DecimalNulo(dr, "ValorSugerido"),
                        Valor = DecimalNulo(dr, "Valor") ?? 0m,
                        MotivoAjuste = Texto(dr, "MotivoAjuste"),
                        Observacion = Texto(dr, "Observacion"),
                        Estado = Texto(dr, "Estado"),
                        Origen = Texto(dr, "Origen"),
                        CodJefe = Texto(dr, "CodJefe"),
                        MotivoRechazo = Texto(dr, "MotivoRechazo"),
                        NombreTipo = Texto(dr, "NombreTipo"),
                        CodigoConcepto = Texto(dr, "CodigoConcepto")
                    };
                    lista.Add(b);
                    porId[b.IdBono] = b;
                }
                if (!dr.NextResult())
                {
                    throw new InvalidOperationException(procedimiento + " no devolvió las semanas.");
                }
                while (dr.Read())
                {
                    EntBono b;
                    if (!porId.TryGetValue(Entero(dr, "IdBono"), out b)) { continue; }
                    b.Lunes.Add(Convert.ToDateTime(dr["Lunes"], CultureInfo.InvariantCulture).Date);
                }
            }
            return lista;
        }

        /// <summary>
        /// Crea (IdBono = 0) o edita. Respuestas = IdBono, -1 cambio de estado,
        /// -2 no hay periodo abierto, -3 doble pago. Los lunes viajan como
        /// 'yyyy-MM-dd' separados por coma.
        /// </summary>
        public static int Guardar(EntBono b, string estadoEsperado, string estadoNuevo, string usuario)
        {
            string lunes = string.Join(",", (b.Lunes ?? new List<DateTime>())
                .Select(l => l.Date).Distinct().OrderBy(l => l)
                .Select(l => l.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));

            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_BonoGuardar"))
            {
                cmd.Parameters.Add("@IdBono", SqlDbType.Int).Value = b.IdBono;
                cmd.Parameters.Add("@CodUsuario", SqlDbType.VarChar, 50).Value = Nulo(b.CodUsuario);
                cmd.Parameters.Add("@Empresa", SqlDbType.VarChar, 10).Value = Nulo(b.Empresa);
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 150).Value = Nulo(b.Nombre);
                cmd.Parameters.Add("@Identificacion", SqlDbType.VarChar, 32).Value = Nulo(b.Identificacion);
                cmd.Parameters.Add("@Lugar", SqlDbType.NVarChar, 100).Value = Nulo(b.Lugar);
                cmd.Parameters.Add("@IdTipo", SqlDbType.Int).Value = b.IdTipo;
                cmd.Parameters.Add("@Semanas", SqlDbType.Int).Value = b.Semanas;
                SqlParameter sugerido = cmd.Parameters.Add("@ValorSugerido", SqlDbType.Decimal);
                sugerido.Precision = 9; sugerido.Scale = 2; sugerido.Value = Nulo(b.ValorSugerido);
                SqlParameter valor = cmd.Parameters.Add("@Valor", SqlDbType.Decimal);
                valor.Precision = 9; valor.Scale = 2; valor.Value = b.Valor;
                cmd.Parameters.Add("@MotivoAjuste", SqlDbType.NVarChar, 500).Value = Nulo(b.MotivoAjuste);
                cmd.Parameters.Add("@Observacion", SqlDbType.NVarChar, 500).Value = Nulo(b.Observacion);
                cmd.Parameters.Add("@Origen", SqlDbType.VarChar, 15).Value = Nulo(b.Origen);
                cmd.Parameters.Add("@CodJefe", SqlDbType.VarChar, 50).Value = Nulo(b.CodJefe);
                cmd.Parameters.Add("@Lunes", SqlDbType.VarChar, -1).Value = lunes;
                cmd.Parameters.Add("@EstadoEsperado", SqlDbType.VarChar, 10).Value = Nulo(estadoEsperado);
                cmd.Parameters.Add("@EstadoNuevo", SqlDbType.VarChar, 10).Value = Nulo(estadoNuevo);
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = Nulo(usuario);
                cnx.Open();
                return Respuesta(cmd);
            }
        }

        /// <summary>
        /// Enviar, aprobar, ajustar o rechazar. valor / motivo / snapshot* null = no cambia.
        /// Respuestas = 0, -1 cambio de estado, -2 su periodo no esta abierto, -3 doble pago.
        /// </summary>
        public static int CambiarEstado(int idBono, string estadoEsperado, string estadoNuevo, decimal? valor, string motivo,
            string snapshotEmpresa, string snapshotNombre, string snapshotIdentificacion, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_BonoCambiarEstado"))
            {
                cmd.Parameters.Add("@IdBono", SqlDbType.Int).Value = idBono;
                cmd.Parameters.Add("@EstadoEsperado", SqlDbType.VarChar, 10).Value = Nulo(estadoEsperado);
                cmd.Parameters.Add("@EstadoNuevo", SqlDbType.VarChar, 10).Value = Nulo(estadoNuevo);
                SqlParameter v = cmd.Parameters.Add("@Valor", SqlDbType.Decimal);
                v.Precision = 9; v.Scale = 2; v.Value = Nulo(valor);
                cmd.Parameters.Add("@Motivo", SqlDbType.NVarChar, 500).Value = Nulo(motivo);
                cmd.Parameters.Add("@SnapshotEmpresa", SqlDbType.VarChar, 10).Value = Nulo(snapshotEmpresa);
                cmd.Parameters.Add("@SnapshotNombre", SqlDbType.NVarChar, 150).Value = Nulo(snapshotNombre);
                cmd.Parameters.Add("@SnapshotIdentificacion", SqlDbType.VarChar, 32).Value = Nulo(snapshotIdentificacion);
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = Nulo(usuario);
                cnx.Open();
                return Respuesta(cmd);
            }
        }

        /// <summary>Respuestas = 0, -1 cambio de estado, -2 es de un periodo que no esta abierto.</summary>
        public static int Borrar(int idBono, string estadoEsperado, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_BonoBorrar"))
            {
                cmd.Parameters.Add("@IdBono", SqlDbType.Int).Value = idBono;
                cmd.Parameters.Add("@EstadoEsperado", SqlDbType.VarChar, 10).Value = Nulo(estadoEsperado);
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = Nulo(usuario);
                cnx.Open();
                return Respuesta(cmd);
            }
        }

        /// <summary>Fila suelta de Bono_Bitacora (correos y notas de la capa de negocio).</summary>
        public static void Bitacora(int? idBono, int? idPeriodo, string accion, string valorAnterior, string valorNuevo,
            string detalle, string usuario)
        {
            using (SqlConnection cnx = Conexion())
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_BonoBitacora"))
            {
                cmd.Parameters.Add("@IdBono", SqlDbType.Int).Value = idBono.HasValue ? (object)idBono.Value : DBNull.Value;
                cmd.Parameters.Add("@IdPeriodo", SqlDbType.Int).Value = idPeriodo.HasValue ? (object)idPeriodo.Value : DBNull.Value;
                cmd.Parameters.Add("@Accion", SqlDbType.VarChar, 20).Value = Nulo(accion);
                cmd.Parameters.Add("@ValorAnterior", SqlDbType.NVarChar, 200).Value = Nulo(valorAnterior);
                cmd.Parameters.Add("@ValorNuevo", SqlDbType.NVarChar, 200).Value = Nulo(valorNuevo);
                cmd.Parameters.Add("@Detalle", SqlDbType.NVarChar, -1).Value = Nulo(detalle);
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = Nulo(usuario);
                cnx.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
