using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoMenuUsuario
    {
        /// <summary>Busca usuarios activos por nombre, código o cédula, con su perfil.</summary>
        public static List<EntUsuarioMenuBusqueda> ListarUsuarios(string filtro)
        {
            List<EntUsuarioMenuBusqueda> lista = new List<EntUsuarioMenuBusqueda>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarUsuariosMenu", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntUsuarioMenuBusqueda()
                        {
                            Cod_Usuario = dr["Cod_Usuario"].ToString(),
                            Nom_Usuario = dr["Nom_Usuario"].ToString(),
                            Cedula = dr["Cedula"].ToString(),
                            Id_Perfil = Convert.ToInt64(dr["Id_Perfil"].ToString()),
                            NombrePerfil = dr["NombrePerfil"].ToString(),
                            TotalExtras = Convert.ToInt32(dr["TotalExtras"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Lista todos los menús con su origen (perfil / extra) para el usuario.</summary>
        public static List<EntMenuUsuario> ListarMenuUsuario(string codUsuario)
        {
            List<EntMenuUsuario> lista = new List<EntMenuUsuario>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarMenuUsuario", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CodUsuario", SqlDbType.VarChar, 50).Value = codUsuario ?? string.Empty;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntMenuUsuario()
                        {
                            Id_Menu = Convert.ToInt32(dr["Id_Menu"].ToString()),
                            Id_MenuPadre = Convert.ToInt32(dr["Id_MenuPadre"].ToString()),
                            Titulo = dr["Titulo"].ToString(),
                            Class_Icon = dr["Class_Icon"].ToString(),
                            ActivoPerfil = Convert.ToInt32(dr["ActivoPerfil"].ToString()),
                            ActivoUsuario = Convert.ToInt32(dr["ActivoUsuario"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Guarda (atómico) los módulos extra del usuario; el SP agrega los padres.</summary>
        public static EntRespuesta GuardarMenuUsuario(string codUsuario, string extrasCsv, string usuarioRegistro)
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
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarMenuUsuario", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CodUsuario", SqlDbType.VarChar, 50).Value = codUsuario ?? string.Empty;
                    cmd.Parameters.Add("@ExtrasCsv", SqlDbType.VarChar, -1).Value = extrasCsv ?? string.Empty;
                    cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.VarChar, 50).Value = usuarioRegistro ?? "SISTEMA";

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
                respuesta.mensaje = "Ocurrió un error al guardar los módulos del usuario. Detalle: " + ex.Message;
            }

            return respuesta;
        }
    }
}
