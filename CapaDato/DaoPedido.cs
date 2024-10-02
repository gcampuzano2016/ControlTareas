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
    public class DaoPedido
    {

        public static EntRespuesta RTA_ActualizarFechaPedido(EntPedido objEntPedido)
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
                cmd = new SqlCommand("Sp_RTActualizarFechaPedido", cnx);

                cmd.Parameters.AddWithValue("@Anio", objEntPedido.Anio);
                cmd.Parameters.AddWithValue("@meses", objEntPedido.meses);
                if (objEntPedido.FECHA == "")
                {
                    cmd.Parameters.AddWithValue("@fecha", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@fecha", objEntPedido.FECHA);
                }
                cmd.Parameters.AddWithValue("@idFecha", objEntPedido.idFecha);
                cmd.Parameters.AddWithValue("@Tipo", objEntPedido.Tipo);
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
        public static EntRespuesta RTA_InsertaNuevoPedido(EntPedido objEntPedido)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevoPedido", cnx);

                cmd.Parameters.AddWithValue("@FECHA", Convert.ToDateTime(objEntPedido.FECHA));
                cmd.Parameters.AddWithValue("@PEDIDO", objEntPedido.PEDIDO);
                cmd.Parameters.AddWithValue("@CLIENTE", objEntPedido.CLIENTE);
                cmd.Parameters.AddWithValue("@SEGMENTACION", objEntPedido.SEGMENTACION);
                cmd.Parameters.AddWithValue("@CLASIFICACION", objEntPedido.CLASIFICACION);
                cmd.Parameters.AddWithValue("@DETALLE", objEntPedido.DETALLE);
                cmd.Parameters.AddWithValue("@VALOR", objEntPedido.VALOR);
                cmd.Parameters.AddWithValue("@RENTABILIDAD", objEntPedido.RENTABILIDAD);
                cmd.Parameters.AddWithValue("@MARGEN", objEntPedido.MARGEN);
                cmd.Parameters.AddWithValue("@ESTADO", objEntPedido.ESTADO);
                cmd.Parameters.AddWithValue("@N_FACTURA", objEntPedido.N_FACTURA);

                if (objEntPedido.FECHA_ESTIMADA_DE_FACTURACION == "")
                {
                    cmd.Parameters.AddWithValue("@FECHA_ESTIMADA_DE_FACTURACION", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FECHA_ESTIMADA_DE_FACTURACION", Convert.ToDateTime(objEntPedido.FECHA_ESTIMADA_DE_FACTURACION));
                }
                if (objEntPedido.FECHA_FACTURACION == "")
                {
                    cmd.Parameters.AddWithValue("@FECHA_FACTURACION", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FECHA_FACTURACION", Convert.ToDateTime(objEntPedido.FECHA_FACTURACION));
                }

                cmd.Parameters.AddWithValue("@OBSERVACION", objEntPedido.OBSERVACION);
                cmd.Parameters.AddWithValue("@VENDEDOR", objEntPedido.VENDEDOR);
                cmd.Parameters.AddWithValue("@GERENTE_DE_PRODUCTO", objEntPedido.GERENTE_DE_PRODUCTO);
                cmd.Parameters.AddWithValue("@ORDEN_DE_COMPRA", objEntPedido.ORDEN_DE_COMPRA);
                cmd.Parameters.AddWithValue("@ORDEN_DE_SERVICIOS_INTERNA", objEntPedido.ORDEN_DE_SERVICIOS_INTERNA);
                cmd.Parameters.AddWithValue("@ORDEN_DE_SERVICIOS_EXTERNA", objEntPedido.ORDEN_DE_SERVICIOS_EXTERNA);
                cmd.Parameters.AddWithValue("@ORDEN_DE_SERVICIOS_DE_FINANZAS", objEntPedido.ORDEN_DE_SERVICIOS_DE_FINANZAS);
                cmd.Parameters.AddWithValue("@SUCURSAL", objEntPedido.SUCURSAL);
                cmd.Parameters.AddWithValue("@Tipo", objEntPedido.Tipo);

                cmd.Parameters.AddWithValue("@IdPedido", objEntPedido.IdPedido);
                cmd.Parameters.AddWithValue("@ChekRenovacion", objEntPedido.ChekRenovacion);
                if (objEntPedido.FechaInicioR == "")
                {
                    cmd.Parameters.AddWithValue("@FechaInicioR", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaInicioR", Convert.ToDateTime(objEntPedido.FechaInicioR));
                }
                if (objEntPedido.FechaFinalR == "")
                {
                    cmd.Parameters.AddWithValue("@FechaFinalR", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaFinalR", Convert.ToDateTime(objEntPedido.FechaFinalR));
                }

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                respuestaSP1 = dr["Respuestas"].ToString();
                if(respuestaSP1 == "")
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

        public static EntRespuesta ConsultaSp_RTAListaPedidoDescargar(string FchIni, string FchFin, string busqueda, int IdCliente, int IdGerenteCuenta, int IdGestorProducto, string sucursal, string estado, int IdClasificacion, int Anio, string meses,int idFecha,int IdRenovacion)
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

                cmd = new SqlCommand("Sp_RTAListaPedido", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@registro", busqueda);

                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@IdGestorProducto", IdGestorProducto);
                cmd.Parameters.AddWithValue("@sucursal", sucursal);
                cmd.Parameters.AddWithValue("@IdClasificacion", IdClasificacion);
                cmd.Parameters.AddWithValue("@estado", estado);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@meses", meses);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.Parameters.AddWithValue("@ChekRenovacion", IdRenovacion);
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
        public static List<EntPedido> ConsultaSp_RTAListaPedido(string FchIni, string FchFin, string busqueda, int IdCliente, int IdGerenteCuenta, int IdGestorProducto, string sucursal, string estado, int IdClasificacion,int Anio,string meses,int idFecha,int IdRenovacion)
        {
            List<EntPedido> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaPedido", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@registro", busqueda);

                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@IdGestorProducto", IdGestorProducto);
                cmd.Parameters.AddWithValue("@sucursal", sucursal);
                cmd.Parameters.AddWithValue("@IdClasificacion", IdClasificacion);
                cmd.Parameters.AddWithValue("@estado", estado);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@meses", meses);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.Parameters.AddWithValue("@ChekRenovacion", IdRenovacion);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntPedido>();

                while (dr.Read())
                {
                    EntPedido Tarea = new EntPedido();

                    Tarea.IdPedido = Convert.ToInt32(dr["IdPedido"].ToString());
                    //Tarea.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
                    //Tarea.IdClasificacion = Convert.ToInt32(dr["IdClasificacion"].ToString());
                    //Tarea.IdVendedor = Convert.ToInt32(dr["IdVendedor"].ToString());
                    //Tarea.IdGerenteProducto = Convert.ToInt32(dr["IdGerenteProducto"].ToString());
                    Tarea.FECHA = dr["FECHA"].ToString();
                    Tarea.PEDIDO = Convert.ToInt32(dr["PEDIDO"].ToString());
                    Tarea.CLIENTE = dr["CLIENTE"].ToString();
                    Tarea.SEGMENTACION = dr["SEGMENTACION"].ToString();
                    Tarea.CLASIFICACION = dr["CLASIFICACION"].ToString();
                    Tarea.DETALLE = dr["DETALLE"].ToString();
                    Tarea.VALOR = Convert.ToDouble( dr["VALOR"].ToString());
                    Tarea.RENTABILIDAD = Convert.ToDouble( dr["RENTABILIDAD"].ToString());
                    Tarea.MARGEN = Convert.ToDouble(dr["MARGEN"].ToString());
                    Tarea.ESTADO = dr["ESTADO"].ToString();
                    Tarea.N_FACTURA = dr["N_FACTURA"].ToString();
                    Tarea.FECHA_FACTURACION = dr["FECHA_FACTURACION"].ToString();
                    Tarea.FECHA_ESTIMADA_DE_FACTURACION = dr["FECHA_ESTIMADA_DE_FACTURACION"].ToString();
                    Tarea.OBSERVACION = dr["OBSERVACION"].ToString();
                    Tarea.VENDEDOR = dr["VENDEDOR"].ToString();
                    Tarea.GERENTE_DE_PRODUCTO = dr["GERENTE_DE_PRODUCTO"].ToString();
                    Tarea.ORDEN_DE_COMPRA = dr["ORDEN_DE_COMPRA"].ToString();
                    Tarea.ORDEN_DE_SERVICIOS_INTERNA = dr["ORDEN_DE_SERVICIOS_INTERNA"].ToString();
                    Tarea.ORDEN_DE_SERVICIOS_EXTERNA = dr["ORDEN_DE_SERVICIOS_EXTERNA"].ToString();
                    Tarea.ORDEN_DE_SERVICIOS_DE_FINANZAS = dr["ORDEN_DE_SERVICIOS_DE_FINANZAS"].ToString();
                    Tarea.SUCURSAL = dr["SUCURSAL"].ToString();
                    Tarea.FechaInicioR = dr["FechaInicioR"].ToString();
                    Tarea.FechaFinalR = dr["FechaFinalR"].ToString();
                    Tarea.ChekRenovacion = Convert.ToInt32(dr["ChekRenovacion"].ToString());
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
        public static List<EntPedido> ConsultaSp_RTAConsultarPedido(int IdPedido, string orden, int tipo)
        {
            List<EntPedido> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultarPedido", cnx);
                cmd.Parameters.AddWithValue("@IdPedido", IdPedido);
                cmd.Parameters.AddWithValue("@orden", orden);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntPedido>();

                while (dr.Read())
                {
                    EntPedido Tarea = new EntPedido();


                    Tarea.IdPedido = Convert.ToInt32(dr["IdPedido"].ToString());
                    Tarea.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
                    Tarea.IdClasificacion = Convert.ToInt32(dr["IdClasificacion"].ToString());
                    Tarea.IdVendedor = Convert.ToInt32(dr["IdVendedor"].ToString());
                    Tarea.IdGerenteProducto = Convert.ToInt32(dr["IdGerenteProducto"].ToString());
                    Tarea.IdSucursal = Convert.ToInt32(dr["IdSucursal"].ToString());
                    Tarea.FECHA = dr["FECHA"].ToString();
                    Tarea.PEDIDO = Convert.ToInt32(dr["PEDIDO"].ToString());
                    Tarea.CLIENTE = dr["CLIENTE"].ToString();
                    Tarea.SEGMENTACION = dr["SEGMENTACION"].ToString();
                    Tarea.CLASIFICACION = dr["CLASIFICACION"].ToString();
                    Tarea.DETALLE = dr["DETALLE"].ToString();
                    Tarea.VALOR = Convert.ToDouble(dr["VALOR"].ToString());
                    Tarea.RENTABILIDAD = Convert.ToDouble(dr["RENTABILIDAD"].ToString());
                    Tarea.MARGEN = Convert.ToDouble(dr["MARGEN"].ToString());
                    Tarea.ESTADO = dr["ESTADO"].ToString();
                    Tarea.N_FACTURA = dr["N_FACTURA"].ToString();
                    Tarea.FECHA_FACTURACION = dr["FECHA_FACTURACION"].ToString();
                    Tarea.FECHA_ESTIMADA_DE_FACTURACION = dr["FECHA_ESTIMADA_DE_FACTURACION"].ToString();
                    Tarea.OBSERVACION = dr["OBSERVACION"].ToString();
                    Tarea.VENDEDOR = dr["VENDEDOR"].ToString();
                    Tarea.GERENTE_DE_PRODUCTO = dr["GERENTE_DE_PRODUCTO"].ToString();
                    Tarea.ORDEN_DE_COMPRA = dr["ORDEN_DE_COMPRA"].ToString();
                    Tarea.ORDEN_DE_SERVICIOS_INTERNA = dr["ORDEN_DE_SERVICIOS_INTERNA"].ToString();
                    Tarea.ORDEN_DE_SERVICIOS_EXTERNA = dr["ORDEN_DE_SERVICIOS_EXTERNA"].ToString();
                    Tarea.ORDEN_DE_SERVICIOS_DE_FINANZAS = dr["ORDEN_DE_SERVICIOS_DE_FINANZAS"].ToString();
                    Tarea.SUCURSAL = dr["SUCURSAL"].ToString();
                    Tarea.FechaInicioR = dr["FechaInicioR"].ToString();
                    Tarea.FechaFinalR = dr["FechaFinalR"].ToString();
                    Tarea.ChekRenovacion = Convert.ToInt32(dr["ChekRenovacion"].ToString());
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
        public static EntRespuesta RTA_InsertaNuevoClientes(EntPedido objEntPedido)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevoCliente", cnx);

                cmd.Parameters.AddWithValue("@CLIENTE", objEntPedido.CLIENTE);               
                cmd.Parameters.AddWithValue("@Tipo", objEntPedido.Tipo);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());

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
                    Respuesta.mensaje = "El cliente ya se encuentra registrado..";
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

        public static EntRespuesta RTA_InsertaNuevoClientesGD(EntPedido objEntPedido)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevoClienteGD", cnx);

                cmd.Parameters.AddWithValue("@CLIENTE", objEntPedido.CLIENTE);
                cmd.Parameters.AddWithValue("@Tipo", objEntPedido.Tipo);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                dr.Read();

                respuestaSP = Convert.ToInt32(dr["Respuestas"].ToString());

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
                    Respuesta.mensaje = "El cliente ya se encuentra registrado..";
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
