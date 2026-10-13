using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDato
{
    public class DaoMenuPerfil
    {
        /// <summary>Lista los perfiles activos (para el combo).</summary>
        public static List<EntPerfil> ListarPerfiles()
        {
            List<EntPerfil> lista = new List<EntPerfil>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarPerfiles", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntPerfil()
                        {
                            Id_Perfil = Convert.ToInt32(dr["Id_Perfil"].ToString()),
                            Nombre = dr["Nombre"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Lista todos los menús con su estado (activo 0/1) para el perfil.</summary>
        public static List<EntMenuPerfil> ListarMenuPerfil(int idPerfil)
        {
            List<EntMenuPerfil> lista = new List<EntMenuPerfil>();
            DaoReporTareaAranda conexion = new DaoReporTareaAranda();

            using (SqlConnection cnx = conexion.conectar())
            using (SqlCommand cmd = new SqlCommand("Sp_RTA_ListarMenuPerfil", cnx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdPerfil", SqlDbType.Int).Value = idPerfil;
                cnx.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new EntMenuPerfil()
                        {
                            Id_Menu = Convert.ToInt32(dr["Id_Menu"].ToString()),
                            Id_MenuPadre = Convert.ToInt32(dr["Id_MenuPadre"].ToString()),
                            Titulo = dr["Titulo"].ToString(),
                            Class_Icon = dr["Class_Icon"].ToString(),
                            Activo = Convert.ToInt32(dr["Activo"].ToString())
                        });
                    }
                }
            }

            return lista;
        }

        /// <summary>Guarda (atómico) el set de menús activos del perfil; el SP agrega los padres.</summary>
        public static EntRespuesta GuardarMenuPerfil(int idPerfil, string activosCsv)
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
                using (SqlCommand cmd = new SqlCommand("Sp_RTA_GuardarMenuPerfil", cnx))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdPerfil", SqlDbType.Int).Value = idPerfil;
                    cmd.Parameters.Add("@ActivosCsv", SqlDbType.VarChar, -1).Value = activosCsv ?? string.Empty;

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
                respuesta.mensaje = "Ocurrió un error al guardar los menús del perfil. Detalle: " + ex.Message;
            }

            return respuesta;
        }
    }
}
