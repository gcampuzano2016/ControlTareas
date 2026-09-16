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

        /// <summary>
        /// Cierra el periodo. Respuestas: 0 bien, -1 no existe, -2 no estaba
        /// abierto, -3 hay filas con valor hora en cero -y cuantas, en
        /// filasConProblema-.
        /// </summary>
        public static int CerrarPeriodo(int idPeriodo, string usuario, string ip, out int filasConProblema)
        {
            int resultado = -1;
            int conProblema = 0;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeCerrarPeriodo", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPeriodo", SqlDbType.Int).Value = idPeriodo;
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario ?? "";
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 64).Value = ip ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        resultado = Convert.ToInt32(dr["Respuestas"]);
                        conProblema = Convert.ToInt32(dr["FilasConProblema"]);
                    }
                }
            }

            filasConProblema = conProblema;
            return resultado;
        }

        /// <summary>
        /// Reabre un periodo cerrado. Respuestas: 0 bien, -1 no existe,
        /// -2 no estaba cerrado.
        ///
        /// Quien puede llamar a esto lo decide la capa web: la base no conoce
        /// perfiles y este metodo no comprueba ninguno.
        /// </summary>
        public static int ReabrirPeriodo(int idPeriodo, string usuario, string ip)
        {
            int resultado = -1;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeReabrirPeriodo", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPeriodo", SqlDbType.Int).Value = idPeriodo;
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
        /// Deja constancia en HE_PeriodoAuditoria de que alguien descargo el
        /// periodo en Excel. Respuestas: 0 registrado, -1 el periodo no existe
        /// -y aun asi se registro-.
        ///
        /// Quien puede llamar a esto lo decide la capa web: la base no conoce
        /// perfiles y este metodo no comprueba ninguno.
        /// </summary>
        public static int RegistrarDescarga(int idPeriodo, string usuario, string ip)
        {
            int resultado = -1;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeRegistrarDescarga", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPeriodo", SqlDbType.Int).Value = idPeriodo;
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

        public static int GuardarFila(int idPeriodo, EntHeFila f, string usuario, string ip, bool auditar)
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
                AddDecimalParam(cmd, "@SalarioBaseSnapshot", f.SalarioBaseSnapshot, 18, 2);
                cmd.Parameters.Add("@AplicaHESnapshot", SqlDbType.Bit).Value = f.AplicaHESnapshot;
                cmd.Parameters.Add("@Divisor", SqlDbType.Int).Value = f.Divisor;
                AddDecimalParam(cmd, "@ValorHoraOrdinaria", f.ValorHoraOrdinaria, 18, 6);
                AddDecimalParam(cmd, "@ValorHora50", f.ValorHora50, 18, 6);
                AddDecimalParam(cmd, "@ValorHora100", f.ValorHora100, 18, 6);
                AddDecimalParam(cmd, "@Horas50", f.Horas50, 9, 2);
                AddDecimalParam(cmd, "@Horas100", f.Horas100, 9, 2);
                AddDecimalParam(cmd, "@Total50", f.Total50, 18, 2);
                AddDecimalParam(cmd, "@Total100", f.Total100, 18, 2);
                AddDecimalParam(cmd, "@TotalHoras", f.TotalHoras, 9, 2);
                AddDecimalParam(cmd, "@TotalHE", f.TotalHE, 18, 2);
                cmd.Parameters.Add("@Observacion", SqlDbType.VarChar, 400).Value = f.Observacion ?? "";
                cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 50).Value = usuario ?? "";
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 64).Value = ip ?? "";
                cmd.Parameters.Add("@Auditar", SqlDbType.Bit).Value = auditar;
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
        /// La tabla esta pensada para historial -UX_HE_Parametro_Clave la
        /// indexa por (Clave, FechaVigenciaDesde DESC)-, asi que el filtro por
        /// FechaVigenciaHasta IS NULL no basta para garantizar una sola fila
        /// por clave. Si algun dia hubiera mas de una vigencia abierta para la
        /// misma clave, se queda con la mas reciente que no sea futura, y no
        /// con la que el lector visite al final.
        /// </summary>
        public static Dictionary<string, decimal> LeerParametrosVigentes()
        {
            Dictionary<string, decimal> valores = new Dictionary<string, decimal>();
            Dictionary<string, DateTime> vigenciaElegida = new Dictionary<string, DateTime>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Clave, Valor, FechaVigenciaDesde FROM dbo.HE_Parametro " +
                "WHERE FechaVigenciaHasta IS NULL AND FechaVigenciaDesde <= GETDATE()", cnx))
            {
                cmd.CommandType = CommandType.Text;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string clave = Convert.ToString(dr["Clave"]).Trim();
                        DateTime desde = Convert.ToDateTime(dr["FechaVigenciaDesde"]);

                        if (vigenciaElegida.ContainsKey(clave) && vigenciaElegida[clave] >= desde) { continue; }

                        valores[clave] = Convert.ToDecimal(dr["Valor"]);
                        vigenciaElegida[clave] = desde;
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

        /// <summary>
        /// Un parametro decimal con Precision y Scale explicitos.
        ///
        /// Sin ellos, ADO.NET infiere la escala del decimal de .NET que le
        /// llegue, y una division decimal puede dar hasta 28 digitos
        /// significativos: eso no encaja en el DECIMAL(18,6) o DECIMAL(18,2)
        /// real de la columna y puede lanzar "Arithmetic overflow". Hoy no
        /// dispara porque los valores llegan ya redondeados, pero el DAO no
        /// deberia depender de que quien lo llame se acuerde de redondear.
        /// </summary>
        private static void AddDecimalParam(SqlCommand cmd, string nombre, decimal valor, byte precision, byte scale)
        {
            SqlParameter p = new SqlParameter(nombre, SqlDbType.Decimal);
            p.Precision = precision;
            p.Scale = scale;
            p.Value = valor;
            cmd.Parameters.Add(p);
        }
    }
}
