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
   public class DaoPoliza
    {
        public static EntRespuesta RTAInsertaNuevaPoliza(EntPoliza objEntPoliza)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevaPoliza", cnx);

                cmd.Parameters.AddWithValue("@IdPoliza", Convert.ToInt32(objEntPoliza.IdPoliza));
                cmd.Parameters.AddWithValue("@IdPedido", objEntPoliza.IdPedido);
                cmd.Parameters.AddWithValue("@NumFactura", objEntPoliza.NumFactura);
                cmd.Parameters.AddWithValue("@NumPoliza", objEntPoliza.NumPoliza);
                if (objEntPoliza.FechaInicio == "")
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", Convert.ToDateTime(objEntPoliza.FechaInicio));
                }
                if (objEntPoliza.FechaFin == "")
                {
                    cmd.Parameters.AddWithValue("@FechaFin", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaFin", Convert.ToDateTime(objEntPoliza.FechaFin));
                }

                cmd.Parameters.AddWithValue("@Monto", objEntPoliza.Monto);
                cmd.Parameters.AddWithValue("@TipoPoliza", objEntPoliza.TipoPoliza);
                cmd.Parameters.AddWithValue("@UsuarioRegistro", objEntPoliza.UsuarioRegistro);
                cmd.Parameters.AddWithValue("@Tipo", objEntPoliza.Tipo);
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

        public static EntRespuesta RTAInsertaNuevaPolizas(EntPoliza objEntPoliza)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevaPolizas", cnx);

                cmd.Parameters.AddWithValue("@IdPoliza", Convert.ToInt32(objEntPoliza.IdPoliza));
                cmd.Parameters.AddWithValue("@IdPedido", objEntPoliza.IdPedido);
                cmd.Parameters.AddWithValue("@NumFactura", objEntPoliza.NumFactura);
                cmd.Parameters.AddWithValue("@OS", objEntPoliza.OS);
                cmd.Parameters.AddWithValue("@PEDIDO", Convert.ToInt32(objEntPoliza.PEDIDO));
                cmd.Parameters.AddWithValue("@NumPoliza", objEntPoliza.NumPoliza);
                cmd.Parameters.AddWithValue("@ANEXO", objEntPoliza.ANEXO);
                cmd.Parameters.AddWithValue("@BENEFICIARIO", objEntPoliza.BENEFICIARIO);
                if (objEntPoliza.FechaInicio == "")
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", Convert.ToDateTime(objEntPoliza.FechaInicio));
                }
                if (objEntPoliza.FechaFin == "")
                {
                    cmd.Parameters.AddWithValue("@FechaFin", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaFin", Convert.ToDateTime(objEntPoliza.FechaFin));
                }

                cmd.Parameters.AddWithValue("@Monto", Convert.ToDouble(objEntPoliza.Monto));
                cmd.Parameters.AddWithValue("@OBJETO", objEntPoliza.OBJETO);
                cmd.Parameters.AddWithValue("@TipoPoliza", objEntPoliza.TipoPoliza);
                cmd.Parameters.AddWithValue("@UsuarioRegistro", objEntPoliza.UsuarioRegistro);
                cmd.Parameters.AddWithValue("@EstadoPoliza", objEntPoliza.EstadoPoliza);
                cmd.Parameters.AddWithValue("@Tipo", objEntPoliza.Tipo);
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

        public static List<EntPoliza> ConsultaSp_RTAConsultarPoliza(int IdPedido, int tipo)
        {
            List<EntPoliza> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultarPoliza", cnx);
                cmd.Parameters.AddWithValue("@IdPedido", IdPedido);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntPoliza>();

                while (dr.Read())
                {
                    EntPoliza Tarea = new EntPoliza();
                    Tarea.IdPoliza = Convert.ToInt32(dr["IdPoliza"].ToString());
                    Tarea.IdPedido = Convert.ToInt32(dr["IdPedido"].ToString());
                    Tarea.NumFactura = dr["FACTURA"].ToString();
                    Tarea.Monto = dr["VALOR_FACTURA"].ToString();
                    Tarea.OC = dr["OC"].ToString();
                    Tarea.OS = dr["OS"].ToString();
                    Tarea.PEDIDO = dr["PEDIDO"].ToString();
                    Tarea.NumPoliza = dr["POLIZA"].ToString();
                    Tarea.ANEXO = dr["ANEXO"].ToString();
                    Tarea.BENEFICIARIO = dr["BENEFICIARIO"].ToString();
                    Tarea.EMISION = dr["EMISION"].ToString();
                    Tarea.FechaInicio = dr["INICIO"].ToString();
                    Tarea.FechaFin = dr["VENCIMIENTO"].ToString();
                    Tarea.VALOR = dr["VALOR"].ToString();
                    Tarea.OBJETO = dr["OBJETO"].ToString();
                    Tarea.TipoPoliza = dr["TipoPoliza"].ToString();
                    Tarea.UsuarioRegistro = dr["UsuarioRegistro"].ToString();
                    Tarea.Proceso = dr["Proceso"].ToString();
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

        public static List<EntPoliza> ConsultaSp_RTAListaPoliza(string buscar, string FchIni, string FchFin, int IdPedido)
        {
            List<EntPoliza> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaPoliza", cnx);
                cmd.Parameters.AddWithValue("@buscar", buscar);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@IdPedido", IdPedido);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntPoliza>();

                while (dr.Read())
                {
                    EntPoliza Tarea = new EntPoliza();


                    Tarea.IdPoliza = Convert.ToInt32(dr["IdPoliza"].ToString());
                    Tarea.IdPedido = Convert.ToInt32(dr["IdPedido"].ToString());
                    Tarea.NumFactura = dr["NumFactura"].ToString();
                    Tarea.NumPoliza = dr["NumPoliza"].ToString();
                    Tarea.FechaInicio = dr["FechaInicio"].ToString();
                    Tarea.FechaFin = dr["FechaFin"].ToString();
                    Tarea.Monto = dr["Monto"].ToString();
                    Tarea.TipoPoliza = dr["TipoPoliza"].ToString();
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

        public static List<EntPoliza> ConsultaSp_RTAListaPolizas(string buscar, string FchIni, string FchFin, int IdPedido, string BENEFICIARIO,string Proceso,int idFecha)
        {
            List<EntPoliza> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaPolizas", cnx);
                cmd.Parameters.AddWithValue("@buscar", buscar);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@IdPedido", IdPedido);
                cmd.Parameters.AddWithValue("@BENEFICIARIO", BENEFICIARIO);
                cmd.Parameters.AddWithValue("@Proceso", Proceso);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntPoliza>();

                while (dr.Read())
                {
                    EntPoliza Tarea = new EntPoliza();
                    Tarea.IdPoliza = Convert.ToInt32(dr["IdPoliza"].ToString());
                    Tarea.IdPedido = Convert.ToInt32(dr["IdPedido"].ToString());
                    Tarea.NumFactura = dr["FACTURA"].ToString();
                    Tarea.Monto = dr["VALOR_FACTURA"].ToString();
                    Tarea.OC = dr["OC"].ToString();
                    Tarea.OS = dr["OS"].ToString();
                    Tarea.PEDIDO = dr["PEDIDO"].ToString();
                    Tarea.NumPoliza = dr["POLIZA"].ToString();
                    Tarea.ANEXO = dr["ANEXO"].ToString();
                    Tarea.BENEFICIARIO = dr["BENEFICIARIO"].ToString();
                    Tarea.EMISION = dr["EMISION"].ToString();
                    Tarea.FechaInicio = dr["INICIO"].ToString();
                    Tarea.FechaFin = dr["VENCIMIENTO"].ToString();
                    Tarea.VALOR = dr["VALOR"].ToString();
                    Tarea.OBJETO = dr["OBJETO"].ToString();
                    Tarea.TipoPoliza = dr["TipoPoliza"].ToString();
                    Tarea.UsuarioRegistro = dr["UsuarioRegistro"].ToString();
                    Tarea.Proceso = dr["Proceso"].ToString();
                    Tarea.conteoArchivosAdjuntos = Convert.ToInt32(dr["conteoArchivosAdjuntos"].ToString());
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

        public static EntRespuesta ConsultaSp_RTAListaPolizasDescargar(string buscar, string FchIni, string FchFin, int IdPedido, string BENEFICIARIO, string Proceso, int idFecha)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            DataTable dtResultados = new DataTable();

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaPolizas", cnx);
                cmd.Parameters.AddWithValue("@buscar", buscar);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@IdPedido", IdPedido);
                cmd.Parameters.AddWithValue("@BENEFICIARIO", BENEFICIARIO);
                cmd.Parameters.AddWithValue("@Proceso", Proceso);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();

                dtResultados.Load(dr);

                Respuesta.estado = "1";
                Respuesta.mensaje = "OK";
                Respuesta.tipoMensaje = "success";
                Respuesta.resultadoTabla = dtResultados;

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
