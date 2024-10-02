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
    public class DaoContrato
    {
        public static EntRespuesta ConsultaSp_RTAListaContratosDescargar(string FchIni, string FchFin, string busqueda, int IdCliente, int IdGerenteCuenta, int IdGestorResponsable, string sucursal, string estado,string area)
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

                cmd = new SqlCommand("Sp_RTAListaContratos", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@registro", busqueda);

                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@IdGestorResponsable", IdGestorResponsable);
                cmd.Parameters.AddWithValue("@sucursal", sucursal);
                cmd.Parameters.AddWithValue("@Area", area);
                cmd.Parameters.AddWithValue("@estado", estado);
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
        public static List<EntContrato> ConsultaSp_RTAListaContratos(string FchIni, string FchFin,string busqueda,int IdCliente,int IdGerenteCuenta,int IdGestorResponsable,string sucursal,string estado, string area,int IdClasificacion)
        {
            List<EntContrato> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAListaContratos", cnx);
                cmd.Parameters.AddWithValue("@FchIni", FchIni);
                cmd.Parameters.AddWithValue("@FchFin", FchFin);
                cmd.Parameters.AddWithValue("@registro", busqueda);

                cmd.Parameters.AddWithValue("@IdCliente", IdCliente);
                cmd.Parameters.AddWithValue("@IdGerenteCuenta", IdGerenteCuenta);
                cmd.Parameters.AddWithValue("@IdGestorResponsable", IdGestorResponsable);
                cmd.Parameters.AddWithValue("@sucursal", sucursal);
                cmd.Parameters.AddWithValue("@Area", area);
                cmd.Parameters.AddWithValue("@estado", estado);
                cmd.Parameters.AddWithValue("@IdClasificacion", IdClasificacion);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntContrato>();

                while (dr.Read())
                {
                    EntContrato Tarea = new EntContrato();

                    Tarea.IdServicio = Convert.ToInt32(dr["IdServicio"].ToString());
                    Tarea.FECHA = dr["FECHA"].ToString();
                    Tarea.PEDIDO = Convert.ToInt32(dr["PEDIDO"].ToString());
                    Tarea.CLIENTE = dr["CLIENTE"].ToString();
                    Tarea.REFERENCIA_CLIENTE = dr["REFERENCIA_CLIENTE"].ToString();
                    Tarea.AREA = dr["AREA"].ToString();
                    Tarea.GESTOR_RESPONSABLE = dr["GESTOR_RESPONSABLE"].ToString();
                    Tarea.ORDEN = dr["ORDEN"].ToString();
                    Tarea.CLASIFICACION = dr["CLASIFICACION"].ToString();
                    Tarea.DESCRIPCION_DE_SERVICIO = dr["DESCRIPCION_DE_SERVICIO"].ToString();
                    Tarea.SLA_CONTRATO = dr["SLA_CONTRATO"].ToString();
                    Tarea.HORAS_CONTRATADAS = dr["HORAS_CONTRATADAS"].ToString();
                    Tarea.HORAS_ENTREGADAS = dr["HORAS_ENTREGADAS"].ToString();
                    Tarea.HORAS_DISPONIBLES = dr["HORAS_DISPONIBLES"].ToString();
                    Tarea.COSTO_PLAN = dr["COSTO_PLAN"].ToString();
                    Tarea.COSTO_REAL = dr["COSTO_REAL"].ToString();
                    Tarea.SALDO_DE_COSTOS = dr["SALDO_DE_COSTOS"].ToString();
                    Tarea.ESTATUS = dr["ESTATUS"].ToString();
                    Tarea.FECHA_ESTIMADA_DE_CIERRE =dr["FECHA_ESTIMADA_DE_CIERRE"].ToString();
                    Tarea.FECHA_CIERRE = dr["FECHA_CIERRE"].ToString();
                    Tarea.MANTENIMIENTO = dr["MANTENIMIENTO"].ToString();
                    Tarea.MANT_ENTREGADOS = dr["MANT_ENTREGADOS"].ToString();
                    Tarea.OBSERVACIONES = dr["OBSERVACIONES"].ToString();
                    Tarea.CONTACTO_CLIENTE = dr["CONTACTO_CLIENTE"].ToString();
                    Tarea.GERENTE_DE_CUENTA = dr["GERENTE_DE_CUENTA"].ToString();
                    Tarea.SUCURSAL = dr["SUCURSAL"].ToString();
                    Tarea.conteoArchivosAdjuntos = Convert.ToInt32(dr["conteoArchivosAdjuntos"].ToString());
                    Tarea.MAIL = dr["MAIL"].ToString();
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
        public static List<EntContrato> ConsultaSp_RTAConsultarContratos(int IdServicio,string orden,int tipo)
        {
            List<EntContrato> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;
            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultarContratos", cnx);
                cmd.Parameters.AddWithValue("@IdServicio", IdServicio);
                cmd.Parameters.AddWithValue("@orden", orden);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntContrato>();

                while (dr.Read())
                {
                    EntContrato Tarea = new EntContrato();


                    Tarea.IdServicio = Convert.ToInt32(dr["IdServicio"].ToString());
                    Tarea.FECHA = dr["FECHA"].ToString();
                    Tarea.PEDIDO = Convert.ToInt32(dr["PEDIDO"].ToString());
                    Tarea.CLIENTE = dr["CLIENTE"].ToString();
                    Tarea.REFERENCIA_CLIENTE = dr["REFERENCIA_CLIENTE"].ToString();
                    Tarea.AREA = dr["AREA"].ToString();
                    Tarea.GESTOR_RESPONSABLE = dr["GESTOR_RESPONSABLE"].ToString();
                    Tarea.ORDEN = dr["ORDEN"].ToString();
                    Tarea.CLASIFICACION = dr["CLASIFICACION"].ToString();
                    Tarea.DESCRIPCION_DE_SERVICIO = dr["DESCRIPCION_DE_SERVICIO"].ToString();
                    Tarea.SLA_CONTRATO = dr["SLA_CONTRATO"].ToString();
                    Tarea.HORAS_CONTRATADAS = dr["HORAS_CONTRATADAS"].ToString();
                    Tarea.HORAS_ENTREGADAS = dr["HORAS_ENTREGADAS"].ToString();
                    Tarea.HORAS_DISPONIBLES = dr["HORAS_DISPONIBLES"].ToString();
                    Tarea.COSTO_PLAN = dr["COSTO_PLAN"].ToString();
                    Tarea.COSTO_REAL = dr["COSTO_REAL"].ToString();
                    Tarea.SALDO_DE_COSTOS = dr["SALDO_DE_COSTOS"].ToString();
                    Tarea.ESTATUS = dr["ESTATUS"].ToString();
                    Tarea.FECHA_ESTIMADA_DE_CIERRE = dr["FECHA_ESTIMADA_DE_CIERRE"].ToString();
                    Tarea.FECHA_CIERRE = dr["FECHA_CIERRE"].ToString();
                    Tarea.MANTENIMIENTO = dr["MANTENIMIENTO"].ToString();
                    Tarea.MANT_ENTREGADOS = dr["MANT_ENTREGADOS"].ToString();
                    Tarea.OBSERVACIONES = dr["OBSERVACIONES"].ToString();
                    Tarea.CONTACTO_CLIENTE = dr["CONTACTO_CLIENTE"].ToString();
                    Tarea.GERENTE_DE_CUENTA = dr["GERENTE_DE_CUENTA"].ToString();
                    Tarea.IdClasificacion = Convert.ToInt32(dr["IdClasificacion"].ToString());
                    Tarea.IdCliente = Convert.ToInt32(dr["IdCliente"].ToString());
                    Tarea.IdGerenteCuenta = Convert.ToInt32(dr["IdGerenteCuenta"].ToString());
                    Tarea.IdGestorResponsable = Convert.ToInt32(dr["IdGestorResponsable"].ToString());
                    Tarea.TiempoRespuesta = dr["TiempoRespuesta"].ToString();
                    Tarea.TiempoSolucion = dr["TiempoSolucion"].ToString();
                    Tarea.TiempoDiagnostico = dr["TiempoDiagnostico"].ToString();
                    Tarea.SUCURSAL = dr["SUCURSAL"].ToString();
                    Tarea.MAIL = dr["MAIL"].ToString();
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
        public static EntRespuesta RTA_InsertaNuevoContrato(EntContrato objEntContrato)
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
                cmd = new SqlCommand("Sp_RTAInsertaNuevoContrato", cnx);

                cmd.Parameters.AddWithValue("@FECHA", Convert.ToDateTime(objEntContrato.FECHA));
                cmd.Parameters.AddWithValue("@PEDIDO", objEntContrato.PEDIDO);
                cmd.Parameters.AddWithValue("@CLIENTE", objEntContrato.CLIENTE);
                cmd.Parameters.AddWithValue("@REFERENCIA_CLIENTE", objEntContrato.REFERENCIA_CLIENTE);
                cmd.Parameters.AddWithValue("@AREA", objEntContrato.AREA);
                cmd.Parameters.AddWithValue("@GESTOR_RESPONSABLE", objEntContrato.GESTOR_RESPONSABLE);
                cmd.Parameters.AddWithValue("@ORDEN", objEntContrato.ORDEN);
                cmd.Parameters.AddWithValue("@CLASIFICACION", objEntContrato.CLASIFICACION);
                cmd.Parameters.AddWithValue("@DESCRIPCION_DE_SERVICIO", objEntContrato.DESCRIPCION_DE_SERVICIO);
                cmd.Parameters.AddWithValue("@SLA_CONTRATO", objEntContrato.SLA_CONTRATO);
                cmd.Parameters.AddWithValue("@HORAS_CONTRATADAS", objEntContrato.HORAS_CONTRATADAS);
                cmd.Parameters.AddWithValue("@HORAS_ENTREGADAS", objEntContrato.HORAS_ENTREGADAS);
                cmd.Parameters.AddWithValue("@HORAS_DISPONIBLES", objEntContrato.HORAS_DISPONIBLES);
                cmd.Parameters.AddWithValue("@COSTO_PLAN", objEntContrato.COSTO_PLAN);
                cmd.Parameters.AddWithValue("@COSTO_REAL", objEntContrato.COSTO_REAL);
                cmd.Parameters.AddWithValue("@SALDO_DE_COSTOS", objEntContrato.SALDO_DE_COSTOS);
                cmd.Parameters.AddWithValue("@ESTATUS", objEntContrato.ESTATUS);
                cmd.Parameters.AddWithValue("@FECHA_ESTIMADA_DE_CIERRE", Convert.ToDateTime(objEntContrato.FECHA_ESTIMADA_DE_CIERRE));
                if(objEntContrato.FECHA_CIERRE=="")
                {
                    cmd.Parameters.AddWithValue("@FECHA_CIERRE", Convert.ToDateTime("1900-01-01"));
                }   
                else
                {
                    cmd.Parameters.AddWithValue("@FECHA_CIERRE", Convert.ToDateTime(objEntContrato.FECHA_CIERRE));
                }
                cmd.Parameters.AddWithValue("@MANTENIMIENTO", objEntContrato.MANTENIMIENTO);
                cmd.Parameters.AddWithValue("@MANT_ENTREGADOS", objEntContrato.MANT_ENTREGADOS);
                cmd.Parameters.AddWithValue("@OBSERVACIONES", objEntContrato.OBSERVACIONES);
                cmd.Parameters.AddWithValue("@CONTACTO_CLIENTE", objEntContrato.CONTACTO_CLIENTE);
                cmd.Parameters.AddWithValue("@GERENTE_DE_CUENTA", objEntContrato.GERENTE_DE_CUENTA);
                cmd.Parameters.AddWithValue("@SUCURSAL", objEntContrato.SUCURSAL);
                cmd.Parameters.AddWithValue("@TiempoRespuesta", objEntContrato.TiempoRespuesta);
                cmd.Parameters.AddWithValue("@TiempoSolucion", objEntContrato.TiempoSolucion);
                cmd.Parameters.AddWithValue("@Usuario", objEntContrato.Usuario);
                cmd.Parameters.AddWithValue("@MAIL", objEntContrato.MAIL);
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
                else if (respuestaSP == 0)
                {
                    //Respuesta.estado = respuestaSP.ToString();
                    Respuesta.estado = "1";
                    Respuesta.mensaje = "La información ya se encuentra registrada ";
                    Respuesta.tipoMensaje = "success";
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
        public static EntRespuesta RTA_ActualizarNuevoContrato(EntContrato objEntContrato)
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
                cmd = new SqlCommand("Sp_RTActualizarNuevoContrato", cnx);

                cmd.Parameters.AddWithValue("@FECHA", Convert.ToDateTime(objEntContrato.FECHA));
                cmd.Parameters.AddWithValue("@PEDIDO", objEntContrato.PEDIDO);
                cmd.Parameters.AddWithValue("@CLIENTE", objEntContrato.CLIENTE);
                cmd.Parameters.AddWithValue("@REFERENCIA_CLIENTE", objEntContrato.REFERENCIA_CLIENTE);
                cmd.Parameters.AddWithValue("@AREA", objEntContrato.AREA);
                cmd.Parameters.AddWithValue("@GESTOR_RESPONSABLE", objEntContrato.GESTOR_RESPONSABLE);
                cmd.Parameters.AddWithValue("@ORDEN", objEntContrato.ORDEN);
                cmd.Parameters.AddWithValue("@CLASIFICACION", objEntContrato.CLASIFICACION);
                cmd.Parameters.AddWithValue("@DESCRIPCION_DE_SERVICIO", objEntContrato.DESCRIPCION_DE_SERVICIO);
                cmd.Parameters.AddWithValue("@SLA_CONTRATO", objEntContrato.SLA_CONTRATO);
                cmd.Parameters.AddWithValue("@HORAS_CONTRATADAS", Convert.ToDouble( objEntContrato.HORAS_CONTRATADAS));
                cmd.Parameters.AddWithValue("@HORAS_ENTREGADAS", Convert.ToDouble( objEntContrato.HORAS_ENTREGADAS));
                cmd.Parameters.AddWithValue("@HORAS_DISPONIBLES", Convert.ToDouble(objEntContrato.HORAS_DISPONIBLES));
                cmd.Parameters.AddWithValue("@COSTO_PLAN", Convert.ToDouble(objEntContrato.COSTO_PLAN));
                cmd.Parameters.AddWithValue("@COSTO_REAL", Convert.ToDouble(objEntContrato.COSTO_REAL));
                cmd.Parameters.AddWithValue("@SALDO_DE_COSTOS", Convert.ToDouble(objEntContrato.SALDO_DE_COSTOS));
                cmd.Parameters.AddWithValue("@ESTATUS", objEntContrato.ESTATUS);
                cmd.Parameters.AddWithValue("@FECHA_ESTIMADA_DE_CIERRE", Convert.ToDateTime(objEntContrato.FECHA_ESTIMADA_DE_CIERRE));
                if (objEntContrato.FECHA_CIERRE == "")
                {
                    cmd.Parameters.AddWithValue("@FECHA_CIERRE", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FECHA_CIERRE", Convert.ToDateTime(objEntContrato.FECHA_CIERRE));
                }
                cmd.Parameters.AddWithValue("@MANTENIMIENTO", objEntContrato.MANTENIMIENTO);
                cmd.Parameters.AddWithValue("@MANT_ENTREGADOS", objEntContrato.MANT_ENTREGADOS);
                cmd.Parameters.AddWithValue("@OBSERVACIONES", objEntContrato.OBSERVACIONES);
                cmd.Parameters.AddWithValue("@CONTACTO_CLIENTE", objEntContrato.CONTACTO_CLIENTE);
                cmd.Parameters.AddWithValue("@GERENTE_DE_CUENTA", objEntContrato.GERENTE_DE_CUENTA);
                cmd.Parameters.AddWithValue("@SUCURSAL", objEntContrato.SUCURSAL);
                cmd.Parameters.AddWithValue("@TiempoRespuesta", objEntContrato.TiempoRespuesta);
                cmd.Parameters.AddWithValue("@TiempoSolucion", objEntContrato.TiempoSolucion);
                cmd.Parameters.AddWithValue("@Usuario", objEntContrato.Usuario);
                cmd.Parameters.AddWithValue("@MAIL", objEntContrato.MAIL);
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

        public static EntRespuesta RTA_EliminarNuevoContrato(EntContrato objEntContrato)
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
                cmd = new SqlCommand("Sp_RTEliminarNuevoContrato", cnx);

                cmd.Parameters.AddWithValue("@FECHA", Convert.ToDateTime(objEntContrato.FECHA));
                cmd.Parameters.AddWithValue("@PEDIDO", objEntContrato.PEDIDO);
                cmd.Parameters.AddWithValue("@CLIENTE", objEntContrato.CLIENTE);
                cmd.Parameters.AddWithValue("@REFERENCIA_CLIENTE", objEntContrato.REFERENCIA_CLIENTE);
                cmd.Parameters.AddWithValue("@AREA", objEntContrato.AREA);
                cmd.Parameters.AddWithValue("@GESTOR_RESPONSABLE", objEntContrato.GESTOR_RESPONSABLE);
                cmd.Parameters.AddWithValue("@ORDEN", objEntContrato.ORDEN);
                cmd.Parameters.AddWithValue("@CLASIFICACION", objEntContrato.CLASIFICACION);
                cmd.Parameters.AddWithValue("@DESCRIPCION_DE_SERVICIO", objEntContrato.DESCRIPCION_DE_SERVICIO);
                cmd.Parameters.AddWithValue("@SLA_CONTRATO", objEntContrato.SLA_CONTRATO);
                cmd.Parameters.AddWithValue("@HORAS_CONTRATADAS", Convert.ToDouble(objEntContrato.HORAS_CONTRATADAS));
                cmd.Parameters.AddWithValue("@HORAS_ENTREGADAS", Convert.ToDouble(objEntContrato.HORAS_ENTREGADAS));
                cmd.Parameters.AddWithValue("@HORAS_DISPONIBLES", Convert.ToDouble(objEntContrato.HORAS_DISPONIBLES));
                cmd.Parameters.AddWithValue("@COSTO_PLAN", Convert.ToDouble(objEntContrato.COSTO_PLAN));
                cmd.Parameters.AddWithValue("@COSTO_REAL", Convert.ToDouble(objEntContrato.COSTO_REAL));
                cmd.Parameters.AddWithValue("@SALDO_DE_COSTOS", Convert.ToDouble(objEntContrato.SALDO_DE_COSTOS));
                cmd.Parameters.AddWithValue("@ESTATUS", objEntContrato.ESTATUS);
                cmd.Parameters.AddWithValue("@FECHA_ESTIMADA_DE_CIERRE", Convert.ToDateTime(objEntContrato.FECHA_ESTIMADA_DE_CIERRE));
                if (objEntContrato.FECHA_CIERRE == "")
                {
                    cmd.Parameters.AddWithValue("@FECHA_CIERRE", Convert.ToDateTime("1900-01-01"));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FECHA_CIERRE", Convert.ToDateTime(objEntContrato.FECHA_CIERRE));
                }
                cmd.Parameters.AddWithValue("@MANTENIMIENTO", objEntContrato.MANTENIMIENTO);
                cmd.Parameters.AddWithValue("@MANT_ENTREGADOS", objEntContrato.MANT_ENTREGADOS);
                cmd.Parameters.AddWithValue("@OBSERVACIONES", objEntContrato.OBSERVACIONES);
                cmd.Parameters.AddWithValue("@CONTACTO_CLIENTE", objEntContrato.CONTACTO_CLIENTE);
                cmd.Parameters.AddWithValue("@GERENTE_DE_CUENTA", objEntContrato.GERENTE_DE_CUENTA);
                cmd.Parameters.AddWithValue("@SUCURSAL", objEntContrato.SUCURSAL);
                cmd.Parameters.AddWithValue("@TiempoRespuesta", objEntContrato.TiempoRespuesta);
                cmd.Parameters.AddWithValue("@TiempoSolucion", objEntContrato.TiempoSolucion);
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
        public static List<EntCombo> RTAListaComboContrato(Int32 Tipo,string Cod_Usuario, Int32 IdSucursal,Int32 IdCliente,string IdSucursalGerente)
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

    }
}