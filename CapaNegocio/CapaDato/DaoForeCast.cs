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
    public class DaoForeCast
    {

        CultureInfo ci = new CultureInfo("es-EC");

        #region Constructores
        public DaoForeCast()
        {
            ci.NumberFormat.NumberDecimalSeparator = ","; //especificas el separador para decimales
            ci.NumberFormat.NumberGroupSeparator = ""; //especificas el separador para millares
        }
        #endregion
        public static EntRespuesta RTAInsertaNuevaForeCast(EntForeCast objEntForeCast)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            Int32 respuestaSP = 0;
            string respuestaSP1 = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            //Convert.ToDouble(dataSet.Tables["infoFactura"].Rows[0]["importeTotal"].ToString().Replace(".", ","), ci);
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAInsertaNuevaForeCast", cnx);

                cmd.Parameters.AddWithValue("@IdForeCast", Convert.ToInt32(objEntForeCast.IdForeCast));
                cmd.Parameters.AddWithValue("@IdMarca", objEntForeCast.IdMarca);
                cmd.Parameters.AddWithValue("@IdCliente", objEntForeCast.IdCliente);
                cmd.Parameters.AddWithValue("@IdNegocio", objEntForeCast.IdNegocio);
                cmd.Parameters.AddWithValue("@IdPrioridad", objEntForeCast.IdPrioridad);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", objEntForeCast.IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@IdGerenteProducto", objEntForeCast.IdGerenteProducto);
                cmd.Parameters.AddWithValue("@Marca", objEntForeCast.Marca);
                cmd.Parameters.AddWithValue("@Cliente", objEntForeCast.Cliente);
                cmd.Parameters.AddWithValue("@DetalleProyecto", objEntForeCast.DetalleProyecto);
                cmd.Parameters.AddWithValue("@PVPEstimado", objEntForeCast.PVPEstimado);
                cmd.Parameters.AddWithValue("@Utilidad", objEntForeCast.Utilidad);
                cmd.Parameters.AddWithValue("@UtilidadBrutaEstimada", objEntForeCast.UtilidadBrutaEstimada);
                cmd.Parameters.AddWithValue("@FechaFacturacion", objEntForeCast.FechaFacturacion);
                cmd.Parameters.AddWithValue("@CierreNegocio", objEntForeCast.CierreNegocio);
                cmd.Parameters.AddWithValue("@MesEstimadoCierre", objEntForeCast.MesEstimadoCierre);
                cmd.Parameters.AddWithValue("@Observaciones", objEntForeCast.Observaciones);
                cmd.Parameters.AddWithValue("@GerentedeProducto", objEntForeCast.GerentedeProducto);
                cmd.Parameters.AddWithValue("@GerentedeCuenta", objEntForeCast.GerentedeCuenta);
                cmd.Parameters.AddWithValue("@SegmentodeMercado", objEntForeCast.SegmentodeMercado);
                cmd.Parameters.AddWithValue("@Prioridad", objEntForeCast.Prioridad);
                cmd.Parameters.AddWithValue("@Sucursal", objEntForeCast.Sucursal);
                cmd.Parameters.AddWithValue("@RegistroAprobado", objEntForeCast.RegistroAprobado);
                cmd.Parameters.AddWithValue("@TipoEmpresa", objEntForeCast.TipoEmpresa);
                cmd.Parameters.AddWithValue("@NumeroRegistro", objEntForeCast.NumeroRegistro);
                cmd.Parameters.AddWithValue("@GerenteCuenFabricante", objEntForeCast.GerenteCuenFabricante);
                cmd.Parameters.AddWithValue("@LiderProyectoCliente", objEntForeCast.LiderProyectoCliente);
                cmd.Parameters.AddWithValue("@Mayorista", objEntForeCast.Mayorista);
                cmd.Parameters.AddWithValue("@FormaPago", objEntForeCast.FormaPago);
                if (objEntForeCast.FechaInicioProyecto == "")
                {
                    cmd.Parameters.AddWithValue("@FechaInicioProyecto", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaInicioProyecto", Convert.ToDateTime(objEntForeCast.FechaInicioProyecto));
                }
             
                if (objEntForeCast.FechaFacturacionProyecto == "")
                {
                    cmd.Parameters.AddWithValue("@FechaFacturacionProyecto", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaFacturacionProyecto", Convert.ToDateTime(objEntForeCast.FechaFacturacionProyecto));
                }
                cmd.Parameters.AddWithValue("@Contacto", objEntForeCast.Contacto);
                cmd.Parameters.AddWithValue("@Usuario", objEntForeCast.Usuario);
                cmd.Parameters.AddWithValue("@Justificacion", objEntForeCast.Justificacion);
                cmd.Parameters.AddWithValue("@prsd", objEntForeCast.prsd);
                cmd.Parameters.AddWithValue("@ObservacionCierre", objEntForeCast.ObservacionCierre);
                cmd.Parameters.AddWithValue("@TipoProyecto", objEntForeCast.TipoProyecto);
                cmd.Parameters.AddWithValue("@Tipo", objEntForeCast.Tipo);
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

        public static EntRespuesta RTAAgregarPrioriodadForeCast(EntForeCast objEntForeCast)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            Int32 respuestaSP = 0;
            string respuestaSP1 = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            //Convert.ToDouble(dataSet.Tables["infoFactura"].Rows[0]["importeTotal"].ToString().Replace(".", ","), ci);
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAAgregarPrioriodadForeCast", cnx);
                cmd.Parameters.AddWithValue("@Id_ForeCast", Convert.ToInt32(objEntForeCast.IdForeCast));
                cmd.Parameters.AddWithValue("@ActivarPrioridad", Convert.ToInt32(objEntForeCast.ActivarPrioridad));
                cmd.Parameters.AddWithValue("@Tipo", Convert.ToInt32(objEntForeCast.Tipo));
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

        public static EntRespuesta RTAInsertaNuevaForeCastGD(EntForeCast objEntForeCast)
        {
            EntRespuesta Respuesta = new EntRespuesta();
            Int32 respuestaSP = 0;
            string respuestaSP1 = "";
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            //Convert.ToDouble(dataSet.Tables["infoFactura"].Rows[0]["importeTotal"].ToString().Replace(".", ","), ci);
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAInsertaNuevaForeCastGD", cnx);

                cmd.Parameters.AddWithValue("@IdForeCast", Convert.ToInt32(objEntForeCast.IdForeCast));
                cmd.Parameters.AddWithValue("@IdMarca", objEntForeCast.IdMarca);
                cmd.Parameters.AddWithValue("@IdCliente", objEntForeCast.IdCliente);
                cmd.Parameters.AddWithValue("@IdNegocio", objEntForeCast.IdNegocio);
                cmd.Parameters.AddWithValue("@IdPrioridad", objEntForeCast.IdPrioridad);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", objEntForeCast.IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@IdGerenteProducto", objEntForeCast.IdGerenteProducto);
                cmd.Parameters.AddWithValue("@Marca", objEntForeCast.Marca);
                cmd.Parameters.AddWithValue("@Cliente", objEntForeCast.Cliente);
                cmd.Parameters.AddWithValue("@DetalleProyecto", objEntForeCast.DetalleProyecto);
                cmd.Parameters.AddWithValue("@PVPEstimado", objEntForeCast.PVPEstimado);
                cmd.Parameters.AddWithValue("@Utilidad", objEntForeCast.Utilidad);
                cmd.Parameters.AddWithValue("@UtilidadBrutaEstimada", objEntForeCast.UtilidadBrutaEstimada);
                cmd.Parameters.AddWithValue("@FechaFacturacion", objEntForeCast.FechaFacturacion);
                cmd.Parameters.AddWithValue("@CierreNegocio", objEntForeCast.CierreNegocio);
                cmd.Parameters.AddWithValue("@MesEstimadoCierre", objEntForeCast.MesEstimadoCierre);
                cmd.Parameters.AddWithValue("@Observaciones", objEntForeCast.Observaciones);
                cmd.Parameters.AddWithValue("@GerentedeProducto", objEntForeCast.GerentedeProducto);
                cmd.Parameters.AddWithValue("@GerentedeCuenta", objEntForeCast.GerentedeCuenta);
                cmd.Parameters.AddWithValue("@SegmentodeMercado", objEntForeCast.SegmentodeMercado);
                cmd.Parameters.AddWithValue("@Prioridad", objEntForeCast.Prioridad);
                cmd.Parameters.AddWithValue("@Sucursal", objEntForeCast.Sucursal);
                cmd.Parameters.AddWithValue("@RegistroAprobado", objEntForeCast.RegistroAprobado);
                cmd.Parameters.AddWithValue("@TipoEmpresa", objEntForeCast.TipoEmpresa);
                cmd.Parameters.AddWithValue("@NumeroRegistro", objEntForeCast.NumeroRegistro);
                cmd.Parameters.AddWithValue("@GerenteCuenFabricante", objEntForeCast.GerenteCuenFabricante);
                cmd.Parameters.AddWithValue("@LiderProyectoCliente", objEntForeCast.LiderProyectoCliente);
                cmd.Parameters.AddWithValue("@Mayorista", objEntForeCast.Mayorista);
                cmd.Parameters.AddWithValue("@FormaPago", objEntForeCast.FormaPago);
                if (objEntForeCast.FechaInicioProyecto == "")
                {
                    cmd.Parameters.AddWithValue("@FechaInicioProyecto", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaInicioProyecto", Convert.ToDateTime(objEntForeCast.FechaInicioProyecto));
                }

                if (objEntForeCast.FechaFacturacionProyecto == "")
                {
                    cmd.Parameters.AddWithValue("@FechaFacturacionProyecto", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaFacturacionProyecto", Convert.ToDateTime(objEntForeCast.FechaFacturacionProyecto));
                }
                cmd.Parameters.AddWithValue("@Contacto", objEntForeCast.Contacto);
                cmd.Parameters.AddWithValue("@Usuario", objEntForeCast.Usuario);
                cmd.Parameters.AddWithValue("@Justificacion", objEntForeCast.Justificacion);
                cmd.Parameters.AddWithValue("@Tipo", objEntForeCast.Tipo);
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

        public static List<EntForeCast> ConsultaSp_RTAListaForeCast(string FechaFacturacion, string MesEstimadoCierre, int IdCliente, int IdGerenteCuenta, string IdGestorProducto, string IdMarca, string sucursal,string SegmentodeMercado, string StrIdPrioridad,string Idusuario,int idFecha, int Anio,string cierrenegocio,int IdPrioProyecto,int ProyecEstrategico,string TipoProyecto)
        {
            List<EntForeCast> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaForeCast", cnx);
                cmd.Parameters.AddWithValue("@FechaFacturacion", FechaFacturacion);
                cmd.Parameters.AddWithValue("@MesEstimadoCierre", MesEstimadoCierre);
                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@IdGestorProducto", IdGestorProducto);
                cmd.Parameters.AddWithValue("@IdMarca", IdMarca);
                cmd.Parameters.AddWithValue("@sucursal", sucursal);
                cmd.Parameters.AddWithValue("@SegmentodeMercado", SegmentodeMercado);
                cmd.Parameters.AddWithValue("@IdPrioridad", StrIdPrioridad);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@cierrenegocio", cierrenegocio);
                cmd.Parameters.AddWithValue("@ActivarPrioridad", IdPrioProyecto);
                cmd.Parameters.AddWithValue("@ProyecEstrategico", ProyecEstrategico);
                cmd.Parameters.AddWithValue("@TipoProyecto", TipoProyecto);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntForeCast>();

                while (dr.Read())
                {
                    EntForeCast Tarea = new EntForeCast();
                    Tarea.IdForeCast = Convert.ToInt32(dr["IdForeCast"].ToString());
                    Tarea.IdMarca = Convert.ToInt32(dr["IdMarca"].ToString());
                    Tarea.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
                    Tarea.IdNegocio = Convert.ToInt32(dr["IdNegocio"].ToString());
                    Tarea.IdPrioridad = Convert.ToInt32(dr["IdPrioridad"].ToString());
                    Tarea.Marca = dr["Marca"].ToString();
                    Tarea.Cliente = dr["Cliente"].ToString();
                    Tarea.DetalleProyecto = dr["DetalleProyecto"].ToString();

                    Tarea.PVPEstimado = Convert.ToDecimal(dr["PVPEstimado"].ToString());
                    Tarea.Utilidad = Convert.ToDecimal(dr["Utilidad"].ToString());
                    Tarea.UtilidadBrutaEstimada = Convert.ToDecimal(dr["UtilidadBrutaEstimada"].ToString());

                    Tarea.strPVPEstimado = Convert.ToDecimal(dr["PVPEstimado"].ToString()).ToString("N");
                    Tarea.strUtilidad = Convert.ToDecimal(dr["Utilidad"].ToString()).ToString("N");
                    Tarea.strUtilidadBrutaEstimada = Convert.ToDecimal(dr["UtilidadBrutaEstimada"].ToString()).ToString("N");

                    Tarea.FechaFacturacion = dr["FechaFacturacion"].ToString();
                    Tarea.StrCierreNegocio = dr["CierreNegocio"].ToString();
                    Tarea.MesEstimadoCierre = dr["MesEstimadoCierre"].ToString();
                    Tarea.Observaciones = dr["Observaciones"].ToString();
                    Tarea.GerentedeProducto = dr["GerentedeProducto"].ToString();
                    Tarea.GerentedeCuenta = dr["GerentedeCuenta"].ToString();
                    Tarea.SegmentodeMercado = dr["SegmentodeMercado"].ToString();
                    Tarea.Prioridad = dr["Prioridad"].ToString();
                    Tarea.Sucursal = dr["Sucursal"].ToString();
                    Tarea.RegistroAprobado = dr["RegistroAprobado"].ToString();
                    Tarea.FechaCobro = dr["FechaCobro"].ToString();
                    Tarea.Usuario = dr["Usuario"].ToString();
                    Tarea.TipoEmpresa = dr["TipoEmpresa"].ToString();

                    Tarea.NumeroRegistro = dr["NumeroRegistro"].ToString();
                    Tarea.GerenteCuenFabricante = dr["GerenteCuenFabricante"].ToString();
                    Tarea.LiderProyectoCliente = dr["LiderProyectoCliente"].ToString();
                    Tarea.Mayorista = dr["Mayorista"].ToString();
                    Tarea.FormaPago = dr["FormaPago"].ToString();
                    Tarea.prsd = Convert.ToInt32(dr["prsd"].ToString());
                    Tarea.ObservacionCierre = dr["ObservacionCierre"].ToString();
                    Tarea.ActivarPrioridad = Convert.ToInt32(dr["ActivarPrioridad"].ToString());
                    Tarea.ProyecEstrategico = Convert.ToInt32(dr["ProyecEstrategico"].ToString());
                    Tarea.TipoProyecto = dr["TipoProyecto"].ToString();
                    Tarea.ActivarPrioridadhtml = dr["EstadoPrioridad"].ToString();
                    Tarea.ProyecEstrategicohtml = dr["Estrategico"].ToString();
                    Tarea.ValorVacio = dr["ValorVacio"].ToString();
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

        public static List<EntForeCast> ConsultaSp_RTAListaForeCastGD(string FechaFacturacion, string MesEstimadoCierre, int IdCliente, int IdGerenteCuenta, int IdGestorProducto, string IdMarca, string sucursal, string SegmentodeMercado, string StrIdPrioridad, string Idusuario, int idFecha, int Anio, string cierrenegocio)
        {
            List<EntForeCast> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaForeCastGD", cnx);
                cmd.Parameters.AddWithValue("@FechaFacturacion", FechaFacturacion);
                cmd.Parameters.AddWithValue("@MesEstimadoCierre", MesEstimadoCierre);
                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@IdGestorProducto", IdGestorProducto);
                cmd.Parameters.AddWithValue("@IdMarca", IdMarca);
                cmd.Parameters.AddWithValue("@sucursal", sucursal);
                cmd.Parameters.AddWithValue("@SegmentodeMercado", SegmentodeMercado);
                cmd.Parameters.AddWithValue("@IdPrioridad", StrIdPrioridad);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@cierrenegocio", cierrenegocio);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntForeCast>();

                while (dr.Read())
                {
                    EntForeCast Tarea = new EntForeCast();
                    Tarea.IdForeCast = Convert.ToInt32(dr["IdForeCast"].ToString());
                    Tarea.IdMarca = Convert.ToInt32(dr["IdMarca"].ToString());
                    Tarea.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
                    Tarea.IdNegocio = Convert.ToInt32(dr["IdNegocio"].ToString());
                    Tarea.IdPrioridad = Convert.ToInt32(dr["IdPrioridad"].ToString());
                    Tarea.Marca = dr["Marca"].ToString();
                    Tarea.Cliente = dr["Cliente"].ToString();
                    Tarea.DetalleProyecto = dr["DetalleProyecto"].ToString();

                    Tarea.PVPEstimado = Convert.ToDecimal(dr["PVPEstimado"].ToString());
                    Tarea.Utilidad = Convert.ToDecimal(dr["Utilidad"].ToString());
                    Tarea.UtilidadBrutaEstimada = Convert.ToDecimal(dr["UtilidadBrutaEstimada"].ToString());

                    Tarea.strPVPEstimado = Convert.ToDecimal(dr["PVPEstimado"].ToString()).ToString("N");
                    Tarea.strUtilidad = Convert.ToDecimal(dr["Utilidad"].ToString()).ToString("N");
                    Tarea.strUtilidadBrutaEstimada = Convert.ToDecimal(dr["UtilidadBrutaEstimada"].ToString()).ToString("N");

                    Tarea.FechaFacturacion = dr["FechaFacturacion"].ToString();
                    Tarea.StrCierreNegocio = dr["CierreNegocio"].ToString();
                    Tarea.MesEstimadoCierre = dr["MesEstimadoCierre"].ToString();
                    Tarea.Observaciones = dr["Observaciones"].ToString();
                    Tarea.GerentedeProducto = dr["GerentedeProducto"].ToString();
                    Tarea.GerentedeCuenta = dr["GerentedeCuenta"].ToString();
                    Tarea.SegmentodeMercado = dr["SegmentodeMercado"].ToString();
                    Tarea.Prioridad = dr["Prioridad"].ToString();
                    Tarea.Sucursal = dr["Sucursal"].ToString();
                    Tarea.RegistroAprobado = dr["RegistroAprobado"].ToString();
                    Tarea.FechaCobro = dr["FechaCobro"].ToString();
                    Tarea.Usuario = dr["Usuario"].ToString();
                    Tarea.TipoEmpresa = dr["TipoEmpresa"].ToString();

                    Tarea.NumeroRegistro = dr["NumeroRegistro"].ToString();
                    Tarea.GerenteCuenFabricante = dr["GerenteCuenFabricante"].ToString();
                    Tarea.LiderProyectoCliente = dr["LiderProyectoCliente"].ToString();
                    Tarea.Mayorista = dr["Mayorista"].ToString();
                    Tarea.FormaPago = dr["FormaPago"].ToString();
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
        public static EntRespuesta ConsultaSp_RTAListaForeCastDescargar(string FechaFacturacion, string MesEstimadoCierre, int IdCliente, int IdGerenteCuenta, string IdGestorProducto, string IdMarca, string sucursal, string SegmentodeMercado, string StrIdPrioridad, string Idusuario, int idFecha, int Anio, string cierrenegocio,int IdPrioProyecto, int ProyecEstrategico,string TipoProyecto)
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

                cmd = new SqlCommand("Sp_RTAListaForeCastDescargar", cnx);
                cmd.Parameters.AddWithValue("@FechaFacturacion", FechaFacturacion);
                cmd.Parameters.AddWithValue("@MesEstimadoCierre", MesEstimadoCierre);
                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@IdGestorProducto", IdGestorProducto);
                cmd.Parameters.AddWithValue("@IdMarca", IdMarca);
                cmd.Parameters.AddWithValue("@sucursal", sucursal);
                cmd.Parameters.AddWithValue("@SegmentodeMercado", SegmentodeMercado);
                cmd.Parameters.AddWithValue("@IdPrioridad", StrIdPrioridad);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@cierrenegocio", cierrenegocio);
                cmd.Parameters.AddWithValue("@ActivarPrioridad", IdPrioProyecto);
                cmd.Parameters.AddWithValue("@ProyecEstrategico", ProyecEstrategico);
                cmd.Parameters.AddWithValue("@TipoProyecto", TipoProyecto);
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

        public static EntRespuesta ConsultaSp_RTAListaForeCastDescargarPersonalizado(string FechaFacturacion, string MesEstimadoCierre, int IdCliente, int IdGerenteCuenta, string IdGestorProducto, string IdMarca, string sucursal, string SegmentodeMercado, string StrIdPrioridad, string Idusuario, int idFecha, int Anio, string cierrenegocio, int IdPrioProyecto, int ProyecEstrategico)
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

                cmd = new SqlCommand("Sp_RTAListaForeCastDescargarPersonalizado", cnx);
                cmd.Parameters.AddWithValue("@FechaFacturacion", FechaFacturacion);
                cmd.Parameters.AddWithValue("@MesEstimadoCierre", MesEstimadoCierre);
                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@IdGestorProducto", IdGestorProducto);
                cmd.Parameters.AddWithValue("@IdMarca", IdMarca);
                cmd.Parameters.AddWithValue("@sucursal", sucursal);
                cmd.Parameters.AddWithValue("@SegmentodeMercado", SegmentodeMercado);
                cmd.Parameters.AddWithValue("@IdPrioridad", StrIdPrioridad);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@cierrenegocio", cierrenegocio);
                cmd.Parameters.AddWithValue("@ActivarPrioridad", IdPrioProyecto);
                cmd.Parameters.AddWithValue("@ProyecEstrategico", ProyecEstrategico);
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

        public static EntRespuesta ConsultaSp_RTAListaForeCastDescargarGerencia(string FechaFacturacion, string MesEstimadoCierre, int IdCliente, int IdGerenteCuenta, string IdGestorProducto, string IdMarca, string sucursal, string SegmentodeMercado, string StrIdPrioridad, string Idusuario, int idFecha, int Anio, string cierrenegocio, int IdPrioProyecto, int ProyecEstrategico)
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

                cmd = new SqlCommand("Sp_RTAListaForeCastDescargarGerencia", cnx);
                cmd.Parameters.AddWithValue("@FechaFacturacion", FechaFacturacion);
                cmd.Parameters.AddWithValue("@MesEstimadoCierre", MesEstimadoCierre);
                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@IdGestorProducto", IdGestorProducto);
                cmd.Parameters.AddWithValue("@IdMarca", IdMarca);
                cmd.Parameters.AddWithValue("@sucursal", sucursal);
                cmd.Parameters.AddWithValue("@SegmentodeMercado", SegmentodeMercado);
                cmd.Parameters.AddWithValue("@IdPrioridad", StrIdPrioridad);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@cierrenegocio", cierrenegocio);
                cmd.Parameters.AddWithValue("@ActivarPrioridad", IdPrioProyecto);
                cmd.Parameters.AddWithValue("@ProyecEstrategico", ProyecEstrategico);
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

        public static EntRespuesta ConsultaSp_RTAListaForeCastGDDescargar(string FechaFacturacion, string MesEstimadoCierre, int IdCliente, int IdGerenteCuenta, int IdGestorProducto, string IdMarca, string sucursal, string SegmentodeMercado, string StrIdPrioridad, string Idusuario, int idFecha, int Anio, string cierrenegocio)
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

                cmd = new SqlCommand("Sp_RTAListaForeCastDescargarGD", cnx);
                cmd.Parameters.AddWithValue("@FechaFacturacion", FechaFacturacion);
                cmd.Parameters.AddWithValue("@MesEstimadoCierre", MesEstimadoCierre);
                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@IdGestorProducto", IdGestorProducto);
                cmd.Parameters.AddWithValue("@IdMarca", IdMarca);
                cmd.Parameters.AddWithValue("@sucursal", sucursal);
                cmd.Parameters.AddWithValue("@SegmentodeMercado", SegmentodeMercado);
                cmd.Parameters.AddWithValue("@IdPrioridad", StrIdPrioridad);
                cmd.Parameters.AddWithValue("@Idusuario", Idusuario);
                cmd.Parameters.AddWithValue("@idFecha", idFecha);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@cierrenegocio", cierrenegocio);
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
        public static List<EntForeCast> ConsultaSp_RTAConsultarForeCast(int IdForeCast, int tipo)
        {
            List<EntForeCast> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultarForeCast", cnx);
                cmd.Parameters.AddWithValue("@IdForeCast", IdForeCast);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntForeCast>();

                while (dr.Read())
                {
                    EntForeCast Tarea = new EntForeCast();
                    Tarea.IdForeCast = Convert.ToInt32(dr["IdForeCast"].ToString());
                    Tarea.IdMarca = Convert.ToInt32(dr["IdMarca"].ToString());
                    Tarea.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
                    Tarea.IdNegocio = Convert.ToInt32(dr["IdNegocio"].ToString());
                    Tarea.IdPrioridad = Convert.ToInt32(dr["IdPrioridad"].ToString());
                    Tarea.IdGerenteCuenta = Convert.ToInt32(dr["IdGerenteCuenta"].ToString());
                    Tarea.IdGerenteProducto = Convert.ToInt32(dr["IdGerenteProducto"].ToString());
                    Tarea.Marca = dr["Marca"].ToString();
                    Tarea.Cliente = dr["Cliente"].ToString();
                    Tarea.DetalleProyecto = dr["DetalleProyecto"].ToString();
                    Tarea.PVPEstimado = Convert.ToDecimal(dr["PVPEstimado"].ToString());
                    Tarea.Utilidad = Convert.ToDecimal(dr["Utilidad"].ToString());
                    Tarea.UtilidadBrutaEstimada = Convert.ToDecimal(dr["UtilidadBrutaEstimada"].ToString());
                    Tarea.FechaFacturacion = dr["FechaFacturacion"].ToString();
                    Tarea.CierreNegocio = Convert.ToDouble(dr["CierreNegocio"].ToString());
                    Tarea.MesEstimadoCierre = dr["MesEstimadoCierre"].ToString();
                    Tarea.Observaciones = dr["Observaciones"].ToString();
                    Tarea.GerentedeProducto = dr["GerentedeProducto"].ToString();
                    Tarea.GerentedeCuenta = dr["GerentedeCuenta"].ToString();
                    Tarea.SegmentodeMercado = dr["SegmentodeMercado"].ToString();
                    Tarea.Prioridad = dr["Prioridad"].ToString();
                    Tarea.Sucursal = dr["Sucursal"].ToString();
                    Tarea.RegistroAprobado = dr["RegistroAprobado"].ToString();
                    Tarea.FechaCobro = dr["FechaCobro"].ToString();
                    Tarea.Usuario = dr["Usuario"].ToString();
                    Tarea.TipoEmpresa = dr["TipoEmpresa"].ToString();

                    Tarea.NumeroRegistro = dr["NumeroRegistro"].ToString();
                    Tarea.GerenteCuenFabricante = dr["GerenteCuenFabricante"].ToString();
                    Tarea.LiderProyectoCliente = dr["LiderProyectoCliente"].ToString();
                    Tarea.Mayorista = dr["Mayorista"].ToString();
                    Tarea.FormaPago = dr["FormaPago"].ToString();

                    Tarea.FechaInicioProyecto = dr["FechaInicioProyecto"].ToString();
                    Tarea.FechaFacturacionProyecto = dr["FechaFacturacionProyecto"].ToString();
                    Tarea.Contacto = dr["Contacto"].ToString();

                    Tarea.strPVPEstimado = Convert.ToDecimal(dr["PVPEstimado"].ToString()).ToString("N");
                    Tarea.strUtilidad = Convert.ToDecimal(dr["Utilidad"].ToString()).ToString("N");
                    Tarea.strUtilidadBrutaEstimada = Convert.ToDecimal(dr["UtilidadBrutaEstimada"].ToString()).ToString("N");
                    Tarea.Justificacion = dr["Justificacion"].ToString();
                    Tarea.prsd = Convert.ToInt32(dr["prsd"].ToString());
                    Tarea.Cod_Usuario = dr["Cod_Usuario"].ToString();
                    Tarea.ObservacionCierre = dr["ObservacionCierre"].ToString();
                    Tarea.TipoProyecto = dr["TipoProyecto"].ToString();
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

        public static List<EntForeCast> ConsultaSp_RTAConsultarForeCastGD(int IdForeCast, int tipo)
        {
            List<EntForeCast> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultarForeCastGD", cnx);
                cmd.Parameters.AddWithValue("@IdForeCast", IdForeCast);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntForeCast>();

                while (dr.Read())
                {
                    EntForeCast Tarea = new EntForeCast();
                    Tarea.IdForeCast = Convert.ToInt32(dr["IdForeCast"].ToString());
                    Tarea.IdMarca = Convert.ToInt32(dr["IdMarca"].ToString());
                    Tarea.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
                    Tarea.IdNegocio = Convert.ToInt32(dr["IdNegocio"].ToString());
                    Tarea.IdPrioridad = Convert.ToInt32(dr["IdPrioridad"].ToString());
                    Tarea.IdGerenteCuenta = Convert.ToInt32(dr["IdGerenteCuenta"].ToString());
                    Tarea.IdGerenteProducto = Convert.ToInt32(dr["IdGerenteProducto"].ToString());
                    Tarea.Marca = dr["Marca"].ToString();
                    Tarea.Cliente = dr["Cliente"].ToString();
                    Tarea.DetalleProyecto = dr["DetalleProyecto"].ToString();
                    Tarea.PVPEstimado = Convert.ToDecimal(dr["PVPEstimado"].ToString());
                    Tarea.Utilidad = Convert.ToDecimal(dr["Utilidad"].ToString());
                    Tarea.UtilidadBrutaEstimada = Convert.ToDecimal(dr["UtilidadBrutaEstimada"].ToString());
                    Tarea.FechaFacturacion = dr["FechaFacturacion"].ToString();
                    Tarea.CierreNegocio = Convert.ToDouble(dr["CierreNegocio"].ToString());
                    Tarea.MesEstimadoCierre = dr["MesEstimadoCierre"].ToString();
                    Tarea.Observaciones = dr["Observaciones"].ToString();
                    Tarea.GerentedeProducto = dr["GerentedeProducto"].ToString();
                    Tarea.GerentedeCuenta = dr["GerentedeCuenta"].ToString();
                    Tarea.SegmentodeMercado = dr["SegmentodeMercado"].ToString();
                    Tarea.Prioridad = dr["Prioridad"].ToString();
                    Tarea.Sucursal = dr["Sucursal"].ToString();
                    Tarea.RegistroAprobado = dr["RegistroAprobado"].ToString();
                    Tarea.FechaCobro = dr["FechaCobro"].ToString();
                    Tarea.Usuario = dr["Usuario"].ToString();
                    Tarea.TipoEmpresa = dr["TipoEmpresa"].ToString();

                    Tarea.NumeroRegistro = dr["NumeroRegistro"].ToString();
                    Tarea.GerenteCuenFabricante = dr["GerenteCuenFabricante"].ToString();
                    Tarea.LiderProyectoCliente = dr["LiderProyectoCliente"].ToString();
                    Tarea.Mayorista = dr["Mayorista"].ToString();
                    Tarea.FormaPago = dr["FormaPago"].ToString();

                    Tarea.FechaInicioProyecto = dr["FechaInicioProyecto"].ToString();
                    Tarea.FechaFacturacionProyecto = dr["FechaFacturacionProyecto"].ToString();
                    Tarea.Contacto = dr["Contacto"].ToString();

                    Tarea.strPVPEstimado = Convert.ToDecimal(dr["PVPEstimado"].ToString()).ToString("N");
                    Tarea.strUtilidad = Convert.ToDecimal(dr["Utilidad"].ToString()).ToString("N");
                    Tarea.strUtilidadBrutaEstimada = Convert.ToDecimal(dr["UtilidadBrutaEstimada"].ToString()).ToString("N");
                    Tarea.Justificacion = dr["Justificacion"].ToString();
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
        public static List<EntForeCast> ConsultaSp_RTAValidarClienteForeCast(string Descripcion, int tipo)
        {
            List<EntForeCast> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAValidarClienteForeCast", cnx);
                cmd.Parameters.AddWithValue("@Descripcion", Descripcion);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntForeCast>();

                while (dr.Read())
                {
                    EntForeCast Tarea = new EntForeCast();

                    if (tipo == 1)
                    {
                        Tarea.IdForeCast = Convert.ToInt32(dr["IdForeCast"].ToString());
                        listaTareas.Add(Tarea);
                    }
                    else if(tipo == 2)
                    {
                        Tarea.TipoEmpresa = dr["TipoEmpresa"].ToString();
                        Tarea.SegmentodeMercado = dr["SegmentodeMercado"].ToString();
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

    }
}
