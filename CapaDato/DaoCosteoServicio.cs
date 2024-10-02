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
    public class DaoCosteoServicio
    {
        public static EntRespuesta RTAInsertaNuevoCosteo(EntCosteoServicio objEntCosteoServicio)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevoCosteo", cnx);

                cmd.Parameters.AddWithValue("@IdCosteo", Convert.ToInt32(objEntCosteoServicio.IdCosteo));
                cmd.Parameters.AddWithValue("@Ticket", objEntCosteoServicio.Ticket);
                if (objEntCosteoServicio.FechaSolicitud == "")
                {
                    cmd.Parameters.AddWithValue("@FechaSolicitud", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaSolicitud", Convert.ToDateTime(objEntCosteoServicio.FechaSolicitud));
                }
                if (objEntCosteoServicio.FechaActual == "")
                {
                    cmd.Parameters.AddWithValue("@FechaActual", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaActual", Convert.ToDateTime(objEntCosteoServicio.FechaActual));
                }
                cmd.Parameters.AddWithValue("@IdVendedor", objEntCosteoServicio.IdVendedor);
                cmd.Parameters.AddWithValue("@Vendedor", objEntCosteoServicio.Vendedor);
                cmd.Parameters.AddWithValue("@Sucursal", objEntCosteoServicio.Sucursal);
                cmd.Parameters.AddWithValue("@Sector", objEntCosteoServicio.Sector);
                cmd.Parameters.AddWithValue("@IdCliente", objEntCosteoServicio.IdCliente);
                cmd.Parameters.AddWithValue("@Cliente", objEntCosteoServicio.Cliente);
                cmd.Parameters.AddWithValue("@Concepto", objEntCosteoServicio.Concepto);
                cmd.Parameters.AddWithValue("@UnidadNegocio", objEntCosteoServicio.UnidadNegocio );
                cmd.Parameters.AddWithValue("@ResponsableDimen", objEntCosteoServicio.ResponsableDimen );
                cmd.Parameters.AddWithValue("@TipoServicio", objEntCosteoServicio.TipoServicio );
                if (objEntCosteoServicio.PlazoEntrega == "")
                {
                    cmd.Parameters.AddWithValue("@PlazoEntrega", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@PlazoEntrega", Convert.ToDateTime(objEntCosteoServicio.PlazoEntrega));
                }
                cmd.Parameters.AddWithValue("@EstadoServicio", objEntCosteoServicio.EstadoServicio);
                if (objEntCosteoServicio.FechaEntregaEsp == "")
                {
                    cmd.Parameters.AddWithValue("@FechaEntregaEsp", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaEntregaEsp", Convert.ToDateTime(objEntCosteoServicio.FechaEntregaEsp));
                }
                if (objEntCosteoServicio.FechaEntregaAlc == "")
                {
                    cmd.Parameters.AddWithValue("@FechaEntregaAlc", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaEntregaAlc", Convert.ToDateTime(objEntCosteoServicio.FechaEntregaAlc));
                }
                cmd.Parameters.AddWithValue("@Revision", objEntCosteoServicio.Revision);
                cmd.Parameters.AddWithValue("@Costo", objEntCosteoServicio.Costo);
                cmd.Parameters.AddWithValue("@Observacion", objEntCosteoServicio.Observacion);
                cmd.Parameters.AddWithValue("@Usuario", objEntCosteoServicio.Usuario);
                cmd.Parameters.AddWithValue("@Tipo", objEntCosteoServicio.Tipo );            
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

        public static List<EntCosteoServicio> ConsultaSp_RTAConsultarCosteServicios(int IdCosteo, int tipo)
        {
            List<EntCosteoServicio> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultarCosteServicios", cnx);
                cmd.Parameters.AddWithValue("@IdCosteo", IdCosteo);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntCosteoServicio>();

                while (dr.Read())
                {
                    EntCosteoServicio Tarea = new EntCosteoServicio();
                    Tarea.IdCosteo = Convert.ToInt32(dr["IdCosteo"].ToString());
                    Tarea.Ticket = dr["Ticket"].ToString();
                    Tarea.FechaSolicitud = dr["FechaSolicitud"].ToString();
                    Tarea.FechaActual = dr["FechaActual"].ToString();
                    Tarea.IdVendedor = Convert.ToInt32(dr["IdVendedor"].ToString());
                    Tarea.Vendedor = dr["Vendedor"].ToString();
                    Tarea.Sucursal = dr["Sucursal"].ToString();
                    Tarea.Sector = dr["Sector"].ToString();
                    Tarea.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
                    Tarea.Cliente = dr["Cliente"].ToString();
                    Tarea.Concepto = dr["Concepto"].ToString();
                    Tarea.UnidadNegocio =dr["UnidadNegocio"].ToString();
                    Tarea.ResponsableDimen = dr["ResponsableDimen"].ToString();
                    Tarea.TipoServicio = dr["TipoServicio"].ToString();
                    Tarea.PlazoEntrega = dr["PlazoEntrega"].ToString();
                    Tarea.EstadoServicio = dr["EstadoServicio"].ToString();
                    Tarea.FechaEntregaEsp = dr["FechaEntregaEsp"].ToString();
                    Tarea.FechaEntregaAlc = dr["FechaEntregaAlc"].ToString();
                    Tarea.Revision = dr["Revision"].ToString();
                    Tarea.Costo = Convert.ToDecimal(dr["Costo"].ToString());
                    Tarea.Observacion = dr["Observacion"].ToString();
                    Tarea.IdSucursal = Convert.ToInt32(dr["IdSucursal"].ToString());
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

        public static List<EntCosteoServicio> ConsultaSp_RTAListaCosteServicios(string FchIni, string FchFin, int IdGerenteCuenta,  string sucursal, string ResponsableDimen, int idFecha)
        {
            List<EntCosteoServicio> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaCosteServicios", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@sucursal", sucursal);
                cmd.Parameters.AddWithValue("@ResponsableDimen", ResponsableDimen);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntCosteoServicio>();

                while (dr.Read())
                {
                    EntCosteoServicio Tarea = new EntCosteoServicio();
                    Tarea.IdCosteo = Convert.ToInt32(dr["IdCosteo"].ToString());
                    Tarea.Ticket = dr["Ticket"].ToString();
                    Tarea.FechaSolicitud = dr["FechaSolicitud"].ToString();
                    Tarea.FechaActual = dr["FechaActual"].ToString();
                    Tarea.IdVendedor = Convert.ToInt32(dr["IdVendedor"].ToString());
                    Tarea.Vendedor = dr["Vendedor"].ToString();
                    Tarea.Sucursal = dr["Sucursal"].ToString();
                    Tarea.Sector = dr["Sector"].ToString();
                    Tarea.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
                    Tarea.Cliente = dr["Cliente"].ToString();
                    Tarea.Concepto = dr["Concepto"].ToString();
                    Tarea.UnidadNegocio = dr["UnidadNegocio"].ToString();
                    Tarea.ResponsableDimen = dr["ResponsableDimen"].ToString();
                    Tarea.TipoServicio = dr["TipoServicio"].ToString();
                    Tarea.PlazoEntrega = dr["PlazoEntrega"].ToString();
                    Tarea.EstadoServicio = dr["EstadoServicio"].ToString();
                    Tarea.FechaEntregaEsp = dr["FechaEntregaEsp"].ToString();
                    Tarea.FechaEntregaAlc = dr["FechaEntregaAlc"].ToString();
                    Tarea.Revision = dr["Revision"].ToString();
                    Tarea.Costo = Convert.ToDecimal(dr["Costo"].ToString());
                    Tarea.Observacion = dr["Observacion"].ToString();
                    Tarea.conteoArchivosAdjuntos = Convert.ToInt32(dr["conteoArchivosAdjuntos"].ToString());
                    Tarea.IdSucursal = Convert.ToInt32(dr["IdSucursal"].ToString());
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
        public static EntRespuesta ConsultaSp_RTAListaCosteServiciosDescargar(string FchIni, string FchFin, int IdGerenteCuenta, string sucursal, string ResponsableDimen, int idFecha)
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

                cmd = new SqlCommand("Sp_RTAListaCosteServicios", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@sucursal", sucursal);
                cmd.Parameters.AddWithValue("@ResponsableDimen", ResponsableDimen);
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
