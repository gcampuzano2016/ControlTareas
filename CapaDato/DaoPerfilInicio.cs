using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoPerfilInicio
    {
        /// <summary>Lista todos los perfiles con su configuración de inicio (o NULL si no la tienen).</summary>
        public static List<EntPerfilInicio> ListarPerfilInicio()
        {
            List<EntPerfilInicio> lista = new List<EntPerfilInicio>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarPerfilInicio", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntPerfilInicio()
                        {
                            IdPerfil = Convert.ToInt32(dr["IdPerfil"].ToString()),
                            NombrePerfil = dr["NombrePerfil"].ToString(),
                            Href = dr["Href"] == DBNull.Value ? "" : dr["Href"].ToString(),
                            TituloPagina = dr["TituloPagina"] == DBNull.Value ? "" : dr["TituloPagina"].ToString(),
                            IdTipo = dr["IdTipo"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdTipo"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Inserta o actualiza la configuración de inicio de un perfil.</summary>
        public static EntRespuesta GuardarPerfilInicio(int idPerfil, string href, int idTipo, string usuarioRegistro)
        {
            EntRespuesta respuesta = new EntRespuesta()
            {
                estado = "0",
                resultado = "0",
                tipoMensaje = "danger",
                mensaje = ""
            };

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarPerfilInicio", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdPerfil", SqlDbType.Int).Value = idPerfil;
                    cmd.Parameters.Add("@Href", SqlDbType.VarChar, 150).Value = href ?? string.Empty;
                    cmd.Parameters.Add("@IdTipo", SqlDbType.Int).Value = idTipo;
                    cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 100).Value = (object)usuarioRegistro ?? DBNull.Value;

                    cnx.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            int respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());
                            respuesta.resultado = respuestaSP.ToString();
                            respuesta.mensaje = dr["Mensaje"].ToString();

                            if (respuestaSP > 0)
                            {
                                respuesta.estado = "1";
                                respuesta.tipoMensaje = "success";
                            }
                            else
                            {
                                respuesta.estado = "0";
                                respuesta.tipoMensaje = "warning";
                            }
                        }
                        else
                        {
                            respuesta.mensaje = "El procedimiento no devolvió información.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.estado = "0";
                respuesta.resultado = "0";
                respuesta.tipoMensaje = "danger";
                respuesta.mensaje = "Ocurrió un error al guardar la configuración. Detalle: " + ex.Message;
            }

            return respuesta;
        }

        /// <summary>Devuelve la página de inicio y el "tipo" del perfil. Fail-safe: Principal.aspx / 0.</summary>
        public static EntPerfilInicio ObtenerPerfilInicio(int idPerfil)
        {
            EntPerfilInicio inicio = new EntPerfilInicio()
            {
                IdPerfil = idPerfil,
                Href = "Principal.aspx",
                IdTipo = 0
            };

            try
            {
                DaoReporTareaAranda conexion = new DaoReporTareaAranda();

                using (SqlConnection cnx = conexion.conectar())
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_ObtenerPerfilInicio", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdPerfil", SqlDbType.Int).Value = idPerfil;
                    cnx.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string href = dr["Href"] == DBNull.Value ? "" : dr["Href"].ToString().Trim();
                            if (!string.IsNullOrEmpty(href))
                            {
                                inicio.Href = href;
                            }
                            inicio.IdTipo = dr["IdTipo"] == DBNull.Value ? 0 : Convert.ToInt32(dr["IdTipo"].ToString());
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fail-safe: nunca bloquear el login. Se mantienen los valores por defecto.
                inicio.Href = "Principal.aspx";
                inicio.IdTipo = 0;
            }

            return inicio;
        }

        /// <summary>Lista las páginas navegables de MenuDos (para el desplegable).</summary>
        public static List<EntMenuDos> ListarPaginasMenu()
        {
            List<EntMenuDos> lista = new List<EntMenuDos>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarPaginasMenu", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntMenuDos()
                        {
                            Titulo = dr["Titulo"].ToString(),
                            Href = dr["Href"].ToString()
                        });
                    }
                }
            }

            return lista;
        }
    }
}
