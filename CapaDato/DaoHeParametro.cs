using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Acceso a datos de la pantalla de parametros de horas extras.
    ///
    /// Distinta de DaoHorasExtras.LeerHistorialParametros a proposito: aquella
    /// lee el historial completo hasta una fecha de corte, sin IdParametro,
    /// porque el calculo elige por Clave y FechaVigenciaDesde y nunca necesita
    /// referirse a una fila concreta. Esta pantalla si necesita saber que fila
    /// exacta se esta editando, asi que su SELECT trae IdParametro y no
    /// reutiliza aquel metodo.
    /// </summary>
    public class DaoHeParametro
    {
        /// <summary>
        /// Todo el historial de HE_Parametro, vigente y cerrado, mas reciente
        /// primero dentro de cada clave. Sp_RTA_HeParametrosListar no recibe
        /// parametros: filtrar por vigencia es trabajo de quien pinta la
        /// pantalla, no de este DAO.
        /// </summary>
        public static List<EntHeParametroFila> Listar()
        {
            List<EntHeParametroFila> filas = new List<EntHeParametroFila>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeParametrosListar", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read()) { filas.Add(LeerFila(dr)); }
                }
            }

            return filas;
        }

        /// <summary>
        /// Da de alta una nueva version de la clave. Sp_RTA_HeParametroGuardar
        /// hace todo el trabajo de validacion y de cierre de la version
        /// vigente; este metodo solo traslada el codigo que devuelve.
        ///
        /// Respuestas: 0 guardado, -1 clave desconocida, -2 valor no mayor que
        /// cero, -3 ya existe una version con esa misma fecha de inicio, -4 la
        /// fecha nueva no es posterior a la vigente, -5 DecimalesMonto con mas
        /// de 2 decimales -hay un CHECK en la tabla-.
        /// </summary>
        public static int Guardar(string clave, decimal valor, DateTime desde, string usuario, string ip)
        {
            int resultado = -1;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_HeParametroGuardar", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Clave", SqlDbType.VarChar, 60).Value = clave ?? "";
                AddDecimalParam(cmd, "@Valor", valor, 18, 6);
                cmd.Parameters.Add("@FechaVigenciaDesde", SqlDbType.Date).Value = desde.Date;
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

        private static EntHeParametroFila LeerFila(SqlDataReader dr)
        {
            EntHeParametroFila f = new EntHeParametroFila();
            f.IdParametro = Convert.ToInt32(dr["IdParametro"]);
            f.Clave = Texto(dr, "Clave");
            f.Valor = Convert.ToDecimal(dr["Valor"]);
            f.FechaVigenciaDesde = Convert.ToDateTime(dr["FechaVigenciaDesde"]);
            f.FechaVigenciaHasta = dr["FechaVigenciaHasta"] == DBNull.Value
                                   ? (DateTime?)null
                                   : Convert.ToDateTime(dr["FechaVigenciaHasta"]);
            f.Usu_Modificacion = Texto(dr, "Usu_Modificacion");
            f.Fec_Modificacion = dr["Fec_Modificacion"] == DBNull.Value
                                 ? (DateTime?)null
                                 : Convert.ToDateTime(dr["Fec_Modificacion"]);
            return f;
        }

        private static string Texto(SqlDataReader dr, string columna)
        {
            return dr[columna] == DBNull.Value ? "" : Convert.ToString(dr[columna]).Trim();
        }

        /// <summary>
        /// Un parametro decimal con Precision y Scale explicitos: sin ellos,
        /// ADO.NET infiere la escala del decimal de .NET que le llegue, y eso
        /// puede no encajar en el DECIMAL(18,6) real del parametro.
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
