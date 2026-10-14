using CapaEntidad;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace CapaDato
{
    /// <summary>
    /// Sp_RTA_PermisosUsuario devuelve dos resultados: pantallas y acciones. Si
    /// falta el segundo se lanza una excepcion: un "sin acciones" silencioso
    /// le quitaria a alguien un permiso sin ningun error visible.
    /// </summary>
    public class DaoPermisos
    {
        public static EntPermisos Cargar(int idPerfil, string codUsuario)
        {
            EntPermisos p = new EntPermisos();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PermisosUsuario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPerfil", SqlDbType.Int).Value = idPerfil;
                cmd.Parameters.Add("@CodUsuario", SqlDbType.VarChar, 50).Value = (object)codUsuario ?? DBNull.Value;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string href = Convert.ToString(dr["Href"], CultureInfo.InvariantCulture).Trim();
                        if (href != "") { p.Pantallas.Add(href); }
                    }
                    if (!dr.NextResult())
                    {
                        throw new InvalidOperationException("Sp_RTA_PermisosUsuario no devolvió las acciones.");
                    }
                    while (dr.Read())
                    {
                        string clave = Convert.ToString(dr["Clave"], CultureInfo.InvariantCulture).Trim();
                        string href = Convert.ToString(dr["Href"], CultureInfo.InvariantCulture).Trim();
                        if (clave != "") { p.Acciones[clave] = href; }
                    }
                }
            }
            return p;
        }
    }
}
