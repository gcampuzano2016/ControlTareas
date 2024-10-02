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
    public class DaoInfoContrato
    {
        public static EntRespuesta Consulta_Sp_InsertarActualizarContrato(EntInfoContrato objContrato)
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
                cmd = new SqlCommand("Sp_RTAInsUpdContrato", cnx);

                cmd.Parameters.AddWithValue("@IdContrato", objContrato.IdContrato);
                cmd.Parameters.AddWithValue("@Con_Cliente", objContrato.CLIENTE ?? "");
                cmd.Parameters.AddWithValue("@CLI_NOMBRE", objContrato.CLI_NOMBRE ?? "");
                cmd.Parameters.AddWithValue("@CLI_TELEFONO", objContrato.CLI_TELEFONO ?? "");
                cmd.Parameters.AddWithValue("@CLI_DIRECCION", objContrato.CLI_DIRECCION ?? "");
                cmd.Parameters.AddWithValue("@CLI_CORREO", objContrato.CLI_CORREO ?? "");
                cmd.Parameters.AddWithValue("@Con_Numero", objContrato.NUM_CONTRATO);
                cmd.Parameters.AddWithValue("@Con_Objeto", objContrato.OBJETO ?? "");
                cmd.Parameters.AddWithValue("@Con_ValorTotal", objContrato.VALOR_TOTAL_CONTRATO);
                cmd.Parameters.AddWithValue("@Con_Alcance", objContrato.ALCANCE ?? "");
                cmd.Parameters.AddWithValue("@Con_Hardware", objContrato.HARDWARE ?? "");
                cmd.Parameters.AddWithValue("@Con_Licencias", objContrato.LICENCIAS ?? "");
                cmd.Parameters.AddWithValue("@Con_ServiciosFabricante", objContrato.SERVICIOS_FABRICANTE ?? "");
                cmd.Parameters.AddWithValue("@Con_ServiciosDOS", objContrato.SERVICIO_DOS ?? "");
                cmd.Parameters.AddWithValue("@Con_ServiciosExternos", objContrato.SERVICIO_EXTERNOS ?? "");
                cmd.Parameters.AddWithValue("@Con_Polizas", objContrato.POLIZAS ?? "");
                cmd.Parameters.AddWithValue("@Con_TerminosTDR", objContrato.TERMINOS_TDR ?? "");

                cmd.Parameters.AddWithValue("@Con_ActaPreguntas", objContrato.ACTA_PREGUNTAS ?? "");
                cmd.Parameters.AddWithValue("@Con_ActaAdjudicacion", objContrato.ACTA_ADJUDICACION ?? "");
                cmd.Parameters.AddWithValue("@Con_ActaNegociacion", objContrato.ACTA_NEGOCIACION ?? "");
                //cmd.Parameters.AddWithValue("@Con_ValorAgregado", objContrato.Con_ValorAgregado ?? "");
                cmd.Parameters.AddWithValue("@Con_BomSolucion", objContrato.BOM_SOLUCION ?? "");
                cmd.Parameters.AddWithValue("@Con_AcuerdosMay", objContrato.ACUERDOS_MAY ?? "");
                cmd.Parameters.AddWithValue("@Con_AcuerdosFab", objContrato.ACUERDOS_FAB ?? "");
                cmd.Parameters.AddWithValue("@Con_FormaPago", objContrato.FORMA_PAGO ?? "");
                cmd.Parameters.AddWithValue("@Con_GarantiasFin", objContrato.GARANTIAS_FIN ?? "");
                cmd.Parameters.AddWithValue("@Con_GarantiasTEC", objContrato.GARANTIAS_TEC ?? "");
                cmd.Parameters.AddWithValue("@Con_GeneracionPedidos", objContrato.GENERACION_PEDIDOS ?? "");
                if (!string.IsNullOrEmpty(objContrato.FECHA_SUSCRIPCION_CONTRATO))
                {
                    DateTime fechaSuscripcion;
                    if (DateTime.TryParse(objContrato.FECHA_SUSCRIPCION_CONTRATO, out fechaSuscripcion))
                    {
                        cmd.Parameters.AddWithValue("@Con_FechaSuscripcion", fechaSuscripcion);
                    }
                    else
                    {
                        // Manejar el caso donde la fecha no es válida si es necesario
                        cmd.Parameters.AddWithValue("@Con_FechaSuscripcion", DBNull.Value);
                    }
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Con_FechaSuscripcion", DBNull.Value);
                }
                // Validación y asignación del parámetro @Con_FechaNotificacion
                if (!string.IsNullOrEmpty(objContrato.FECHA_NOTIF_ANTICIPO))
                {
                    DateTime fechaNotificacion;
                    if (DateTime.TryParse(objContrato.FECHA_NOTIF_ANTICIPO, out fechaNotificacion))
                    {
                        cmd.Parameters.AddWithValue("@Con_FechaNotificacion", fechaNotificacion);
                    }
                    else
                    {
                        // Manejar el caso donde la fecha no es válida si es necesario
                        cmd.Parameters.AddWithValue("@Con_FechaNotificacion", DBNull.Value);
                    }
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Con_FechaNotificacion", DBNull.Value);
                }
                if (!string.IsNullOrEmpty(objContrato.FECHA_INICIO_GARANTIA))
                {
                    DateTime fechaIniActivacionGar;
                    if (DateTime.TryParse(objContrato.FECHA_INICIO_GARANTIA, out fechaIniActivacionGar))
                    {
                        cmd.Parameters.AddWithValue("@Con_FechaIniActivacionGar", fechaIniActivacionGar);
                    }
                    else
                    {
                        // Manejar el caso donde la fecha no es válida si es necesario
                        cmd.Parameters.AddWithValue("@Con_FechaIniActivacionGar", DBNull.Value);
                    }
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Con_FechaIniActivacionGar", DBNull.Value);
                }

                // Validación y asignación del parámetro @Con_FechaFinActivacionGar
                if (!string.IsNullOrEmpty(objContrato.FECHA_FIN_GARANTIA))
                {
                    DateTime fechaFinActivacionGar;
                    if (DateTime.TryParse(objContrato.FECHA_FIN_GARANTIA, out fechaFinActivacionGar))
                    {
                        cmd.Parameters.AddWithValue("@Con_FechaFinActivacionGar", fechaFinActivacionGar);
                    }
                    else
                    {
                        // Manejar el caso donde la fecha no es válida si es necesario
                        cmd.Parameters.AddWithValue("@Con_FechaFinActivacionGar", DBNull.Value);
                    }
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Con_FechaFinActivacionGar", DBNull.Value);
                }

                cmd.Parameters.AddWithValue("@Con_PlazoActivacion", objContrato.PLAZO_ACTIVACION ?? ""); 
                cmd.Parameters.AddWithValue("@Con_PlazoActivacionLic", objContrato.PLAZO_ACTIVACION_LIC ?? "");
                cmd.Parameters.AddWithValue("@Con_DuracionVigencia", objContrato.DURACION_VIGENCIA_TEC ?? "");
                cmd.Parameters.AddWithValue("@Con_EntregaLicTemporales", objContrato.ENTREGA_LIC_TEMPORALES ?? "");
                cmd.Parameters.AddWithValue("@Con_OrdenesServicio", objContrato.ORDEN_SERVICIO ?? "");

                cmd.Parameters.AddWithValue("@ConObs_Cliente", objContrato.OBS_CLIENTE ?? "");
                cmd.Parameters.AddWithValue("@ConObs_Numero", objContrato.OBS_NUM_CONTRATO ?? ""); 
                cmd.Parameters.AddWithValue("@ConObs_ValorTotal", objContrato.OBS_VALOR_TOTAL ?? "");
                cmd.Parameters.AddWithValue("@ConObs_Objeto", objContrato.OBS_OBJETO ?? "");
                cmd.Parameters.AddWithValue("@ConObs_Alcance", objContrato.OBS_ALCANCE ?? "");
                cmd.Parameters.AddWithValue("@ConObs_Hardware", objContrato.OBS_HARDWARE ?? "");
                cmd.Parameters.AddWithValue("@ConObs_Licencias", objContrato.OBS_LICENCIAS ?? "");
                cmd.Parameters.AddWithValue("@ConObs_ServiciosFabricante", objContrato.OBS_SERVICIOS_FABRICANTE ?? "");
                cmd.Parameters.AddWithValue("@ConObs_ServiciosDOS", objContrato.OBS_SERVICIO_DOS ?? "");
                cmd.Parameters.AddWithValue("@ConObs_ServiciosExternos", objContrato.OBS_SERVICIO_EXTERNOS ?? "");
                cmd.Parameters.AddWithValue("@ConObs_Polizas", objContrato.OBS_POLIZAS ?? "");
                cmd.Parameters.AddWithValue("@ConObs_TerminosTDR", objContrato.OBS_TERMINOS_TDR ?? "");

                cmd.Parameters.AddWithValue("@ConObs_ActaPreguntas", objContrato.OBS_ACTA_PREGUNTAS ?? "");
                cmd.Parameters.AddWithValue("@ConObs_ActaAdjudicacion", objContrato.OBS_ACTA_ADJUDICACION ?? "");
                cmd.Parameters.AddWithValue("@ConObs_ActaNegociacion", objContrato.OBS_ACTA_NEGOCIACION ?? "");
                cmd.Parameters.AddWithValue("@ConObs_BomSolucion", objContrato.OBS_BOM_SOLUCION ?? "");
                cmd.Parameters.AddWithValue("@ConObs_AcuerdosMay", objContrato.OBS_ACUERDOS_MAY ?? "");
                cmd.Parameters.AddWithValue("@ConObs_AcuerdosFab", objContrato.OBS_ACUERDOS_FAB ?? "");
                cmd.Parameters.AddWithValue("@ConObs_FormaPago", objContrato.OBS_FORMA_PAGO ?? "");
                cmd.Parameters.AddWithValue("@ConObs_GarantiasFin", objContrato.OBS_GARANTIAS_FIN ?? "");
                cmd.Parameters.AddWithValue("@ConObs_GarantiasTEC", objContrato.OBS_GARANTIAS_TEC ?? "");
                cmd.Parameters.AddWithValue("@ConObs_GeneracionPedidos", objContrato.OBS_GENERACION_PEDIDOS ?? "");

                /*cmd.Parameters.AddWithValue("@ConObs_PlazoActivacion", objContrato.ConObs_PlazoActivacion ?? ""); 
                cmd.Parameters.AddWithValue("@ConObs_FechaSuscripcion", objContrato.OBS_FECHA_SUSCRIPCION_CONTRATO ?? "");
                cmd.Parameters.AddWithValue("@ConObs_FechaNotificacion", objContrato.OBS_FECHA_NOTIF_ANTICIPO ?? "");               
                cmd.Parameters.AddWithValue("@ConObs_FechaIniActivacionGar", objContrato.ConObs_FechaIniActivacionGar ?? "");
                cmd.Parameters.AddWithValue("@ConObs_FechaFinActivacionGar", objContrato.ConObs_FechaFinActivacionGar ?? "");
                cmd.Parameters.AddWithValue("@ConObs_EntregaLicTemporales", objContrato.ConObs_EntregaLicTemporales ?? "");*/

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

        public static List<EntInfoContrato> Consulta_Sp_RTAConsultarpermisoContratos()
        {
            List<EntInfoContrato> listaTareas = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("Sp_RTAConsultarPermisosContrato", cnx);


                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                listaTareas = new List<EntInfoContrato>();

                while (dr.Read())
                {
                    EntInfoContrato objContrato = new EntInfoContrato();
                    objContrato.NUM_CONTRATO = dr["num_Contrato"].ToString();
                    objContrato.COD_USUARIO = dr["cod_Usuario"].ToString();
                    objContrato.PERMISOS = dr["permisos"].ToString();
                    objContrato.OBSERVACIONES = dr["observaciones"].ToString();
                    objContrato.FECHA_CREACION = dr["FechaCreacion"].ToString(); //date
                    listaTareas.Add(objContrato);
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

        public static EntInfoContrato Consulta_Sp_RTAConsultarPermisoContratoNum(string numContrato, String codUsuario)
        {
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            EntInfoContrato objContrato = new EntInfoContrato();

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAConsultarPermisosContratoPorNum", cnx);
                cmd.Parameters.AddWithValue("@NumContrato", numContrato);
                cmd.Parameters.AddWithValue("@CodUsuario", codUsuario);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    objContrato.NUM_CONTRATO = dr["num_Contrato"].ToString();
                    objContrato.COD_USUARIO = dr["cod_Usuario"].ToString();
                    objContrato.PERMISOS = dr["permisos"].ToString();
                    objContrato.OBSERVACIONES = dr["Observaciones"].ToString();
                    objContrato.FECHA_CREACION = dr["fechaCreacion"].ToString(); //date
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocurrió un error: " + ex.Message);
                Console.WriteLine("Detalles del error: " + ex.ToString());
                objContrato = null;
            }
            finally
            {
                if (cmd != null && cmd.Connection != null)
                {
                    cmd.Connection.Close();
                }
            }

            return objContrato;
        }
        
        public static EntInfoContrato Consulta_Sp_RTAConsultarContratoNum(string numContrato)
        {
            SqlCommand cmd = null;
            SqlDataReader dr = null;
            EntInfoContrato objContrato = new EntInfoContrato();

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();
                cmd = new SqlCommand("Sp_RTAConsultarContratosPorNum", cnx);
                cmd.Parameters.AddWithValue("@NumContrato", numContrato);

                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    objContrato.NUM_CONTRATO = dr["Con_Numero"].ToString();
                    objContrato.CLIENTE = dr["Con_Cliente"].ToString();
                    objContrato.CLI_NOMBRE = dr["CLI_NOMBRE"].ToString();
                    objContrato.CLI_TELEFONO = dr["CLI_TELEFONO"].ToString();
                    objContrato.CLI_DIRECCION = dr["CLI_DIRECCION"].ToString();
                    objContrato.CLI_CORREO = dr["CLI_CORREO"].ToString();
                    objContrato.OBJETO = dr["Con_Objeto"].ToString();
                    objContrato.VALOR_TOTAL_CONTRATO = dr.IsDBNull(dr.GetOrdinal("ValorTotal")) ? 0 : dr.GetDecimal(dr.GetOrdinal("ValorTotal"));
                    objContrato.ALCANCE = dr["Con_Alcance"].ToString();
                    objContrato.HARDWARE = dr["Con_Hardware"].ToString();
                    objContrato.LICENCIAS = dr["Con_Licencias"].ToString();
                    objContrato.SERVICIOS_FABRICANTE = dr["Con_ServiciosFabricante"].ToString();
                    objContrato.SERVICIO_DOS = dr["Con_ServiciosDOS"].ToString();
                    objContrato.SERVICIO_EXTERNOS = dr["Con_ServiciosExternos"].ToString();
                    objContrato.POLIZAS = dr["Con_Polizas"].ToString();
                    objContrato.TERMINOS_TDR = dr["Con_TerminosTDR"].ToString();
                    objContrato.ACTA_PREGUNTAS = dr["Con_ActaPreguntas"].ToString();
                    objContrato.ACTA_ADJUDICACION = dr["Con_ActaAdjudicacion"].ToString();
                    objContrato.ACTA_NEGOCIACION = dr["Con_ActaNegociacion"].ToString();
                    objContrato.BOM_SOLUCION = dr["Con_BomSolucion"].ToString();
                    objContrato.ACUERDOS_MAY = dr["Con_AcuerdosMay"].ToString();
                    objContrato.ACUERDOS_FAB = dr["Con_AcuerdosFab"].ToString();
                    objContrato.FORMA_PAGO = dr["Con_FormaPago"].ToString();
                    objContrato.GARANTIAS_FIN = dr["Con_GarantiasFin"].ToString();
                    objContrato.GARANTIAS_TEC = dr["Con_GarantiasTEC"].ToString();
                    objContrato.GENERACION_PEDIDOS = dr["Con_GeneracionPedidos"].ToString();
                    objContrato.FECHA_SUSCRIPCION_CONTRATO = dr["Con_FechaSuscripcion"].ToString();
                    objContrato.FECHA_NOTIF_ANTICIPO = dr["Con_FechaNotificacion"].ToString();                    
                    objContrato.FECHA_INICIO_GARANTIA = dr["Con_FechaIniActivacionGar"].ToString();
                    objContrato.FECHA_FIN_GARANTIA = dr["Con_FechaFinActivacionGar"].ToString(); 
                    objContrato.PLAZO_ACTIVACION = dr["Con_PlazoActivacion"].ToString();
                    objContrato.PLAZO_ACTIVACION_LIC = dr["Con_PlazoActivacionLic"].ToString();
                    objContrato.DURACION_VIGENCIA_TEC = dr["Con_DuracionVigencia"].ToString();
                    objContrato.ENTREGA_LIC_TEMPORALES = dr["Con_EntregaLicTemporales"].ToString();

                    objContrato.OBS_CLIENTE = dr["ConObs_Cliente"].ToString();
                    objContrato.OBS_NUM_CONTRATO = dr["ConObs_Numero"].ToString(); 
                    objContrato.OBS_VALOR_TOTAL = dr["ConObs_ValorTotal"].ToString();
                    objContrato.OBS_OBJETO = dr["ConObs_Objeto"].ToString();
                    objContrato.OBS_ALCANCE = dr["ConObs_Alcance"].ToString();
                    objContrato.OBS_HARDWARE = dr["ConObs_Hardware"].ToString();
                    objContrato.OBS_LICENCIAS = dr["ConObs_Licencias"].ToString();
                    objContrato.OBS_SERVICIOS_FABRICANTE = dr["ConObs_ServiciosFabricante"].ToString();
                    objContrato.OBS_SERVICIO_DOS = dr["ConObs_ServiciosDOS"].ToString();
                    objContrato.OBS_SERVICIO_EXTERNOS = dr["ConObs_ServiciosExternos"].ToString();
                    objContrato.OBS_POLIZAS = dr["ConObs_Polizas"].ToString();
                    objContrato.OBS_TERMINOS_TDR = dr["ConObs_TerminosTDR"].ToString();
                    objContrato.OBS_ACTA_PREGUNTAS = dr["ConObs_ActaPreguntas"].ToString();
                    objContrato.OBS_ACTA_ADJUDICACION = dr["ConObs_ActaAdjudicacion"].ToString();
                    objContrato.OBS_ACTA_NEGOCIACION = dr["ConObs_ActaNegociacion"].ToString();
                    objContrato.OBS_BOM_SOLUCION = dr["ConObs_BomSolucion"].ToString();
                    objContrato.OBS_ACUERDOS_MAY = dr["ConObs_AcuerdosMay"].ToString();
                    objContrato.OBS_ACUERDOS_FAB = dr["ConObs_AcuerdosFab"].ToString();
                    objContrato.OBS_FORMA_PAGO = dr["ConObs_FormaPago"].ToString();
                    objContrato.OBS_GARANTIAS_FIN = dr["ConObs_GarantiasFin"].ToString();
                    objContrato.OBS_GARANTIAS_TEC = dr["ConObs_GarantiasTEC"].ToString();
                    objContrato.OBS_GENERACION_PEDIDOS = dr["ConObs_GeneracionPedidos"].ToString();
                    objContrato.ORDEN_SERVICIO = dr["Con_OrdenesServicio"].ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocurrió un error: " + ex.Message);
                Console.WriteLine("Detalles del error: " + ex.ToString());
                objContrato = null;
            }
            finally
            {
                if (cmd != null && cmd.Connection != null)
                {
                    cmd.Connection.Close();
                }
            }

            return objContrato;
        }

        public static List<EntOrdenServicio> Consulta_Sp_ConsultarOrdenServicio(float pedido)
{
            List<EntOrdenServicio> listaOrdenServicio = null;

            SqlCommand cmd = null;
            SqlDataReader dr = null;

            try
            {
                DaoReporTareaAranda cn = new DaoReporTareaAranda();
                SqlConnection cnx = cn.conectar();
                cnx.Open();

                cmd = new SqlCommand("sp_RTAConsultarOrdenServicioPedido", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PEDIDO", pedido);

                dr = cmd.ExecuteReader();
                listaOrdenServicio = new List<EntOrdenServicio>();

                while (dr.Read())
                {
                    EntOrdenServicio objOrdenServicio = new EntOrdenServicio();
                    objOrdenServicio.IdServicio = dr["IdServicio"]!= DBNull.Value? (Int64)dr["IdServicio"] : 0;
                    objOrdenServicio.IdClasificacion = dr["IdClasificacion"]!= DBNull.Value? (Int64)dr["IdClasificacion"] : 0;
                    objOrdenServicio.IdCliente = dr["IdCliente"]!= DBNull.Value? (Int64)dr["IdCliente"] : 0;
                    objOrdenServicio.IdGerenteCuenta = dr["IdGerenteCuenta"]!= DBNull.Value? (Int64)dr["IdGerenteCuenta"] : 0;
                    objOrdenServicio.IdGestorResponsable = dr["IdGestorResponsable"]!= DBNull.Value? (Int64)dr["IdGestorResponsable"] : 0;
                    objOrdenServicio.TiempoRespuesta = dr["TiempoRespuesta"].ToString();
                    objOrdenServicio.TiempoSolucion = dr["TiempoSolucion"].ToString();
                    objOrdenServicio.TiempoDiagnostico = dr["TiempoDiagnostico"].ToString();
                    objOrdenServicio.PEDIDO = dr["PEDIDO"] != DBNull.Value ? Convert.ToSingle(dr["PEDIDO"]) : 0;
                    objOrdenServicio.CLIENTE = dr["CLIENTE"].ToString();
                    objOrdenServicio.REFERENCIA_CLIENTE = dr["REFERENCIA_CLIENTE"].ToString();
                    objOrdenServicio.AREA = dr["AREA"].ToString();
                    objOrdenServicio.GESTOR_RESPONSABLE = dr["GESTOR_RESPONSABLE"].ToString();
                    objOrdenServicio.ORDEN = dr["ORDEN"].ToString();
                    objOrdenServicio.CLASIFICACION = dr["CLASIFICACION"].ToString();
                    objOrdenServicio.DESCRIPCION_DE_SERVICIO = dr["DESCRIPCION_DE_SERVICIO"].ToString();
                    objOrdenServicio.SLA_CONTRATO = dr["SLA_CONTRATO"].ToString();
                    objOrdenServicio.HORAS_CONTRATADAS = dr["HORAS_CONTRATADAS"] != DBNull.Value ? Convert.ToSingle(dr["HORAS_CONTRATADAS"]) : 0;
                    objOrdenServicio.HORAS_ENTREGADAS = dr["HORAS_ENTREGADAS"] != DBNull.Value ? Convert.ToSingle(dr["HORAS_ENTREGADAS"]) : 0;
                    objOrdenServicio.HORAS_DISPONIBLES = dr["HORAS_DISPONIBLES"] != DBNull.Value ? Convert.ToSingle(dr["HORAS_DISPONIBLES"]) : 0;
                    objOrdenServicio.COSTO_PLAN = dr["COSTO_PLAN"] != DBNull.Value ? Convert.ToSingle(dr["COSTO_PLAN"]) : 0;
                    objOrdenServicio.COSTO_REAL = dr["COSTO_REAL"] != DBNull.Value ? Convert.ToSingle(dr["COSTO_REAL"]) : 0;
                    objOrdenServicio.SALDO_DE_COSTOS = dr["SALDO_DE_COSTOS"] != DBNull.Value ? Convert.ToSingle(dr["SALDO_DE_COSTOS"]) : 0;
                    objOrdenServicio.ESTATUS = dr["ESTATUS"].ToString();
                    objOrdenServicio.FECHA_ESTIMADA_DE_CIERRE = dr["FECHA_ESTIMADA_DE_CIERRE"] != DBNull.Value ? Convert.ToDateTime(dr["FECHA_ESTIMADA_DE_CIERRE"]) : DateTime.MinValue;
                    objOrdenServicio.FECHA_CIERRE = dr["FECHA_CIERRE"] != DBNull.Value ? Convert.ToDateTime(dr["FECHA_CIERRE"]) : DateTime.MinValue;
                    objOrdenServicio.MANTENIMIENTO = dr["MANTENIMIENTO"].ToString();
                    objOrdenServicio.MANT_ENTREGADOS = dr["MANT_ENTREGADOS"].ToString();
                    objOrdenServicio.OBSERVACIONES = dr["OBSERVACIONES"].ToString();
                    objOrdenServicio.CONTACTO_CLIENTE = dr["CONTACTO_CLIENTE"].ToString();
                    objOrdenServicio.GERENTE_DE_CUENTA = dr["GERENTE_DE_CUENTA"].ToString();
                    objOrdenServicio.SUCURSAL = dr["SUCURSAL"].ToString();
                    objOrdenServicio.Estado = dr["Estado"] != DBNull.Value ? Convert.ToInt32(dr["Estado"]) : 0;
                    objOrdenServicio.COSTO_PLAN1 = dr["COSTO_PLAN1"] != DBNull.Value ? Convert.ToSingle(dr["COSTO_PLAN1"]) : 0;
                    objOrdenServicio.COSTO_REAL1 = dr["COSTO_REAL1"] != DBNull.Value ? Convert.ToSingle(dr["COSTO_REAL1"]) : 0;
                    objOrdenServicio.SALDO_DE_COSTOS1 = dr["SALDO_DE_COSTOS1"] != DBNull.Value ? Convert.ToSingle(dr["SALDO_DE_COSTOS1"]) : 0;
                    objOrdenServicio.COSTO_PLAN_OR = dr["COSTO_PLAN_OR"] != DBNull.Value ? Convert.ToSingle(dr["COSTO_PLAN_OR"]) : 0;
                    objOrdenServicio.COSTO_REAL_OR = dr["COSTO_REAL_OR"] != DBNull.Value ? Convert.ToSingle(dr["COSTO_REAL_OR"]) : 0;
                    objOrdenServicio.SALDO_DE_COSTOS_OR = dr["SALDO_DE_COSTOS_OR"] != DBNull.Value ? Convert.ToSingle(dr["SALDO_DE_COSTOS_OR"]) : 0;
                    objOrdenServicio.CambiarEstado = dr["CambiarEstado"] != DBNull.Value ? Convert.ToInt32(dr["CambiarEstado"]) : 0;
                    objOrdenServicio.FechaIngreso = dr["FechaIngreso"] != DBNull.Value ? Convert.ToDateTime(dr["FechaIngreso"]) : DateTime.MinValue;

                    objOrdenServicio.Usuario = dr["Usuario"].ToString();

                    listaOrdenServicio.Add(objOrdenServicio);
                }

            }
            catch (Exception ex)
            {
                listaOrdenServicio = null;
            }
            finally
            {
                cmd.Connection.Close();
            }

            return listaOrdenServicio;
        }

    }
}
