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

        /// <summary>
        /// Guarda el contacto editable. Devuelve el resultado listo para el cliente.
        ///
        /// El procedimiento devuelve -2 en Respuestas cuando el Cod_Usuario esta
        /// repetido entre usuarios activos: el mismo caso que Sp_RTA_PerfilColaborador
        /// bloquea en la lectura. Sin esta traduccion, dos sesiones con el mismo
        /// codigo podrian pisarse el contacto sin que ninguna se entere -la lectura
        /// ya esta cerrada para ese caso, pero el guardado no lo estaba-.
        /// </summary>
        public static EntRespuesta GuardarContacto(string codUsuario, EntPerfilContacto contacto, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();
            int resultado;

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilGuardarContacto", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario",      SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@CorreoPersonal",   SqlDbType.VarChar, 150).Value = contacto.CorreoPersonal;
                cmd.Parameters.Add("@TelefonoPersonal", SqlDbType.VarChar,  50).Value = contacto.TelefonoPersonal;
                cmd.Parameters.Add("@Direccion",        SqlDbType.VarChar, 400).Value = contacto.Direccion;
                cmd.Parameters.Add("@EstadoCivil",      SqlDbType.VarChar, 100).Value = contacto.EstadoCivil;
                cmd.Parameters.Add("@Ip",               SqlDbType.VarChar,  64).Value = ip ?? "";
                cnx.Open();
                resultado = (int)cmd.ExecuteScalar();
            }

            if (resultado == -2)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "No pudimos identificar tu perfil de forma única. Escribe a Talento Humano para que corrijan tu código de usuario.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            respuesta.estado = "1";
            respuesta.mensaje = "Tus datos de contacto se guardaron correctamente.";
            respuesta.tipoMensaje = "success";
            return respuesta;
        }

        /// <summary>
        /// Alta o edicion de un contacto de emergencia.
        ///
        /// El procedimiento devuelve -2 en Respuestas cuando el Cod_Usuario esta
        /// repetido entre usuarios activos: el mismo caso que
        /// Sp_RTA_PerfilColaborador y Sp_RTA_PerfilGuardarContacto ya bloquean.
        /// Sin esta traduccion, dos personas distintas compartiendo el mismo
        /// codigo verian y podrian borrar los contactos de emergencia de la
        /// otra -el dato cuyo proposito es que alguien reciba una llamada
        /// cuando hay una urgencia-.
        /// </summary>
        public static EntRespuesta GuardarEmergencia(string codUsuario, EntPerfilEmergencia c, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();
            int resultado = -1;

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilGuardarEmergencia", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar,  50).Value = codUsuario;
                cmd.Parameters.Add("@IdContacto",  SqlDbType.Int).Value          = c.IdContacto;
                cmd.Parameters.Add("@Nombre",      SqlDbType.VarChar, 150).Value = c.Nombre;
                cmd.Parameters.Add("@Parentesco",  SqlDbType.VarChar,  50).Value = c.Parentesco;
                cmd.Parameters.Add("@Telefono",    SqlDbType.VarChar,  50).Value = c.Telefono;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar,  64).Value = ip ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read()) { resultado = int.Parse(dr["Respuestas"].ToString()); }
                }
            }

            if (resultado == -2)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "No pudimos identificar tu perfil de forma única. Escribe a Talento Humano para que corrijan tu código de usuario.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            if (resultado == 0)
            {
                respuesta.estado = "1";
                respuesta.mensaje = "Contacto de emergencia guardado.";
                respuesta.tipoMensaje = "success";
            }
            else
            {
                // El unico camino a -1 es un IdContacto que no es de esta persona.
                respuesta.estado = "0";
                respuesta.mensaje = "No se encontró ese contacto de emergencia.";
                respuesta.tipoMensaje = "warning";
            }

            return respuesta;
        }

        /// <summary>
        /// Borrado logico de un contacto de emergencia.
        ///
        /// Misma traduccion del -2 que GuardarEmergencia y por la misma razon:
        /// el borrado tambien queda cerrado cuando el Cod_Usuario esta repetido.
        /// </summary>
        public static EntRespuesta EliminarEmergencia(string codUsuario, int idContacto, string ip)
        {
            EntRespuesta respuesta = new EntRespuesta();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();
            int resultado = -1;

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_PerfilEliminarEmergencia", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Cod_Usuario", SqlDbType.VarChar, 50).Value = codUsuario;
                cmd.Parameters.Add("@IdContacto",  SqlDbType.Int).Value         = idContacto;
                cmd.Parameters.Add("@Ip",          SqlDbType.VarChar, 64).Value = ip ?? "";
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read()) { resultado = int.Parse(dr["Respuestas"].ToString()); }
                }
            }

            if (resultado == -2)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "No pudimos identificar tu perfil de forma única. Escribe a Talento Humano para que corrijan tu código de usuario.";
                respuesta.tipoMensaje = "warning";
                return respuesta;
            }

            respuesta.estado      = resultado == 0 ? "1" : "0";
            respuesta.mensaje     = resultado == 0 ? "Contacto eliminado." : "No se encontró ese contacto.";
            respuesta.tipoMensaje = resultado == 0 ? "success" : "warning";
            return respuesta;
        }

        private static string Texto(SqlDataReader dr, string columna)
        {
            return dr[columna] == System.DBNull.Value ? "" : dr[columna].ToString().Trim();
        }
    }
}
