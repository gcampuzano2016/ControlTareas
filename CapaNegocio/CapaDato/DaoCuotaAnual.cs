using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using CapaEntidad;
using System.Globalization;

namespace CapaDato
{
    public class DaoCuotaAnual
    {

        public static List<EntCuotaAnual> ConsultaRTA_ConsultarCuotasAnual(string Anio, int tipo)
        {
            List<EntCuotaAnual> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTA_ConsultarCuotasAnual", cnx);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntCuotaAnual>();

                while (dr.Read())
                {
                    EntCuotaAnual Tarea = new EntCuotaAnual();
                    if (tipo != 3)
                    {
                        Tarea.ID = Convert.ToInt32(dr["ID"].ToString());
                        Tarea.IdCuotaAnual = Convert.ToInt32(dr["IdCuotaAnual"].ToString());
                        Tarea.Nombres = dr["Nombres"].ToString();
                        Tarea.CuotaAnual = dr["CuotaAnual"].ToString();
                        Tarea.Estado = dr["Estado"].ToString();
                        Tarea.IdSucursal = Convert.ToInt32(dr["IdSucursal"].ToString());
                        listaTareas.Add(Tarea);
                    }
                    else if (tipo == 3)
                    {
                        Tarea.IdMetas = Convert.ToInt32(dr["IdMetas"].ToString());
                        Tarea.MetaFacturacion = dr["MetaFacturacion"].ToString();
                        Tarea.MetaMargenBruto = dr["MetaMargenBruto"].ToString();
                        Tarea.Estado = dr["Estado"].ToString();
                        Tarea.Anio = dr["Anio"].ToString();
                        listaTareas.Add(Tarea);
                    }
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

        public static EntRespuesta RTA_CrearCuotasAnual(EntCuotaAnual objEntCuotaAnual)
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
                cmd = new SqlCommand("Sp_RTA_CrearCuotasAnual", cnx);
                cmd.Parameters.AddWithValue("@IdCuotaAnual", objEntCuotaAnual.IdCuotaAnual);
                cmd.Parameters.AddWithValue("@IdProceso", objEntCuotaAnual.IdProceso);
                cmd.Parameters.AddWithValue("@Fecha", Convert.ToDateTime(objEntCuotaAnual.Fecha));
                cmd.Parameters.AddWithValue("@CuotaAnual", objEntCuotaAnual.CuotaAnual);
                cmd.Parameters.AddWithValue("@Nombres", objEntCuotaAnual.Nombres);
                cmd.Parameters.AddWithValue("@IdEstado", objEntCuotaAnual.IdEstado);
                cmd.Parameters.AddWithValue("@IdSucursal", objEntCuotaAnual.IdSucursal);
                cmd.Parameters.AddWithValue("@Tipo", objEntCuotaAnual.Tipo);
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
                    Respuesta.estado = "1";
                    Respuesta.mensaje = "Datos Guardados con Exito.";
                    Respuesta.tipoMensaje = "success";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else if (respuestaSP == -1)
                {
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

        public static EntRespuesta RTA_CrearCuotasAnualEmpresa(EntCuotaAnual objEntCuotaAnual)
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
                cmd = new SqlCommand("Sp_RTA_CrearCuotasAnualEmpresa", cnx);
                cmd.Parameters.AddWithValue("@IdMetas", objEntCuotaAnual.IdMetas);
                cmd.Parameters.AddWithValue("@Anio", objEntCuotaAnual.Anio);
                cmd.Parameters.AddWithValue("@IdGerencial", Convert.ToInt32(objEntCuotaAnual.IdGerencial));
                cmd.Parameters.AddWithValue("@MetaFacturacion", objEntCuotaAnual.MetaFacturacion);
                cmd.Parameters.AddWithValue("@MetaMargenBruto", objEntCuotaAnual.MetaMargenBruto);
                cmd.Parameters.AddWithValue("@Tipo", objEntCuotaAnual.Tipo);
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
                    Respuesta.estado = "1";
                    Respuesta.mensaje = "Datos Guardados con Exito.";
                    Respuesta.tipoMensaje = "success";
                    Respuesta.resultado = respuestaSP.ToString();
                }
                else if (respuestaSP == -1)
                {
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

    }
}
