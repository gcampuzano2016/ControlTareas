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
    public class DaoMantenimiento
    {

        public static EntRespuesta RTA_ConsultaReporteServiciosDescarga(int Anio, String Gestor, String Sucursal, String Estado, String Area, String GerenteCuenta)
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

                cmd = new SqlCommand("RTA_ConsultaReporteServicios", cnx);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@Gestor", Gestor);
                cmd.Parameters.AddWithValue("@Sucursal", Sucursal);
                cmd.Parameters.AddWithValue("@Estado", Estado);
                cmd.Parameters.AddWithValue("@Area", Area);
                cmd.Parameters.AddWithValue("@GerenteCuenta", GerenteCuenta);

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

        public static EntRespuesta Sp_RTA_ConsultarOrdenServicioDescarga(int Tipo, String ORDEN)
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

                cmd = new SqlCommand("Sp_RTA_ConsultarOrdenServicio", cnx);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.Parameters.AddWithValue("@ORDEN", ORDEN);

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
        public static EntRespuesta RTA_InsertaNuevoMantenimiento(EntMantenimiento objEntMantenimiento)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevoMantenimiento", cnx);

                cmd.Parameters.AddWithValue("@IdRequerimiento", objEntMantenimiento.IdRequerimiento);
                cmd.Parameters.AddWithValue("@IdServicio", objEntMantenimiento.IdServicio);
                cmd.Parameters.AddWithValue("@Orden", objEntMantenimiento.Orden);
                cmd.Parameters.AddWithValue("@OrdenServicio", objEntMantenimiento.OrdenServicio);
                cmd.Parameters.AddWithValue("@FechaInicio", Convert.ToDateTime(objEntMantenimiento.FechaInicio));
                //cmd.Parameters.AddWithValue("@FechaFinal", Convert.ToDateTime(objEntMantenimiento.FechaFinal));
                if (objEntMantenimiento.FechaFinal == "")
                {
                    cmd.Parameters.AddWithValue("@FechaFinal", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FechaFinal", Convert.ToDateTime(objEntMantenimiento.FechaFinal));
                }
                cmd.Parameters.AddWithValue("@Valor", objEntMantenimiento.Valor);
                cmd.Parameters.AddWithValue("@Descripcion", objEntMantenimiento.Descripcion);
                cmd.Parameters.AddWithValue("@Observacion", objEntMantenimiento.Observacion);
                cmd.Parameters.AddWithValue("@Clasificacion", objEntMantenimiento.Clasificacion);
                cmd.Parameters.AddWithValue("@Estado", objEntMantenimiento.Estado);
                cmd.Parameters.AddWithValue("@FechIngreso", Convert.ToDateTime(objEntMantenimiento.FechIngreso));
                cmd.Parameters.AddWithValue("@Tipo", objEntMantenimiento.Tipo);
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

        public static List<EntMantenimiento> ConsultSp_RTAConsultaMantenimientoContrato(string FchIni, string FchFin, string orden, int tipo,int IdRequerimiento)
        {
            List<EntMantenimiento> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultaMantenimientoContrato", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@numeroOS", orden);
                cmd.Parameters.AddWithValue("@IdRequerimiento", IdRequerimiento);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntMantenimiento>();

                while (dr.Read())
                {
                    EntMantenimiento Tarea = new EntMantenimiento();
                    Tarea.IdRequerimiento = Convert.ToInt32(dr["IdRequerimiento"].ToString());
                    Tarea.IdServicio = Convert.ToInt32(dr["IdServicio"].ToString());
                    Tarea.Orden = dr["Orden"].ToString();
                    Tarea.FechaInicio = dr["FechaInicio"].ToString();
                    Tarea.FechaFinal = dr["FechaFinal"].ToString();
                    Tarea.Descripcion = dr["Descripcion"].ToString();
                    Tarea.Observacion = dr["Observacion"].ToString();
                    Tarea.Clasificacion = dr["Clasificacion"].ToString();
                    Tarea.Valor = Convert.ToDouble(dr["Valor"].ToString());
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
        public static List<EntMantenimiento> ConsultSp_RTAListaMantenimientoContrato(string FchIni, string FchFin, string orden,int IdCliente, string Clasificacion, int tipo)
        {
            List<EntMantenimiento> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaMantenimientoContrato", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@numeroOS", orden);
                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@Clasificacion", Clasificacion);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntMantenimiento>();

                while (dr.Read())
                {
                    EntMantenimiento Tarea = new EntMantenimiento();
                    Tarea.IdRequerimiento = Convert.ToInt32(dr["IdRequerimiento"].ToString());
                    Tarea.IdServicio = Convert.ToInt32(dr["IdServicio"].ToString());
                    Tarea.CLIENTE = dr["CLIENTE"].ToString();
                    Tarea.Orden = dr["Orden"].ToString();
                    Tarea.FechaInicio = dr["FechaInicio"].ToString();
                    Tarea.FechaFinal = dr["FechaFinal"].ToString();
                    Tarea.Descripcion = dr["Descripcion"].ToString();
                    Tarea.Clasificacion = dr["Clasificacion"].ToString();
                    Tarea.conteoArchivosAdjuntos = Convert.ToInt32(dr["conteoArchivosAdjuntos"].ToString());
                    Tarea.Valor =Convert.ToDouble(dr["Valor"].ToString());
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

        public static EntRespuesta ConsultSp_RTAListaMantenimientoContratoDescargar(string FchIni, string FchFin, string orden, int IdCliente, string Clasificacion, int tipo)
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

                cmd = new SqlCommand("Sp_RTAListaMantenimientoContratoDescarga", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@numeroOS", orden);
                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@Clasificacion", Clasificacion);
                cmd.Parameters.AddWithValue("@tipo", tipo);
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

        //METODO 1 
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

        // METODO 2 
        public static List<EntMantenimiento> Sp_RTA_ConsultarOrdenServicio(int Tipo, String ORDEN)
        {
            List<EntMantenimiento> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTA_ConsultarOrdenServicio", cnx);
                cmd.Parameters.AddWithValue("@Tipo", Tipo);
                cmd.Parameters.AddWithValue("@ORDEN", ORDEN);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntMantenimiento>();

                while (dr.Read())
                {
                    EntMantenimiento Tarea = new EntMantenimiento();
                    if (Tipo == 1)
                    {
                        Tarea.ORDEN = dr["ORDEN"].ToString();
                        Tarea.ID_TAREA = Convert.ToInt32(dr["ID_TAREA"].ToString());
                        Tarea.ID_COMPRA = dr["ID_COMPRA"].ToString();
                        Tarea.NOM_RESPONSABLE = dr["NOM_RESPONSABLE"].ToString();
                        Tarea.NOM_EMPRESA = dr["NOM_EMPRESA"].ToString();
                        Tarea.DETALLE = dr["DETALLE"].ToString();
                        Tarea.FECHA_CREACION = dr["FECHA_CREACION"].ToString();

                    }
                    if (Tipo == 2)
                    {
                        Tarea.ID_TAREA = Convert.ToInt32(dr["ID_TAREA"].ToString());
                        Tarea.ORDEN = dr["ORDEN"].ToString();
                        Tarea.FECHA_INICIO = dr["FECHA_INICIO"].ToString();
                        Tarea.FECHA_FIN = dr["FECHA_FIN"].ToString();
                        Tarea.TIEMPO = dr["TIEMPO"].ToString();
                        Tarea.DETALLE_TAREA = dr["DETALLE_TAREA"].ToString();
                        Tarea.OBSERVACIONES = dr["OBSERVACIONES"].ToString();

                    }
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

        //METODO 3 
        public static List<EntMantenimiento> RTA_ConsultaReporteServicios(int Anio, String Gestor, String Sucursal, String Estado, String Area)
        {
            List<EntMantenimiento> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("RTA_ConsultaReporteServicios", cnx);
                cmd.Parameters.AddWithValue("@Anio", Anio);
                cmd.Parameters.AddWithValue("@Gestor", Gestor);
                cmd.Parameters.AddWithValue("@Sucursal", Sucursal);
                cmd.Parameters.AddWithValue("@Estado", Estado);
                cmd.Parameters.AddWithValue("@Area", Area);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntMantenimiento>();

                while (dr.Read())
                {
                    EntMantenimiento Tarea = new EntMantenimiento();
                    Tarea.FECHA = dr["FECHA"].ToString();
                    Tarea.PEDIDO = Convert.ToInt32(dr["PEDIDO"].ToString());
                    Tarea.ORDEN = dr["ORDEN"].ToString();
                    Tarea.CLIENTE = dr["CLIENTE"].ToString();
                    Tarea.AREA = dr["AREA"].ToString();
                    Tarea.SUCURSAL = dr["SUCURSAL"].ToString();
                    Tarea.GESTOR_RESPONSABLE = dr["GESTOR_RESPONSABLE"].ToString();
                    Tarea.ESTATUS = dr["ESTATUS"].ToString();
                    Tarea.HORAS_CONTRATADAS = Convert.ToSingle(dr["HORAS_CONTRATADAS"].ToString());
                    Tarea.HORAS_ENTREGADAS = Convert.ToSingle(dr["HORAS_ENTREGADAS"].ToString());
                    Tarea.HORAS_DISPONIBLES = Convert.ToSingle(dr["HORAS_DISPONIBLES"].ToString());
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
