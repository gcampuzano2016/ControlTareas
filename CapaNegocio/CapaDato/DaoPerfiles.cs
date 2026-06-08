using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDato
{
    public class DaoPerfiles
    {



        public static EntRespuesta RTAInsertarNuevoPerfil(EntPerfiles objPerfil)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            Int32 respuestaSP = 0;
            string respuestaSP1 = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAInsertaNuevoPerfil", cnx);

                cmd.Parameters.AddWithValue("@Codigo", (objPerfil.Codigo));
                cmd.Parameters.AddWithValue("@NombrePerfil", (objPerfil.NombrePerfil));
                cmd.Parameters.AddWithValue("@Estado", (objPerfil.Estado));
                cmd.Parameters.AddWithValue("@Fecha", (objPerfil.Fecha));

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                respuestaSP1 = dr["Respuestas"].ToString();
                if (respuestaSP1 == "")
                {
                    respuestaSP = 1;
                }
                else
                {
                    respuestaSP = Convert.ToInt32(respuestaSP1);
                }

                if (respuestaSP >= 1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "1";
                    Respuesta.mensaje = "Datos Guardados con Exito.";
                    Respuesta.tipoMensaje = "success";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else if (respuestaSP == -1)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "-1";
                    Respuesta.mensaje = "Por favor validar que la informacion este correcto";
                    Respuesta.tipoMensaje = "danger";
                    Respuesta.resultado = respuestaSP.ToString();
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


        public static EntRespuesta Sp_RTActualizarPerfil(EntPerfiles objTarea)
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

                cmd = new SqlCommand("Sp_RTActualizarPerfil", cnx);

                cmd.Parameters.AddWithValue("@IdCambioPerfil", objTarea.IdPerfil);
                cmd.Parameters.AddWithValue("@IdUsuario", objTarea.IdUsuario);
                cmd.Parameters.AddWithValue("@CorreoCambio", objTarea.Mail);
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



        public static List<EntCombo> RTAListaComboContrato(Int32 Tipo, string Cod_Usuario, Int32 IdSucursal, Int32 IdCliente, string IdSucursalGerente)
        {
            List<EntCombo> cmbEstados = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();

                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAComboContrato", cnx);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.Parameters.AddWithValue("@Cod_Usuario", Cod_Usuario);
                cmd.Parameters.AddWithValue("@IdSucursal", IdSucursal);
                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@IdSucursalGerente", IdSucursalGerente);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                cmbEstados = new List<EntCombo>();
                while (dr.Read())
                {
                    EntCombo estado = new EntCombo();

                    estado.Id = dr["ID"].ToString();
                    estado.Valor = dr["NOMBRE"].ToString();

                    cmbEstados.Add(estado);
                }

            }
            catch (Exception ex)
            {
                cmbEstados = null;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return cmbEstados;
        }

        public static List<EntPerfiles> Sp_RTA_ConsultarUsuariosPerfil(int tipo)
        {
            List<EntPerfiles> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTA_ConsultarUsuariosPerfil", cnx);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntPerfiles>();

                while (dr.Read())
                {
                    EntPerfiles Tarea = new EntPerfiles();

                    Tarea.Codigo = Convert.ToInt32(dr["Codigo"].ToString());
                    Tarea.IdUsuario = Convert.ToInt32(dr["Id_Usuario"].ToString());
                    Tarea.NombrePerfil = dr["NombrePerfil"].ToString();
                    Tarea.NombreUsuario = dr["Nom_Usuario"].ToString();
                    Tarea.Mail = dr["E_Mail"].ToString();
                    Tarea.StrEstado = dr["Estado"].ToString();
                    Tarea.Combo = dr["Combo"].ToString();
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


    }
}
