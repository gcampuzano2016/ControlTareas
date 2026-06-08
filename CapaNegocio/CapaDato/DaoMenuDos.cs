using System;
using CapaEntidad;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace CapaDato
{
    public class DaoMenuDos
    {
        public static List<EntMenuDos> Sp_RTA_ConsultarMenuDos(int tipo)
        {
            List<EntMenuDos> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTA_ConsultarMenuDos", cnx);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntMenuDos>();

                while (dr.Read())
                {
                    EntMenuDos Tarea = new EntMenuDos();

                    Tarea.Id_Menu = Convert.ToInt32(dr["Id_Menu"].ToString());
                    Tarea.Href = dr["Href"].ToString();
                    Tarea.Titulo = dr["Titulo"].ToString();
                    Tarea.Id_MenuPadre = Convert.ToInt32(dr["Id_MenuPadre"].ToString());
                    Tarea.Class_Icon = dr["Class_Icon"].ToString();
                    listaTareas.Add(Tarea);


                }

            }
            catch (Exception ex)
            {
                listaTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return listaTareas;
        }

        public static EntRespuesta Sp_RTA_InsertarMenuNuevo(int tipoMenu, String Titulo, String Descripcion, String Icono,String Referencia, int MenuPadre)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            int respuestaSP = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();

                cnx.Open();

                cmd = new SqlCommand("Sp_RTA_InsertarMenuNuevo", cnx);

                cmd.Parameters.AddWithValue("@TipoMenu", tipoMenu);
                cmd.Parameters.AddWithValue("@Titulo", Titulo);
                cmd.Parameters.AddWithValue("@Referencia ", Referencia);
                cmd.Parameters.AddWithValue("@Descripcion", Descripcion);
                cmd.Parameters.AddWithValue("@Icono", Icono);
                cmd.Parameters.AddWithValue("@MenuPadre", MenuPadre);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();
                respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());

                if (respuestaSP == 1)
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Datos Insertados con Exito.";
                    Respuesta.tipoMensaje = "success";
                }
                else
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Ocurrio un error al insertar los datos.";
                    Respuesta.tipoMensaje = "danger";
                }

            }
            catch (Exception ex)
            {
                Respuesta.estado = "0";
                Respuesta.mensaje = ex.Message.ToString();
                Respuesta.tipoMensaje = "danger";
            }
            finally
            {
                cmd.Connection.Close();
            }


            return Respuesta;

        }

        public static List<EntMenuDos> Sp_RTA_ConsultarMenuPerfil(int tipo)
        {
            List<EntMenuDos> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTA_ConsultarMenuPerfil", cnx);
                cmd.Parameters.AddWithValue("@tipoPerfil", tipo);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntMenuDos>();

                while (dr.Read())
                {
                    EntMenuDos Tarea = new EntMenuDos();

                    Tarea.Id_Menu = Convert.ToInt32(dr["Id_Menu"].ToString());
                    Tarea.Titulo = dr["Titulo"].ToString();
                    Tarea.Estado = Convert.ToInt32(dr["Estado"].ToString());
                    Tarea.Id_MenuPadre = Convert.ToInt32(dr["Id_MenuPadre"].ToString());
                    Tarea.Class_Icon = dr["Class_Icon"].ToString();
                    Tarea.Href = dr["Href"].ToString();
                    listaTareas.Add(Tarea);


                }

            }
            catch (Exception ex)
            {
                listaTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return listaTareas;
        }

        public static List<EntMenuDos> Sp_RTA_ConsultarMenuPerfilUsuario(int tipo)
        {
            List<EntMenuDos> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTA_ConsultarMenuPerfilUsuario", cnx);
                cmd.Parameters.AddWithValue("@tipoPerfil", tipo);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntMenuDos>();

                while (dr.Read())
                {
                    EntMenuDos Tarea = new EntMenuDos();

                    Tarea.Id_Menu = Convert.ToInt32(dr["Id_Menu"].ToString());
                    Tarea.Titulo = dr["Titulo"].ToString();
                    Tarea.Estado = Convert.ToInt32(dr["Estado"].ToString());
                    Tarea.Id_MenuPadre = Convert.ToInt32(dr["Id_MenuPadre"].ToString());
                    Tarea.Class_Icon= dr["Class_Icon"].ToString();
                    Tarea.Href = dr["Href"].ToString();
                    listaTareas.Add(Tarea);


                }

            }
            catch (Exception ex)
            {
                listaTareas = null;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return listaTareas;
        }

        public static EntRespuesta Sp_RTActualizarPerfilMenu(EntMenuDos objTarea)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            int respuestaSP = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();

                cnx.Open();

                cmd = new SqlCommand("Sp_RTActualizarPerfilMenu", cnx);

                cmd.Parameters.AddWithValue("@tipoPerfil", objTarea.Id_Perfil);
                cmd.Parameters.AddWithValue("@IdMenu", objTarea.Id_Menu);
                cmd.Parameters.AddWithValue("@Estado", objTarea.Estado);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();
                respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());

                if (respuestaSP == 1)
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Datos Guardados con Exito.";
                    Respuesta.tipoMensaje = "success";
                }
                else
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Ocurrio un error al guardar los datos.";
                    Respuesta.tipoMensaje = "danger";
                }

            }
            catch (Exception ex)
            {
                Respuesta.estado = "0";
                Respuesta.mensaje = ex.Message.ToString();
                Respuesta.tipoMensaje = "danger";
            }
            finally
            {
                cmd.Connection.Close();
            }


            return Respuesta;

        }

        public static EntRespuesta Sp_RTActualizarIconoMenu(EntMenuDos objTarea)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            int respuestaSP = 0;
            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();

                cnx.Open();

                cmd = new SqlCommand("Sp_RTActualizarIconoMenu", cnx);

                cmd.Parameters.AddWithValue("@IdMenu", objTarea.Id_Menu);
                cmd.Parameters.AddWithValue("@Icono", objTarea.Class_Icon);
                cmd.Parameters.AddWithValue("@Href", objTarea.Href);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();
                respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());

                if (respuestaSP == 1)
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Datos Guardados con Exito.";
                    Respuesta.tipoMensaje = "success";
                }
                else
                {
                    Respuesta.estado = respuestaSP.ToString();
                    Respuesta.mensaje = "Ocurrio un error al guardar los datos.";
                    Respuesta.tipoMensaje = "danger";
                }

            }
            catch (Exception ex)
            {
                Respuesta.estado = "0";
                Respuesta.mensaje = ex.Message.ToString();
                Respuesta.tipoMensaje = "danger";
            }
            finally
            {
                cmd.Connection.Close();
            }


            return Respuesta;

        }


    

}


}

