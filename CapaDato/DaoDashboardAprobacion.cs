using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Lee los cinco conjuntos del dashboard en UNA sola ida a la base.
    ///
    /// El orden de los NextResult() es el mismo en que el procedimiento declara
    /// los SELECT, y no hay forma de que el compilador lo verifique: si alguien
    /// agrega un conjunto en el medio, esta clase lee el equivocado sin dar
    /// error. Por eso el script tiene una comprobacion que cuenta los cinco.
    /// </summary>
    public class DaoDashboardAprobacion
    {
        public static EntDashboardAprobacion Cargar(string idUsuarioJefe, string fechaDesde, string fechaHasta)
        {
            EntDashboardAprobacion d = new EntDashboardAprobacion();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_DashboardAprobacionJefatura", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdUsuarioJefe", SqlDbType.VarChar, 10).Value = idUsuarioJefe ?? "";
                cmd.Parameters.Add("@FechaInicio", SqlDbType.VarChar, 20).Value = fechaDesde ?? "";
                cmd.Parameters.Add("@FechaFin", SqlDbType.VarChar, 20).Value = fechaHasta ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    /* 1. totales */
                    if (dr.Read())
                    {
                        d.Totales.MinutosAprobados  = Entero(dr, "MinutosAprobados");
                        d.Totales.MinutosPendientes = Entero(dr, "MinutosPendientes");
                        d.Totales.MinutosOtros      = Entero(dr, "MinutosOtros");
                        d.Totales.PersonasDiaTotal  = Entero(dr, "PersonasDiaTotal");
                        d.Totales.PersonasDiaAprob  = Entero(dr, "PersonasDiaAprob");
                        d.Totales.PersonasDiaPend   = Entero(dr, "PersonasDiaPend");
                        d.Totales.Responsables      = Entero(dr, "Responsables");
                    }

                    /* 2. por semana */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            d.Semanas.Add(new EntDashboardSemana
                            {
                                Semana            = Convert.ToDateTime(dr["Semana"]),
                                MinutosAprobados  = Entero(dr, "MinutosAprobados"),
                                MinutosPendientes = Entero(dr, "MinutosPendientes")
                            });
                        }
                    }

                    /* 3. por responsable */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            d.Responsables.Add(new EntDashboardResponsable
                            {
                                Nombre            = Texto(dr, "Nombre"),
                                MinutosAprobados  = Entero(dr, "MinutosAprobados"),
                                MinutosPendientes = Entero(dr, "MinutosPendientes"),
                                PersonasDia       = Entero(dr, "PersonasDia"),
                                DiasBajoJornada   = Entero(dr, "DiasBajoJornada")
                            });
                        }
                    }

                    /* 4. demora */
                    if (dr.NextResult() && dr.Read())
                    {
                        d.Demora.DiasPromedio          = Decimales(dr, "DiasPromedio");
                        d.Demora.DiasMaximo            = Entero(dr, "DiasMaximo");
                        d.Demora.AprobadasConFecha     = Entero(dr, "AprobadasConFecha");
                        d.Demora.AprobadasSinFecha     = Entero(dr, "AprobadasSinFecha");
                        d.Demora.DiasMasViejoPendiente = Entero(dr, "DiasMasViejoPendiente");
                    }

                    /* 5. por empresa */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            d.Empresas.Add(new EntDashboardEmpresa
                            {
                                Empresa = Texto(dr, "Empresa"),
                                Minutos = Entero(dr, "Minutos"),
                                MinutosAprobados = Entero(dr, "MinutosAprobados")
                            });
                        }
                    }

                    /* 6. persona x cliente.

                       Si el procedimiento todavia es el viejo, NextResult()
                       devuelve false y la lista queda vacia: la pantalla dice
                       "sin horas aprobadas" en vez de fallar. Ese es el sentido
                       seguro de la ventana entre el script y los binarios. */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            d.PersonaEmpresa.Add(new EntDashboardPersonaEmpresa
                            {
                                Id_Responsable = Texto(dr, "Id_Responsable"),
                                Nombre = Texto(dr, "Nombre"),
                                Empresa = Texto(dr, "Empresa"),
                                Minutos = Entero(dr, "MinutosAprobados")
                            });
                        }
                    }
                }
            }

            return d;
        }

        private static int Entero(SqlDataReader dr, string columna)
        {
            return dr[columna] == DBNull.Value ? 0 : Convert.ToInt32(dr[columna]);
        }

        /* DiasPromedio viene como DECIMAL: el procedimiento promedia en decimal
           porque AVG sobre INT trunca. Leerlo con Entero lo volveria a truncar
           aca y el arreglo del procedimiento no se notaria. */
        private static decimal Decimales(SqlDataReader dr, string columna)
        {
            return dr[columna] == DBNull.Value ? 0m : Convert.ToDecimal(dr[columna]);
        }

        private static string Texto(SqlDataReader dr, string columna)
        {
            return dr[columna] == DBNull.Value ? "" : dr[columna].ToString().Trim();
        }
    }
}
