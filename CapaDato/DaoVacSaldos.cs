using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace CapaDato
{
    /// <summary>
    /// Lecturas de la pantalla de saldos de vacaciones. El calculo es todo de
    /// Sp_RTA_VacCalcular (docs/sql/2026-10-28-vacaciones-motor.sql); aqui solo
    /// se lee. El maestro y lo cargado de cada movimiento salen de
    /// docs/sql/2026-10-28-vacaciones-saldos-pantalla.sql.
    /// </summary>
    public class DaoVacSaldos
    {
        public static EntVacDatos Tablero(DateTime fechaCalculo)
        {
            EntVacDatos d = new EntVacDatos();
            using (SqlConnection cnx = Conexion())
            {
                cnx.Open();
                d.Colaboradores = Colaboradores(cnx, null);
                d.Saldos = Saldos(cnx, fechaCalculo, null);
            }
            return d;
        }

        /// <summary>Null si la persona no esta en Vac_Colaborador.</summary>
        public static EntVacFicha Ficha(DateTime fechaCalculo, int idColaborador)
        {
            using (SqlConnection cnx = Conexion())
            {
                cnx.Open();
                List<EntVacColaborador> col = Colaboradores(cnx, idColaborador);
                if (col.Count == 0) { return null; }

                List<EntVacSaldo> saldo = Saldos(cnx, fechaCalculo, idColaborador);
                EntVacFicha f = new EntVacFicha
                {
                    FechaCalculo = fechaCalculo.Date,
                    Colaborador = col[0],
                    Saldo = saldo.Count > 0 ? saldo[0] : null,
                    Periodos = Periodos(cnx, fechaCalculo, idColaborador),
                    Movimientos = Movimientos(cnx, fechaCalculo, idColaborador)
                };
                CompletarOrigen(cnx, idColaborador, f.Movimientos);
                return f;
            }
        }

        // ------------------------------------------------------- lecturas

        private static List<EntVacColaborador> Colaboradores(SqlConnection cnx, int? idColaborador)
        {
            List<EntVacColaborador> lista = new List<EntVacColaborador>();
            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_VacColaboradores"))
            {
                cmd.Parameters.Add("@IdColaborador", SqlDbType.Int).Value = Nulo(idColaborador);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntVacColaborador
                        {
                            IdColaborador = Entero(dr, "IdColaborador"),
                            Cedula = Texto(dr, "Cedula"),
                            Nombre = Texto(dr, "Nombre"),
                            Empresa = Texto(dr, "Empresa"),
                            Area = Texto(dr, "Area"),
                            Cargo = Texto(dr, "Cargo"),
                            FechaIngreso = Convert.ToDateTime(dr["FechaIngreso"], CultureInfo.InvariantCulture),
                            Estado = Texto(dr, "Estado"),
                            FechaSalida = FechaNula(dr, "FechaSalida"),
                            FechaCorte = FechaNula(dr, "FechaCorte"),
                            SaldoCorte = DecimalNulo(dr, "SaldoCorte"),
                            Observacion = Texto(dr, "Observacion")
                        });
                    }
                }
            }
            return lista;
        }

        private static List<EntVacSaldo> Saldos(SqlConnection cnx, DateTime fechaCalculo, int? idColaborador)
        {
            List<EntVacSaldo> lista = new List<EntVacSaldo>();
            using (SqlCommand cmd = Motor(cnx, fechaCalculo, idColaborador, "SALDOS"))
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    lista.Add(new EntVacSaldo
                    {
                        IdColaborador = Entero(dr, "IdColaborador"),
                        FechaCalculo = Convert.ToDateTime(dr["FechaCalculo"], CultureInfo.InvariantCulture),
                        Cumplidos = Entero(dr, "Cumplidos"),
                        DiasPorAnio = Numero(dr, "DiasPorAnio"),
                        InicioPeriodo = Convert.ToDateTime(dr["InicioPeriodo"], CultureInfo.InvariantCulture),
                        ProximoAniversario = Convert.ToDateTime(dr["ProximoAniversario"], CultureInfo.InvariantCulture),
                        AjusteCorte = Numero(dr, "AjusteCorte"),
                        Disponible = Numero(dr, "Disponible"),
                        UsadoEnCurso = Numero(dr, "UsadoEnCurso"),
                        Proporcional = Numero(dr, "Proporcional"),
                        Saldo = Numero(dr, "Saldo"),
                        PorVencer = Numero(dr, "PorVencer"),
                        ProximoVencimiento = FechaNula(dr, "ProximoVencimiento"),
                        Vencidos = Numero(dr, "Vencidos"),
                        TotalGozado = Numero(dr, "TotalGozado"),
                        Estado = Texto(dr, "Estado")
                    });
                }
            }
            return lista;
        }

        private static List<EntVacPeriodo> Periodos(SqlConnection cnx, DateTime fechaCalculo, int idColaborador)
        {
            List<EntVacPeriodo> lista = new List<EntVacPeriodo>();
            using (SqlCommand cmd = Motor(cnx, fechaCalculo, idColaborador, "PERIODOS"))
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    lista.Add(new EntVacPeriodo
                    {
                        N = Entero(dr, "N"),
                        Periodo = Texto(dr, "Periodo"),
                        Inicio = Convert.ToDateTime(dr["Inicio"], CultureInfo.InvariantCulture),
                        Fin = Convert.ToDateTime(dr["Fin"], CultureInfo.InvariantCulture),
                        DiasGanados = Numero(dr, "DiasGanados"),
                        DiasTomados = Numero(dr, "DiasTomados"),
                        DiasVencidos = Numero(dr, "DiasVencidos"),
                        Saldo = Numero(dr, "Saldo"),
                        Vence = Convert.ToDateTime(dr["Vence"], CultureInfo.InvariantCulture),
                        Estado = Texto(dr, "Estado")
                    });
                }
            }
            return lista;
        }

        private static List<EntVacMovimiento> Movimientos(SqlConnection cnx, DateTime fechaCalculo, int idColaborador)
        {
            List<EntVacMovimiento> lista = new List<EntVacMovimiento>();
            using (SqlCommand cmd = Motor(cnx, fechaCalculo, idColaborador, "MOVIMIENTOS"))
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    lista.Add(new EntVacMovimiento
                    {
                        IdMovimiento = Entero(dr, "IdMovimiento"),
                        Tipo = Texto(dr, "Tipo"),
                        Estado = Texto(dr, "Estado"),
                        Desde = FechaNula(dr, "Desde"),
                        Hasta = FechaNula(dr, "Hasta"),
                        Situacion = Texto(dr, "Situacion"),
                        AnioServicio = EnteroNulo(dr, "AnioServicio"),
                        DiasBase = Numero(dr, "DiasBase"),
                        DiasFinDeSemana = Numero(dr, "DiasFinDeSemana"),
                        DiasDescuenta = Numero(dr, "DiasDescuenta"),
                        Cruces = Entero(dr, "Cruces")
                    });
                }
            }
            return lista;
        }

        private static void CompletarOrigen(SqlConnection cnx, int idColaborador, List<EntVacMovimiento> movimientos)
        {
            Dictionary<int, EntVacMovimiento> porId = new Dictionary<int, EntVacMovimiento>();
            foreach (EntVacMovimiento m in movimientos) { porId[m.IdMovimiento] = m; }

            using (SqlCommand cmd = Comando(cnx, "Sp_RTA_VacMovimientosOrigen"))
            {
                cmd.Parameters.Add("@IdColaborador", SqlDbType.Int).Value = idColaborador;
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        EntVacMovimiento m;
                        if (!porId.TryGetValue(Entero(dr, "IdMovimiento"), out m)) { continue; }
                        m.Dias = DecimalNulo(dr, "Dias");
                        m.Horas = DecimalNulo(dr, "Horas");
                        m.Minutos = DecimalNulo(dr, "Minutos");
                        m.BeneficioDos = dr["BeneficioDos"] != DBNull.Value && Convert.ToBoolean(dr["BeneficioDos"], CultureInfo.InvariantCulture);
                        m.Observacion = Texto(dr, "Observacion");
                        m.Origen = Texto(dr, "Origen");
                    }
                }
            }
        }

        // ------------------------------------------------------- utilidades

        private static SqlConnection Conexion()
        {
            return new DaoReporTareaAranda().conectar();
        }

        private static SqlCommand Comando(SqlConnection cnx, string procedimiento)
        {
            SqlCommand cmd = new SqlCommand(procedimiento, cnx);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;
            return cmd;
        }

        private static SqlCommand Motor(SqlConnection cnx, DateTime fechaCalculo, int? idColaborador, string salida)
        {
            SqlCommand cmd = Comando(cnx, "Sp_RTA_VacCalcular");
            cmd.Parameters.Add("@FechaCalculo", SqlDbType.Date).Value = fechaCalculo.Date;
            cmd.Parameters.Add("@IdColaborador", SqlDbType.Int).Value = Nulo(idColaborador);
            cmd.Parameters.Add("@Salida", SqlDbType.VarChar, 12).Value = salida;
            return cmd;
        }

        private static object Nulo(int? v)
        {
            return v.HasValue ? (object)v.Value : DBNull.Value;
        }

        private static string Texto(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? "" : Convert.ToString(v, CultureInfo.InvariantCulture).Trim();
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

        private static decimal Numero(IDataRecord dr, string columna)
        {
            object v = dr[columna];
            return v == DBNull.Value ? 0m : Convert.ToDecimal(v, CultureInfo.InvariantCulture);
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
