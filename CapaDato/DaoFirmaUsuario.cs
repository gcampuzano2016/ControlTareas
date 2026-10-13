using System;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// La firma que cada persona dejo guardada, para no volver a subirla en cada
    /// solicitud.
    ///
    /// Solo lee. La escritura la hace Sp_RTA_GuardarFirmaSolicitud al registrar
    /// una firma: la ultima que uso queda como la suya, sin un paso aparte que
    /// alguien tenga que acordarse de dar.
    /// </summary>
    public class DaoFirmaUsuario
    {
        /// <summary>
        /// La firma guardada como data URI, listo para el src de una imagen, o
        /// cadena vacia si no tiene ninguna.
        ///
        /// Devuelve vacio tambien cuando el codigo de usuario esta repetido en
        /// R_Usuarios: el procedimiento no la entrega en ese caso, porque la fila
        /// seria de dos personas distintas. Aqui no hay nada especial que hacer,
        /// se dice para que quien lea esto no lo busque.
        /// </summary>
        public static string Obtener(string codUsuario)
        {
            if (string.IsNullOrEmpty(codUsuario)) { return string.Empty; }

            string dataUri = string.Empty;
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_FirmaGuardadaUsuario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 64).Value = codUsuario;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        string base64 = dr["TrazoBase64"] == DBNull.Value
                            ? string.Empty
                            : dr["TrazoBase64"].ToString();

                        if (base64 != string.Empty)
                        {
                            string tipo = dr["TrazoTipo"] == DBNull.Value
                                ? "image/png"
                                : dr["TrazoTipo"].ToString();

                            if (tipo == string.Empty) { tipo = "image/png"; }

                            dataUri = "data:" + tipo + ";base64," + base64;
                        }
                    }
                }
            }

            return dataUri;
        }
    }
}
