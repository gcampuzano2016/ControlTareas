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
    public class DaoEgresoInventario
    {
        #region RTAInsertaNuevoEgresoInventario
        public static EntRespuesta RTAInsertaNuevoEgresoInventario(EntEgresoInventario objEgresoInventario)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevoEgresoInventario", cnx);

                cmd.Parameters.AddWithValue("@IdEncabezado", Convert.ToInt32(objEgresoInventario.IdEncabezado));
                cmd.Parameters.AddWithValue("@IdCliente", Convert.ToInt32(objEgresoInventario.IdCliente));
                cmd.Parameters.AddWithValue("@Cod_Usuario", objEgresoInventario.Cod_Usuario);
                cmd.Parameters.AddWithValue("@FechaRegistro", objEgresoInventario.FechaRegistro);
                cmd.Parameters.AddWithValue("@CantidadComprada", objEgresoInventario.CantidadComprada);
                cmd.Parameters.AddWithValue("@SubTotal", objEgresoInventario.SubTotal);
                cmd.Parameters.AddWithValue("@Iva", objEgresoInventario.Iva);
                cmd.Parameters.AddWithValue("@Total", objEgresoInventario.Total);
                cmd.Parameters.AddWithValue("@Observacion", objEgresoInventario.Observacion);

                cmd.Parameters.AddWithValue("@IdDetalle", objEgresoInventario.IdDetalle);
                cmd.Parameters.AddWithValue("@IdInventario", objEgresoInventario.IdInventario);
                cmd.Parameters.AddWithValue("@Cantidad", objEgresoInventario.Cantidad);
                cmd.Parameters.AddWithValue("@PrecioUnitario", objEgresoInventario.PrecioUnitario);
                cmd.Parameters.AddWithValue("@PrecioTotal", objEgresoInventario.PrecioTotal);
                cmd.Parameters.AddWithValue("@Estado", objEgresoInventario.Estado);
                cmd.Parameters.AddWithValue("@Tipo", objEgresoInventario.Tipo);
                cmd.Parameters.AddWithValue("@NombreArchivo", objEgresoInventario.NombreArchivo);
                cmd.Parameters.AddWithValue("@CodigoProceso", objEgresoInventario.CodigoProceso);
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
        #endregion

        #region ConsultaRTAListaEgresos
        public static List<EntEgresoInventario> ConsultaRTAListaEgresos(int IdCliente, string FchIni, string FchFin)
        {
            List<EntEgresoInventario> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaEgresos", cnx);
                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntEgresoInventario>();

                while (dr.Read())
                {
                    EntEgresoInventario Tarea = new EntEgresoInventario();

                    Tarea.IdEncabezado = Convert.ToInt32(dr["IdEncabezado"].ToString());
                    Tarea.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
                    Tarea.Cod_Usuario = dr["Cod_Usuario"].ToString();
                    Tarea.StrFechaRegistro = dr["FechaRegistro"].ToString();
                    Tarea.CantidadComprada = Convert.ToDouble(dr["CantidadComprada"].ToString());
                    Tarea.SubTotal = Convert.ToDouble(dr["SubTotal"].ToString());
                    Tarea.Iva = Convert.ToDouble(dr["Iva"].ToString());
                    Tarea.Total = Convert.ToDouble(dr["Total"].ToString());
                    Tarea.NombreArchivo = dr["NombreArchivo"].ToString();
                    Tarea.CodigoProceso = dr["CodigoProceso"].ToString();
                    Tarea.Nombres = dr["Nombres"].ToString();
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
        #endregion

        #region ConsultaRTAListaEgresosDescargar
        public static EntRespuesta ConsultaRTAListaEgresosDescargar(int IdCliente, string FchIni, string FchFin)
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

                cmd = new SqlCommand("Sp_RTAListaEgresos", cnx);
                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
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
        #endregion

        #region RTA_ConsultaClientePedidoLike
        public static List<EntEgresoInventario> RTA_ConsultaClientePedidoLike(int tipo, string Descripcion)
        {
            List<EntEgresoInventario> objInventario = null;
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cmd = new SqlCommand("RTA_ConsultaLike", cnx);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.Parameters.AddWithValue("@Descripcion", Descripcion);
                cmd.CommandType = CommandType.StoredProcedure;
                cnx.Open();
                dr = cmd.ExecuteReader();
                objInventario = new List<EntEgresoInventario>();
                while (dr.Read())
                {
                    EntEgresoInventario Tarea = new EntEgresoInventario();
                    Tarea.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
                    Tarea.CLIENTE = (dr["CLIENTE"].ToString());
                    objInventario.Add(Tarea);
                }
            }
            catch (Exception ex)
            {
                objInventario = null;

            }
            finally
            {
                cmd.Connection.Close();
            }
            return objInventario;
        }
        #endregion

        #region ConsultaRTAListaClienteInventario
        public static List<EntClienteEgrInventario> ConsultaRTAListaClienteInventario(int IdEncabezado)
        {
            List<EntClienteEgrInventario> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultasEgresosInv", cnx);
                cmd.Parameters.AddWithValue("@IdEncabezado", IdEncabezado);
                cmd.Parameters.AddWithValue("@Op", 1);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntClienteEgrInventario>();

                while (dr.Read())
                {
                    EntClienteEgrInventario Tarea = new EntClienteEgrInventario();

                    Tarea.IdEncabezado = Convert.ToInt32(dr["IdEncabezado"].ToString());
                    Tarea.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
                    //Tarea.Estado = dr["Estado"].ToString();
                    Tarea.Nombres = dr["Nombres"].ToString();
                    Tarea.FechaIngreso = dr["FechaIngreso"].ToString();
                    Tarea.Cod_Usuario = dr["Cod_Usuario"].ToString();
                    Tarea.FechaRegistro = dr["FechaRegistroFormateada"].ToString();
                    Tarea.CantidadComprada = Convert.ToDouble(dr["CantidadComprada"].ToString());
                    Tarea.SubTotal = Convert.ToDouble(dr["SubTotal"].ToString());
                    Tarea.Iva = Convert.ToDouble(dr["Iva"].ToString());
                    Tarea.Total = Convert.ToDouble(dr["Total"].ToString());
                    Tarea.NombreArchivo = dr["NombreArchivo"].ToString();
                    Tarea.CodigoProceso = dr["CodigoProceso"].ToString();
                    Tarea.Observacion = dr["Observacion"].ToString();
                    //Tarea.EstadoEnc = dr["Nombres"].ToString();
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
        #endregion

        #region ConsultaRTAListaDetallesInv
        public static List<EntDetalleInventario> ConsultaRTAListaDetallesInv(int IdEncabezado)
        {
            List<EntDetalleInventario> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultasEgresosInv", cnx);
                cmd.Parameters.AddWithValue("@IdEncabezado", IdEncabezado);
                cmd.Parameters.AddWithValue("@Op", 2);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntDetalleInventario>();

                while (dr.Read())
                {
                    EntDetalleInventario Tarea = new EntDetalleInventario();

                    Tarea.IdEncabezado = Convert.ToInt32(dr["IdEncabezado"].ToString());
                    Tarea.IdDetalle = Convert.ToInt32(dr["IdDetalle"].ToString());
                    Tarea.IdInventario = Convert.ToInt32(dr["IdInventario"].ToString());
                    Tarea.Cantidad = Convert.ToDouble(dr["CantidadDetalle"].ToString());
                    Tarea.PrecioUnitario = Convert.ToDouble(dr["PrecioUnitarioDetalle"].ToString());
                    Tarea.PrecioTotal = Convert.ToDouble(dr["PrecioTotalDetalle"].ToString());

                    Tarea.IdMarca = Convert.ToInt32(dr["IdMarca"].ToString());
                    Tarea.CodigoSAP = dr["CodigoSAP"].ToString();
                    Tarea.NumParte = dr["NumParte"].ToString();
                    Tarea.Descripcion = dr["Descripcion"].ToString();
                    Tarea.CantidadInv = Convert.ToDouble(dr["CantidadInventario"].ToString());
                    Tarea.Ubicacion = dr["Ubicacion"].ToString();
                    Tarea.Almacen = dr["Almacen"].ToString();
                    Tarea.NumSerie = dr["NumSerie"].ToString();
                    Tarea.PrecioUnitarioInv = Convert.ToDouble(dr["PrecioUnitarioInventario"].ToString());
                    Tarea.PrecioTotalInv = Convert.ToDouble(dr["PrecioTotalInventario"].ToString());
                    Tarea.FechaIngreso = dr["FechaIngreso"].ToString();
                    Tarea.Usuario = dr["Usuario"].ToString();
                    //Tarea.EstadoEnc = dr["Nombres"].ToString();
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
        #endregion
    }
}
