using CapaEntidad;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    /// <summary>
    /// Acceso a datos del modulo de perfil.
    ///
    /// Las consultas van con parametros tipados (SqlDbType explicito), no con
    /// AddWithValue: en esta base Cod_Usuario es varchar(50) y AddWithValue lo
    /// manda como nvarchar, lo que descarta el indice en tablas grandes.
    ///
    /// Esta clase NO calcula la edad, aunque tenga la fecha a mano: la
    /// dependencia de la solucion va CapaNegocio -> CapaDato, nunca al reves.
    /// Llamar a NegPerfilCampos desde aca seria una referencia circular y no
    /// compila. La edad la pone NegPerfil despues de leer.
    /// </summary>
    public class DaoPerfil
    {
        /// <summary>
        /// Todo el perfil en una sola ida. Recorre los result sets con
        /// NextResult() en el mismo orden en que los declara el procedimiento.
        /// </summary>
        public static EntPerfilCompleto CargarPerfil(string codUsuario)
        {
            EntPerfilCompleto perfil = new EntPerfilCompleto();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilColaborador", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    /* 1. cabecera */
                    if (dr.Read())
                    {
                        perfil.PerfilEncontrado            = true;
                        perfil.Cabecera.CodUsuario         = Texto(dr, "Cod_Usuario");
                        perfil.Cabecera.NombreCompleto     = Texto(dr, "NombreCompleto");
                        perfil.Cabecera.Cedula             = Texto(dr, "Cedula");
                        perfil.Cabecera.FechaNacTexto      = Texto(dr, "FechaNacTexto");
                        perfil.Cabecera.Cargo              = Texto(dr, "Cargo");
                        perfil.Cabecera.Area               = Texto(dr, "Area");
                        perfil.Cabecera.Ciudad             = Texto(dr, "Ciudad");
                        perfil.Cabecera.CorreoNotificacion = Texto(dr, "CorreoNotificacion");
                        perfil.Cabecera.JefeInmediato      = Texto(dr, "JefeInmediato");
                        perfil.Cabecera.Horario            = Texto(dr, "Horario");
                        perfil.Cabecera.TieneFicha         = Texto(dr, "TieneFicha") == "1";
                        perfil.Cabecera.EsJefe             = Texto(dr, "EsJefe") == "1";
                    }

                    /* 2. contacto personal */
                    if (dr.NextResult() && dr.Read())
                    {
                        perfil.Contacto.CorreoPersonal   = Texto(dr, "CorreoPersonal");
                        perfil.Contacto.TelefonoPersonal = Texto(dr, "TelefonoPersonal");
                        perfil.Contacto.Direccion        = Texto(dr, "Direccion");
                        perfil.Contacto.EstadoCivil      = Texto(dr, "EstadoCivil");
                    }

                    /* 3. contactos de emergencia */
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            perfil.Emergencia.Add(new EntPerfilEmergencia
                            {
                                IdContacto = int.Parse(Texto(dr, "IdContacto")),
                                Nombre     = Texto(dr, "Nombre"),
                                Parentesco = Texto(dr, "Parentesco"),
                                Telefono   = Texto(dr, "Telefono")
                            });
                        }
                    }

                    /* Los result sets 4 a 7 existen y se ignoran hasta la fase 2. */
                }
            }

            return perfil;
        }

        private static string Texto(SqlDataReader dr, string columna)
        {
            return dr[columna] == System.DBNull.Value ? "" : dr[columna].ToString().Trim();
        }
    }
}
