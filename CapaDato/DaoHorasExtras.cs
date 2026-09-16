using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Acceso a datos del modulo de horas extras.
    ///
    /// Esta clase NO calcula: no hay un solo valor hora ni un solo total que
    /// salga de aqui. La formula vive una sola vez, en CapaNegocio. La
    /// dependencia va CapaNegocio -> CapaDato y nunca al reves, asi que llamar
    /// a NegHorasExtras desde aqui seria una referencia circular y no compila.
    /// Por eso CargarPeriodo devuelve los totales en cero: los suma quien puede.
    /// </summary>
    public class DaoHorasExtras
    {
        public static List<EntHePeriodo> ListarPeriodos()
        {
            List<EntHePeriodo> lista = new List<EntHePeriodo>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeListarPeriodos", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read()) { lista.Add(LeerPeriodo(dr)); }
                }
            }

            return lista;
        }

        public static int CrearPeriodo(int anio, int mes, string usuario, string ip, out int idPeriodo)
        {
            int resultado = -1;
            int id = 0;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeCrearPeriodo", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Anio", SqlDbType.Int).Value = anio;
                cmd.Parameters.Add("@Mes", SqlDbType.Int).Value = mes;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario ?? "";
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 64).Value = ip ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        resultado = Convert.ToInt32(dr["Respuestas"]);
                        id = Convert.ToInt32(dr["IdPeriodo"]);
                    }
                }
            }

            idPeriodo = id;
            return resultado;
        }

        /// <summary>
        /// La materia prima para armar un snapshot. Devuelve los colaboradores y,
        /// aparte, TODO su historial de sueldos sin resolver cual rige: eso lo
        /// decide NegHorasExtras.SalarioVigente, que a igual fecha da prioridad
        /// al ajuste sobre el rol.
        /// </summary>
        public static void LeerInsumos(DateTime corte, out List<EntHeFila> colaboradores,
                                       out Dictionary<long, List<EntHeSalario>> salarios)
        {
            List<EntHeFila> filas = new List<EntHeFila>();
            Dictionary<long, List<EntHeSalario>> historial = new Dictionary<long, List<EntHeSalario>>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeInsumos", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FechaCorte", SqlDbType.Date).Value = corte;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        EntHeFila f = new EntHeFila();
                        f.IdEmpleado = Convert.ToInt64(dr["IdEmpleado"]);
                        f.CedulaSnapshot = Texto(dr, "Cedula");
                        f.NombreSnapshot = Texto(dr, "Nombre");
                        f.CargoSnapshot = Texto(dr, "Cargo");
                        f.EmpresaSnapshot = Texto(dr, "Empresa");
                        f.JornadaHorasDiaSnapshot = Convert.ToInt32(dr["JornadaHorasDia"]);
                        f.DivisorManual = dr["DivisorManual"] == DBNull.Value
                                          ? (int?)null : Convert.ToInt32(dr["DivisorManual"]);
                        f.AplicaHESnapshot = Convert.ToBoolean(dr["AplicaHE"]);
                        f.MotivoNoAplica = Texto(dr, "MotivoNoAplica");
                        f.Observacion = "";
                        filas.Add(f);
                    }

                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            long id = Convert.ToInt64(dr["IdEmpleado"]);
                            if (!historial.ContainsKey(id)) { historial[id] = new List<EntHeSalario>(); }

                            EntHeSalario s = new EntHeSalario();
                            s.Monto = Convert.ToDecimal(dr["Monto"]);
                            s.FechaVigenciaDesde = Convert.ToDateTime(dr["FechaVigenciaDesde"]);
                            s.Origen = Texto(dr, "Origen");
                            historial[id].Add(s);
                        }
                    }
                }
            }

            colaboradores = filas;
            salarios = historial;
        }

        public static int GuardarFila(int idPeriodo, EntHeFila f, string usuario, string ip)
        {
            int resultado = -1;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeGuardarFila", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPeriodo", SqlDbType.Int).Value = idPeriodo;
                cmd.Parameters.Add("@IdEmpleado", SqlDbType.BigInt).Value = f.IdEmpleado;
                cmd.Parameters.Add("@CedulaSnapshot", SqlDbType.VarChar, 20).Value = f.CedulaSnapshot ?? "";
                cmd.Parameters.Add("@NombreSnapshot", SqlDbType.VarChar, 400).Value = f.NombreSnapshot ?? "";
                cmd.Parameters.Add("@EmpresaSnapshot", SqlDbType.VarChar, 120).Value = f.EmpresaSnapshot ?? "";
                cmd.Parameters.Add("@CargoSnapshot", SqlDbType.VarChar, 200).Value = f.CargoSnapshot ?? "";
                cmd.Parameters.Add("@JornadaHorasDiaSnapshot", SqlDbType.Int).Value = f.JornadaHorasDiaSnapshot;
                cmd.Parameters.Add("@SalarioBaseSnapshot", SqlDbType.Decimal).Value = f.SalarioBaseSnapshot;
                cmd.Parameters.Add("@AplicaHESnapshot", SqlDbType.Bit).Value = f.AplicaHESnapshot;
                cmd.Parameters.Add("@Divisor", SqlDbType.Int).Value = f.Divisor;
                cmd.Parameters.Add("@ValorHoraOrdinaria", SqlDbType.Decimal).Value = f.ValorHoraOrdinaria;
                cmd.Parameters.Add("@ValorHora50", SqlDbType.Decimal).Value = f.ValorHora50;
                cmd.Parameters.Add("@ValorHora100", SqlDbType.Decimal).Value = f.ValorHora100;
                cmd.Parameters.Add("@Horas50", SqlDbType.Decimal).Value = f.Horas50;
                cmd.Parameters.Add("@Horas100", SqlDbType.Decimal).Value = f.Horas100;
                cmd.Parameters.Add("@Total50", SqlDbType.Decimal).Value = f.Total50;
                cmd.Parameters.Add("@Total100", SqlDbType.Decimal).Value = f.Total100;
                cmd.Parameters.Add("@TotalHoras", SqlDbType.Decimal).Value = f.TotalHoras;
                cmd.Parameters.Add("@TotalHE", SqlDbType.Decimal).Value = f.TotalHE;
                cmd.Parameters.Add("@Observacion", SqlDbType.VarChar, 400).Value = f.Observacion ?? "";
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario ?? "";
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 64).Value = ip ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read()) { resultado = Convert.ToInt32(dr["Respuestas"]); }
                }
            }

            return resultado;
        }

        /// <summary>
        /// El periodo y sus filas. Los totales quedan en cero: sumarlos aqui
        /// obligaria a CapaDato a conocer la regla de redondeo, que es de negocio.
        /// </summary>
        public static EntHePantalla CargarPeriodo(int idPeriodo)
        {
            EntHePantalla pantalla = new EntHePantalla();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeCargarPeriodo", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPeriodo", SqlDbType.Int).Value = idPeriodo;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read()) { pantalla.Periodo = LeerPeriodo(dr); }

                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            EntHeFila f = new EntHeFila();
                            f.IdDetalle = Convert.ToInt32(dr["IdDetalle"]);
                            f.IdEmpleado = Convert.ToInt64(dr["IdEmpleado"]);
                            f.CedulaSnapshot = Texto(dr, "CedulaSnapshot");
                            f.NombreSnapshot = Texto(dr, "NombreSnapshot");
                            f.EmpresaSnapshot = Texto(dr, "EmpresaSnapshot");
                            f.CargoSnapshot = Texto(dr, "CargoSnapshot");
                            f.JornadaHorasDiaSnapshot = Convert.ToInt32(dr["JornadaHorasDiaSnapshot"]);
                            f.SalarioBaseSnapshot = Convert.ToDecimal(dr["SalarioBaseSnapshot"]);
                            f.AplicaHESnapshot = Convert.ToBoolean(dr["AplicaHESnapshot"]);
                            f.Divisor = Convert.ToInt32(dr["Divisor"]);
                            f.ValorHoraOrdinaria = Convert.ToDecimal(dr["ValorHoraOrdinaria"]);
                            f.ValorHora50 = Convert.ToDecimal(dr["ValorHora50"]);
                            f.ValorHora100 = Convert.ToDecimal(dr["ValorHora100"]);
                            f.Horas50 = Convert.ToDecimal(dr["Horas50"]);
                            f.Horas100 = Convert.ToDecimal(dr["Horas100"]);
                            f.Total50 = Convert.ToDecimal(dr["Total50"]);
                            f.Total100 = Convert.ToDecimal(dr["Total100"]);
                            f.TotalHoras = Convert.ToDecimal(dr["TotalHoras"]);
                            f.TotalHE = Convert.ToDecimal(dr["TotalHE"]);
                            f.Observacion = Texto(dr, "Observacion");
                            f.MotivoNoAplica = Texto(dr, "MotivoNoAplica");
                            pantalla.Filas.Add(f);
                        }
                    }
                }
            }

            return pantalla;
        }

        /// <summary>
        /// Los parametros vigentes hoy, tal cual estan en HE_Parametro.
        ///
        /// Todavia no lo consume nadie: lo entrega esta tarea porque el DAO se
        /// termina de una sola vez, y lo usara NegHorasExtrasPantalla en la
        /// siguiente tarea de esta fase.
        /// </summary>
        public static Dictionary<string, decimal> LeerParametrosVigentes()
        {
            Dictionary<string, decimal> valores = new Dictionary<string, decimal>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Clave, Valor FROM dbo.HE_Parametro WHERE FechaVigenciaHasta IS NULL", cnx))
            {
                cmd.CommandType = CommandType.Text;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        valores[Convert.ToString(dr["Clave"]).Trim()] = Convert.ToDecimal(dr["Valor"]);
                    }
                }
            }

            return valores;
        }

        private static EntHePeriodo LeerPeriodo(SqlDataReader dr)
        {
            EntHePeriodo p = new EntHePeriodo();
            p.IdPeriodo = Convert.ToInt32(dr["IdPeriodo"]);
            p.Anio = Convert.ToInt32(dr["Anio"]);
            p.Mes = Convert.ToInt32(dr["Mes"]);
            p.Descripcion = Texto(dr, "Descripcion");
            p.EstadoPeriodo = Texto(dr, "EstadoPeriodo");
            p.FechaCierre = dr["FechaCierre"] == DBNull.Value
                            ? (DateTime?)null : Convert.ToDateTime(dr["FechaCierre"]);
            p.UsuarioCierre = Texto(dr, "UsuarioCierre");
            return p;
        }

        private static string Texto(SqlDataReader dr, string columna)
        {
            return dr[columna] == DBNull.Value ? "" : Convert.ToString(dr[columna]).Trim();
        }
    }
}
