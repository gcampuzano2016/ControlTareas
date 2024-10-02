using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using JSONHelper;
using System.Text;
using CapaEntidad;
using CapaNegocio;
using SeguridadAppHelper;
using System.Web.Script.Serialization;
using System.Globalization;
using CorreoHelper;
using ClosedXML.Excel;
using System.Data;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using PDF;

namespace JsonJQueryNetTareas
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class ObtenerListaTareas : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            dynamic parametros;
            StringBuilder responseAction = new StringBuilder();
            if (context.Request.ContentType.Contains("json"))
            {
                var inputStream = new System.IO.StreamReader(context.Request.InputStream);
                var inputJson = inputStream.ReadToEnd();
                List<RespuestaJson> collectionJson = inputJson.DeserializarJsonTo<List<RespuestaJson>>();

                JavaScriptSerializer i = new JavaScriptSerializer();
                parametros = i.Deserialize(inputJson.ToString(), typeof(object));

                var parameters = parametros[0]["parameters"];
                var Action = parametros[0]["action"];
                Boolean existAction = false;


                if (Action == "ListaComboPerfiles")
                {
                    existAction = true;
                    responseAction.Append(ObtenerComboPerfiles(parameters));
                }


                if (Action == "ListaUsuariosPerfil")
                {
                    existAction = true;
                    responseAction.Append(ObtenerDetallePerfilesUsuario(parameters));
                }

                if (Action == "GuardarUsuarioPerfil")
                {
                    existAction = true;
                    responseAction.Append(GuardarUsuarioPerfil(parameters));
                }


                if (Action == "ListaTareasPorUsuario")
                {
                    existAction = true;
                    responseAction.Append(ObtenerTareasPorUsuario(parameters));
                }

                if (Action == "ListaTareasPorFechaUsuario")
                {
                    existAction = true;
                    responseAction.Append(ObtenerTareasPorFechaUsuario(parameters));
                }
                

                if (Action == "ListaDetalleTareasPorUsuario")
                {
                    existAction = true;
                    responseAction.Append(ObtenerDetalleTareasPorUsuario(parameters));
                }

                if (Action == "ListaDetalleTareasPorUsuarioRechazada")
                {
                    existAction = true;
                    responseAction.Append(ObtenerDetalleTareasPorUsuarioRechazadas(parameters));
                }

                if (Action == "ListaUsuarios")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaUsuarios(parameters));
                }
                
                if (Action == "DetalleTarea")
                {
                    existAction = true;
                    responseAction.Append(ObtenerDetalleTarea(parameters));
                }

                if (Action == "DetalleTareasDescargaXLS")
                {
                    existAction = true;
                    responseAction.Append(ObtenerDetalleTareaDescargaXLS(parameters));
                }

                if (Action == "DetalleHorasExtrasDescargaXLS")
                {
                    existAction = true;
                    responseAction.Append(ObtenerDetalleHorasExtrasDescargaXLS(parameters));
                }

                if (Action == "DetalleTareaPrincipal")
                {
                    existAction = true;
                    responseAction.Append(ObtenerDetalleTareaPrincipal(parameters));
                }
                
                if (Action == "ListaComboContrato")
                {
                    existAction = true;
                    responseAction.Append(ObtenerComboContrato(parameters));
                }

                if (Action == "ListaComboEstados")
                {
                    existAction = true;
                    responseAction.Append(ObtenerComboEstados(parameters));
                }

                if (Action == "ListaRecursosHorasDiarias")
                {
                    existAction = true;
                    responseAction.Append(ObtenerRecursosHorasDiarias(parameters));
                }

                if (Action == "ListaGastosCuentasContable")
                {
                    existAction = true;
                    responseAction.Append(ObtenerGastosCuentasContable(parameters));
                }

                if (Action == "ListaDatosCoutasAnual")
                {
                    existAction = true;
                    responseAction.Append(ObtenerDatosCoutasAnual(parameters));
                }

                if (Action == "ListaComboCatalogo")
                {
                    existAction = true;
                    responseAction.Append(ObtenerCatalogoCombo(parameters));
                }
             
                if (Action == "ListaComboGasto")
                {
                    existAction = true;
                    responseAction.Append(ObtenerCatalogoCombo(parameters));
                }

                if (Action == "ListaComboGastoReporte")
                {
                    existAction = true;
                    responseAction.Append(ObtenerCatalogoTodosCombo(parameters));
                }

                if (Action == "ListaComboFecha")
                {
                    existAction = true;
                    responseAction.Append(ObtenerCatalogoFechaCombo(parameters));
                }

                if (Action == "ListaComboAprobacionTarea")
                {
                    existAction = true;
                    responseAction.Append(ObtenerCatalogoComboAprobacion(parameters));
                }

                if (Action == "ListaComboAprobacionTareaQA")
                {
                    existAction = true;
                    responseAction.Append(ObtenerCatalogoComboAprobacion(parameters));
                }

                if (Action == "ListaParaHorasExtras")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaTareasHorasExtras(parameters));
                }

                if (Action == "ListaConsultaTareasGeneradasPorRevisar")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaTareasGeneradasPorRevisar(parameters));
                }

                if (Action == "ListaConsultaTareasGeneradasPorRevisarUnificado")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaTareasGeneradasPorRevisarUnificado(parameters));
                }

                if (Action == "ListaConsultaTareasGeneradas")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaTareasGeneradas(parameters));
                }

                if (Action == "ListaConsultaTareasDescargaXLS")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaTareasGeneradasDescargar(parameters));
                }

                if (Action == "ListaConsultaTareasGeneradasDescargaPorRevisar")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaTareasGeneradasDescargarPorRevisar(parameters));
                }

                if (Action == "ListaConsultaTareasGeneradasDescargaPorRevisarUnificado")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaTareasGeneradasDescargarPorRevisarUnificado(parameters));
                }

                if (Action == "ListaConsultaHorasContratadas")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaHorasContratadas(parameters));
                }

                if (Action == "ListaConsultaTareasGeneradasContrato")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaTareasGeneradasContrato(parameters));
                }


                if (Action == "ListaConsultaTareasContratoDescargaXLS")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaTareasGeneradasContratoDescargar(parameters));
                }

                if (Action == "ReporteGPFGeneradas")
                {
                    existAction = true;
                    responseAction.Append(ObtenerReporteGPFGeneradas(parameters));
                }

                if (Action == "ReporteGPFDescargaXLS")
                {
                    existAction = true;
                    responseAction.Append(ObtenerReporteGPFGeneradasDescargar(parameters));
                }

                if (Action == "ReporteListaContratos")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaContratos(parameters));
                }

                if (Action == "ReporteListaPedidos")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaPedidos(parameters));
                }

                if (Action == "ReporteListaSolicitud")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaSolicitud(parameters));
                }

                if (Action == "ReporteListaSolicitudActualizar")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaSolicitudAct(parameters));
                }

                if (Action == "SolicitudIndividual")
                {
                    existAction = true;
                    responseAction.Append(ObtenerSolicitudIndividual(parameters));
                }

                if (Action == "ReporteListaSaldoVacaciones")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaSaldoVacaciones(parameters));
                }

                if (Action == "ReporteListaSaldoVacacionesIndividual")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaSaldoVacacionesIndividual(parameters));
                }

                if (Action == "ReporteDatosEmpleado")
                {
                    existAction = true;
                    responseAction.Append(ObtenerDatosEmpleado(parameters));
                }

                if (Action == "ListadeCorreos")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListadeCorreos(parameters));
                }

                if (Action == "ReporteListaRebates")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaRebates(parameters));
                }

                if (Action == "ReporteConsultarContratos")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarContratos(parameters));
                }

                if (Action == "ReporteConsultarPedidos")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarPedidos(parameters));
                }

                if (Action == "ReporteConsultarPoliza")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarPoliza(parameters));
                }

                if (Action == "ReporteConsultarForeCast")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarForeCast(parameters));
                }

                if (Action == "ConsultarMensajeForeCast")
                {
                    existAction = true;
                    responseAction.Append(ConsultarMensajeForeCast(parameters));
                }

                if (Action == "ReporteConsultarForeCastGD")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarForeCastGD(parameters));
                }

                if (Action == "ReporteConsultarCosteo")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarCosteo(parameters));
                }

                if (Action == "ReporteValidacionForeCast")
                {
                    existAction = true;
                    responseAction.Append(ObtenerValidacionForeCast(parameters));
                }

                if (Action == "ReporteConsultarPolizaFiltros")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarPolizaFiltro(parameters));
                }

                if (Action == "ReporteConsultarPolizasFiltros")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarPolizasFiltro(parameters));
                }

                if (Action == "ReporteConsultarReporteVentas")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarReporteVentas(parameters));
                }

                if (Action == "ReporteConsultarReporteVentasDescargar")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarReporteVentasDescargar(parameters));
                }

                if (Action == "ReporteConsultarForeCastFiltros")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarForeCastFiltro(parameters));
                }

                if (Action == "ReporteConsultarForeCastFiltrosDescargar")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarForeCastFiltroDescargar(parameters));
                }

                if (Action == "ReporteConsultarForeCastFiltrosDescargarPersonalizado")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarForeCastFiltroDescargarPersonalizado(parameters));
                }


                if (Action == "ReporteConsultarForeCastFiltrosDescargarGerencia")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarForeCastFiltroDescargarGerencia(parameters));
                }

                //DESCARGA
                if (Action == "ReporteDescargaXLS")
                {
                    existAction = true;
                    responseAction.Append(ObtenerReporteDescargar(parameters));
                }

                if (Action == "ReporteDescargaTabla7XLS")
                {
                    existAction = true;
                    responseAction.Append(ObtenerReporteDescargarTabla7(parameters));
                }

                if (Action == "ReporteConsultarForeCastFiltrosGD")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarForeCastGDFiltro(parameters));
                }

                if (Action == "ReporteConsultarForeCastFiltrosDescargarGD")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarForeCastGDFiltroDescargar(parameters));
                }


                if (Action == "ReporteConsultarCosteoFiltros")
                {
                    existAction = true;
                    responseAction.Append(ObteneReporteConsultarCosteoFiltros(parameters));
                }

                if (Action == "ReporteConsultarCosteoFiltrosDescargar")
                {
                    existAction = true;
                    responseAction.Append(ObteneReporteConsultarCosteoFiltrosDescargar(parameters));
                }



                if (Action == "ReporteConsultarPolizasFiltrosDescargar")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarPolizasFiltroDescargar(parameters));
                }

                if (Action == "ReporteConsultarRebates")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarRebetes(parameters));
                }

                if (Action == "ReporteConsultarPeriodoFiscal")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarPeriodoFiscal(parameters));
                }

                if (Action == "ListaContratosDescargaXLS")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaContratosDescargar(parameters));
                }

                if (Action == "ListaPedidosDescargaXLS")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaPedidosDescargar(parameters));
                }

                if (Action == "ListaSolicitudDescargaXLS")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaSolicitudDescargar(parameters));
                }

                if (Action == "ListaRebatesDescargaXLS")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaRebatesDescargar(parameters));
                }

                if (Action == "ListaGastosCuentasContableDescargaXLS")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaGastosCuentasDescargar(parameters));
                }

                if (Action == "GuardarNuevoContrato")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoContratoBase(parameters));
                }


                if (Action == "GuardarNuevoPedido")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoPedidoBase(parameters));
                }

                if (Action == "GuardarNuevoForecast")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoForecastBase(parameters));
                }

                if (Action == "GuardarPrioridad")
                {
                    existAction = true;
                    responseAction.Append(GuardarPrioridadForeCast(parameters));
                }

                if (Action == "GuardarNuevoMensajeForecast")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoMensajeForecastBase(parameters));
                }

                if (Action == "GuardarNuevoForecastGD")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoForecastGDBase(parameters));
                }

                if (Action == "GuardarInventario")
                {
                    existAction = true;
                    responseAction.Append(GGuardarNuevoEgresoInventarioBase(parameters));
                }

                if (Action == "GuardarNuevoProducto")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoProductoBase(parameters));
                }

                if (Action == "GuardarNuevoCosteo")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoCosteoBase(parameters));
                }

                if (Action == "GuardarNuevoRebates")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoRebatesBase(parameters));
                }

                if (Action == "GuardarNuevoPeridoFiscal")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoPeridoFiscalBase(parameters));
                }


                if (Action == "GuardarNuevoBanco")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoBancoBase(parameters));
                }

                if (Action == "ActualizarFechaPedido")
                {
                    existAction = true;
                    responseAction.Append(ActualizarFechaPedidoBase(parameters));
                }

                if (Action == "GuardarNuevoCliente")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoClienteBase(parameters));
                }

                if (Action == "GuardarNuevoClienteGD")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoClienteGDBase(parameters));
                }

                if (Action == "GuardarCuotaAnual")
                {
                    existAction = true;
                    responseAction.Append(GuardarCuotaAnualBase(parameters));
                }

                if (Action == "GuardarCuotaAnualEmpresa")
                {
                    existAction = true;
                    responseAction.Append(GuardarCuotaAnualEmpresaBase(parameters));
                }

                if (Action == "GuardarNuevaPoliza")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevaPolizaBase(parameters));
                }

                if (Action == "GuardarNuevoMantenimiento")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoMantenimientoBase(parameters));
                }

                if (Action == "ActualizarNuevoContrato")
                {
                    existAction = true;
                    responseAction.Append(ActualizarNuevoContratoBase(parameters));
                }

                if (Action == "EliminarNuevoContrato")
                {
                    existAction = true;
                    responseAction.Append(EliminarNuevoContratoBase(parameters));
                }

                if (Action == "CargarRequerimientosContrato")
                {
                    existAction = true;
                    responseAction.Append(CargarRequerimientos(parameters));
                }

                if (Action == "ListaRequerimientosContrato")
                {
                    existAction = true;
                    responseAction.Append(ListaRequerimientos(parameters));
                }

                if (Action == "ListaRequerimientosContratoDescargar")
                {
                    existAction = true;
                    responseAction.Append(ListaRequerimientosDescargar(parameters));
                }

                if (Action == "BuscarListaCliente")
                {
                    existAction = true;
                    responseAction.Append(BuscarListaClientes(parameters));
                }

                if (Action == "MostrarListaEgreso")
                {
                    existAction = true;
                    responseAction.Append(MostrarListaEgresos(parameters));
                }

                if (Action == "MostrarListaEgresoDescargar")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarEgresosDescargar(parameters));
                }

                if (Action == "BuscarListaInventario")
                {
                    existAction = true;
                    responseAction.Append(BuscarListaInventario(parameters));
                }

                if (Action == "BuscarListaInventarioDescargar")
                {
                    existAction = true;
                    responseAction.Append(BuscarListaInventarioDescargar(parameters));
                }

                if (Action == "BuscarListaInventarioFinalDescargar")
                {
                    existAction = true;
                    responseAction.Append(BuscarListaInventarioDescargar(parameters));
                }

                if (Action == "BuscarClientePedido")
                {
                    existAction = true;
                    responseAction.Append(BuscarClientePedidos(parameters));
                }

                if (Action == "GuardarNuevoTicket")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoTicketBase(parameters));
                }

                if (Action == "GuardarNuevoPermiso")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoPermisoBase(parameters));
                }

                if (Action == "GuardarNuevaVacaciones")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevaVacacionesBase(parameters));
                }

                if (Action == "ActualizarSolicitud")
                {
                    existAction = true;
                    responseAction.Append(ActualizarSolicitudBase(parameters));
                }

                if (Action == "ResetearPassword")
                {
                    existAction = true;
                    responseAction.Append(ResetearPasswordBase(parameters));
                }

                if (Action == "ListaComboAnioServicioYGestor")
                {
                    existAction = true;
                    responseAction.Append(ObtenerComboServicios(parameters));
                }

                if (Action == "ListaReportes")
                {
                    existAction = true;
                    responseAction.Append(ObtenerDetalleReportes(parameters));
                }

                if (Action == "DetalleOrdenServicio")
                {
                    existAction = true;
                    responseAction.Append(ObtenerOrdenServicio(parameters));
                }



                if (Action == "ListaMenu")
                {
                    existAction = true;
                    responseAction.Append(ObtenerDetalleMenu(parameters));
                }


                if (Action == "AgregarMenu")
                {
                    existAction = true;
                    responseAction.Append(InsertarNuevoMenu(parameters));
                }

                if (Action == "GuardarDatosMenu")
                {
                    existAction = true;
                    responseAction.Append(GuardarDatosMenu(parameters));
                }

                if (Action == "GuardarDatosIcono")
                {
                    existAction = true;
                    responseAction.Append(GuardarDatosIcono(parameters));
                }

                if (Action == "ConsultarClienteInventarios")
                {
                    existAction = true;
                    responseAction.Append(ConsultarClienteInventarios(parameters));
                }

                if (Action == "GuardarNuevoInfoContrato")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoInfoContrato(parameters));
                }

                if (Action == "ConsultarOrdenServicioPedido")
                {
                    existAction = true;
                    responseAction.Append(ConsultarOrdenServicioPedido(parameters));
                }

                if (Action == "BuscarContrato")
                {
                    existAction = true;
                    responseAction.Append(BuscarContrato(parameters));
                }

                if (Action == "ReporteFacturasProveedor")
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarReporteFacturasProveedor(parameters));
                }

                if (Action == "GuardarNuevaHistoria")
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevaHistoria(parameters));
                }


                if (Action == "GuardarHisInmunizaciones")
                {
                    existAction = true;
                    responseAction.Append(GuardarHisInmunizaciones(parameters));
                }

                if (Action == "ConsultarProvinciasCiudades")
                {
                    existAction = true;
                    responseAction.Append(ConsultarProvinciasCiudades(parameters));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger", ""));
                }


            }
            context.Response.ContentType = "application/json";
            context.Response.Charset = "utf-8";
            context.Response.Write(responseAction.ToString());
        }

        public string BuscarContrato(dynamic parameters)
        {
            EntInfoContrato contrato = new EntInfoContrato();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string idUsuario = seguridad.Desencripta(session.ToString());
            string numContrato = parameters["descripcion"].ToString();

            /*try
            {
                // Obtener la información del contrato con los permisos
                contrato = NegInfoContrato.Sp_RTAConsultarPermisoContratoNum(numContrato, idUsuario);
 
                // Verificar si el idUsuario tiene permisos
                if (contrato == null)
                {
                    return responseMessage("0", "No se encontró el contrato especificado.", "warning", "");
                }
 
                List<string> permisos = contrato.PERMISOS.Split(',').ToList();
 
                if (permisos.Contains("1"))
                {
                    contrato.opcion = 1;
                }
                else if (permisos.Contains("2"))
                {
                    // Si tiene permiso, cargar la información del contrato
                    contrato = NegInfoContrato.Sp_RTAConsultarContratoNum(numContrato);
                    if (contrato == null)
                    {
                        return responseMessage("0", "No se encontró el contrato especificado.", "warning", "");
                    }
                    contrato.opcion = 2;
                }
                else if (permisos.Contains("3"))
                {
                    contrato = NegInfoContrato.Sp_RTAConsultarContratoNum(numContrato);
                    if (contrato == null)
                    {
                        return responseMessage("0", "No se encontró el contrato especificado.", "warning", "");
                    }
                    contrato.opcion = 3;
                }
                else
                {
                    // Si no tiene permiso, devolver un mensaje de error
                    return responseMessage("0", "No tienes permisos para ver este contrato.", "danger", "");
                }*/


            try
            {
                // Obtener la información del contrato
                contrato = NegInfoContrato.Sp_RTAConsultarContratoNum(numContrato);

                // Verificar si el contrato fue encontrado
                if (contrato == null)
                {
                    return responseMessage("0", "No se encontró el contrato especificado.", "warning", "");
                }

                contrato.opcion = 2; // O cualquier valor que sea necesario para 'opcion'
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al obtener los datos. " + ex.Message, "danger", "");
            }

            return contrato.SerializaToJson();
        }

        public string GuardarNuevoInfoContrato(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            //EntInfoContrato contrato = new EntInfoContrato();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            //contrato = NegInfoContrato.Sp_InsertarActualizarContrato(IdUsuarioSession);

            try
            {
                EntInfoContrato registro = new EntInfoContrato();
                //registro.IdContrato = campos["txtIdEmpleado"]);
                int usuario = 0;
                usuario = campos["formulario"];

                //if(usuario == 1)
                //{
                registro.opcion = usuario;
                registro.CLIENTE = campos["txtCliente"];
                registro.CLI_NOMBRE = campos["txtNomContacto"];
                registro.CLI_TELEFONO = campos["txtTelefono"];
                registro.CLI_DIRECCION = campos["txtDireccion"];
                registro.CLI_CORREO = campos["txtCorreo"];
                registro.NUM_CONTRATO = campos["txtNumContrato"];
                registro.OBJETO = campos["txtObjeto"];
                registro.VALOR_TOTAL_CONTRATO = Convert.ToDecimal(campos["txtValorContrato"], CultureInfo.InvariantCulture);
                registro.ALCANCE = campos["txtAlcance"];
                registro.HARDWARE = campos["txtHardware"];
                registro.LICENCIAS = campos["txtLicencias"];
                registro.SERVICIOS_FABRICANTE = campos["txtServiciosFab"];
                registro.SERVICIO_DOS = campos["txtServiciosDOS"];
                registro.SERVICIO_EXTERNOS = campos["txtServiciosExt"];
                registro.POLIZAS = campos["txtPolizas"];
                registro.TERMINOS_TDR = campos["txtTDR"];
                registro.FORMA_PAGO = campos["selectFormaPago"];
                registro.FECHA_SUSCRIPCION_CONTRATO = campos["fechaSuscripContrato"];
                registro.FECHA_NOTIF_ANTICIPO = campos["fechaNotifAnticipo"];
                registro.FECHA_INICIO_GARANTIA = campos["fechaIniActivacion"];
                registro.FECHA_FIN_GARANTIA = campos["fechaFinActivacion"];
                registro.PLAZO_ACTIVACION = campos["txtPlazoActGarantia"];
                registro.PLAZO_ACTIVACION_LIC = campos["txtPlazoActLicencia"];
                registro.DURACION_VIGENCIA_TEC = campos["txtDuracionVigTec"];

                registro.OBS_NUM_CONTRATO = campos["txtNumContratoObs"];
                registro.OBS_VALOR_TOTAL = campos["txtValorContratoObs"];
                registro.OBS_OBJETO = campos["txtObjetoObs"];
                registro.OBS_ALCANCE = campos["txtAlcanceObjetoObs"];
                registro.OBS_HARDWARE = campos["txtHardwareObs"];
                registro.OBS_LICENCIAS = campos["txtLicenciasObs"];
                registro.OBS_SERVICIOS_FABRICANTE = campos["txtServiciosFabObs"];
                registro.OBS_SERVICIO_DOS = campos["txtServiciosDOSObs"];
                registro.OBS_SERVICIO_EXTERNOS = campos["txtServiciosExternosObs"];
                registro.OBS_POLIZAS = campos["txtPolizasObs"];
                registro.OBS_TERMINOS_TDR = campos["txtTerminosObs"];
                registro.OBS_FORMA_PAGO = campos["txtFormaPagoObs"];

                registro.ACTA_PREGUNTAS = campos["txtPreguntas"];
                registro.ACTA_ADJUDICACION = campos["txtActAdj"];
                registro.ACTA_NEGOCIACION = campos["txtActNeg"];
                registro.BOM_SOLUCION = campos["txtBomSolucion"];
                registro.ACUERDOS_MAY = campos["txtAcuMayoristas"];
                registro.ACUERDOS_FAB = campos["txtAcuFabricantes"];
                registro.OBS_ACTA_PREGUNTAS = campos["txtActaPreguntasObs"];
                registro.OBS_ACTA_ADJUDICACION = campos["txtActaAdjObs"];
                registro.OBS_ACTA_NEGOCIACION = campos["txtActaNegObs"];
                registro.OBS_BOM_SOLUCION = campos["txtBomSolucionsObs"];
                registro.OBS_ACUERDOS_MAY = campos["txtAcuMayoristasObs"];
                registro.OBS_ACUERDOS_FAB = campos["txtAcuFabricantesObs"];

                //registro.opcion = 3;
                registro.GARANTIAS_FIN = campos["txtGarantiasFIN"];
                registro.GARANTIAS_TEC = campos["txtGarantiasTEC"];
                registro.GENERACION_PEDIDOS = campos["txtGenPedidos"];
                registro.OBS_GARANTIAS_FIN = campos["txtGarFinObs"];
                registro.OBS_GARANTIAS_TEC = campos["txtGarTECObs"];
                registro.OBS_GENERACION_PEDIDOS = campos["txtGenPedidosObs"];




                //registro.PLAZO_ACTIVACION = campos["txtFechaNacimiento"];
                //registro.DURACION_VIGENCIA_TEC = campos["txtFechaNacimiento"];

                registro.ENTREGA_LIC_TEMPORALES = campos["txtLicenciaTemporales"];

                //registro.OBS_CLIENTE = campos["txtClienteObs"];

                respuesta = NegInfoContrato.Sp_InsertarActualizarContrato(registro);

                if (respuesta.estado == "1")
                {
                    List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
                    EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                    EntParametrosCorreo parametrosServidorCorreo = new EntParametrosCorreo();
                    string contenidoCorreo = "";

                    listaCamposCorreo.Add(new EntItemValor() { Item = "txtcliente", Valor = campos["txtCliente"] });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Contrato:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "txtnumContrato", Valor = campos["txtNumContrato"] });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "titValorContrato", Valor = "Valor Total de Contrato:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "txtValorContrato", Valor = campos["txtValorContrato"] });

                    parametrosServidorCorreo.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
                    parametrosServidorCorreo.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
                    parametrosServidorCorreo.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
                    parametrosServidorCorreo.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
                    parametrosServidorCorreo.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
                    parametrosServidorCorreo.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

                    bool respuestaEnvioCorreo = false;

                    /*
                    listaCamposCorreo.Add(new EntItemValor() { Item = "txtcliente", Valor = "PLANIFICACION DE VACACIONES" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Estimados2. " });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "titValorContrato", Valor = "Estimados1. " });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "txtValorContrato", Valor = "12543767 " });
                    */
                    //contenidoCorreo = estructuraContenidoCorreo;

                    foreach (EntItemValor parametrosContenido in listaCamposCorreo)
                    {
                        contenidoCorreo = contenidoCorreo.Replace("[" + parametrosContenido.Item + "]", parametrosContenido.Valor);
                    }

                    //respuestaEnvioCorreo=envioCorreo.EnviarCorreo("leoct1871@gmail.com", "Correo de prueba", "HOLA MUNDO...!!", parametrosServidorCorreo);
                    respuestaEnvioCorreo = envioCorreo.EnvioCorreo("lsalazar@dos.com.ec;prueda@dos.com.ec", "Ingreso de nuevo contrato", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoFormatoCorreoContrato.txt"), listaCamposCorreo);

                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");
            }
            return respuesta.SerializaToJson();
        }

        public string ConsultarOrdenServicioPedido(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            List<EntOrdenServicio> Ent1 = new List<EntOrdenServicio>();

            //string IdUsuarioSession = "";
            //IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            float pedido = float.Parse(campos["session"]);
            try
            {
                Ent1 = NegInfoContrato.Sp_RTAConsultarOSnumPedido(pedido);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");
            }
            //GuardarHistoria();

            return Ent1.SerializaToJson();

        }

        public string ConsultarClienteInventarios(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            List<EntClienteEgrInventario> Ent1 = new List<EntClienteEgrInventario>();
            List<EntDetalleInventario> Ent2 = new List<EntDetalleInventario>();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            int idEncabezado  =Convert.ToInt32(campos["tipo"]);
            int op = Convert.ToInt32(campos["opcion"]);
            try
            {
                if (op == 1)
                {
                    Ent1 = NegEgresoInventario.ConsultaRTAListaClienteInventario(idEncabezado);
                }
                if (op == 2)
                {
                    Ent2 = NegEgresoInventario.ConsultaRTAListaDetallesInv(idEncabezado);
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");
            }
            //GuardarHistoria();
            if (op == 1)
            {
                return Ent1.SerializaToJson();
            }
            else if (op == 2)
            {
                return Ent2.SerializaToJson();
            }
            else
            {
                return "";
            }
        }

        public string ObtenerTareasPorUsuario(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();
            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string codigoResponsable = parameters["codigoResponsable"].ToString();

            try
            {

                string IdUsuario = seguridad.Desencripta(session);

                if (codigoResponsable == "" || codigoResponsable == null)
                {
                    listaTareas = NegTareas.ListaTareas(IdUsuario);
                }
                else
                {
                    listaTareas = NegTareas.ListaTareas(codigoResponsable);
                }

                //respuesta.estado = "1";
                //respuesta.mensaje = "OK";
                //respuesta.tipoMensaje = "success";
                //respuesta.resultado = listaTareas.SerializaToJson();

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }


            return listaTareas.SerializaToJson();

        }

        public string ObtenerTareasPorFechaUsuario(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();
            SeguridadHelper seguridad = new SeguridadHelper();

            string idUsuario = parameters["usuario"].ToString();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string session = parameters["session"].ToString();
            int tipo = 0;
            string IdUsuarioSession = "";
            bool usuarioEsJefe = false;

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                usuarioEsJefe = NegUsuario.RTA_ConsultaUsuarioEsJefe(IdUsuarioSession);

                if (usuarioEsJefe)
                {
                    tipo = 1;

                    if (Convert.ToInt32(idUsuario) > 0)
                    {
                        tipo = 0;
                    }
                }
                else
                {
                    tipo = 0;
                }

                listaTareas = NegTareas.ListaTareasFechas(idUsuario, fechaDesde, fechaHasta, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaTareas.SerializaToJson();


        }

        public string ObtenerDetalleTareasPorUsuario(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntDetTarea> listaTareas = new List<EntDetTarea>();
            SeguridadHelper seguridad = new SeguridadHelper();

            string idUsuario = parameters["usuario"].ToString();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string session = parameters["session"].ToString();

            string cliente = parameters["cliente"].ToString();
            string orden = parameters["orden"].ToString();
            int idgasto = Convert.ToInt32(parameters["idtipogasto"].ToString());

            int idTareaAprobada = Convert.ToInt32(parameters["AprobarTarea"].ToString());
            int idTareaAprobadaQA = Convert.ToInt32(parameters["AprobarTareaQA"].ToString());

            int IdFecha = Convert.ToInt32(parameters["TipoFecha"].ToString());

            int tipo = 0;
            string IdUsuarioSession = "";
            bool usuarioEsJefe = false;

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                usuarioEsJefe = NegUsuario.RTA_ConsultaUsuarioEsJefe(IdUsuarioSession);

                if (usuarioEsJefe)
                {
                    tipo = 1;

                    if (Convert.ToInt32(idUsuario) > 0)
                    {
                        tipo = 0;
                        idUsuario = parameters["usuario"].ToString();
                    }
                    else
                    {
                        idUsuario = IdUsuarioSession;
                    }
                }
                else
                {
                    tipo = 0;
                    idUsuario = parameters["usuario"].ToString();
                }
                DateTime fechaInicial = Convert.ToDateTime(fechaDesde);
                DateTime fechaFinal = Convert.ToDateTime(fechaHasta);
                listaTareas = NegTareas.ListaDetTareas(idUsuario, fechaInicial.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), fechaFinal.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), cliente, orden, idgasto, idTareaAprobada, idTareaAprobadaQA, IdFecha, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaTareas.SerializaToJson();


        }
        public string ObtenerDetalleTareasPorUsuarioRechazadas(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntDetTarea> listaTareas = new List<EntDetTarea>();
            SeguridadHelper seguridad = new SeguridadHelper();

            string idUsuario = parameters["usuario"].ToString();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string session = parameters["session"].ToString();

            string cliente = parameters["cliente"].ToString();
            string orden = parameters["orden"].ToString();
            int idgasto = Convert.ToInt32(parameters["idtipogasto"].ToString());

            int idTareaAprobada = Convert.ToInt32(parameters["AprobarTarea"].ToString());
            int idTareaAprobadaQA = Convert.ToInt32(parameters["AprobarTareaQA"].ToString());

            int IdFecha = Convert.ToInt32(parameters["TipoFecha"].ToString());

            int tipo = 0;
            string IdUsuarioSession = "";
            bool usuarioEsJefe = false;

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                usuarioEsJefe = NegUsuario.RTA_ConsultaUsuarioEsJefe(IdUsuarioSession);

                if (usuarioEsJefe)
                {
                    tipo = 1;

                    if (Convert.ToInt32(idUsuario) > 0)
                    {
                        tipo = 0;
                        idUsuario = parameters["usuario"].ToString();
                    }
                    else
                    {
                        idUsuario = IdUsuarioSession;
                    }
                }
                else
                {
                    tipo = 0;
                    idUsuario = parameters["usuario"].ToString();
                }
                DateTime fechaInicial = Convert.ToDateTime(fechaDesde);
                DateTime fechaFinal = Convert.ToDateTime(fechaHasta);
                listaTareas = NegTareas.ListaDetTareasRechazadas(idUsuario, fechaInicial.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), fechaFinal.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), cliente, orden, idgasto, idTareaAprobada, idTareaAprobadaQA, IdFecha, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaTareas.SerializaToJson();


        }
        public string ObtenerRecursosHorasDiarias(dynamic parameters)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();

            string idUsuario = parameters["usuario"].ToString();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            int estado = Convert.ToInt32(parameters["estado"].ToString());

            string session = parameters["session"].ToString();
            int tipo = 0;
            string IdUsuarioSession = "";
            bool usuarioEsJefe = false;
            string IdUsuarioConsulta = "";

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                usuarioEsJefe = NegUsuario.RTA_ConsultaUsuarioEsJefe(IdUsuarioSession);

                if (usuarioEsJefe)
                {
                    tipo = 1;

                    if (Convert.ToInt32(idUsuario) > 0)
                    {
                        IdUsuarioConsulta = idUsuario;
                    }
                    else
                    {
                        IdUsuarioConsulta = IdUsuarioSession;
                    }
                }
                else
                {
                    tipo = 0;
                    IdUsuarioConsulta = idUsuario;
                }

                respuesta = NegTareas.ListaHorasRecursosPorJefatura(IdUsuarioConsulta, fechaDesde, fechaHasta, tipo, estado);

                
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            //return respuesta.resultadoTabla.SerializaToJson();
            return JsonConvert.SerializeObject(respuesta.resultadoTabla);


        }
        public string ObtenerListaUsuarios(dynamic parameters)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            List<EntCombo> cmbUsuario = new List<EntCombo>();

            try
            {
                string IdUsuario = seguridad.Desencripta(parameters.ToString());
                cmbUsuario = NegUsuario.ListaUsuariosCombo(IdUsuario);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return cmbUsuario.SerializaToJson();


        }

        public string ObtenerGastosCuentasContable(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntCuentaContable> listaGastosCuentas = new List<EntCuentaContable>();

            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string sucursal = parameters["sucursal"].ToString();
            string area = parameters["area"].ToString();
            if (sucursal == "0")
            {
                sucursal = "";
            }

            if (area == "0")
            {
                area = "";
            }

            try
            {
                listaGastosCuentas = NegCuentaContable.ConsultaSp_RTAListaCuentasContables(fechaDesde, fechaHasta, sucursal, area);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaGastosCuentas.SerializaToJson();


        }

        public string ObtenerDatosCoutasAnual(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntCuotaAnual> listaCuotaAnual = new List<EntCuotaAnual>();

            string anio = parameters["anio"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                listaCuotaAnual = NegCuotaAnual.ConsultaRTA_ConsultarCuotasAnual(anio, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaCuotaAnual.SerializaToJson();


        }

        public string ObtenerListaTareasHorasExtras(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();
            SeguridadHelper seguridad = new SeguridadHelper();

            string IdUsuario = seguridad.Desencripta(parameters["session"].ToString());
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string IdResponsable = parameters["usuario"].ToString();
            int estado = Convert.ToInt32(parameters["estado"].ToString());

            if (IdResponsable == "null")
                IdResponsable = "0";

            try
            {
                listaTareas = NegTareas.ConsultaHorasExtrasPorAutorizar(fechaDesde, fechaHasta, IdResponsable, estado, IdUsuario);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaTareas.SerializaToJson();


        }

        public string ObtenerListaTareasGeneradas(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();

            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            int Id_RegTareas = Convert.ToInt32(parameters["registro"].ToString());
            int estado = Convert.ToInt32(parameters["estado"].ToString());
            string busqueda = parameters["busqueda"].ToString();



            try
            {
                listaTareas = NegTareas.ConsultaTareasGeneradas(fechaDesde, fechaHasta,  estado, Id_RegTareas,"", busqueda);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaTareas.SerializaToJson();
        }

        public string ObtenerListaTareasGeneradasPorRevisar(dynamic parameters)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();

            string idUsuario = parameters["usuario"].ToString();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            int Id_RegTareas = Convert.ToInt32(parameters["registro"].ToString());
            int estado = Convert.ToInt32(parameters["estado"].ToString());
            string busqueda = parameters["busqueda"].ToString();
            string session = parameters["session"].ToString();

            string IdUsuarioSession = "";
            bool usuarioEsJefe = false;
            string IdUsuarioConsulta = "";

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                usuarioEsJefe = NegUsuario.RTA_ConsultaUsuarioEsJefe(IdUsuarioSession);
                if (usuarioEsJefe)
                {
                    if (Convert.ToInt32(idUsuario) > 0)
                    {
                        IdUsuarioConsulta = idUsuario;
                    }
                    else
                    {
                        IdUsuarioConsulta = IdUsuarioSession;
                    }
                }
                else
                {
                    IdUsuarioConsulta = IdUsuarioSession;
                }


                listaTareas = NegTareas.ConsultaTareasGeneradas(fechaDesde, fechaHasta, 2, Id_RegTareas, IdUsuarioConsulta, busqueda);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaTareas.SerializaToJson();


        }

        public string ObtenerListaTareasGeneradasDescargarPorRevisar(dynamic parameters)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();

            string idUsuario = parameters["usuario"].ToString();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            int Id_RegTareas = Convert.ToInt32(parameters["registro"].ToString());
            int estado = Convert.ToInt32(parameters["estado"].ToString());
            string busqueda = parameters["busqueda"].ToString();

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            string session = parameters["session"].ToString();

            string IdUsuarioSession = "";
            bool usuarioEsJefe = false;
            string IdUsuarioConsulta = "";

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                usuarioEsJefe = NegUsuario.RTA_ConsultaUsuarioEsJefe(IdUsuarioSession);
                if (usuarioEsJefe)
                {
                    if (Convert.ToInt32(idUsuario) > 0)
                    {
                        IdUsuarioConsulta = idUsuario;
                    }
                    else
                    {
                        IdUsuarioConsulta = IdUsuarioSession;
                    }
                }
                else
                {
                    IdUsuarioConsulta = IdUsuarioSession;
                }

                respuesta = NegTareas.ConsultaTareasGeneradasDescargar(fechaDesde, fechaHasta, 2, Id_RegTareas, IdUsuarioConsulta, busqueda);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Tareas");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Tareas" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");


        }


        public string ObtenerListaTareasGeneradasDescargarPorRevisarUnificado(dynamic parameters)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();

            string idUsuario = parameters["usuario"].ToString();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            int Id_RegTareas = Convert.ToInt32(parameters["registro"].ToString());
            int estado = Convert.ToInt32(parameters["estado"].ToString());
            string busqueda = parameters["busqueda"].ToString();

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            string session = parameters["session"].ToString();

            string IdUsuarioSession = "";
            bool usuarioEsJefe = false;
            string IdUsuarioConsulta = "";

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                usuarioEsJefe = NegUsuario.RTA_ConsultaUsuarioEsJefe(IdUsuarioSession);
                if (usuarioEsJefe)
                {
                    if (Convert.ToInt32(idUsuario) > 0)
                    {
                        IdUsuarioConsulta = idUsuario;
                    }
                    else
                    {
                        IdUsuarioConsulta = IdUsuarioSession;
                    }
                }
                else
                {
                    IdUsuarioConsulta = IdUsuarioSession;
                }

                respuesta = NegTareas.ConsultaTareasGeneradasDescargar(fechaDesde, fechaHasta, 2, Id_RegTareas, IdUsuarioConsulta, busqueda);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Tareas");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Tareas" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");


        }


        public string ObtenerListaTareasGeneradasPorRevisarUnificado(dynamic parameters)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();

            string idUsuario = parameters["usuario"].ToString();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            int Id_RegTareas = Convert.ToInt32(parameters["registro"].ToString());
            int estado = Convert.ToInt32(parameters["estado"].ToString());
            string busqueda = parameters["busqueda"].ToString();
            string session = parameters["session"].ToString();

            string IdUsuarioSession = "";
            bool usuarioEsJefe = false;
            string IdUsuarioConsulta = "";

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                usuarioEsJefe = NegUsuario.RTA_ConsultaUsuarioEsJefe(IdUsuarioSession);
                if (usuarioEsJefe)
                {
                    if (Convert.ToInt32(idUsuario) > 0)
                    {
                        IdUsuarioConsulta = idUsuario;
                    }
                    else
                    {
                        IdUsuarioConsulta = IdUsuarioSession;
                    }
                }
                else
                {
                    IdUsuarioConsulta = IdUsuarioSession;
                }


                listaTareas = NegTareas.ConsultaTareasGeneradas(fechaDesde, fechaHasta, 3, Id_RegTareas, IdUsuarioConsulta, busqueda);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaTareas.SerializaToJson();


        }

        public string ObtenerListaTareasGeneradasDescargar(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();

            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            int Id_RegTareas = Convert.ToInt32(parameters["registro"].ToString());
            int estado = Convert.ToInt32(parameters["estado"].ToString());
            string busqueda = parameters["busqueda"].ToString();

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            try
            {
                respuesta = NegTareas.ConsultaTareasGeneradasDescargar(fechaDesde, fechaHasta, estado, Id_RegTareas,"", busqueda);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Tareas");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Tareas" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");


        }


        public string ObtenerListaHorasContratadas(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();

            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            int Id_RegTareas = Convert.ToInt32(parameters["registro"].ToString());
            int estado = Convert.ToInt32(parameters["estado"].ToString());
            int idCliente = Convert.ToInt32(parameters["idCliente"].ToString());
            string busqueda = parameters["busqueda"].ToString();
            string os = parameters["os"].ToString();

            int idProceso = Convert.ToInt32(parameters["idProceso"].ToString());


            try
            {
                listaTareas = NegTareas.ConsultaHorasContratadas(fechaDesde, fechaHasta, estado, Id_RegTareas, busqueda,os, idCliente, idProceso);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaTareas.SerializaToJson();


        }

        public string ObtenerListaTareasGeneradasContrato(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();

            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            int Id_RegTareas = Convert.ToInt32(parameters["registro"].ToString());
            int estado = Convert.ToInt32(parameters["estado"].ToString());
            int idCliente = Convert.ToInt32(parameters["idCliente"].ToString());
            string busqueda = parameters["busqueda"].ToString();
            string os = parameters["os"].ToString();

            int idProceso = Convert.ToInt32(parameters["idProceso"].ToString());


            try
            {
                listaTareas = NegTareas.ConsultaTareasGeneradasContrato(fechaDesde, fechaHasta, estado, Id_RegTareas, busqueda, os, idCliente, idProceso);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaTareas.SerializaToJson();


        }
        public string ObtenerListaTareasGeneradasContratoDescargar(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();

            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            int Id_RegTareas = Convert.ToInt32(parameters["registro"].ToString());
            int estado = Convert.ToInt32(parameters["estado"].ToString());
            string busqueda = parameters["busqueda"].ToString();
            string os = parameters["os"].ToString();
            int idCliente = Convert.ToInt32(parameters["idCliente"].ToString());

            int idProceso = Convert.ToInt32(parameters["idProceso"].ToString());

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            try
            {
                respuesta = NegTareas.ConsultaTareasGeneradasContratoDescargar(fechaDesde, fechaHasta, estado, Id_RegTareas, busqueda,os, idCliente, idProceso);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Tareas");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Tareas" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");


        }
        public string ObtenerListaContratos(dynamic parameters)
        {
            List<EntContrato> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string busqueda = parameters["busqueda"].ToString();
            string sucursal = "";
            string estado = "";
            string area = "";
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());
            int IdGestorResponsable = Convert.ToInt32(parameters["IdGestorResponsable"].ToString());
            int IdClasificacion = Convert.ToInt32(parameters["IdClasificacion"].ToString());
            if (parameters["area"].ToString() == "SELECCIONAR AREA")
            {
                area = "";
            }
            else
            {
                area = parameters["area"].ToString();
            }
            if (parameters["sucursal"].ToString()== "SELECCIONAR SUCURSAL")
            {
                sucursal = "";
            }
            else
            {
                 sucursal = parameters["sucursal"].ToString();
            }

            if (parameters["estado"].ToString() == "SELECCIONAR ESTADO")
            {
                estado = "";
            }
            else
            {
                 estado = parameters["estado"].ToString();
            }

            try
            {
                Lista = NegContrato.ConsultaSp_RTAListaContratos(fechaDesde, fechaHasta, busqueda, IdCliente, IdGerenteCuenta,IdGestorResponsable, sucursal, estado, area, IdClasificacion);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObtenerListaPedidos(dynamic parameters)
        {
            List<EntPedido> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string busqueda = parameters["busqueda"].ToString();
            string sucursal = "";
            String meses = "";
            string estado = "";
            string area = "";
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());
            int IdGestorProducto = Convert.ToInt32(parameters["IdGestorProducto"].ToString());
            int IdClasificacion = Convert.ToInt32(parameters["IdClasificacion"].ToString());
            int idFecha = Convert.ToInt32(parameters["idFecha"].ToString());
            int IdRenovacion = Convert.ToInt32(parameters["IdRenovacion"].ToString());

            int Anio = Convert.ToInt32(parameters["Anio"].ToString());
            //int meses = Convert.ToInt32(parameters["meses"].ToString());

            if (parameters["meses"].ToString() == "-- SELECCIONE --" || parameters["meses"].ToString() == "null")
            {
                meses = "";
            }
            else
            {
                meses = parameters["meses"].ToString();
            }

            if (parameters["sucursal"].ToString() == "-- SELECCIONE --")
            {
                sucursal = "";
            }
            else
            {
                sucursal = parameters["sucursal"].ToString();
            }

            if (parameters["estado"].ToString() == "SELECCIONAR")
            {
                estado = "";
            }
            else
            {
                estado = parameters["estado"].ToString();
            }

            try
            {
                Lista = NegPedido.ConsultaSp_RTAListaPedido(fechaDesde, fechaHasta, busqueda, IdCliente, IdGerenteCuenta, IdGestorProducto, sucursal, estado, IdClasificacion, Anio, meses, idFecha, IdRenovacion);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObtenerListaSolicitud(dynamic parameters)
        {
            List<EntSolicitud> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());

            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string busqueda = parameters["busqueda"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());
            int pagina = Convert.ToInt32(parameters["pagina"].ToString());
            string estadosolicitud = parameters["estadosolicitud"].ToString();
            string usuario = parameters["usuario"].ToString();
            try
            {
                Lista = NegSolicitud.ConsultaSp_RTAListaSolicitud(tipo, IdUsuarioSession, pagina, estadosolicitud, fechaDesde, fechaHasta, Convert.ToInt32(busqueda), usuario);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObtenerListaSolicitudAct(dynamic parameters)
        {
            List<EntSolicitud> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());

            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string busqueda = parameters["busqueda"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());
            int pagina = Convert.ToInt32(parameters["pagina"].ToString());
            string estadosolicitud = parameters["estadosolicitud"].ToString();

            try
            {
                respuesta = NegSolicitud.ConsultaSp_RTAListaSolicitudAct(tipo, IdUsuarioSession, pagina, estadosolicitud, fechaDesde, fechaHasta, Convert.ToInt32(busqueda));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return respuesta.SerializaToJson();
        }


        public string ObtenerListaSaldoVacacionesIndividual(dynamic parameters)
        {
            List<EntVacaciones> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = session;//seguridad.Desencripta(session.ToString());
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());
            try
            {
                Lista = NegVacaciones.ConsultarSaldoVacaciones(IdUsuarioSession, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObtenerListaSaldoVacaciones(dynamic parameters)
        {
            List<EntVacaciones> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());
            try
            {
                Lista = NegVacaciones.ConsultarSaldoVacaciones(IdUsuarioSession, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObtenerListadeCorreos(dynamic parameters)
        {
            List<EntUsuario> Lista = null;
            string Correos = "";
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Correos = NegUsuario.RTACorreoUsuarioPlanificacionVac(IdUsuarioSession);
                if (Correos == "")
                {

                }
                else
                {
                    List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
                    EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                    bool respuestaEnvioCorreo = false;
                    if (tipo == 0)
                    {
                        listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "PLANIFICACION DE VACACIONES" });
                        listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Estimados. " });
                        listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = "<td valign='top' style='padding:15pt 0 15pt 15pt;'><div><p style='font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>Colaboradores, Solicitamos que ingresen al por Sistema de Control de Gestión Interno para realizar la planificación de sus vacaciones anuales,</span></strong><strong><span style='font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'></span></strong></p><p style='font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>Procedimiento:</span></strong></p><ul type='disc' style='margin-top:0;margin-bottom:0;'><li style='color:#383838;font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='color:windowtext;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>D</span></strong><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>ar clic en la opción Solicitar Vacaciones y Permisos,</span></strong><strong><span style='font-size:10.5pt;font-family:Arial,sans-serif;font-weight:normal;line-height:150%;'></span></strong></li><li style='color:#383838;font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='color:windowtext;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>E</span></strong><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>n Tipo </span></strong><strong><span style='color:windowtext;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>de </span></strong><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>Solicitud seleccionar Planificación de vacaciones y llenar los datos correspondientes a las fechas en las que se planean tomar sus vacaciones durante el </span></strong><strong><span style='color:windowtext;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>año</span></strong><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'> 2023. </span></strong><strong><span style='font-size:10.5pt;font-family:Arial,sans-serif;font-weight:normal;line-height:150%;'></span></strong></li><li style='font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>Nota: la planificación deberá cumplirse en coordinación con su jefe inmediato</span></strong><span style='color:#383838;font-size:10.5pt;font-family:Arial,sans-serif;line-height:150%;'> </span></li></ul></div></td>" });

                        respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(Correos, "Planificación de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoPlanificacionVacaciones.txt"), listaCamposCorreo, "contenidoCorreoPlanificacionVacaciones.txt");
                    }
                    else if (tipo == 1)
                    {
                        listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "CARGAR ARCHIVO DE PERMISO" });
                        listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Estimados. " });
                        listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = "<td valign='top' style='padding:15pt 0 15pt 15pt;'><div><p style='font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>Colaboradores, Solicitamos que ingresen al por Sistema de Control de Gestión Interno para realizar la planificación de sus vacaciones anuales,</span></strong><strong><span style='font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'></span></strong></p><p style='font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>Procedimiento:</span></strong></p><ul type='disc' style='margin-top:0;margin-bottom:0;'><li style='color:#383838;font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='color:windowtext;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>D</span></strong><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>ar clic en la opción Solicitar Vacaciones y Permisos,</span></strong><strong><span style='font-size:10.5pt;font-family:Arial,sans-serif;font-weight:normal;line-height:150%;'></span></strong></li><li style='color:#383838;font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='color:windowtext;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>E</span></strong><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>n Tipo </span></strong><strong><span style='color:windowtext;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>de </span></strong><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>Solicitud seleccionar Planificación de vacaciones y llenar los datos correspondientes a las fechas en las que se planean tomar sus vacaciones durante el </span></strong><strong><span style='color:windowtext;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>año</span></strong><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'> 2023. </span></strong><strong><span style='font-size:10.5pt;font-family:Arial,sans-serif;font-weight:normal;line-height:150%;'></span></strong></li><li style='font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>Nota: la planificación deberá cumplirse en coordinación con su jefe inmediato</span></strong><span style='color:#383838;font-size:10.5pt;font-family:Arial,sans-serif;line-height:150%;'> </span></li></ul></div></td>" });

                        respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(Correos, "Planificación de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoPlanificacionVacaciones.txt"), listaCamposCorreo, "contenidoCorreoPlanificacionVacaciones.txt");
                    }
                    else if (tipo == 2)
                    {
                        listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "POR FAVOR APROBAR LAS HORAS" });
                        listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Estimados. " });
                        listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = "<td valign='top' style='padding:15pt 0 15pt 15pt;'><div><p style='font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>Colaboradores, Solicitamos que ingresen al por Sistema de Control de Gestión Interno para realizar la planificación de sus vacaciones anuales,</span></strong><strong><span style='font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'></span></strong></p><p style='font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>Procedimiento:</span></strong></p><ul type='disc' style='margin-top:0;margin-bottom:0;'><li style='color:#383838;font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='color:windowtext;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>D</span></strong><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>ar clic en la opción Solicitar Vacaciones y Permisos,</span></strong><strong><span style='font-size:10.5pt;font-family:Arial,sans-serif;font-weight:normal;line-height:150%;'></span></strong></li><li style='color:#383838;font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='color:windowtext;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>E</span></strong><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>n Tipo </span></strong><strong><span style='color:windowtext;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>de </span></strong><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>Solicitud seleccionar Planificación de vacaciones y llenar los datos correspondientes a las fechas en las que se planean tomar sus vacaciones durante el </span></strong><strong><span style='color:windowtext;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>año</span></strong><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'> 2023. </span></strong><strong><span style='font-size:10.5pt;font-family:Arial,sans-serif;font-weight:normal;line-height:150%;'></span></strong></li><li style='font-size:11pt;font-family:Calibri,sans-serif;margin:0;line-height:150%;'><strong><span style='color:#1E1E1E;font-size:10.5pt;font-family:Helvetica,sans-serif;line-height:150%;'>Nota: la planificación deberá cumplirse en coordinación con su jefe inmediato</span></strong><span style='color:#383838;font-size:10.5pt;font-family:Arial,sans-serif;line-height:150%;'> </span></li></ul></div></td>" });

                        respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(Correos, "Planificación de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoPlanificacionVacaciones.txt"), listaCamposCorreo, "contenidoCorreoPlanificacionVacaciones.txt");
                    }

                    if (respuestaEnvioCorreo==true)
                    {
                        respuesta.estado = "1";
                        respuesta.mensaje = "E-Mail enviado correctamente.";
                    }

                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return respuesta.SerializaToJson();
        }

        public string ObtenerDatosEmpleado(dynamic parameters)
        {
            List<EntUsuario> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegUsuario.ConsultarDatosReemplazo(IdUsuarioSession, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObtenerListaGastosCuentasDescargar(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string sucursal = parameters["sucursal"].ToString();
            string area = parameters["area"].ToString();
            if (sucursal == "0")
            {
                sucursal = "";
            }

            if (area == "0")
            {
                area = "";
            }

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            try
            {
                respuesta = NegCuentaContable.ConsultaSp_RTAListaCuentasContablesDescargar(fechaDesde, fechaHasta, sucursal, area);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Gastos Cuentas");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte Gastos Cuentas" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }

        public string ObtenerListaRebatesDescargar(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string estado = "";
            int IdTipoIngreso = Convert.ToInt32(parameters["IdTipoIngreso"].ToString());
            int IdPago = Convert.ToInt32(parameters["IdPago"].ToString());
            int IdMarca = Convert.ToInt32(parameters["IdMarca"].ToString());

            int idFecha = Convert.ToInt32(parameters["idFecha"].ToString());

            string Anio = "";
            if (parameters["Anio"].ToString() == "null" || parameters["Anio"].ToString() == "")
            {
                Anio = "";
            }
            else
            {
                Anio = parameters["Anio"].ToString();
            }

            string meses = parameters["meses"].ToString();

            if (parameters["estado"].ToString() == "SELECCIONAR")
            {
                estado = "";
            }
            else
            {
                estado = parameters["estado"].ToString();
            }

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            try
            {
                respuesta = NegRebates.ConsultaSp_RTAListaRebetesDescargar(fechaDesde, fechaHasta, IdTipoIngreso, IdMarca, IdPago, estado, Anio, meses, idFecha);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Rebates");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Rebates" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }

        public string ObtenerListaRebates(dynamic parameters)
        {
            List<EntRebates> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string estado = "";
            int IdTipoIngreso = Convert.ToInt32(parameters["IdTipoIngreso"].ToString());
            int IdPago = Convert.ToInt32(parameters["IdPago"].ToString());
            int IdMarca = Convert.ToInt32(parameters["IdMarca"].ToString());

            int idFecha = Convert.ToInt32(parameters["idFecha"].ToString());

            string  Anio ="";
            if(parameters["Anio"].ToString() == "null" || parameters["Anio"].ToString() == "")
            {
                Anio = "";
            }
            else
            {
                Anio= parameters["Anio"].ToString();
            }
            string meses = parameters["meses"].ToString();

            if (parameters["estado"].ToString() == "SELECCIONAR")
            {
                estado = "";
            }
            else
            {
                estado = parameters["estado"].ToString();
            }

            try
            {
                Lista = NegRebates.ConsultaSp_RTAListaRebetes(fechaDesde, fechaHasta, IdTipoIngreso, IdMarca, IdPago, estado, Anio, meses, idFecha);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarContratos(dynamic parameters)
        {
            List<EntContrato> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int idServicio = Convert.ToInt32(parameters["idServicio"].ToString());
            string orden = parameters["orden"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegContrato.ConsultaSp_RTAConsultarContratos(idServicio, orden, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarPedidos(dynamic parameters)
        {
            List<EntPedido> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int idPedidos = Convert.ToInt32(parameters["idPedidos"].ToString());
            string orden = parameters["orden"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegPedido.ConsultaSp_RTAConsultarPedido(idPedidos, orden, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarPolizaFiltro(dynamic parameters)
        {
            List<EntPoliza> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int idPedidos = Convert.ToInt32(parameters["idPedidos"].ToString());
            string buscar = parameters["buscar"].ToString();
            string fechaInicio = parameters["fechaInicio"].ToString();
            string fechaFinal = parameters["fechaFinal"].ToString();
            //int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegPoliza.ConsultaSp_RTAListaPoliza(buscar, fechaInicio, fechaFinal, idPedidos);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarPolizasFiltro(dynamic parameters)
        {
            List<EntPoliza> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int idPedidos = Convert.ToInt32(parameters["idPedidos"].ToString());
            string buscar = parameters["buscar"].ToString();
            string fechaInicio = parameters["fechaInicio"].ToString();
            string fechaFinal = parameters["fechaFinal"].ToString();
            string beneficiario = parameters["beneficiario"].ToString();
            string Proceso = parameters["Proceso"].ToString();
            int idFecha = Convert.ToInt32(parameters["idFecha"].ToString());

            try
            {
                Lista = NegPoliza.ConsultaSp_RTAListaPolizas(buscar, fechaInicio, fechaFinal, idPedidos, beneficiario, Proceso, idFecha);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarReporteVentasDescargar(dynamic parameters)
        {
            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            List<EntForeCast> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());

            //int idForeCast = Convert.ToInt32(parameters["idForeCast"].ToString());
            //string FechaFacturacion = parameters["FechaFacturacion"].ToString();
            //string MesEstimadoCierre = parameters["MesEstimadoCierre"].ToString();
            //int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            //int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());
            //string IdGestorProducto = "";
            ////int IdMarca = Convert.ToInt32(parameters["IdMarca"].ToString());
            //string StrIdPrioridad = parameters["IdPrioridad"].ToString();
            //MesEstimadoCierre = parameters["mespago"].ToString();

            //int idFecha = Convert.ToInt32(parameters["tipofecha"].ToString());
            int Anio = Convert.ToInt32(parameters["anio"].ToString());

            
            try
            {
                respuesta = NegReporteGe.ConsultaSp_RTAListaForeCastDescargar(Anio,1);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Venta");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Ventas" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }


        public string ObteneConsultarForeCastFiltroDescargar(dynamic parameters)
        {
            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            List<EntForeCast> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());

            int idForeCast = Convert.ToInt32(parameters["idForeCast"].ToString());
            string FechaFacturacion = parameters["FechaFacturacion"].ToString();
            string MesEstimadoCierre = parameters["MesEstimadoCierre"].ToString();
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());
            int IdPrioProyecto = Convert.ToInt32(parameters["IdPrioProyecto"].ToString());
            int ProyecEstrategico = Convert.ToInt32(parameters["ProyecEstrategico"].ToString());
            string IdGestorProducto = "";
            //int IdMarca = Convert.ToInt32(parameters["IdMarca"].ToString());
            string StrIdPrioridad = parameters["IdPrioridad"].ToString();
            MesEstimadoCierre = parameters["mespago"].ToString();

            int idFecha = Convert.ToInt32(parameters["tipofecha"].ToString());
            int Anio = Convert.ToInt32(parameters["anio"].ToString());

            if (parameters["IdGestorProducto"].ToString() == "null")
            {
                IdGestorProducto = "";
            }
            else
            {
                IdGestorProducto = parameters["IdGestorProducto"].ToString();
            }

            string Marca = "";
            if (parameters["IdMarca"].ToString() == "SELECCIONAR" || parameters["IdMarca"].ToString() == "null")
            {
                Marca = "";
            }
            else
            {
                Marca = parameters["IdMarca"].ToString();
            }

            string sucursal = "";
            if (parameters["sucursal"].ToString() == "SELECCIONAR SUCURSAL" || parameters["sucursal"].ToString() == "null")
            {
                sucursal = "";
            }
            else
            {
                sucursal = parameters["sucursal"].ToString();
            }

            string cierrenegocio = "";

            if (parameters["cierrenegocio"].ToString() == "null")
            {
                cierrenegocio = "";
            }
            else
            {
                cierrenegocio = parameters["cierrenegocio"].ToString();
            }

            string SegmentodeMercado = parameters["SegmentodeMercado"].ToString();

            string TipoProyecto = parameters["TipoProyecto"].ToString();

            try
            {
                respuesta = NegForeCast.ConsultaSp_RTAListaForeCastDescargar(FechaFacturacion, MesEstimadoCierre, IdCliente, IdGerenteCuenta, IdGestorProducto, Marca, sucursal, SegmentodeMercado, StrIdPrioridad, IdUsuarioSession, idFecha, Anio, cierrenegocio, IdPrioProyecto, ProyecEstrategico,TipoProyecto);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Forecast");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Forecast" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }

        public string ObteneConsultarForeCastFiltroDescargarGerencia(dynamic parameters)
        {
            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            List<EntForeCast> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());

            int idForeCast = Convert.ToInt32(parameters["idForeCast"].ToString());
            string FechaFacturacion = parameters["FechaFacturacion"].ToString();
            string MesEstimadoCierre = parameters["MesEstimadoCierre"].ToString();
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());
            int IdPrioProyecto = Convert.ToInt32(parameters["IdPrioProyecto"].ToString());
            int ProyecEstrategico = Convert.ToInt32(parameters["ProyecEstrategico"].ToString());
            string IdGestorProducto = "";
            //int IdMarca = Convert.ToInt32(parameters["IdMarca"].ToString());
            string StrIdPrioridad = parameters["IdPrioridad"].ToString();
            MesEstimadoCierre = parameters["mespago"].ToString();

            int idFecha = Convert.ToInt32(parameters["tipofecha"].ToString());
            int Anio = Convert.ToInt32(parameters["anio"].ToString());

            if (parameters["IdGestorProducto"].ToString() == "null")
            {
                IdGestorProducto = "";
            }
            else
            {
                IdGestorProducto = parameters["IdGestorProducto"].ToString();
            }

            string Marca = "";
            if (parameters["IdMarca"].ToString() == "SELECCIONAR" || parameters["IdMarca"].ToString() == "null")
            {
                Marca = "";
            }
            else
            {
                Marca = parameters["IdMarca"].ToString();
            }

            string sucursal = "";
            if (parameters["sucursal"].ToString() == "SELECCIONAR SUCURSAL" || parameters["sucursal"].ToString() == "null")
            {
                sucursal = "";
            }
            else
            {
                sucursal = parameters["sucursal"].ToString();
            }

            string cierrenegocio = "";

            if (parameters["cierrenegocio"].ToString() == "null")
            {
                cierrenegocio = "";
            }
            else
            {
                cierrenegocio = parameters["cierrenegocio"].ToString();
            }

            string SegmentodeMercado = parameters["SegmentodeMercado"].ToString();

            try
            {
                respuesta = NegForeCast.ConsultaSp_RTAListaForeCastDescargarGerencia(FechaFacturacion, MesEstimadoCierre, IdCliente, IdGerenteCuenta, IdGestorProducto, Marca, sucursal, SegmentodeMercado, StrIdPrioridad, IdUsuarioSession, idFecha, Anio, cierrenegocio, IdPrioProyecto, ProyecEstrategico);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Gerencia");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Gerencia" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }



        public string ObteneConsultarForeCastFiltroDescargarPersonalizado(dynamic parameters)
        {
            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            List<EntForeCast> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());

            int idForeCast = Convert.ToInt32(parameters["idForeCast"].ToString());
            string FechaFacturacion = parameters["FechaFacturacion"].ToString();
            string MesEstimadoCierre = parameters["MesEstimadoCierre"].ToString();
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());
            int IdPrioProyecto = Convert.ToInt32(parameters["IdPrioProyecto"].ToString());
            int ProyecEstrategico = Convert.ToInt32(parameters["ProyecEstrategico"].ToString());
            string IdGestorProducto = "";
            //int IdMarca = Convert.ToInt32(parameters["IdMarca"].ToString());
            string StrIdPrioridad = parameters["IdPrioridad"].ToString();
            MesEstimadoCierre = parameters["mespago"].ToString();

            int idFecha = Convert.ToInt32(parameters["tipofecha"].ToString());
            int Anio = Convert.ToInt32(parameters["anio"].ToString());

            if (parameters["IdGestorProducto"].ToString() == "null")
            {
                IdGestorProducto = "";
            }
            else
            {
                IdGestorProducto = parameters["IdGestorProducto"].ToString();
            }

            string Marca = "";
            if (parameters["IdMarca"].ToString() == "SELECCIONAR" || parameters["IdMarca"].ToString() == "null")
            {
                Marca = "";
            }
            else
            {
                Marca = parameters["IdMarca"].ToString();
            }

            string sucursal = "";
            if (parameters["sucursal"].ToString() == "SELECCIONAR SUCURSAL" || parameters["sucursal"].ToString() == "null")
            {
                sucursal = "";
            }
            else
            {
                sucursal = parameters["sucursal"].ToString();
            }

            string cierrenegocio = "";

            if (parameters["cierrenegocio"].ToString() == "null")
            {
                cierrenegocio = "";
            }
            else
            {
                cierrenegocio = parameters["cierrenegocio"].ToString();
            }

            string SegmentodeMercado = parameters["SegmentodeMercado"].ToString();

            try
            {
                respuesta = NegForeCast.ConsultaSp_RTAListaForeCastDescargarPersonalizado(FechaFacturacion, MesEstimadoCierre, IdCliente, IdGerenteCuenta, IdGestorProducto, Marca, sucursal, SegmentodeMercado, StrIdPrioridad, IdUsuarioSession, idFecha, Anio, cierrenegocio, IdPrioProyecto,ProyecEstrategico);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Forecast");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Forecast" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }


        public string ObteneConsultarForeCastGDFiltroDescargar(dynamic parameters)
        {
            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            List<EntForeCast> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());

            int idForeCast = Convert.ToInt32(parameters["idForeCast"].ToString());
            string FechaFacturacion = parameters["FechaFacturacion"].ToString();
            string MesEstimadoCierre = parameters["MesEstimadoCierre"].ToString();
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());
            int IdGestorProducto = Convert.ToInt32(parameters["IdGestorProducto"].ToString());
            //int IdMarca = Convert.ToInt32(parameters["IdMarca"].ToString());
            string StrIdPrioridad = parameters["IdPrioridad"].ToString();
            MesEstimadoCierre = parameters["mespago"].ToString();

            int idFecha = Convert.ToInt32(parameters["tipofecha"].ToString());
            int Anio = Convert.ToInt32(parameters["anio"].ToString());

            string Marca = "";
            if (parameters["IdMarca"].ToString() == "SELECCIONAR" || parameters["IdMarca"].ToString() == "null")
            {
                Marca = "";
            }
            else
            {
                Marca = parameters["IdMarca"].ToString();
            }

            string sucursal = "";
            if (parameters["sucursal"].ToString() == "SELECCIONAR SUCURSAL" || parameters["sucursal"].ToString() == "null")
            {
                sucursal = "";
            }
            else
            {
                sucursal = parameters["sucursal"].ToString();
            }

            string cierrenegocio = "";

            if (parameters["cierrenegocio"].ToString() == "null")
            {
                cierrenegocio = "";
            }
            else
            {
                cierrenegocio = parameters["cierrenegocio"].ToString();
            }

            string SegmentodeMercado = parameters["SegmentodeMercado"].ToString();

            try
            {
                respuesta = NegForeCast.ConsultaSp_RTAListaForeCastGDDescargar(FechaFacturacion, MesEstimadoCierre, IdCliente, IdGerenteCuenta, IdGestorProducto, Marca, sucursal, SegmentodeMercado, StrIdPrioridad, IdUsuarioSession, idFecha, Anio, cierrenegocio);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Forecast");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Forecast" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }


        public string ObteneReporteConsultarCosteoFiltrosDescargar(dynamic parameters)
        {
            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            List<EntCosteoServicio> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());

            string FchIni = parameters["FchIni"].ToString();
            string FchFin = parameters["FchFin"].ToString();
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());
            string sucursal = "";
            if (parameters["sucursal"].ToString() == "SELECCIONAR SUCURSAL" || parameters["sucursal"].ToString() == "null")
            {
                sucursal = "";
            }
            else
            {
                sucursal = parameters["sucursal"].ToString();
            }
            string ResponsableDimen = parameters["ResponsableDimen"].ToString();
            int idFecha = Convert.ToInt32(parameters["tipofecha"].ToString());

            try
            {
                respuesta = NegCosteoServicio.ConsultaSp_RTAListaCosteServiciosDescargar(FchIni, FchFin, IdGerenteCuenta, sucursal, ResponsableDimen, idFecha);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Costeo");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Costeo" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }

        public string ObteneConsultarPolizasFiltroDescargar(dynamic parameters)
        {
            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            List<EntPoliza> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int idPedidos = Convert.ToInt32(parameters["idPedidos"].ToString());
            string buscar = parameters["buscar"].ToString();
            string fechaInicio = parameters["fechaInicio"].ToString();
            string fechaFinal = parameters["fechaFinal"].ToString();
            string beneficiario = parameters["beneficiario"].ToString();
            string Proceso = parameters["Proceso"].ToString();
            int idFecha = Convert.ToInt32(parameters["idFecha"].ToString());

            try
            {
                respuesta = NegPoliza.ConsultaSp_RTAListaPolizasDescargar(buscar, fechaInicio, fechaFinal, idPedidos, beneficiario, Proceso, idFecha);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Polizas");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Polizas" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }

        public string ConsultarMensajeForeCast(dynamic parameters)
        {
            List<EntMensajeForeCast> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int idForeCast = Convert.ToInt32(parameters["idForeCast"].ToString());
            try
            {
                Lista = NegMensajeForeCast.ConsultaSp_RTAMostrarMensajeForeCast(idForeCast);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }
        public string ObteneConsultarReporteVentas(dynamic parameters)
        {
            List<EntReporteGe> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());
            int Anio = Convert.ToInt32(parameters["anio"].ToString());
            try
            {
                Lista = NegReporteGe.ConsultaSp_RTAConsultarForeCast(Anio,0);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarForeCastFiltro(dynamic parameters)
        {
            List<EntForeCast> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            string IdGestorProducto = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());

            int idForeCast = Convert.ToInt32(parameters["idForeCast"].ToString());
            string FechaFacturacion = parameters["FechaFacturacion"].ToString();
            string MesEstimadoCierre = parameters["MesEstimadoCierre"].ToString();
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());

            int IdPrioProyecto = Convert.ToInt32(parameters["IdPrioProyecto"].ToString());

            int ProyecEstrategico = Convert.ToInt32(parameters["ProyecEstrategico"].ToString());


            if (parameters["IdGestorProducto"].ToString() == "null")
            {
                IdGestorProducto = "";
            }
            else
            {
                IdGestorProducto = parameters["IdGestorProducto"].ToString();
            }
            //int IdMarca = Convert.ToInt32(parameters["IdMarca"].ToString());
            string StrIdPrioridad = parameters["IdPrioridad"].ToString();
            MesEstimadoCierre = parameters["mespago"].ToString();

            int idFecha = Convert.ToInt32(parameters["tipofecha"].ToString());
            int Anio = Convert.ToInt32(parameters["anio"].ToString());

            string Marca = "";
            if (parameters["IdMarca"].ToString() == "SELECCIONAR" || parameters["IdMarca"].ToString() == "null")
            {
                Marca = "";
            }
            else
            {
                Marca = parameters["IdMarca"].ToString();
            }

            string sucursal = "";
            if (parameters["sucursal"].ToString() == "SELECCIONAR SUCURSAL" || parameters["sucursal"].ToString() == "null")
            {
                 sucursal = "";
            }
            else
            {
                 sucursal = parameters["sucursal"].ToString();
            }

            string cierrenegocio = "";

            if (parameters["cierrenegocio"].ToString() == "null")
            {
                cierrenegocio = "";
            }
            else
            {
                cierrenegocio = parameters["cierrenegocio"].ToString();
            }

            string SegmentodeMercado = parameters["SegmentodeMercado"].ToString();

            string TipoProyecto = parameters["TipoProyecto"].ToString();

            try
            {
                Lista = NegForeCast.ConsultaSp_RTAListaForeCast(FechaFacturacion, MesEstimadoCierre, IdCliente, IdGerenteCuenta, IdGestorProducto, Marca, sucursal, SegmentodeMercado, StrIdPrioridad, IdUsuarioSession, idFecha, Anio, cierrenegocio,IdPrioProyecto,ProyecEstrategico, TipoProyecto);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarForeCastGDFiltro(dynamic parameters)
        {
            List<EntForeCast> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());

            int idForeCast = Convert.ToInt32(parameters["idForeCast"].ToString());
            string FechaFacturacion = parameters["FechaFacturacion"].ToString();
            string MesEstimadoCierre = parameters["MesEstimadoCierre"].ToString();
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());
            int IdGestorProducto = Convert.ToInt32(parameters["IdGestorProducto"].ToString());
            //int IdMarca = Convert.ToInt32(parameters["IdMarca"].ToString());
            string StrIdPrioridad = parameters["IdPrioridad"].ToString();
            MesEstimadoCierre = parameters["mespago"].ToString();

            int idFecha = Convert.ToInt32(parameters["tipofecha"].ToString());
            int Anio = Convert.ToInt32(parameters["anio"].ToString());

            string Marca = "";
            if (parameters["IdMarca"].ToString() == "SELECCIONAR" || parameters["IdMarca"].ToString() == "null")
            {
                Marca = "";
            }
            else
            {
                Marca = parameters["IdMarca"].ToString();
            }

            string sucursal = "";
            if (parameters["sucursal"].ToString() == "SELECCIONAR SUCURSAL" || parameters["sucursal"].ToString() == "null")
            {
                sucursal = "";
            }
            else
            {
                sucursal = parameters["sucursal"].ToString();
            }

            string cierrenegocio = "";

            if (parameters["cierrenegocio"].ToString() == "null")
            {
                cierrenegocio = "";
            }
            else
            {
                cierrenegocio = parameters["cierrenegocio"].ToString();
            }

            string SegmentodeMercado = parameters["SegmentodeMercado"].ToString();

            try
            {
                Lista = NegForeCast.ConsultaSp_RTAListaForeCastGD(FechaFacturacion, MesEstimadoCierre, IdCliente, IdGerenteCuenta, IdGestorProducto, Marca, sucursal, SegmentodeMercado, StrIdPrioridad, IdUsuarioSession, idFecha, Anio, cierrenegocio);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneReporteConsultarCosteoFiltros(dynamic parameters)
        {
            List<EntCosteoServicio> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());


            string FchIni = parameters["FchIni"].ToString();
            string FchFin = parameters["FchFin"].ToString();
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());
            string sucursal = "";
            if (parameters["sucursal"].ToString() == "SELECCIONAR SUCURSAL" || parameters["sucursal"].ToString() == "null")
            {
                sucursal = "";
            }
            else
            {
                sucursal = parameters["sucursal"].ToString();
            }
            string ResponsableDimen = parameters["ResponsableDimen"].ToString();
            int idFecha = Convert.ToInt32(parameters["tipofecha"].ToString());

            try
            {
                Lista = NegCosteoServicio.ConsultaSp_RTAListaCosteServicios(FchIni, FchFin, IdGerenteCuenta, sucursal, ResponsableDimen, idFecha);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarForeCast(dynamic parameters)
        {
            List<EntForeCast> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int idForeCast = Convert.ToInt32(parameters["idForeCast"].ToString());
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegForeCast.ConsultaSp_RTAConsultarForeCast(idForeCast, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarForeCastGD(dynamic parameters)
        {
            List<EntForeCast> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int idForeCast = Convert.ToInt32(parameters["idForeCast"].ToString());
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegForeCast.ConsultaSp_RTAConsultarForeCastGD(idForeCast, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarCosteo(dynamic parameters)
        {
            List<EntCosteoServicio> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int idCosteo = Convert.ToInt32(parameters["idCosteo"].ToString());
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegCosteoServicio.ConsultaSp_RTAConsultarCosteServicios(idCosteo, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObtenerValidacionForeCast(dynamic parameters)
        {
            List<EntForeCast> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            string Descripcion = parameters["Descripcion"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegForeCast.ConsultaSp_RTAValidarClienteForeCast(Descripcion, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }
        public string ObteneConsultarPoliza(dynamic parameters)
        {
            List<EntPoliza> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int idPedidos = Convert.ToInt32(parameters["idPedidos"].ToString());
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegPoliza.ConsultaSp_RTAConsultarPoliza(idPedidos, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarPeriodoFiscal(dynamic parameters)
        {
            List<EntRebates> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int IdMarca = Convert.ToInt32(parameters["IdMarca"].ToString());
            int IdAnioFiscal = Convert.ToInt32(parameters["IdAnioFiscal"].ToString());
            string fechaDesde = parameters["fechaInicio"].ToString();
            string fechaHasta = parameters["fechaFinal"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegRebates.ConsultaSp_RTAConsultarPeriodoFiscal(IdMarca, fechaDesde, fechaHasta, IdAnioFiscal, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarRebetes(dynamic parameters)
        {
            List<EntRebates> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int IdRebates = Convert.ToInt32(parameters["IdRebates"].ToString());
            string orden = parameters["orden"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegRebates.ConsultaSp_RTAConsultarRebetes(IdRebates, orden, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string CargarRequerimientos(dynamic parameters)
        {
            List<EntMantenimiento> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string busqueda = parameters["os"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());
            int IdRequerimiento = Convert.ToInt32(parameters["IdRequerimiento"].ToString());
            try
            {
                Lista = NegMantenimiento.ConsultSp_RTAConsultaMantenimientoContrato(fechaDesde, fechaHasta, busqueda, tipo, IdRequerimiento);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string BuscarListaClientes(dynamic parameters)
        {
            List<EntUsuario> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            string descripcion = parameters["descripcion"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegUsuario.RTA_ConsultaLike(tipo, descripcion);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string BuscarListaInventario(dynamic parameters)
        {
            List<EntInventario> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            string descripcion = parameters["descripcion"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegInventario .RTA_ConsultaInventarioLike(tipo, descripcion);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }


        public string BuscarListaInventarioDescargar(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            string descripcion = parameters["descripcion"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            try
            {
                respuesta = NegInventario.RTA_ConsultaInventarioLikeDescargar(tipo, descripcion);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Inventario");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Inventario" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }

        public string BuscarListaInventarioFinalDescargar(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            string descripcion = parameters["descripcion"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            try
            {
                respuesta = NegInventario.RTA_ConsultaInventarioLikeDescargar(tipo, descripcion);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Inventario");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Inventario" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }

        public string BuscarClientePedidos(dynamic parameters)
        {
            List<EntEgresoInventario> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            string descripcion = parameters["descripcion"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegEgresoInventario.RTA_ConsultaClientePedidoLike(tipo, descripcion);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string MostrarListaEgresos(dynamic parameters)
        {
            List<EntEgresoInventario> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            string fechaDesde = parameters["fecha1"].ToString();
            string fechaHasta = parameters["fecha2"].ToString();
            try
            {
                Lista = NegEgresoInventario.ConsultaRTAListaEgresos(IdCliente, fechaDesde, fechaHasta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }

        public string ObteneConsultarEgresosDescargar(dynamic parameters)
        {
            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            List<EntPoliza> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            string fechaDesde = parameters["fecha1"].ToString();
            string fechaHasta = parameters["fecha2"].ToString();

            try
            {
                respuesta = NegEgresoInventario.ConsultaRTAListaEgresosDescargar(IdCliente, fechaDesde, fechaHasta);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Egreso Inventario");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Egreso Inventario" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }
        public string ListaRequerimientos(dynamic parameters)
        {
            List<EntMantenimiento> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string busqueda = parameters["os"].ToString();
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            string Clasificacion = parameters["Clasificacion"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                Lista = NegMantenimiento.ConsultSp_RTAListaMantenimientoContrato(fechaDesde, fechaHasta, busqueda, IdCliente, Clasificacion, tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }
        public string ListaRequerimientosDescargar(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string busqueda = parameters["os"].ToString();
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            string Clasificacion = parameters["Clasificacion"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            try
            {
                respuesta = NegMantenimiento.ConsultSp_RTAListaMantenimientoContratoDescargar(fechaDesde, fechaHasta, busqueda, IdCliente, Clasificacion, tipo);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Mantenimiento");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Mantenimiento" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }
        public string ObtenerListaContratosDescargar(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string busqueda = parameters["busqueda"].ToString();
            string sucursal = "";
            string estado = "";
            string area = "";
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());
            int IdGestorResponsable = Convert.ToInt32(parameters["IdGestorResponsable"].ToString());
            if (parameters["area"].ToString() == "SELECCIONAR AREA")
            {
                area = "";
            }
            else
            {
                area = parameters["area"].ToString();
            }

            if (parameters["sucursal"].ToString() == "SELECCIONAR SUCURSAL")
            {
                sucursal = "";
            }
            else
            {
                sucursal = parameters["sucursal"].ToString();
            }

            if (parameters["estado"].ToString() == "SELECCIONAR ESTADO")
            {
                estado = "";
            }
            else
            {
                estado = parameters["estado"].ToString();
            }

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            try
            {
                respuesta = NegContrato.ConsultaSp_RTAListaContratosDescargar(fechaDesde, fechaHasta, busqueda, IdCliente, IdGerenteCuenta, IdGestorResponsable, sucursal, estado, area);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Contrato");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Contrato" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }

        public string ObtenerListaPedidosDescargar(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string busqueda = parameters["busqueda"].ToString();
            string sucursal = "";
            string estado = "";
            string area = "";
            string meses = "";
            int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());
            int IdGerenteCuenta = Convert.ToInt32(parameters["IdGerenteCuenta"].ToString());
            int IdGestorProducto = Convert.ToInt32(parameters["IdGestorProducto"].ToString());
            int IdClasificacion = Convert.ToInt32(parameters["IdClasificacion"].ToString());
            int idFecha = Convert.ToInt32(parameters["idFecha"].ToString());
            int IdRenovacion = Convert.ToInt32(parameters["IdRenovacion"].ToString());
            int Anio = Convert.ToInt32(parameters["Anio"].ToString());

            if (parameters["meses"].ToString() == "-- SELECCIONE --" || parameters["meses"].ToString() == "null")
            {
                meses = "";
            }
            else
            {
                meses = parameters["meses"].ToString();
            }

            if (parameters["sucursal"].ToString() == "-- SELECCIONE --")
            {
                sucursal = "";
            }
            else
            {
                sucursal = parameters["sucursal"].ToString();
            }

            if (parameters["estado"].ToString() == "SELECCIONAR")
            {
                estado = "";
            }
            else
            {
                estado = parameters["estado"].ToString();
            }

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            try
            {
                respuesta = NegPedido.ConsultaSp_RTAListaPedidoDescargar(fechaDesde, fechaHasta, busqueda, IdCliente, IdGerenteCuenta, IdGestorProducto, sucursal, estado, IdClasificacion, Anio, meses, idFecha, IdRenovacion);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Pedidos");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Contrato" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }

        public string ObtenerListaSolicitudDescargar(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());

            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string busqueda = parameters["busqueda"].ToString();
            int tipo = Convert.ToInt32(parameters["tipo"].ToString());
            int pagina = Convert.ToInt32(parameters["pagina"].ToString());
            string estadosolicitud = parameters["estadosolicitud"].ToString();

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            try
            {
                respuesta = NegSolicitud.ConsultaSp_RTAListaSolicitudDescargar(tipo, IdUsuarioSession, pagina, estadosolicitud, fechaDesde, fechaHasta, Convert.ToInt32(busqueda));

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Solicitud");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Solicitud" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }

        public string ObtenerReporteGPFGeneradas(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();

            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            int idtipo = Convert.ToInt32(parameters["idtipo"].ToString());

            try
            {
                respuesta = NegTareas.ObtenerReporteGPFGeneradasDescargar(fechaDesde, fechaHasta, idtipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return respuesta.SerializaToJson().ToString();
        }

        public string ObtenerReporteGPFGeneradasDescargar(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();

            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            int idtipo =Convert.ToInt32 ( parameters["idtipo"].ToString());

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            try
            {
                respuesta = NegTareas.ObtenerReporteGPFGeneradasDescargar(fechaDesde, fechaHasta, idtipo);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de GPF");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de GPF" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }

        public string ObtenerCatalogoCombo(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntCombo> cmbElementosCatalogo = new List<EntCombo>();
            SeguridadHelper seguridad = new SeguridadHelper();

            int idTipoCatalogo = 0;
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                idTipoCatalogo = Convert.ToInt32(parameters["catalogo"].ToString());

                cmbElementosCatalogo = NegTareas.ListaCatalogoCombo(idTipoCatalogo); 
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return cmbElementosCatalogo.SerializaToJson();

        }
        public string ObtenerCatalogoTodosCombo(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntCombo> cmbElementosCatalogo = new List<EntCombo>();
            SeguridadHelper seguridad = new SeguridadHelper();

            int idTipoCatalogo = 0;
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                idTipoCatalogo = Convert.ToInt32(parameters["catalogo"].ToString());

                cmbElementosCatalogo = NegTareas.ListaCatalogoTodosCombo(idTipoCatalogo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return cmbElementosCatalogo.SerializaToJson();

        }
        public string ObtenerCatalogoFechaCombo(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntCombo> cmbElementosCatalogo = new List<EntCombo>();
            SeguridadHelper seguridad = new SeguridadHelper();

            int idTipoCatalogo = 0;
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                idTipoCatalogo = Convert.ToInt32(parameters["catalogo"].ToString());

                cmbElementosCatalogo = NegTareas.ListaCatalogoFechaCombo(idTipoCatalogo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return cmbElementosCatalogo.SerializaToJson();

        }
        public string ObtenerCatalogoComboAprobacion(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntCombo> cmbElementosCatalogo = new List<EntCombo>();
            SeguridadHelper seguridad = new SeguridadHelper();

            int idTipoCatalogo = 0;
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                idTipoCatalogo = Convert.ToInt32(parameters["catalogo"].ToString());

                cmbElementosCatalogo = NegTareas.ListaCatalogoTodosCombo(idTipoCatalogo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return cmbElementosCatalogo.SerializaToJson();

        }
        public string ObtenerComboEstados(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntCombo> cmbEstados = new List<EntCombo>();
            SeguridadHelper seguridad = new SeguridadHelper();
            int idEstado = 0;
            int IdProyecto = 0;
            try
            {
                idEstado = Convert.ToInt32(parameters["idTarea"].ToString());
                IdProyecto= Convert.ToInt32(parameters["idProyeto"].ToString());

                cmbEstados = NegTareas.ListaEstadosCombo(idEstado, IdProyecto); 
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return cmbEstados.SerializaToJson();

        }

        public string ObtenerComboContrato(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntCombo> cmbEstados = new List<EntCombo>();
            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            try
            {
                string IdSucursalGerente = "";
                int IdSucursal = 0;
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                int Tipo = Convert.ToInt32(parameters["Tipo"].ToString());
                if (parameters["IdSucursal"].ToString().Contains(","))
                {
                    IdSucursalGerente = parameters["IdSucursal"].ToString();
                }
                else
                {
                    if(parameters["IdSucursal"].ToString()=="null")
                    {
                        IdSucursal = 0;
                    }
                    else
                    {
                        IdSucursal = Convert.ToInt32(parameters["IdSucursal"].ToString());
                    }
                }
                int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());

                cmbEstados = NegContrato.RTAListaComboContrato(Tipo, IdUsuarioSession, IdSucursal, IdCliente, IdSucursalGerente);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return cmbEstados.SerializaToJson();

        }
        public string ObtenerDetalleTarea(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            EntDetalleTarea detalleTarea = new EntDetalleTarea();
            SeguridadHelper seguridad = new SeguridadHelper();

            int diaSemana = 0;
            int diaSemanaMasMenosFecha = 0;
            string diasBloqueoEditarTarea = "";

            try
            {
                //Int32 IdDetalleTarea = Convert.ToInt32(collectionJson[0].Valor.ToString());
                Int32 IdDetalleTarea = Convert.ToInt32(parameters.ToString());

                detalleTarea = NegTareas.RTA_ConsultaDetalleTareaRTA(IdDetalleTarea);
                //respuesta.estado = "1";
                //respuesta.mensaje = "OK";
                //respuesta.tipoMensaje = "success";
                //respuesta.resultado = detalleTarea.SerializaToJson();
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            string fechaInicial = detalleTarea.Det_Fch_RegDetalleIni.ToString().Substring(0, 10).ToString();

            DateTime dtFechaInicio = DateTime.ParseExact(fechaInicial, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime dtFechaMinima = DateTime.Today;
            //int diaSemana = (int) dtFechaMinima.DayOfWeek;
            //diaSemanaMasMenosFecha = (diaSemana + 7) * - 1;

            diasBloqueoEditarTarea = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("DIAS_BLOQUEO_EDITAR_TAREA"); 

            diaSemanaMasMenosFecha = Convert.ToInt32(diasBloqueoEditarTarea);
            dtFechaMinima = dtFechaMinima.AddDays(diaSemanaMasMenosFecha);
            if (dtFechaMinima.Date > dtFechaInicio.Date)
            {
                return responseMessage("0", "La tarea seleccionada ya se encuentra <b>bloqueada</b> fecha permitida de modificación desde el <b>" + dtFechaMinima.Date + "</b>.", "warning", "");
            }

            // Validación de estado de tarea para permitir actualización.
            if (detalleTarea.Det_Aprobacion_Tarea_Estado == 2)
            {
                return responseMessage("0", "La tarea seleccionada ya se encuentra <b>bloqueada</b> su estado actual es <b>" + detalleTarea.Det_Aprobacion_Tarea_Estado_Descripcion + "</b>.", "warning", "");
            }

            if (detalleTarea.Det_Aprobacion_Tarea_Estado_QA == 2)
            {
                return responseMessage("0", "La tarea seleccionada ya se encuentra <b>bloqueada</b> su estado actual es <b>" + detalleTarea.Det_Aprobacion_Tarea_Estado_QA_Descripcion + "</b>.", "warning", "");
            }

            return detalleTarea.SerializaToJson();

        }
        public string ObtenerDetalleTareaPrincipal(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            EntTareas detalleTarea = new EntTareas();
            SeguridadHelper seguridad = new SeguridadHelper();

            try
            {
                Int32 IdTarea = Convert.ToInt32(parameters.ToString());

                detalleTarea = NegTareas.RTA_ConsultaTareaRTA(IdTarea);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return detalleTarea.SerializaToJson();

        }
        public string ObtenerDetalleTareaDescargaXLS(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntDetTarea> listaTareas = new List<EntDetTarea>();
            SeguridadHelper seguridad = new SeguridadHelper();

            string idUsuario = parameters["usuario"].ToString();
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string session = parameters["session"].ToString(); 

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            string IdUsuarioSession = "";
            bool usuarioEsJefe = false;
            int tipo = 0;

            string cliente = parameters["cliente"].ToString();
            string orden = parameters["orden"].ToString();
            int idgasto = Convert.ToInt32(parameters["idtipogasto"].ToString());

            int idTareaAprobada = Convert.ToInt32(parameters["AprobarTarea"].ToString());
            int idTareaAprobadaQA = Convert.ToInt32(parameters["AprobarTareaQA"].ToString());

            int IdFecha = Convert.ToInt32(parameters["TipoFecha"].ToString());



            DataTable dtResultados = null;

            try
            {
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                usuarioEsJefe = NegUsuario.RTA_ConsultaUsuarioEsJefe(IdUsuarioSession);

                if (usuarioEsJefe)
                {
                    tipo = 1;

                    if (Convert.ToInt32(idUsuario) > 0)
                    {
                        tipo = 0;
                        idUsuario = parameters["usuario"].ToString();
                    }
                    else
                    {
                        idUsuario = IdUsuarioSession;
                    }
                }
                else
                {
                    tipo = 0;
                    idUsuario = parameters["usuario"].ToString();
                }


                respuesta = NegTareas.ListaDetalleTareasDescarga(idUsuario, fechaDesde, fechaHasta, cliente, orden, idgasto, idTareaAprobada, idTareaAprobadaQA, IdFecha, tipo);

                if (respuesta.estado == "1")
                {
                    dtResultados = respuesta.resultadoTabla;

                    // Creación de archivo XLS
                    var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Reporte de Horas");

                    int numeroColumna = 1;
                    foreach (DataColumn column in dtResultados.Columns)
                    {
                        worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                        worksheet.Columns(1, numeroColumna).Width=20;
                        worksheet.Rows(1, numeroColumna).AdjustToContents();
                        numeroColumna += 1;
                    }

                    rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                    worksheet.Cell(2, 1).Value = dtResultados;
                    //worksheet.RangeUsed().SetAutoFilter();

                    // Guardo archivo en carpeta publica
                    DateTime fechaActual = DateTime.Now;
                    nombreArchivo = "F-CS-001 Reporte Gestión OS" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                    workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                }
                else
                {
                    return respuesta.SerializaToJson().ToString();
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", ""); 

        }

        static DataTable ConvertListToDataTable(List<EntTareas> list)
        {
            // New table.
            DataTable table = new DataTable();

            // Get max columns.
            int columns = 0;
            //foreach (var array in list)
            //{
            //    if (array.Length > columns)
            //    {
            //        columns = array.Length;
            //    }
            //}

            // Add columns.
            for (int i = 0; i < list.Count; i++)
            {
                table.Columns.Add();
            }

            // Add rows.
            foreach (var array in list)
            {
                table.Rows.Add(array);
            }

            return table;
        }
        public string ObtenerDetalleHorasExtrasDescargaXLS(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntTareas> listaTareas = new List<EntTareas>();
            SeguridadHelper seguridad = new SeguridadHelper();

            string IdUsuario = seguridad.Desencripta(parameters["session"].ToString());
            string fechaDesde = parameters["fechaDesde"].ToString();
            string fechaHasta = parameters["fechaHasta"].ToString();
            string IdResponsable = parameters["usuario"].ToString();
            int estado = Convert.ToInt32(parameters["estado"].ToString());

            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";


            if (IdResponsable == "null")
                IdResponsable = "0";

            DataTable dtResultados = null;
            try
            {
                respuesta = NegTareas.ConsultaHorasExtrasPorAutorizarDescargar(fechaDesde, fechaHasta, IdResponsable, estado, IdUsuario);

                try
                {

                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Horas Extras");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte Gestión Horas Extras" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }
            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }
        public string GuardarNuevoContratoBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntContrato registro = new EntContrato();
                registro.FECHA = campos["txtFechaDesde"];
                registro.PEDIDO = Convert.ToInt32(campos["txtPedido"]);
                registro.CLIENTE = campos["txtCliente"];
                registro.REFERENCIA_CLIENTE = campos["txtReferencia"];
                registro.AREA = campos["cboArea"];
                registro.GESTOR_RESPONSABLE = campos["txtResponsable"];
                registro.ORDEN = campos["txtOrden"];
                registro.CLASIFICACION = campos["txtClasificacion"];
                registro.DESCRIPCION_DE_SERVICIO = campos["txtDescripcion"];
                registro.SLA_CONTRATO = campos["txtSla"];
                registro.HORAS_CONTRATADAS = campos["txtHContratadas"];
                registro.HORAS_ENTREGADAS = campos["txtHEntregadas"];
                registro.HORAS_DISPONIBLES = campos["txtHDisponible"];
                registro.COSTO_PLAN = campos["txtCostoPlan"];
                registro.COSTO_REAL = campos["txtCostoReal"];
                registro.SALDO_DE_COSTOS = campos["txtSaldoCosto"];
                registro.ESTATUS = campos["cboEstado"];
                registro.FECHA_ESTIMADA_DE_CIERRE = campos["txtFechaECierre"];
                registro.FECHA_CIERRE = campos["txtFechaCierre"];
                registro.MANTENIMIENTO = campos["txtMantenimiento"];
                registro.MANT_ENTREGADOS = campos["txtMantEntregado"];
                registro.OBSERVACIONES = campos["txtObservacion"];
                registro.CONTACTO_CLIENTE = campos["txtContactoCliente"];
                registro.GERENTE_DE_CUENTA = campos["txtGerenteCuenta"];
                registro.SUCURSAL = campos["txtSucursal"];
                registro.TiempoRespuesta = campos["cbotiempo"];
                registro.TiempoSolucion = campos["txtSolucion"];
                registro.Usuario = usuario.Nom_Usuario;
                registro.MAIL = campos["txtMail"];
                respuesta = NegContrato.RTA_InsertaNuevoContrato(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger","");

            }

            return respuesta.SerializaToJson();

        }

        public string ObtenerSolicitudIndividual(dynamic campos)
        {
            EntSolicitud Lista = new EntSolicitud();
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string session = campos["session"].ToString();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(session.ToString());
            int tipo =0;
            int idVacaciones = Convert.ToInt32(campos["IdVacaciones"].ToString());
            try
            {
                Lista = NegSolicitud.ConsultaSp_RTANotificarSolicitud(tipo, idVacaciones);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson();
        }
        public string GuardarNuevoPermisoBase(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            int TipoProceso = 0;
            try
            {
                EntSolicitud registro = new EntSolicitud();
                registro.IdVacaciones = Convert.ToInt32(campos["IdVacaciones"]);
                registro.IdTipoSolicitud = Convert.ToInt32(campos["IdTipoSolicitud"]);
                registro.FechaRegistro = campos["txtfechaP"];
                registro.CodSap = 0;
                registro.Cedula = campos["txtCedulaP"];
                registro.Colaborador = campos["txtNombreColaboradorP"];
                registro.Departamento = campos["txtDepartamentoP"];
                registro.JefeInmediato = campos["txtJefaAreaP"];
                registro.Remplazo = "";
                if (registro.IdTipoSolicitud == 1)
                {
                    registro.FechaDesde = campos["txtfechaP"] + " " + campos["frmTxtHoraDesdeP"];
                    registro.FechaHasta = campos["txtfechaP"] + " " + campos["frmTxtHoraHastaP"];
                }
                else
                {
                    registro.FechaDesde = campos["frmTxtHoraDesdeP"];
                    registro.FechaHasta = campos["frmTxtHoraHastaP"];
                }


                registro.TotalDias = Convert.ToDouble(0);
                registro.Feriado = Convert.ToDouble(0);
                registro.SaldoDias = Convert.ToDouble(0);
                registro.CargoVacaciones = Convert.ToInt32(campos["IdSI"]); 
                registro.Horas = campos["frmTxtTiempoP"];
                registro.Actividad = campos["txtActividadP"];
                registro.Observacion = campos["frmTxtObservacionesP"];
                registro.EstadoSolicitud = "POR APROBAR";
                registro.FechaAprobacion = "";
                registro.FechaRechazo = "";
                registro.UsuarioAprobo = "";
                registro.UsuarioRechazo = "";
                registro.Cod_Usuario = IdUsuarioSession;
                registro.Tipo = Convert.ToInt32(campos["tipo"]);
                TipoProceso = registro.Tipo;
                respuesta = NegSolicitud.RTA_InsertaNuevaSolicitud(registro);

                #region Envio Mail
                if (respuesta.estado == "1")
                {
                    string correoJefeInmediato = "";
                    string correoUsuario = "";
                    correoJefeInmediato = NegUsuario.RTA_CorreoJefeInmediato(IdUsuarioSession);
                    correoUsuario = NegUsuario.RTA_CorreoUsuario(IdUsuarioSession);
                    string IdSolicitud = "";
                    if (TipoProceso == 0)
                    {
                       IdSolicitud = respuesta.resultado.ToString();
                    }
                    else if (TipoProceso == 1)
                    {
                        IdSolicitud = campos["IdVacaciones"];
                    }                     
                    EntSolicitud Lista = new EntSolicitud();
                    List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
                    int tipo = 0;
                    EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                    Lista = NegSolicitud.ConsultaSp_RTANotificarSolicitud(tipo, Convert.ToInt32(IdSolicitud));


                    // Se encripta los parametros de aprobación y rechazo para enviar en la url de aprobación.
                    string valorEncritar1 = Lista.IdVacaciones.ToString() + ";" + Lista.IdVacaciones.ToString() + ";" + "SA" + ";" + registro.Cod_Usuario.ToString() + ";NO";
                    string valorEncritar2 = Lista.IdVacaciones.ToString() + ";" + Lista.IdVacaciones.ToString() + ";" + "SR" + ";" + registro.Cod_Usuario.ToString() + ";NO";
                    string valorIncritado1 = envioCorreo.Encrypt(valorEncritar1, "3m1l10100", "3m1l10100");
                    string valorIncritado2 = envioCorreo.Encrypt(valorEncritar2, "3m1l10100", "3m1l10100");
                    // Se trae el parametro de la URL del sitio de aprobaciones.
                    string urlSiteAprobacion = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE_APROBACIONES");
                    // Se estrcutura las URL de aprobación y rechazo de las horas extras
                    string urlAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado1;
                    string urlRechazarAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado2;
                    // Llenado de listado de parámetros a ser reemplazados en el contenido del correo electrónico.
                    listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "SOLICITUD DE PERMISO" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Fecha:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = Lista.FechaRegistro });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta11", Valor = "Cédula:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto11", Valor = Lista.Cedula });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Colaborador:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto2", Valor = Lista.Colaborador });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta21", Valor = "Departamento:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto21", Valor = Lista.Departamento });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta3", Valor = "Jefe Inmedianto:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto3", Valor = Lista.JefeInmediato });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta4", Valor = "Actividad a Realizar:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto4", Valor = Lista.Actividad });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto22", Valor = "Horas de Permiso" });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta23", Valor = "Desde:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto23", Valor = Lista.FechaDesde });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta25", Valor = "Hasta:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto25", Valor = Lista.FechaHasta });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta26", Valor = "Total de Horas:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto26", Valor = Lista.Horas });


                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta27", Valor = "Cargo a vacaciones:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto27", Valor = Lista.StrCargoVacaciones });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta28", Valor = "Observación:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto28", Valor = Lista.Observacion });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton1", Valor = "Aprobar" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton1", Valor = urlAprobacion });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton2", Valor = "Rechazar" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton2", Valor = urlRechazarAprobacion });

                    bool respuestaEnvioCorreo = false;
                    bool respuestaEnvioCorreoUsuario = false;
                    if (TipoProceso == 0)
                    {
                        respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(correoJefeInmediato, "Solicitud de Permiso", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionPermiso.txt"), listaCamposCorreo, "contenidoCorreoNotificacionPermiso.txt");
                    }
                    else if (TipoProceso == 1)
                    {
                        respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(correoJefeInmediato, "Solicitud de Permiso (Actualizar)", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionPermiso.txt"), listaCamposCorreo, "contenidoCorreoNotificacionPermiso.txt");
                    }
                    respuestaEnvioCorreoUsuario = envioCorreo.EnvioCorreoSolicitudEmpleado(correoUsuario, "Copia Solicitud de Permiso", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionPermisoUsuario.txt"), listaCamposCorreo, "contenidoCorreoNotificacionPermisoUsuario.txt", Convert.ToInt32(IdSolicitud));

                }
                #endregion

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevaVacacionesBase(dynamic campos)
        {
            int tipoSolicitud = 0;
            int TipoProceso = 0;
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntSolicitud registro = new EntSolicitud();
                registro.IdVacaciones = Convert.ToInt32(campos["IdVacaciones"]);
                registro.IdTipoSolicitud = Convert.ToInt32(campos["IdTipoSolicitud"]);
                tipoSolicitud = Convert.ToInt32(campos["IdTipoSolicitud"]);
                registro.FechaRegistro = campos["txtfechaV"];
                registro.CodSap = 0;
                registro.Cedula = campos["txtCedulaV"];
                registro.Colaborador = campos["txtNombreColaboradorV"];
                registro.Departamento = campos["txtDepartamentoV"];
                registro.JefeInmediato = campos["txtJefaAreaV"];
                registro.Remplazo = campos["txtReemplazo"];
                registro.FechaDesde = campos["frmTxtHoraDesdeV"];
                registro.FechaHasta = campos["frmTxtHoraHastaV"];
                registro.TotalDias = Convert.ToDouble(campos["frmTxtTiempoDiasV"]);
                registro.Feriado = Convert.ToDouble(campos["frmTxtTiempoF"]);
                registro.SaldoDias = Convert.ToDouble(campos["SaldoDias"]);
                registro.CargoVacaciones = 0;
                registro.Horas = "";
                registro.Actividad = "";
                registro.Observacion = "";
                registro.EstadoSolicitud = "POR APROBAR";
                registro.FechaAprobacion = "";
                registro.FechaRechazo = "";
                registro.UsuarioAprobo = "";
                registro.UsuarioRechazo ="";
                registro.Cod_Usuario = IdUsuarioSession;
                registro.Tipo = Convert.ToInt32(campos["tipo"]);
                TipoProceso = registro.Tipo;
                respuesta = NegSolicitud.RTA_InsertaNuevaSolicitud(registro);

                #region Envio Mail
                if (respuesta.estado == "1")
                {
                    string correoJefeInmediato = "";
                    string correoUsuario = "";
                    correoJefeInmediato = NegUsuario.RTA_CorreoJefeInmediato(IdUsuarioSession);
                    correoUsuario = NegUsuario.RTA_CorreoUsuario(IdUsuarioSession);
                    string IdSolicitud = "";
                    if (TipoProceso == 0)
                    {
                        IdSolicitud = respuesta.resultado.ToString();
                    }
                    else if (TipoProceso == 1)
                    {
                        IdSolicitud = campos["IdVacaciones"];
                    }
                    EntSolicitud Lista = new EntSolicitud();
                    List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
                    int tipo = 0;
                    EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                    Lista = NegSolicitud.ConsultaSp_RTANotificarSolicitud(tipo,Convert.ToInt32(IdSolicitud));


                    // Se encripta los parametros de aprobación y rechazo para enviar en la url de aprobación.
                    string valorEncritar1 = Lista.IdVacaciones.ToString() + ";" + Lista.IdVacaciones.ToString() + ";" + "SA" + ";" + registro.Cod_Usuario.ToString() + ";EM";
                    string valorEncritar2 = Lista.IdVacaciones.ToString() + ";" + Lista.IdVacaciones.ToString() + ";" + "SR" + ";" + registro.Cod_Usuario.ToString() + ";EM";
                    string valorIncritado1 = envioCorreo.Encrypt(valorEncritar1, "3m1l10100", "3m1l10100");
                    string valorIncritado2 = envioCorreo.Encrypt(valorEncritar2, "3m1l10100", "3m1l10100");
                    // Se trae el parametro de la URL del sitio de aprobaciones.
                    string urlSiteAprobacion = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE_APROBACIONES");
                    // Se estrcutura las URL de aprobación y rechazo de las horas extras
                    string urlAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado1;
                    string urlRechazarAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado2;
                    // Llenado de listado de parámetros a ser reemplazados en el contenido del correo electrónico.
                    if (tipoSolicitud == 2)
                    {
                        listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "SOLICITUD DE VACACIONES" });
                    }
                    else if (tipoSolicitud == 3)
                    {
                        listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "SOLICITUD DE PLANIFICACIÓN DE VACACIONES" });
                    }
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Fecha:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = Lista.FechaRegistro });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta11", Valor = "Cédula:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto11", Valor = Lista.Cedula });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Colaborador:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto2", Valor = Lista.Colaborador });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta21", Valor = "Departamento:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto21", Valor = Lista.Departamento });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta3", Valor = "Jefe Inmedianto:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto3", Valor = Lista.JefeInmediato });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta4", Valor = "Reemplazo:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto4", Valor = Lista.Remplazo });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto22", Valor = "Dias de Vacaciones" });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta23", Valor = "Desde:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto23", Valor = Lista.FechaDesde });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta25", Valor = "Hasta:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto25", Valor = Lista.FechaHasta });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta26", Valor = "Días Solicitados:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto26", Valor = Lista.TotalDias.ToString() });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta27", Valor = "Saldo de días:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "texto27", Valor = Lista.SaldoDias.ToString() });

                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton1", Valor = "Aprobar" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton1", Valor = urlAprobacion });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton2", Valor = "Rechazar" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton2", Valor = urlRechazarAprobacion });

                    bool respuestaEnvioCorreo = false;
                    bool respuestaEnvioCorreoUsuario = false;
                    if (tipoSolicitud == 2)
                    {
                        if (TipoProceso == 0)
                        {
                            respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(correoJefeInmediato, "Solicitud de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitud.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitud.txt");
                        }
                        else if (TipoProceso == 1)
                        {
                            respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(correoJefeInmediato, "Solicitud de Vacaciones (Actualizar)", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitud.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitud.txt");
                        }
                        respuestaEnvioCorreoUsuario = envioCorreo.EnvioCorreoSolicitudEmpleado(correoUsuario, "Copia Solicitud de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitudUsuario.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitudUsuario.txt", Convert.ToInt32(IdSolicitud));
                    }
                    else if (tipoSolicitud == 3)
                    {
                        if (TipoProceso == 0)
                        {
                            respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(correoJefeInmediato, "Solicitud de Planificación de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitud.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitud.txt");
                        }
                        else if (TipoProceso == 1)
                        {
                            respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(correoJefeInmediato, "Solicitud de Planificación de Vacaciones  (Actualizar)", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitud.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitud.txt");
                        }
                        respuestaEnvioCorreoUsuario = envioCorreo.EnvioCorreoSolicitudEmpleado(correoUsuario, "Copia Solicitud de Planificación de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitudUsuario.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitudUsuario.txt", Convert.ToInt32(IdSolicitud));
                    }

                }
                #endregion

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string EnviarCorreo(int IdVacaciones,string IdUsuarioSession, string Cod_Usuario,string  tipoSolicitud,string EstadoSolicitud)
        {
            string respuestaProceso = "";

            string correoJefeInmediato = "";
            string correoUsuario = "";
            correoJefeInmediato = NegUsuario.RTA_CorreoJefeInmediato(IdUsuarioSession);
            correoUsuario = NegUsuario.RTA_CorreoUsuario(IdUsuarioSession);
            string IdSolicitud = "";
            IdSolicitud = IdVacaciones.ToString();
            EntSolicitud Lista = new EntSolicitud();
            List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
            int tipo = 0;
            EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
            Lista = NegSolicitud.ConsultaSp_RTANotificarSolicitud(tipo, Convert.ToInt32(IdSolicitud));

            // Se encripta los parametros de aprobación y rechazo para enviar en la url de aprobación.
            string valorEncritar1 = Lista.IdVacaciones.ToString() + ";" + Lista.IdVacaciones.ToString() + ";" + "SA" + ";" + Cod_Usuario.ToString() + ";EM";
            string valorEncritar2 = Lista.IdVacaciones.ToString() + ";" + Lista.IdVacaciones.ToString() + ";" + "SR" + ";" + Cod_Usuario.ToString() + ";EM";
            string valorIncritado1 = envioCorreo.Encrypt(valorEncritar1, "3m1l10100", "3m1l10100");
            string valorIncritado2 = envioCorreo.Encrypt(valorEncritar2, "3m1l10100", "3m1l10100");
            // Se trae el parametro de la URL del sitio de aprobaciones.
            string urlSiteAprobacion = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE_APROBACIONES");
            // Se estrcutura las URL de aprobación y rechazo de las horas extras
            string urlAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado1;
            string urlRechazarAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado2;
            // Llenado de listado de parámetros a ser reemplazados en el contenido del correo electrónico.

            //Validar por Recurso humano EstadoSolicitud
            if (EstadoSolicitud == "RECHAZADO GTH")
            {
                correoJefeInmediato = NegUsuario.RTA_CorreoJefeInmediato(Lista.Cod_Usuario);
                correoUsuario = NegUsuario.RTA_CorreoUsuario(Lista.Cod_Usuario);
            }
            //Validar por Recurso humano EstadoSolicitud

            if (tipoSolicitud == "VACACIONES")
            {
                listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "SOLICITUD DE VACACIONES CANCELADO" });
            }
            else if (tipoSolicitud == "PLANIFICAR VACACIONES")
            {
                listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "SOLICITUD DE PLANIFICACIÓN DE VACACIONES CANCELADO" });
            }
            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Fecha:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = Lista.FechaRegistro });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta11", Valor = "Cédula:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto11", Valor = Lista.Cedula });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Colaborador:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto2", Valor = Lista.Colaborador });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta21", Valor = "Departamento:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto21", Valor = Lista.Departamento });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta3", Valor = "Jefe Inmedianto:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto3", Valor = Lista.JefeInmediato });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta4", Valor = "Reemplazo:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto4", Valor = Lista.Remplazo });

            listaCamposCorreo.Add(new EntItemValor() { Item = "texto22", Valor = "Dias de Vacaciones" });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta23", Valor = "Desde:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto23", Valor = Lista.FechaDesde });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta25", Valor = "Hasta:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto25", Valor = Lista.FechaHasta });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta26", Valor = "Días Solicitados:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto26", Valor = Lista.TotalDias.ToString() });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta27", Valor = "Saldo de días:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto27", Valor = Lista.SaldoDias.ToString() });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta28", Valor = "Motivo Cancelación:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto28", Valor = Lista.MotivoAnulacion.ToString() });

            //listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton1", Valor = "Aprobar" });
            //listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton1", Valor = urlAprobacion });
            //listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton2", Valor = "Rechazar" });
            //listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton2", Valor = urlRechazarAprobacion });

            bool respuestaEnvioCorreo = false;
            bool respuestaEnvioCorreoUsuario = false;
            if (tipoSolicitud == "VACACIONES")
            {
                respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(correoJefeInmediato, "Cancelacion de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitudCancelado.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitudCancelado.txt");
                respuestaEnvioCorreoUsuario = envioCorreo.EnvioCorreoSolicitudEmpleado(correoUsuario, "Copia Solicitud de Vacaciones (Cancelado)", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitudUsuarioCancelado.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitudUsuarioCancelado.txt", Convert.ToInt32(IdSolicitud));
            }
            else if (tipoSolicitud == "PLANIFICAR VACACIONES")
            {

                respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(correoJefeInmediato, "Cancelacion Planificación de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitudCancelado.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitudCancelado.txt");
                respuestaEnvioCorreoUsuario = envioCorreo.EnvioCorreoSolicitudEmpleado(correoUsuario, "Copia Solicitud de Planificación de Vacaciones (Cancelado)", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitudUsuarioCancelado.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitudUsuarioCancelado.txt", Convert.ToInt32(IdSolicitud));
            }

            return respuestaProceso;
        }

        public string ActualizarSolicitudBase(dynamic campos)
        {
            int TipoProceso = 0;
            int tipoSolicitud = 0;
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntSolicitud registro = new EntSolicitud();
                registro.IdVacaciones = Convert.ToInt32(campos["IdVacaciones"]);
                registro.EstadoSolicitud = campos["EstadoSolicitud"];
                registro.MotivoAnulacion = campos["Motivo"];
                registro.Cod_Usuario = IdUsuarioSession;
                registro.Tipo = Convert.ToInt32(campos["tipo"]);
                TipoProceso = Convert.ToInt32(campos["tipo"]);
                registro.UsuarioAprobo = "";
                registro.UsuarioRechazo = "";
                respuesta = NegSolicitud.RTA_ActualizarSolicitud(registro);
                if (registro.EstadoSolicitud == "APROBADO")
                {
                    EntSolicitud Lista = new EntSolicitud();
                    List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
                    string CorreoRH = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CORREORH");
                    int tipo = 0;
                    EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                    Lista = NegSolicitud.ConsultaSp_RTANotificarSolicitud(tipo, Convert.ToInt32(campos["IdVacaciones"]));
                    EnviarCorreoRH(Lista, campos["IdVacaciones"], CorreoRH, IdUsuarioSession);

                }
                else
                {
                    EnviarCorreo(Convert.ToInt32(campos["IdVacaciones"]), IdUsuarioSession, IdUsuarioSession, campos["StrTipoSolicitud"], campos["EstadoSolicitud"]);
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        #region EnviarCorreoRH
        public void EnviarCorreoRH(EntSolicitud Lista, string IdSolicitud, string correoUsuario, string IdUsuarioSession)
        {
            EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
            List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
            // Se encripta los parametros de aprobación y rechazo para enviar en la url de aprobación.
            string valorEncritar1 = Lista.IdVacaciones.ToString() + ";" + Lista.IdVacaciones.ToString() + ";" + "NOT" + ";" + IdUsuarioSession.ToString();
            string valorEncritar2 = Lista.IdVacaciones.ToString() + ";" + Lista.IdVacaciones.ToString() + ";" + "SR" + ";" + IdUsuarioSession.ToString();
            string valorIncritado1 = envioCorreo.Encrypt(valorEncritar1, "3m1l10100", "3m1l10100");
            string valorIncritado2 = envioCorreo.Encrypt(valorEncritar2, "3m1l10100", "3m1l10100");

            // Se trae el parametro de la URL del sitio de aprobaciones.
            string urlSiteAprobacion = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_SITE_APROBACIONES");
            // Se estrcutura las URL de aprobación y rechazo de las horas extras
            string urlAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado1;
            string urlRechazarAprobacion = urlSiteAprobacion + "/Formulario/RespuestaAprobacion.aspx?idValor=" + valorIncritado2;
            // Llenado de listado de parámetros a ser reemplazados en el contenido del correo electrónico.
            listaCamposCorreo.Add(new EntItemValor() { Item = "tituloNotificacion", Valor = "REGISTRAR SOLICITUD DE VACACIONES" });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta1", Valor = "Fecha:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto1", Valor = Lista.FechaRegistro });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta11", Valor = "Cédula:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto11", Valor = Lista.Cedula });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Colaborador:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto2", Valor = Lista.Colaborador });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta21", Valor = "Departamento:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto21", Valor = Lista.Departamento });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta3", Valor = "Jefe Inmedianto:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto3", Valor = Lista.JefeInmediato });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta4", Valor = "Reemplazo:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto4", Valor = Lista.Remplazo });

            listaCamposCorreo.Add(new EntItemValor() { Item = "texto22", Valor = "Dias de Vacaciones" });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta23", Valor = "Desde:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto23", Valor = Lista.FechaDesde });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta25", Valor = "Hasta:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto25", Valor = Lista.FechaHasta });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta26", Valor = "Días Solicitados:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto26", Valor = Lista.TotalDias.ToString() });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta27", Valor = "Saldo de días:" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "texto27", Valor = Lista.SaldoDias.ToString() });

            listaCamposCorreo.Add(new EntItemValor() { Item = "etiquetaBoton1", Valor = "Enviar solicitud de aprobación" });
            listaCamposCorreo.Add(new EntItemValor() { Item = "urlBoton1", Valor = urlAprobacion });

            bool respuestaEnvioCorreo = false;
            bool respuestaEnvioCorreoUsuario = false;
            //respuestaEnvioCorreoUsuario = envioCorreo.EnvioCorreoSolicitudEmpleado(correoUsuario, "Copia Solicitud de Autorización de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitudUsuario.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitudUsuario.txt", Convert.ToInt32(IdSolicitud));
            respuestaEnvioCorreo = envioCorreo.EnvioCorreoSolicitudJefe(correoUsuario, "Registrar Autorización de Vacaciones", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoCorreoNotificacionSolicitudRH.txt"), listaCamposCorreo, "contenidoCorreoNotificacionSolicitudRH.txt");
        }
        #endregion

        public string ResetearPasswordBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string ServerAD = "";
            string DominioAD = "";
            string Usuario = campos["Usuario"];
            string Password = campos["Password"];
            string ConfirmarPassword = campos["ConfirmarPassword"];
            string CodigoSeguridad = campos["CodigoSeguridad"];
            //string IdUsuarioSession = "";
            //IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            //usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {

                usuario = NegUsuario.RTA_ConsultarCodigoSeguridad(Usuario);

                //if (CodigoSeguridad == usuario.CodigoReset)
                //{
                    if (Password == ConfirmarPassword)
                    {
                        ServerAD = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("SERVERAD");
                        DominioAD = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("DOMINIOAD");
                        if (ServerAD != "" && DominioAD != "")
                        {
                            ReporteTareas.AD.ActualizarCuentaAD actualizarCuenta = new ReporteTareas.AD.ActualizarCuentaAD();
                            string respuestaAD = "";
                            respuestaAD = actualizarCuenta.CambiarContrasenia(ServerAD, DominioAD, Usuario, Password, ConfirmarPassword, ConfirmarPassword, 2);
                            if (respuestaAD == "Contraseña actualizada.")
                            {
                                respuesta.estado = "1";
                                respuesta.mensaje = respuestaAD;
                            }
                            else
                            {
                                respuesta.estado = "0";
                                respuesta.mensaje = "La Contraseña debe incluir caracteres de complejidad, es decir letras mayúsculas, minúsculas, número y caracteres especiales";
                            }
                        }
                    }
                    else
                    {
                        respuesta.estado = "0";
                        respuesta.mensaje = "No coiciden las claves por favor corregir";
                    }
                //}
                //else
                //{
                //    respuesta.estado = "0";
                //    respuesta.mensaje = "El codigo ingresado no es correcto.";
                //}
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevoPedidoBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntPedido registro = new EntPedido();
                registro.FECHA = campos["txtFechaDesde"];
                registro.PEDIDO = Convert.ToInt32(campos["txtPedido"]);
                registro.CLIENTE = campos["cboCliente"];
                registro.SEGMENTACION = campos["cboSegmentacion"];
                registro.CLASIFICACION = campos["txtClasificacion"];
                registro.VALOR = Convert.ToDouble(campos["txtValor"].ToString().Replace(".", ","));
                registro.RENTABILIDAD = Convert.ToDouble(campos["txtRentabilidad"].ToString().Replace(".", ","));
                registro.MARGEN = Convert.ToDouble(campos["txtMargen"].ToString().Replace(".", ","));
                registro.ESTADO = campos["cboEstado"];
                registro.DETALLE = campos["txtDetalle"];
                registro.N_FACTURA = campos["txtNFactura"];
                registro.FECHA_FACTURACION = campos["txtFechaFacturacion"];
                registro.FECHA_ESTIMADA_DE_FACTURACION = campos["txtFechaEstimadaFacturacion"];
                registro.SUCURSAL = campos["txtSucursal"];
                registro.OBSERVACION = campos["txtObservacion"];
                registro.VENDEDOR = campos["cboVendedor"];
                registro.GERENTE_DE_PRODUCTO = campos["cboGerente"];
                registro.ORDEN_DE_COMPRA = campos["txtOrdenCompra"];
                registro.ORDEN_DE_SERVICIOS_INTERNA = campos["txtOrdenServicioInterno"];
                registro.ORDEN_DE_SERVICIOS_EXTERNA = campos["txtOrdenServicioExterno"];
                registro.ORDEN_DE_SERVICIOS_DE_FINANZAS = campos["txtOrdenServicioFinanza"];
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);
                registro.IdPedido = Convert.ToInt32(campos["IdPedido"]);

                registro.ChekRenovacion = Convert.ToInt32(campos["checkIngreso"]);
                registro.FechaInicioR = campos["txtFechaDesdeRenova"];
                registro.FechaFinalR = campos["txtFechaHastaRenova"];

                respuesta = NegPedido.RTA_InsertaNuevoPedido(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarPrioridadForeCast(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntForeCast registro = new EntForeCast();

                registro.IdForeCast = Convert.ToInt32(campos["IdForeCast"]);
                registro.ActivarPrioridad = Convert.ToInt32(campos["ActivarPrioridad"]);
                registro.Tipo = Convert.ToInt32(campos["tipo"]);
                respuesta = NegForeCast.RTAAgregarPrioriodadForeCast(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevoForecastBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntForeCast registro = new EntForeCast();

                registro.IdForeCast = Convert.ToInt32(campos["IdForeCast"]);
                registro.IdMarca = Convert.ToInt32(campos["IdMarca"]);
                registro.IdCliente = Convert.ToInt32(campos["idCliente"]);
                registro.IdNegocio = Convert.ToInt32(campos["IdNegocio"]);
                registro.IdPrioridad = Convert.ToInt32(campos["IdPrioridad"]);
                registro.Marca = campos["cboMarca"];
                registro.Cliente = campos["cboCliente2"];// Convert.ToDouble(campos["txtValor"].ToString().Replace(".", ","));
                registro.DetalleProyecto = campos["txtDetalleProyecto"];
                registro.PVPEstimado = Convert.ToDecimal(campos["txtPrecioEstimado"].ToString());
                registro.Utilidad = Convert.ToDecimal(campos["txtUtilidad"]);
                registro.UtilidadBrutaEstimada = Convert.ToDecimal(campos["txtUtilidadBruta"]);
                registro.FechaFacturacion = campos["cboFecha1"];
                registro.CierreNegocio = Convert.ToInt32(campos["IdNegocio"]);
                registro.MesEstimadoCierre = campos["cboFecha2"];
                registro.Observaciones = campos["txtObservacion"];
                registro.GerentedeProducto = campos["cboGerente"];
                registro.IdGerenteProducto = Convert.ToInt32(campos["IdGerenteProducto"]);
                registro.GerentedeCuenta = campos["cboVendedor"];
                registro.IdGerenteCuenta = Convert.ToInt32(campos["IdGerenteCuenta"]);
                registro.SegmentodeMercado = campos["cboSegmentacion"];
                registro.Prioridad = campos["cboPrioridad"];
                registro.Sucursal = campos["cboSucursal"];
                registro.RegistroAprobado = campos["cboRegistro"];
                registro.Usuario = usuario.Nom_Usuario;
                registro.TipoEmpresa = campos["cboInstitucion"];

                registro.NumeroRegistro = campos["txtNumeroRegistro"];
                registro.GerenteCuenFabricante = campos["txtGerenteCuenFabricante"];
                registro.LiderProyectoCliente = campos["txtLiderProyectoCliente"];
                registro.Mayorista = campos["txtMayorista"];
                registro.FormaPago = campos["txtFormaPago"];

                registro.Contacto = campos["txtContactoLiderProyecto"];
                registro.FechaInicioProyecto = campos["txtFechaInicioProyecto"];
                registro.FechaFacturacionProyecto = campos["txtFechaFacturacionProyecto"];
                registro.Justificacion = campos["cboJustificacion"];
                registro.prsd = Convert.ToInt32(campos["IdPRSD"]);
                registro.ObservacionCierre = campos["txtComentarioProceso"];
                registro.TipoProyecto = campos["cboTipoProyecto"];
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);

                respuesta = NegForeCast.RTAInsertaNuevaForeCast(registro);

                if (respuesta.estado == "1")
                {
                    if (campos["IdEmpleado"] != "0")
                    {
                        bool respuestaEnvioCorreo = false;
                        EntUsuario usuario2 = new EntUsuario();
                        usuario2 = NegUsuario.RTAConsultaUsuarioPorCodigo(campos["IdEmpleado"]);
                        EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                        string MensajeProceso = usuario2.Log_Usuario + "↨" + campos["txtObservacion"];
                        respuestaEnvioCorreo = envioCorreo.EnvioCorreoForeCast(usuario2.E_Mail, "", MensajeProceso, MensajeProceso, Convert.ToInt32(respuesta.resultado));
                    }
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevoMensajeForecastBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntMensajeForeCast registro = new EntMensajeForeCast();

                registro.IdMensaje = 0;
                registro.IdForeCast = Convert.ToInt32(campos["IdForeCast"]);         
                registro.Mensaje = campos["Mensaje"];        
                registro.Usuario = usuario.Nom_Usuario;              
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);

                respuesta = NegMensajeForeCast.RTAInsertaNuevaMensajeForeCast(registro);
                if(respuesta.estado == "1")
                {
                    if (campos["IdEmpleado"] != "0")
                    {
                        bool respuestaEnvioCorreo = false;
                        EntUsuario usuario2 = new EntUsuario();
                        usuario2 = NegUsuario.RTAConsultaUsuarioPorCodigo(campos["IdEmpleado"]);
                        EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                        string MensajeProceso = usuario2.Log_Usuario + "↨" + campos["Mensaje"];
                        respuestaEnvioCorreo = envioCorreo.EnvioCorreoForeCast(usuario2.E_Mail, "", MensajeProceso, MensajeProceso, Convert.ToInt32(campos["IdForeCast"]));
                    }
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevoForecastGDBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntForeCast registro = new EntForeCast();

                registro.IdForeCast = Convert.ToInt32(campos["IdForeCast"]);
                registro.IdMarca = Convert.ToInt32(campos["IdMarca"]);
                registro.IdCliente = Convert.ToInt32(campos["idCliente"]);
                registro.IdNegocio = Convert.ToInt32(campos["IdNegocio"]);
                registro.IdPrioridad = Convert.ToInt32(campos["IdPrioridad"]);
                registro.Marca = campos["cboMarca"];
                registro.Cliente = campos["cboCliente2"];// Convert.ToDouble(campos["txtValor"].ToString().Replace(".", ","));
                registro.DetalleProyecto = campos["txtDetalleProyecto"];
                registro.PVPEstimado = Convert.ToDecimal(campos["txtPrecioEstimado"].ToString());
                registro.Utilidad = Convert.ToDecimal(campos["txtUtilidad"]);
                registro.UtilidadBrutaEstimada = Convert.ToDecimal(campos["txtUtilidadBruta"]);
                registro.FechaFacturacion = campos["cboFecha1"];
                registro.CierreNegocio = Convert.ToInt32(campos["IdNegocio"]);
                registro.MesEstimadoCierre = campos["cboFecha2"];
                registro.Observaciones = campos["txtObservacion"];
                registro.GerentedeProducto = campos["cboGerente"];
                registro.IdGerenteProducto = Convert.ToInt32(campos["IdGerenteProducto"]);
                registro.GerentedeCuenta = campos["cboVendedor"];
                registro.IdGerenteCuenta = Convert.ToInt32(campos["IdGerenteCuenta"]);
                registro.SegmentodeMercado = campos["cboSegmentacion"];
                registro.Prioridad = campos["cboPrioridad"];
                registro.Sucursal = campos["cboSucursal"];
                registro.RegistroAprobado = campos["cboRegistro"];
                registro.Usuario = usuario.Nom_Usuario;
                registro.TipoEmpresa = campos["cboInstitucion"];

                registro.NumeroRegistro = campos["txtNumeroRegistro"];
                registro.GerenteCuenFabricante = campos["txtGerenteCuenFabricante"];
                registro.LiderProyectoCliente = campos["txtLiderProyectoCliente"];
                registro.Mayorista = campos["txtMayorista"];
                registro.FormaPago = campos["txtFormaPago"];

                registro.Contacto = campos["txtContactoLiderProyecto"];
                registro.FechaInicioProyecto = campos["txtFechaInicioProyecto"];
                registro.FechaFacturacionProyecto = campos["txtFechaFacturacionProyecto"];
                registro.Justificacion = campos["cboJustificacion"];

                registro.Tipo = Convert.ToInt32(campos["Tipo"]);

                respuesta = NegForeCast.RTAInsertaNuevaForeCastGD(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GGuardarNuevoEgresoInventarioBase(dynamic campos)
        {
            string EstructuraCliente = "";
            int IdEncabezado = 0;
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            ReporteTareas.clases.GenerarXml generar = new ReporteTareas.clases.GenerarXml();
            try
            {
                EntEgresoInventario  registro = new EntEgresoInventario();

                registro.IdEncabezado = 0;
                registro.IdCliente = Convert.ToInt32(campos["idCliente"]);
                registro.Cod_Usuario = usuario.Log_Usuario;
                registro.FechaRegistro = Convert.ToDateTime ( campos["txtFechaActual"]);
                registro.CantidadComprada = 0;
                registro.SubTotal = Convert.ToDouble(campos["totalEgreso"]);
                registro.Iva = 0;
                registro.Total = Convert.ToDouble(campos["totalEgreso"]);
                registro.Observacion = campos["txtObservacion"];
                registro.Tipo = 0;
                registro.CodigoProceso = campos["CodigoProceso"];
                registro.Estado = 1;
                respuesta = NegEgresoInventario.RTAInsertaNuevoEgresoInventario(registro);

                if(respuesta.resultado != "0")
                {
                    IdEncabezado= Convert.ToInt32(respuesta.resultado);
                    registro.IdEncabezado = Convert.ToInt32(respuesta.resultado);
                    registro.IdDetalle = 0;
                    string[] listas = campos["detalle"].Split('↨');
                    foreach (string r in listas.ToArray())
                    {
                        if (r != "")
                        {
                            string[] lista = r.Split(';');
                            string[] lista2 = lista[0].Split('à');
                            registro .IdInventario = Convert.ToInt32(lista2[0].ToString());
                            registro.Cantidad =Convert.ToDouble (lista[1].ToString().Replace(".", ","));
                            registro.PrecioUnitario = Convert.ToDouble(lista[3].ToString().Replace(".", ","));
                            registro.PrecioTotal = 0;
                            registro.Tipo = 1;
                            respuesta = NegEgresoInventario.RTAInsertaNuevoEgresoInventario(registro);
                        }
                    }
                }


                if (respuesta.estado == "1")
                {
                    EstructuraCliente = "1206039933001" + "↨" + campos["nombreCliente"] + "↨" + "---" + "↨" + "---" + "↨" + "" + "↨" + "" + "↨" + "" + "↨" + "01" + "↨" + "000-000-000000000" + "↨" + "0000000000" + "↨" + "01/01/2022" + "↨" + campos["idCliente"] + "↨" + campos["txtFechaActual"];
                    string mensaje = "";
                    string RutaPdf = "";
                    string Archivo = "";
                    System.Xml.XmlDocument document = new System.Xml.XmlDocument();
                    document = generar.xml(EstructuraCliente, campos["detalle"], IdEncabezado.ToString(), campos["txtObservacion"], usuario.Log_Usuario, usuario.Cod_Usuario);
                    generar.VerErrores("document:" + document.InnerXml, "RevisionPDF", "RevisionPDF");
                    generar.PasarXmlDataset(document.InnerXml, "06", "DOS.jpg", "", "SRI", ref mensaje, ref RutaPdf, ref Archivo);
                    generar.VerErrores("mensaje:" + mensaje, "RevisionPDF", "RevisionPDF");
                    generar.VerErrores("RutaPdf:" + RutaPdf, "RevisionPDF", "RevisionPDF");
                    generar.VerErrores("Archivo:" + Archivo, "RevisionPDF", "RevisionPDF");
                    registro.NombreArchivo = Archivo;
                    registro.Tipo = 2;
                    respuesta = NegEgresoInventario.RTAInsertaNuevoEgresoInventario(registro);
                    respuesta.rutapdf = RutaPdf;
                    respuesta.Archivo = Archivo;

                }

            }
            catch (Exception ex)
            {
                generar.VerErrores("ex.Message.ToString():" + ex.Message.ToString(), "RevisionPDF", "RevisionPDF");
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }


        public string GuardarNuevoProductoBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntInventario  registro = new EntInventario();

                registro.IdInventario = Convert.ToInt32(campos["IdInventario"]);
                registro.IdMarca = Convert.ToInt32(campos["IdMarca"]);
                registro.CodigoSAP = campos["txtCodigoSap"];
                registro.NumParte = campos["txtNumParte"];
                registro.Descripcion  = campos["txtDescripcion"];
                registro.Cantidad = Convert.ToDouble(campos["txtCantidad"].ToString());
                registro.Ubicacion = campos["txtUbicacion"];
                registro.Almacen = campos["txtAlmacen"];
                registro.NumSerie = campos["txtNumSerie"];
                registro.PrecioUnitario = Convert.ToDouble(campos["txtPrecioUnitario"].ToString());
                registro.Usuario  = usuario.Log_Usuario;
                registro.Estado = 1;
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);
                respuesta = NegInventario.RTAInsertaNuevoInventario(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevoCosteoBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntCosteoServicio  registro = new EntCosteoServicio();

                registro.IdCosteo  = Convert.ToInt32(campos["IdCosteo"]);
                registro.Ticket = campos["txtTicket"];
                registro.FechaSolicitud = campos["txtFechaSolicitud"];
                registro.FechaActual = campos["txtFechaActual"];
                registro.IdVendedor = Convert.ToInt32(campos["cboVendedor"]);
                registro.Vendedor = "";
                registro.Sucursal = campos["cboSucursal"];// Convert.ToDouble(campos["txtValor"].ToString().Replace(".", ","));
                registro.IdCliente = Convert.ToInt32(campos["IdCliente"]);
                registro.Cliente = campos["cboCliente2"];
                registro.Sector = campos["txtSector"];
                registro.Concepto = campos["txtConcepto"].ToString();
                registro.UnidadNegocio = campos["txtUnidadNegocio"];
                registro.ResponsableDimen = campos["txtecnico"];
                registro.TipoServicio = campos["txtTipoServicio"];
                registro.PlazoEntrega = campos["txtPlazoEntrega"];
                registro.EstadoServicio = campos["txtEstadoServicio"];
                registro.FechaEntregaEsp = campos["txtFechaEntregaEsp"];
                registro.FechaEntregaAlc = campos["txtFechaEntregaAlc"];
                registro.Revision = campos["txtRevision"];
                registro.Costo = Convert.ToDecimal(campos["txtCosto"]);
                registro.Observacion = campos["txtObservacion"];
                registro.Usuario = usuario.Nom_Usuario;
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);

                respuesta = NegCosteoServicio.RTAInsertaNuevoCosteo(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevoRebatesBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntRebates registro = new EntRebates();
                registro.FECHA = campos["txtFechaDesde"];
                registro.ID_TRANSACCION = campos["txtTransaccion"];
                registro.PROGRAMA = campos["txtPrograma"];
                registro.TIPO_DE_INGRESO = campos["cboTipoIngreso"];
                registro.MARCA = campos["cboMarca"].ToString();
                registro.PROCESO = campos["proceso"].ToString();
                registro.RESPONSABLE = campos["txtReponsable"].ToString();
                registro.DESCRIPCION = campos["txtDetalle"].ToString();
                registro.VALOR = campos["txtValor"].ToString();

                registro.Q_FABRICANTE = campos["txtQFabricante"];
                registro.TIPO_DE_PAGO = campos["cboTipoPago"];
                registro.ESTADO = campos["cboEstado"];
                registro.ID_TRANSACCION1 = campos["txtIdTransaccion2"];
                registro.ID_PAGO = campos["txtIdPago"];
                registro.FECHA_PAGO = campos["txtFechaPago"];
                registro.FECHA_ESTIMADA_PAGO = campos["txtFechaEstimadaPago"];
                registro.OBSERVACIONES = campos["txtObservacion"];
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);
                registro.IdRebates = Convert.ToInt32(campos["IdRebates"]);
                registro.IdBanco = Convert.ToInt32(campos["IdBanco"]);

                respuesta = NegRebates.RTA_InsertaNuevoRebates(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevoBancoBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                DateTime fecha = DateTime.Now;

                EntRebates registro = new EntRebates();
                registro.DescripcionB = campos["txtRegistroBanco"];
                registro.ValorB =campos["txtPorcentaje"];
                registro.FechaIngreso = fecha.ToString();
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);
                registro.IdBanco = Convert.ToInt32(campos["IdBanco"]);
                registro.Estado = 1;
                respuesta = NegRebates.RTA_InsertaNuevoBanco(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevoPeridoFiscalBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                DateTime fecha = DateTime.Now;

                EntRebates registro = new EntRebates();
                registro.IdMarca = Convert.ToInt32(campos["cboMarca3"]);
                registro.DescripcionP = campos["txtDescripcionQ"];
                registro.FechaInicio = campos["txtFechaInicio"];
                registro.FechaFinal = campos["txtFechaFinal"];
                registro.FechaInicioDOS = campos["txtFechaInicioDOS"];
                registro.FechaFinalDOS = campos["txtFechaFinalDOS"];
                registro.Estado = 1;
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);
                registro.IdAnioFiscal = Convert.ToInt32(campos["IdAnioFiscal"]);

                respuesta = NegRebates.RTA_InsertaNuevoPeriodoFiscal(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string ActualizarFechaPedidoBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntPedido registro = new EntPedido();
                registro.FECHA = campos["txtFechaNueva"];
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);
                registro.idFecha = Convert.ToInt32(campos["idFecha"].ToString());
                registro.Anio = Convert.ToInt32(campos["Anio"].ToString());
                registro.meses = Convert.ToInt32(campos["meses"].ToString());

                respuesta = NegPedido.RTA_ActualizarFechaPedido(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevoClienteBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntPedido registro = new EntPedido();
                registro.CLIENTE = campos["txtObservacionReq"];
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);

                respuesta = NegPedido.RTA_InsertaNuevoClientes(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevoClienteGDBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntPedido registro = new EntPedido();
                registro.CLIENTE = campos["txtObservacionReq"];
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);

                respuesta = NegPedido.RTA_InsertaNuevoClientesGD(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarCuotaAnualBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntCuotaAnual registro = new EntCuotaAnual();
                string DatosGuardar = campos["resultadoDatos"];
                DatosGuardar = DatosGuardar.Replace("$ ", "").Replace("-",";");
                string[] listasCuotas = DatosGuardar.Split('↨');
                string fecha;
                fecha = DateTime.Now.ToString("dd/MM/yyyy");
                foreach (string r in listasCuotas.ToArray())
                {
                    if (r != "")
                    {
                        string[] lista = r.Split(';');

                        if (lista[0].ToString() == "")
                        {
                            registro.IdCuotaAnual = 0;
                            registro.IdProceso = 0;
                            registro.Fecha = fecha;

                            registro.Nombres = lista[1];
                            registro.CuotaAnual = lista[2];

                            if (lista[6] == "1")
                            {
                                registro.IdEstado = 1;
                            }
                            else if (lista[6] == "0")
                            {
                                registro.IdEstado = 0;
                            }

                            registro.IdSucursal = Convert.ToInt32(lista[4].ToString());
                            registro.Tipo = Convert.ToInt32(campos["Tipo"]);
                            respuesta = NegCuotaAnual.RTA_CrearCuotasAnual(registro);
                        }
                        else
                        {
                            registro.IdCuotaAnual = Convert.ToInt32(lista[0].ToString());
                            registro.IdProceso = Convert.ToInt32(lista[1].ToString());
                            registro.Fecha = fecha;

                            registro.Nombres = lista[2];
                            registro.CuotaAnual = lista[3];

                            if (lista[6] == "1")
                            {
                                registro.IdEstado = 1;
                            }
                            else if (lista[6] == "0")
                            {
                                registro.IdEstado = 0;
                            }

                            registro.IdSucursal = Convert.ToInt32(lista[5].ToString());
                            registro.Tipo = Convert.ToInt32(campos["Tipo"]);
                            respuesta = NegCuotaAnual.RTA_CrearCuotasAnual(registro);
                        }
                    }
                }

                //registro.Tipo = Convert.ToInt32(campos["Tipo"]);

                    //respuesta = NegPedido.RTA_InsertaNuevoClientes(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarCuotaAnualEmpresaBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntCuotaAnual registro = new EntCuotaAnual();
                string DatosGuardar = campos["resultadoDatos"];
                DatosGuardar = DatosGuardar.Replace("$ ", "").Replace("-", ";");
                string[] listasCuotas = DatosGuardar.Split('↨');
                string fecha;
                fecha = DateTime.Now.ToString("dd/MM/yyyy");
                foreach (string r in listasCuotas.ToArray())
                {
                    if (r != "")
                    {
                        string[] lista = r.Split(';');
                        if (lista.Count() == 5)
                        {
                            if(lista[0].ToString()=="")
                            {
                                registro.IdMetas = 0;
                            }
                            else
                            {
                                registro.IdMetas = Convert.ToInt32(lista[0].ToString());
                            }
                            registro.Anio = lista[1].ToString();
                            registro.MetaFacturacion = lista[2].ToString();
                            registro.MetaMargenBruto = lista[3].ToString();
                            registro.Tipo = 0;
                            respuesta = NegCuotaAnual.RTA_CrearCuotasAnualEmpresa(registro);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevaPolizaBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntPoliza registro = new EntPoliza();
                registro.IdPoliza = Convert.ToInt32(campos["IdPoliza"]);
                registro.IdPedido = Convert.ToInt32(campos["IdPedidos"]);
                registro.NumFactura = campos["txtNumFactura"];
                registro.OS = campos["txtOS"];
                registro.PEDIDO = campos["txtPedido"];
                registro.NumPoliza = campos["txtPoliza"];
                registro.ANEXO = campos["txtAnexo"];
                registro.BENEFICIARIO = campos["txtBeneficiario"];
                registro.FechaInicio = campos["txtFechaInicioPo"];
                registro.FechaFin = campos["txtFechFinalPo"];
                registro.Monto = campos["txtMonto"];
                registro.OBJETO = campos["txtObjetivo"];
                registro.TipoPoliza = campos["cboPoliza"];
                registro.UsuarioRegistro = usuario.Nom_Usuario;
                registro.EstadoPoliza = campos["cboEstadoPoliza2"];
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);

                respuesta = NegPoliza.RTAInsertaNuevaPolizas(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }
        public string GuardarNuevoMantenimientoBase(dynamic campos)
        {
            string fecha;
            fecha = DateTime.Now.ToString("dd/MM/yyyy");
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntMantenimiento registro = new EntMantenimiento();
                registro.IdServicio = 0;
                registro.Orden = campos["txtOrdenReq"];
                registro.OrdenServicio = campos["txtOrden"];
                registro.FechaInicio = campos["txtfechaReq1"];
                registro.FechaFinal = campos["txtfechaReq2"];
                registro.Valor = Convert.ToDouble(campos["txtValor"]);
                registro.Descripcion = campos["txtObservacionReq"];
                registro.Observacion = campos["txtDescripcionReq"];
                registro.Clasificacion = campos["cboClasificacion4"];
                registro.Estado = Convert.ToInt32(campos["estado"]);
                registro.IdRequerimiento = Convert.ToInt32(campos["IdRequerimiento"]);
                registro.Tipo = Convert.ToInt32(campos["Tipo"]);
                registro.FechIngreso = fecha;
              

                respuesta = NegMantenimiento.RTA_InsertaNuevoMantenimiento(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarNuevoTicketBase(dynamic campos)
        {
            string fecha;
            var control = 0;
            fecha = DateTime.Now.ToString("dd/MM/yyyy");
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            EntTareas objTarea = new EntTareas();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                objTarea.Num_OrdenServicio = campos["txt_Os"]; //dbl_OS.SelectedItem.ToString();
                objTarea.Fch_Registro = campos["txtFechaDesde"];
                objTarea.Id_Responsable = campos["idEspecialista"];
                objTarea.Nom_Responsable = campos["txtecnico"];
                objTarea.Nom_Empresa = campos["txtCliente"];
                objTarea.Det_Tarea = campos["txt_DetCamEstado"];
                objTarea.Estado = "A";
                objTarea.Categoria = campos["cboCategoria"];
                objTarea.SubCategoria = "";
                objTarea.EstadoApro = "";
                respuesta = NegTareas.RTA_CreaTareas2(objTarea); 
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string ActualizarNuevoContratoBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntContrato registro = new EntContrato();
                registro.FECHA = campos["txtFechaDesde"];
                registro.PEDIDO = Convert.ToInt32(campos["txtPedido"]);
                registro.CLIENTE = campos["txtCliente"];
                registro.REFERENCIA_CLIENTE = campos["txtReferencia"];
                registro.AREA = campos["cboArea"];
                registro.GESTOR_RESPONSABLE = campos["txtResponsable"];
                registro.ORDEN = campos["txtOrden"];
                registro.CLASIFICACION = campos["txtClasificacion"];
                registro.DESCRIPCION_DE_SERVICIO = campos["txtDescripcion"];
                registro.SLA_CONTRATO = campos["txtSla"];
                registro.HORAS_CONTRATADAS = campos["txtHContratadas"];
                registro.HORAS_ENTREGADAS = campos["txtHEntregadas"];
                registro.HORAS_DISPONIBLES = campos["txtHDisponible"];
                registro.COSTO_PLAN = campos["txtCostoPlan"];
                registro.COSTO_REAL = campos["txtCostoReal"];
                registro.SALDO_DE_COSTOS = campos["txtSaldoCosto"];
                registro.ESTATUS = campos["cboEstado"];
                registro.FECHA_ESTIMADA_DE_CIERRE = campos["txtFechaECierre"];
                registro.FECHA_CIERRE = campos["txtFechaCierre"];
                registro.MANTENIMIENTO = campos["txtMantenimiento"];
                registro.MANT_ENTREGADOS = campos["txtMantEntregado"];
                registro.OBSERVACIONES = campos["txtObservacion"];
                registro.CONTACTO_CLIENTE = campos["txtContactoCliente"];
                registro.GERENTE_DE_CUENTA = campos["txtGerenteCuenta"];
                registro.SUCURSAL = campos["txtSucursal"];
                registro.TiempoRespuesta = campos["cbotiempo"];
                registro.TiempoSolucion = campos["txtSolucion"];
                registro.MAIL = campos["txtMail"];
                registro.Usuario = usuario.Nom_Usuario;
                respuesta = NegContrato.RTA_ActualizarNuevoContrato(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string EliminarNuevoContratoBase(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntContrato registro = new EntContrato();
                registro.FECHA = campos["txtFechaDesde"];
                registro.PEDIDO = Convert.ToInt32(campos["txtPedido"]);
                registro.CLIENTE = campos["txtCliente"];
                registro.REFERENCIA_CLIENTE = campos["txtReferencia"];
                registro.AREA = campos["cboArea"];
                registro.GESTOR_RESPONSABLE = campos["txtResponsable"];
                registro.ORDEN = campos["txtOrden"];
                registro.CLASIFICACION = campos["txtClasificacion"];
                registro.DESCRIPCION_DE_SERVICIO = campos["txtDescripcion"];
                registro.SLA_CONTRATO = campos["txtSla"];
                registro.HORAS_CONTRATADAS = campos["txtHContratadas"];
                registro.HORAS_ENTREGADAS = campos["txtHEntregadas"];
                registro.HORAS_DISPONIBLES = campos["txtHDisponible"];
                registro.COSTO_PLAN = campos["txtCostoPlan"];
                registro.COSTO_REAL = campos["txtCostoReal"];
                registro.SALDO_DE_COSTOS = campos["txtSaldoCosto"];
                registro.ESTATUS = campos["cboEstado"];
                registro.FECHA_ESTIMADA_DE_CIERRE = campos["txtFechaECierre"];
                registro.FECHA_CIERRE = campos["txtFechaCierre"];
                registro.MANTENIMIENTO = campos["txtMantenimiento"];
                registro.MANT_ENTREGADOS = campos["txtMantEntregado"];
                registro.OBSERVACIONES = campos["txtObservacion"];
                registro.CONTACTO_CLIENTE = campos["txtContactoCliente"];
                registro.GERENTE_DE_CUENTA = campos["txtGerenteCuenta"];
                registro.SUCURSAL = campos["txtSucursal"];
                registro.TiempoRespuesta = campos["cbotiempo"];
                registro.TiempoSolucion = campos["txtSolucion"];

                respuesta = NegContrato.RTA_EliminarNuevoContrato(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        // SE AGREGARON LOS SIGUIENTES METODOS

        //METODO 1

        public string ObtenerComboPerfiles(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntCombo> cmbEstados = new List<EntCombo>();
            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            try
            {
                string IdSucursalGerente = "";
                int IdSucursal = 0;
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                int Tipo = Convert.ToInt32(parameters["Tipo"].ToString());
                if (parameters["IdSucursal"].ToString().Contains(","))
                {
                    IdSucursalGerente = parameters["IdSucursal"].ToString();
                }
                else
                {
                    if (parameters["IdSucursal"].ToString() == "null")
                    {
                        IdSucursal = 0;
                    }
                    else
                    {
                        IdSucursal = Convert.ToInt32(parameters["IdSucursal"].ToString());
                    }
                }
                int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());

                cmbEstados = NegPerfiles.RTAListaComboContrato(Tipo, IdUsuarioSession, IdSucursal, IdCliente, IdSucursalGerente);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return cmbEstados.SerializaToJson();

        }


        //METODO 2 

        public string ObtenerReporteDescargarTabla7(dynamic parameters)
        {
            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            List<EntMantenimiento> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            //string session = parameters["session"].ToString();
            int Tipo = Convert.ToInt32(parameters["Tipo"].ToString());
            String ORDEN = parameters["ORDEN"].ToString();





            try
            {
                respuesta = NegMantenimiento.Sp_RTA_ConsultarOrdenServicioDescarga(Tipo, ORDEN);
                try
                {
                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Servicios");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "Reporte de Servicios" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }


        public string ObtenerDetallePerfilesUsuario(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntPerfiles> listaPerfiles = new List<EntPerfiles>();

            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                listaPerfiles = NegPerfiles.Sp_RTA_ConsultarUsuariosPerfil(tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaPerfiles.SerializaToJson();


        }


        //METODO 3 

        public string GuardarUsuarioPerfil(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntPerfiles registro = new EntPerfiles();
                string DatosGuardar = campos["resultadoDatos"];
                DatosGuardar = DatosGuardar.Replace("$ ", "").Replace("-", ";");
                string[] listasUsuario = DatosGuardar.Split('↨');
                string fecha;
                fecha = DateTime.Now.ToString("dd/MM/yyyy");
                foreach (string r in listasUsuario.ToArray())
                {
                    if (r != "")
                    {
                        string[] lista = r.Split(';');
                        registro.IdPerfil = Convert.ToInt32(lista[2].ToString());
                        registro.IdUsuario = Convert.ToInt32(lista[1].ToString());
                        registro.Mail = lista[4].ToString();
                        respuesta = NegPerfiles.Sp_RTActualizarPerfil(registro);

                    }
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        // SE AGREGARON LOS SIGUIENTES METODOS

        //METODO 1 


        public string ObtenerComboServicios(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntCombo> cmbEstados = new List<EntCombo>();
            SeguridadHelper seguridad = new SeguridadHelper();
            string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            try
            {
                string IdSucursalGerente = "";
                int IdSucursal = 0;
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                int Tipo = Convert.ToInt32(parameters["Tipo"].ToString());
                if (parameters["IdSucursal"].ToString().Contains(","))
                {
                    IdSucursalGerente = parameters["IdSucursal"].ToString();
                }
                else
                {
                    if (parameters["IdSucursal"].ToString() == "null")
                    {
                        IdSucursal = 0;
                    }
                    else
                    {
                        IdSucursal = Convert.ToInt32(parameters["IdSucursal"].ToString());
                    }
                }
                int IdCliente = Convert.ToInt32(parameters["IdCliente"].ToString());

                cmbEstados = NegMantenimiento.RTAListaComboContrato(Tipo, IdUsuarioSession, IdSucursal, IdCliente, IdSucursalGerente);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return cmbEstados.SerializaToJson();

        }

        //METODO 2 

        public string ObtenerReporteDescargar(dynamic parameters)
        {
            string rutaCarpetaDescargas = "";
            string nombreArchivo = "";
            string urlDescargas = "";
            DataTable dtResultados = null;

            List<EntMantenimiento> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            //string session = parameters["session"].ToString();
            string IdUsuarioSession = "";
            //IdUsuarioSession = seguridad.Desencripta(session.ToString());

            int Anio = Convert.ToInt32(parameters["Anio"].ToString());
            // String Anio = parameters["Anio"].ToString();


            String Gestor = parameters["Gestor"].ToString();
            String Sucursal = parameters["Sucursal"].ToString();
            String Estado = parameters["Estado"].ToString();
            String Area = parameters["Area"].ToString();
            String GerenteCuenta = parameters["GerenteCuenta"].ToString();





            try
            {
                respuesta = NegMantenimiento.RTA_ConsultaReporteServiciosDescarga(Anio, Gestor, Sucursal, Estado, Area, GerenteCuenta);

                try
                {
                    if (respuesta.estado == "1")
                    {
                        dtResultados = respuesta.resultadoTabla;

                        // Creación de archivo XLS
                        var workbook = new XLWorkbook();
                        var worksheet = workbook.Worksheets.Add("Reporte de Servicios");

                        int numeroColumna = 1;
                        foreach (DataColumn column in dtResultados.Columns)
                        {
                            worksheet.Cell(1, numeroColumna).Value = column.ColumnName;
                            worksheet.Columns(1, numeroColumna).Width = 20;
                            worksheet.Rows(1, numeroColumna).AdjustToContents();
                            numeroColumna += 1;
                        }

                        rutaCarpetaDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("CARPETA_DESCARGAS");

                        worksheet.Cell(2, 1).Value = dtResultados;
                        //worksheet.RangeUsed().SetAutoFilter();

                        // Guardo archivo en carpeta publica
                        DateTime fechaActual = DateTime.Now;
                        nombreArchivo = "F-CS-001 Reporte de Servicios" + fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + "_" + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString() + ".xlsx";
                        workbook.SaveAs(rutaCarpetaDescargas + nombreArchivo);
                    }
                    else
                    {
                        return respuesta.SerializaToJson().ToString();
                    }
                }
                catch (Exception ex)
                {
                    return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
                }

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            dtResultados = null;


            urlDescargas = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("URL_DESCARGAS");

            return responseMessage("1", urlDescargas + nombreArchivo, "success", "");

        }


        public string ObtenerDetalleReportes(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntMantenimiento> listaReporte = new List<EntMantenimiento>();

            int Anio = Convert.ToInt32(parameters["Anio"].ToString());
            // String Anio = parameters["Anio"].ToString();


            String Gestor = parameters["Gestor"].ToString();
            String Sucursal = parameters["Sucursal"].ToString();
            String Estado = parameters["Estado"].ToString();
            String Area = parameters["Area"].ToString();


            try
            {
                listaReporte = NegMantenimiento.RTA_ConsultaReporteServicios(Anio, Gestor, Sucursal, Estado, Area);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaReporte.SerializaToJson();


        }


        //METODO 3 



        public string ObtenerOrdenServicio(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntMantenimiento> listaReporte = new List<EntMantenimiento>();
            int Tipo = Convert.ToInt32(parameters["Tipo"].ToString());


            String ORDEN = parameters["ORDEN"].ToString();


            try
            {
                listaReporte = NegMantenimiento.Sp_RTA_ConsultarOrdenServicio(Tipo, ORDEN);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaReporte.SerializaToJson();


        }




        // Función para armar respuesta que se envía al ajax
        public string responseMessage(string estado, string mensaje, string tipoMensaje, string resultado)
        {

            EntRespuesta respuesta = new EntRespuesta();
            respuesta.estado = estado;
            respuesta.mensaje = mensaje;
            respuesta.tipoMensaje = tipoMensaje;
            respuesta.resultado = resultado;

            return respuesta.SerializaToJson().ToString();

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public string ObtenerDetalleMenu(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<EntMenuDos> listaPerfiles = new List<EntMenuDos>();

            int tipo = Convert.ToInt32(parameters["tipo"].ToString());

            try
            {
                listaPerfiles = NegMenuDos.Sp_RTA_ConsultarMenuPerfil(tipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return listaPerfiles.SerializaToJson();


        }


        public string InsertarNuevoMenu(dynamic parameters)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(parameters["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {

                int tipoMenu = Convert.ToInt32(parameters["tipoMenu"].ToString());
                String Titulo = parameters["Titulo"].ToString();
                String Descripcion = parameters["Descripcion"].ToString();
                String Icono = parameters["Icono"].ToString();
                String Referencia = parameters["Referencia"].ToString();
                int MenuPadre = Convert.ToInt32(parameters["MenuPadre"].ToString());

                respuesta = NegMenuDos.Sp_RTA_InsertarMenuNuevo(tipoMenu, Titulo, Descripcion, Icono, Referencia, MenuPadre);



            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al insertar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarDatosMenu(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntMenuDos registro = new EntMenuDos();
                string DatosGuardar = campos["resultadoDatos"];
                DatosGuardar = DatosGuardar.Replace("$ ", "").Replace("-", ";");
                string[] listasUsuario = DatosGuardar.Split('↨');
                string fecha;
                fecha = DateTime.Now.ToString("dd/MM/yyyy");
                foreach (string r in listasUsuario.ToArray())
                {
                    if (r != "")
                    {
                        string[] lista = r.Split(';');
                        registro.Id_Menu = Convert.ToInt32(lista[0].ToString());
                        registro.Id_Perfil = Convert.ToInt32(campos["tipoPerfil"]);
                        if (lista[2] == "true")
                        {
                            registro.Estado = 0;
                        }
                        else
                        {
                            registro.Estado = 1;
                        }
                        //registro.Estado = Convert.ToInt32(lista[2].ToString());
                        respuesta = NegMenuDos.Sp_RTActualizarPerfilMenu(registro);

                    }
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        public string GuardarDatosIcono(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            EntUsuario usuario = new EntUsuario();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            usuario = NegUsuario.RTAConsultaUsuarioPorCodigo(IdUsuarioSession);
            try
            {
                EntMenuDos registro = new EntMenuDos();


                registro.Id_Menu = Convert.ToInt32(campos["IdMenu"]);
                registro.Class_Icon = (campos["Icono"]);
                registro.Href = (campos["Referencia"]);
                respuesta = NegMenuDos.Sp_RTActualizarIconoMenu(registro);



            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();

        }

        private string ObteneConsultarReporteFacturasProveedor(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            List<EntInfoFacturaProv> Ent1 = new List<EntInfoFacturaProv>();

            //string IdUsuarioSession = "";
            //IdUsuarioSession = seguridad.Desencripta(campos["session"]);
            string fecIni = campos["fechaIni"];
            string fecFin = campos["FechaFin"];
            string txtFactura = campos["Factura"];
            string txtCliente = campos["Cliente"];
            int txtTipo = Convert.ToInt32(campos["tipo"]);
            try
            {
                Ent1 = NegInfoFactProv.Sp_RTAConsultarListaProveedores(fecIni, fecFin, txtFactura, txtCliente, txtTipo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");
            }
            //GuardarHistoria();

            return Ent1.SerializaToJson();
        }



        public string GuardarNuevaHistoria(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            //EntEmpleado empleado = new EntEmpleado();

            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);

            //empleado = NegEmpleado.Sp_RTA_ConsultarEmpleadoPorId(IdUsuarioSession);

            try
            {
                /*EntEmpleado registro = new EntEmpleado();
                registro.IdEmpleado = Convert.ToInt32(campos["txtIdEmpleado"]);
                registro.Nombre = campos["txtNombre"];             
                registro.Provincia = campos["txtProvincia"];
                respuesta = NegEmpleado.Sp_InsertarActualizarEmpleado(registro);*/

                // Creamos variables con los mismos nombres de las etiquetas en el formulario de Excel
                string primerNombre = "";
                string segundoNombre = "";
                string primerApellido = "";
                string segundoApellido = "";
                string sexo = "";
                string edad = "";
                string religion = "";
                string catolica = "";
                string evangelica = "";
                string testigo = "";
                string mormona = "";
                string otrareligion = "";
                string gruposanguineo = "";
                string lateralidad = "";
                string orientacion = "";
                string orientlesbiana = "";
                string orientgay = "";
                string orientbisexual = "";
                string orientheterosexual = "";
                string orientnosabe = "";
                string identidad = "";
                string identfemenino = "";
                string identmasculino = "";
                string identransfem = "";
                string identransmasc = "";
                string identnosabe = "";
                string discapacidadsi = "";
                string discapacidadno = "";
                string discapacidadtipo = "";
                string discapacidadporcentaje = "";
                string fechaingresotrabajo = "";
                string puestotrabajo = "";
                string areatrabajo = "";
                string actividadesrelevantes = "";

                string motivoconsulta = "";
                string antpersonales = "";
                string menarquia = "";
                string antfamiliares = "";
                string ciclos = "";
                string ultmenstruacion = "";
                string gestas = "";
                string partos = "";
                string cesareas = "";
                string abortos = "";
                string hijosvivosF = "";
                string hijosmuertosF = "";
                string vidaSxActivaSi = "";
                string vidaSxActivaNo = "";
                string metodoplanfamSi1 = "";
                string metodoplanfamNo1 = "";
                string metodoplanfamTipo1 = "";
                string papnicolaouSi = "";
                string papnicolaouNo = "";
                string papnicolaouTiempo = "";
                string papnicolaouResultado = "";
                string ecomamaSi = "";
                string ecomamaNo = "";
                string ecomamaTiempo = "";
                string ecomamaResultado = "";
                string colposcopiaSi = "";
                string colposcopiaNo = "";
                string colposcopiaTiempo = "";
                string colposcopiaResultado = "";
                string mamografiaSi = "";
                string mamografiaNo = "";
                string mamografiaTiempo = "";
                string mamografiaResultado = "";
                string antigenoprostSi = "";
                string antigenoprostNo = "";
                string antigenoprostTiempo = "";
                string antigenoprostResultado = "";
                string ecoprostaticoSi = "";
                string ecoprostaticoNo = "";
                string ecoprostaticoTiempo = "";
                string ecoprostaticoResultado = "";
                string metodoplanfamSi2 = "";
                string metodoplanfamNo2 = "";
                string metodoplanfamTipo2 = "";
                string hijosvivosM = "";
                string hijosmuertosM = "";
                string tabacoSi = "";
                string tabacoNo = "";
                string tabacoTiempo = "";
                string tabacoCantidad = "";
                string tabacoExconsumidor = "";
                string tabacoTiempoabst = "";
                string alcoholSi = "";
                string alcoholNo = "";
                string alcoholTiempo = "";
                string alcoholCantidad = "";
                string alcoholExconsumidor = "";
                string alcoholTiempoabst = "";
                string nombreOtrasDrogas = "";
                string otrasdrogasSi = "";
                string otrasdrogasNo = "";
                string otrasdrogasTiempo = "";
                string otrasdrogasCantidad = "";
                string otrasdrogasExconsumidor = "";
                string otrasdrogasTiempoabst = "";
                string actividadfisicaSi = "";
                string actividadfisicaNo = "";
                string actividadfisicaCual = "";
                string tiempoActividadFisica = "";
                string medicacionhabSi = "";
                string medicacionhabNo = "";
                string medicacionhabCual1 = "";
                string medicacionhabCual2 = "";
                string medicacionhabCual3 = "";
                string cantidadMedicamento1 = "";
                string cantidadMedicamento2 = "";
                string cantidadMedicamento3 = "";
                string actextralaborales = "";
                string enfermedadactual = "";

                string antempresa1 = "";
                string antpuestotrabajo1 = "";
                string antactividad1 = "";
                string anttiempotrabajo1 = "";
                string antriesgo = "";
                string antriesgofisico1 = "";
                string antriesgomecanico1 = "";
                string antriesgoquimico1 = "";
                string antriesgobiologico1 = "";
                string antriesgoergonomico1 = "";
                string antriesgopsicosocial1 = "";
                string antobservaciones1 = "";

                string antempresa2 = "";
                string antpuestotrabajo2 = "";
                string antactividad2 = "";
                string anttiempotrabajo2 = "";
                string antriesgo2 = "";
                string antriesgofisico2 = "";
                string antriesgomecanico2 = "";
                string antriesgoquimico2 = "";
                string antriesgobiologico2 = "";
                string antriesgoergonomico2 = "";
                string antriesgopsicosocial2 = "";
                string antobservaciones2 = "";

                string antempresa3 = "";
                string antpuestotrabajo3 = "";
                string antactividad3 = "";
                string anttiempotrabajo3 = "";
                string antriesgo3 = "";
                string antriesgofisico3 = "";
                string antriesgomecanico3 = "";
                string antriesgoquimico3 = "";
                string antriesgobiologico3 = "";
                string antriesgoergonomico3 = "";
                string antriesgopsicosocial3 = "";
                string antobservaciones3 = "";

                string antempresa4 = "";
                string antpuestotrabajo4 = "";
                string antactividad4 = "";
                string anttiempotrabajo4 = "";
                string antriesgo4 = "";
                string antriesgofisico4 = "";
                string antriesgomecanico4 = "";
                string antriesgoquimico4 = "";
                string antriesgobiologico4 = "";
                string antriesgoergonomico4 = "";
                string antriesgopsicosocial4 = "";
                string antobservaciones4 = "";

                string accidentestrabajoSi = "";
                string accidentestrabajoNo = "";
                string especificaracctrabajo = "";
                string acctrabajoobservaciones = "";
                string enfermedadesprofSi = "";
                string enfermedadesprofNo = "";
                string especificarenfermedadesprof = "";
                string enfermedadesprofobservaciones = "";

                string factoresriesgopuestotrabajo1 = "";
                string factoresriesgoactividades1 = "";
                string fisicotempaltas1 = "";
                string fisicotempbajas1 = "";
                string fisicoionizante1 = "";
                string fisicoNoionizante1 = "";
                string fisicoruido1 = "";
                string fisicovibracion1 = "";
                string fisicoiluminacion1 = "";
                string fisicoventilacion1 = "";
                string fisicoelectrico1 = "";
                string fisicootros1 = "";
                string mecatrapmaquinas1 = "";
                string mecatrapsuperficies1 = "";
                string mecatrapobjetos1 = "";
                string meccaidasdeobjetos1 = "";
                string meccaidasmismonivel1 = "";
                string meccaidasdiferentenivel1 = "";
                string meccontactoelectrico1 = "";
                string meccontacosuperficies1 = "";
                string mecproyeccionparticulas1 = "";
                string mecproyeccionfluidos1 = "";
                string mecpinchazos1 = "";
                string meccortes1 = "";
                string mecatropellamientovehiculo1 = "";
                string mecchoquescolision1 = "";
                string mecanicootros1 = "";
                string quimicosolidos1 = "";
                string quimicopolvos1 = "";
                string quimicohumos1 = "";
                string quimicoliquidos1 = "";
                string quimicovapores1 = "";
                string quimicoaerosoles1 = "";
                string quimiconeblinas1 = "";
                string quimicogaseosos1 = "";
                string quimicootros1 = "";
                string biologicovirus1 = "";
                string biologicohongos1 = "";
                string biologicobacterias1 = "";
                string biologicoparasitos1 = "";
                string biologicoexpovectores1 = "";
                string biologicoexpoanimales1 = "";
                string biologicootros1 = "";
                string ergonomicomanejomanual1 = "";
                string ergonomicomovimientorep1 = "";
                string ergonomicoposturasforzadas1 = "";
                string ergonomicotrabajopvd1 = "";
                string ergonomicootros1 = "";
                string psicosocialmonotonia1 = "";
                string psicosocialsobrecarga1 = "";
                string psicosocialminuciosidad1 = "";
                string psicosocialaltaresponsabilidad1 = "";
                string psicosocialautonomia1 = "";
                string psicosocialsupervision1 = "";
                string psicosocialconflicto1 = "";
                string psicosocialfaltaclaridad1 = "";
                string psicosocialincorrecta1 = "";
                string psicosocialturnos1 = "";
                string psicosocialrelaciones1 = "";
                string psicosocialinestabilidad1 = "";
                string psicosocialotros1 = "";
                string medidaspreventivas1 = "";

                string factoresriesgopuestotrabajo2 = "";
                string factoresriesgoactividades2 = "";
                string fisicotempaltas2 = "";
                string fisicotempbajas2 = "";
                string fisicoionizante2 = "";
                string fisicoNoionizante2 = "";
                string fisicoruido2 = "";
                string fisicovibracion2 = "";
                string fisicoiluminacion2 = "";
                string fisicoventilacion2 = "";
                string fisicoelectrico2 = "";
                string fisicootros2 = "";
                string mecatrapmaquinas2 = "";
                string mecatrapsuperficies2 = "";
                string mecatrapobjetos2 = "";
                string meccaidasdeobjetos2 = "";
                string meccaidasmismonivel2 = "";
                string meccaidasdiferentenivel2 = "";
                string meccontactoelectrico2 = "";
                string meccontacosuperficies2 = "";
                string mecproyeccionparticulas2 = "";
                string mecproyeccionfluidos2 = "";
                string mecpinchazos2 = "";
                string meccortes2 = "";
                string mecatropellamientovehiculo2 = "";
                string mecchoquescolision2 = "";
                string mecanicootros2 = "";
                string quimicosolidos2 = "";
                string quimicopolvos2 = "";
                string quimicohumos2 = "";
                string quimicoliquidos2 = "";
                string quimicovapores2 = "";
                string quimicoaerosoles2 = "";
                string quimiconeblinas2 = "";
                string quimicogaseosos2 = "";
                string quimicootros2 = "";
                string biologicovirus2 = "";
                string biologicohongos2 = "";
                string biologicobacterias2 = "";
                string biologicoparasitos2 = "";
                string biologicoexpovectores2 = "";
                string biologicoexpoanimales2 = "";
                string biologicootros2 = "";
                string ergonomicomanejomanual2 = "";
                string ergonomicomovimientorep2 = "";
                string ergonomicoposturasforzadas2 = "";
                string ergonomicotrabajopvd2 = "";
                string ergonomicootros2 = "";
                string psicosocialmonotonia2 = "";
                string psicosocialsobrecarga2 = "";
                string psicosocialminuciosidad2 = "";
                string psicosocialaltaresponsabilidad2 = "";
                string psicosocialautonomia2 = "";
                string psicosocialsupervision2 = "";
                string psicosocialconflicto2 = "";
                string psicosocialfaltaclaridad2 = "";
                string psicosocialincorrecta2 = "";
                string psicosocialturnos2 = "";
                string psicosocialrelaciones2 = "";
                string psicosocialinestabilidad2 = "";
                string psicosocialotros2 = "";
                string medidaspreventivas2 = "";

                string factoresriesgopuestotrabajo3 = "";
                string factoresriesgoactividades3 = "";
                string fisicotempaltas3 = "";
                string fisicotempbajas3 = "";
                string fisicoionizante3 = "";
                string fisicoNoionizante3 = "";
                string fisicoruido3 = "";
                string fisicovibracion3 = "";
                string fisicoiluminacion3 = "";
                string fisicoventilacion3 = "";
                string fisicoelectrico3 = "";
                string fisicootros3 = "";
                string mecatrapmaquinas3 = "";
                string mecatrapsuperficies3 = "";
                string mecatrapobjetos3 = "";
                string meccaidasdeobjetos3 = "";
                string meccaidasmismonivel3 = "";
                string meccaidasdiferentenivel3 = "";
                string meccontactoelectrico3 = "";
                string meccontacosuperficies3 = "";
                string mecproyeccionparticulas3 = "";
                string mecproyeccionfluidos3 = "";
                string mecpinchazos3 = "";
                string meccortes3 = "";
                string mecatropellamientovehiculo3 = "";
                string mecchoquescolision3 = "";
                string mecanicootros3 = "";
                string quimicosolidos3 = "";
                string quimicopolvos3 = "";
                string quimicohumos3 = "";
                string quimicoliquidos3 = "";
                string quimicovapores3 = "";
                string quimicoaerosoles3 = "";
                string quimiconeblinas3 = "";
                string quimicogaseosos3 = "";
                string quimicootros3 = "";
                string biologicovirus3 = "";
                string biologicohongos3 = "";
                string biologicobacterias3 = "";
                string biologicoparasitos3 = "";
                string biologicoexpovectores3 = "";
                string biologicoexpoanimales3 = "";
                string biologicootros3 = "";
                string ergonomicomanejomanual3 = "";
                string ergonomicomovimientorep3 = "";
                string ergonomicoposturasforzadas3 = "";
                string ergonomicotrabajopvd3 = "";
                string ergonomicootros3 = "";
                string psicosocialmonotonia3 = "";
                string psicosocialsobrecarga3 = "";
                string psicosocialminuciosidad3 = "";
                string psicosocialaltaresponsabilidad3 = "";
                string psicosocialautonomia3 = "";
                string psicosocialsupervision3 = "";
                string psicosocialconflicto3 = "";
                string psicosocialfaltaclaridad3 = "";
                string psicosocialincorrecta3 = "";
                string psicosocialturnos3 = "";
                string psicosocialrelaciones3 = "";
                string psicosocialinestabilidad3 = "";
                string psicosocialotros3 = "";
                string medidaspreventivas3 = "";

                string revisionpiel = "";
                string revisionsentidos = "";
                string revisionrespiratorio = "";
                string revisioncardio = "";
                string revisiondigestivo = "";
                string revisiongenito = "";
                string revisionmusculo = "";
                string revisionendocrino = "";
                string revisionhemo = "";
                string revisionnervioso = "";
                string revisionorganosdescripcion = "";

                string constpresion = "";
                string consttemperatura = "";
                string constfreccardiaca = "";
                string constsaturacion = "";
                string constfrecrespiratoria = "";
                string constpeso = "";
                string consttalla = "";
                string constindice = "";
                string constperimetro = "";

                string examfisicopielA = "";
                string examfisicopielB = "";
                string examfisicopielC = "";
                string examfisicoojosA = "";
                string examfisicoojosB = "";
                string examfisicoojosC = "";
                string examfisicoojosD = "";
                string examfisicoojosE = "";
                string examfisicooidoA = "";
                string examfisicooidoB = "";
                string examfisicooidoC = "";
                string examfisicooroA = "";
                string examfisicooroB = "";
                string examfisicooroC = "";
                string examfisicooroD = "";
                string examfisicooroE = "";
                string examfisiconarizA = "";
                string examfisiconarizB = "";
                string examfisiconarizC = "";
                string examfisiconarizD = "";
                string examfisicocuelloA = "";
                string examfisicocuelloB = "";
                string examfisicotorax1A = "";
                string examfisicotorax1B = "";
                string examfisicotorax2A = "";
                string examfisicotorax2B = "";
                string examfisicoabdomenA = "";
                string examfisicoabdomenB = "";
                string examfisicocolumnaA = "";
                string examfisicocolumnaB = "";
                string examfisicocolumnaC = "";
                string examfisicopelvisA = "";
                string examfisicopelvisB = "";
                string examfisicoextremA = "";
                string examfisicoextremB = "";
                string examfisicoextremC = "";
                string examfisiconeuroA = "";
                string examfisiconeuroB = "";
                string examfisiconeuroC = "";
                string examfisiconeuroD = "";
                string examfisicoobservacion = "";

                string examNom1 = "";
                string examNom2 = "";
                string examNom3 = "";
                string examNom4 = "";
                string examFecha1 = "";
                string examFecha2 = "";
                string examFecha3 = "";
                string examFecha4 = "";
                string examResultado1 = "";
                string examResultado2 = "";
                string examResultado3 = "";
                string examResultado4 = "";
                string examObservaciones = "";

                string DiagDescripcion1 = "";
                string DiagCIE1 = "";
                string DiagPre1 = "";
                string DiagDef1 = "";
                string DiagDescripcion2 = "";
                string DiagCIE2 = "";
                string DiagPre2 = "";
                string DiagDef2 = "";
                string DiagDescripcion3 = "";
                string DiagCIE3 = "";
                string DiagPre3 = "";
                string DiagDef3 = "";

                string aptitudApto = "";
                string aptitudObservacion = "";
                string aptitudLimitaciones = "";
                string aptitudNoapto = "";
                string aptitudObservacionDesc = "";
                string aptitudLimitacionDesc = "";
                string descRecomendaciones = "";

                string primerNombreDoc = "";
                string segundoNombreDoc = "";
                string primerApellidoDoc = "";
                string segundoApellidoDoc = "";

                string actividades1 = "";
                string actividades2 = "";
                string actividades3 = "";
                string EvalRetiroSi = "";
                string EvalRetiroNo = "";
                string EvalRetiroObservacion = "";


                string fecEmision = "";
                string evaIngreso = "";
                string evaPeriodico = "";
                string evaReintegro = "";
                string evaRetiro = "";

                string retiroSi = "";
                string retiroNo = "";
                string diagPresuntiva = "";
                string diagDefinitiva = "";
                string diagNoAplica = "";
                string relacionSi = "";
                string relacionNo = "";
                string relacionNoAplica = "";

                string aptitudAptoDesc = "";

                string descAptitud = "";


                var info = new DataTable();


                // Guardamos los datos obtenidos del JSON generado en Departamento.js y los guardamos en las variables previamente creadas.
                var nombreCompleto = campos["txtNombre"];

                // Separar el nombre completo en palabras utilizando el espacio como delimitador
                var palabras = nombreCompleto.Split(' ');

                // Asegurarse de que haya al menos cuatro palabras antes de acceder a los índices
                primerApellido = palabras.Length > 0 ? palabras[0] : "";
                segundoApellido = palabras.Length > 1 ? palabras[1] : "";
                primerNombre = palabras.Length > 2 ? palabras[2] : "";
                segundoNombre = palabras.Length > 3 ? palabras[3] : "";

                if (campos["txtSexo"] == "Femenino" || campos["txtSexo"] == "FEMENINO")
                {
                    sexo = "F";
                }
                else if (campos["txtSexo"] == "Masculino" || campos["txtSexo"] == "MASCULINO")
                {
                    sexo = "M";
                }

                if (campos["formulario"] == "5")
                {
                    puestotrabajo = campos["txtPuestoTrabajo"];
                    fecEmision = campos["txtFechaEmision"];

                    switch (campos["txtEvaluacion"])
                    {
                        case "Ingreso":
                            evaIngreso = "X";
                            break;
                        case "Periódico":
                            evaPeriodico = "X";
                            break;
                        case "Reintegro":
                            evaReintegro = "X";
                            break;
                        case "Retiro":
                            evaRetiro = "X";
                            break;
                        default:
                            break;
                    }
                }

                edad = campos["txtEdad"];

                religion = campos["txtReligion"];

                switch (religion)
                {
                    case "catolica":
                        catolica = "X";
                        break;
                    case "evangelica":
                        evangelica = "X";
                        break;
                    case "testigo":
                        testigo = "X";
                        break;
                    case "mormona":
                        mormona = "X";
                        break;
                    case "otra":
                        otrareligion = "X";
                        break;
                    case "":
                        break;
                    default:
                        break;
                }

                gruposanguineo = campos["txtGruposanguineo"];
                lateralidad = campos["txtLateralidad"];
                orientacion = campos["txtOrientacionSexual"];

                switch (orientacion)
                {
                    case "lesbiana":
                        orientlesbiana = "X";
                        break;
                    case "gay":
                        orientgay = "X";
                        break;
                    case "bisexual":
                        orientbisexual = "X";
                        break;
                    case "heterosexual":
                        orientheterosexual = "X";
                        break;
                    case "nosabe":
                        orientnosabe = "X";
                        break;
                    default:
                        // Opción por defecto si no coincide con ninguna de las anteriores
                        break;
                }

                identidad = campos["txtIdentidadGenero"];
                switch (identidad)
                {
                    case "identfemenino":
                        identfemenino = "X";
                        break;
                    case "identmasculino":
                        identmasculino = "X";
                        break;
                    case "identransfem":
                        identransfem = "X";
                        break;
                    case "identransmasc":
                        identransmasc = "X";
                        break;
                    case "identnosabe":
                        identnosabe = "X";
                        break;
                    default:

                        break;
                }

                if (campos["txtDiscapacidad"] == "SI")
                {
                    discapacidadsi = "X";
                }
                else if (campos["txtDiscapacidad"] == "NO")
                {
                    discapacidadno = "X";
                }
                discapacidadtipo = campos["txtTipoDiscapacidad"];
                discapacidadporcentaje = campos["txtPorcentajeDiscapacidad"];
                motivoconsulta = campos["txtMotivoConsulta"];
                antpersonales = campos["txtAntecedentesPersonales"];
                menarquia = campos["txtMenarquia"];
                ciclos = campos["txtCiclos"];
                ultmenstruacion = campos["fechaUltimaMens"];
                gestas = campos["txtNumGestas"];
                partos = campos["txtNumPartos"];
                cesareas = campos["txtNumCesareas"];
                abortos = campos["txtNumAbortos"];
                hijosvivosF = campos["txtNumHijosVivos"];
                hijosmuertosF = campos["txtNumHijosMuertos"];

                if (campos["txtvidaSxActiva"] == "SI")
                {
                    vidaSxActivaSi = "X";
                }
                else if (campos["txtvidaSxActiva"] == "NO")
                {
                    vidaSxActivaNo = "X";
                }
                if (campos["txtPlanificacionFam1"] == "SI")
                {
                    metodoplanfamSi1 = "X";
                }
                else if (campos["txtPlanificacionFam1"] == "NO")
                {
                    metodoplanfamNo1 = "X";
                }
                metodoplanfamTipo1 = campos["txtTipoPlanificacion1"];
                if (campos["txtExam1"] == "SI")
                {
                    papnicolaouSi = "X";
                }
                else if (campos["txtExam1"] == "NO")
                {
                    papnicolaouNo = "X";
                }
                papnicolaouTiempo = campos["txtanioExam1"];
                papnicolaouResultado = campos["txtResultadoExam1"];
                if (campos["txtExam2"] == "SI")
                {
                    ecomamaSi = "X";
                }
                else if (campos["txtExam2"] == "NO")
                {
                    ecomamaNo = "X";
                }
                ecomamaTiempo = campos["txtanioExam2"];
                ecomamaResultado = campos["txtResultadoExam2"];
                if (campos["txtExam3"] == "SI")
                {
                    colposcopiaSi = "X";
                }
                else if (campos["txtExam3"] == "NO")
                {
                    colposcopiaNo = "X";
                }
                colposcopiaTiempo = campos["txtanioExam3"];
                colposcopiaResultado = campos["txtResultadoExam3"];
                if (campos["txtExam4"] == "SI")
                {
                    mamografiaSi = "X";
                }
                else if (campos["txtExam4"] == "NO")
                {
                    mamografiaNo = "X";
                }
                mamografiaTiempo = campos["txtanioExam4"];
                mamografiaResultado = campos["txtResultadoExam4"];
                if (campos["txtExam5"] == "SI")
                {
                    antigenoprostSi = "X";
                }
                else if (campos["txtExam5"] == "NO")
                {
                    antigenoprostNo = "X";
                }
                antigenoprostTiempo = campos["txtanioExam5"];
                antigenoprostResultado = campos["txtResultadoExam5"];
                if (campos["txtExam6"] == "SI")
                {
                    ecoprostaticoSi = "X";
                }
                else if (campos["txtExam6"] == "NO")
                {
                    ecoprostaticoNo = "X";
                }
                ecoprostaticoTiempo = campos["txtanioExam6"];
                ecoprostaticoResultado = campos["txtResultadoExam6"];
                if (campos["txtPlanificacionFam2"] == "SI")
                {
                    metodoplanfamSi2 = "X";
                }
                else if (campos["txtPlanificacionFam2"] == "NO")
                {
                    metodoplanfamNo2 = "X";
                }
                metodoplanfamTipo2 = campos["txtTipoPlanificacion2"];
                hijosvivosM = campos["txtNumHijosVivosM"];
                hijosmuertosM = campos["txtNumHijosMuertosM"];
                if (campos["txtTabacoSelect"] == "si")
                {
                    tabacoSi = "X";
                }
                else if (campos["txtTabacoSelect"] == "no")
                {
                    tabacoNo = "X";
                }
                tabacoTiempo = campos["txtTiempoconsumoTabaco"];
                tabacoCantidad = campos["txtCantidadTabaco"];
                tabacoExconsumidor = campos["exConsumidoraSelectTabaco"];
                tabacoTiempoabst = campos["txtTiempoAbstinenciaTabaco"];
                if (campos["txtalcoholSelect"] == "si")
                {
                    alcoholSi = "X";
                }
                else if (campos["txtalcoholSelect"] == "no")
                {
                    alcoholNo = "X";
                }
                alcoholTiempo = campos["txtTiempoconsumoAlcohol"];
                alcoholCantidad = campos["txtCantidadAlcohol"];
                alcoholExconsumidor = campos["exConsumidoraSelectAlcohol"];
                alcoholTiempoabst = campos["txtTiempoAbstinenciaAlcohol"];
                nombreOtrasDrogas = campos["txtOtraSustancia"];
                if (campos["txtotraSelect"] == "si")
                {
                    otrasdrogasSi = "X";
                }
                else if (campos["txtotraSelect"] == "no")
                {
                    otrasdrogasNo = "X";
                }
                otrasdrogasTiempo = campos["txtTiempoconsumoOtra"];
                otrasdrogasCantidad = campos["txtCantidadOtra"];
                otrasdrogasExconsumidor = campos["exConsumidorSelectOtra"];
                otrasdrogasTiempoabst = campos["txtTiempoAbstinenciaOtra"];
                if (campos["txtActividadFisiscaSelect"] == "si")
                {
                    actividadfisicaSi = "X";
                }
                else if (campos["txtActividadFisiscaSelect"] == "no")
                {
                    actividadfisicaNo = "X";
                }
                actividadfisicaCual = campos["txtCualActividad"];
                tiempoActividadFisica = campos["txtFrecuenciaActividad"];
                if (campos["txtMedicacionHabSelect"] == "si")
                {
                    medicacionhabSi = "X";
                }
                else if (campos["txtMedicacionHabSelect"] == "no")
                {
                    medicacionhabNo = "X";
                }
                medicacionhabCual1 = campos["txtCualMedicamento1"];
                medicacionhabCual2 = campos["txtCualMedicamento2"];
                medicacionhabCual3 = campos["txtCualMedicamento3"];
                cantidadMedicamento1 = campos["txtCantdidadMed1"];
                cantidadMedicamento2 = campos["txtCantdidadMed2"];
                cantidadMedicamento3 = campos["txtCantdidadMed3"];

                antempresa1 = campos["txtAntEmpresa1"];
                antpuestotrabajo1 = campos["txtAntPuestoTrabajo1"];
                antactividad1 = campos["txtAntActividades1"];
                anttiempotrabajo1 = campos["txtAntTiempoTrabajo1"];

                antriesgo = campos["riesgoTrabajoSelect1"];
                switch (antriesgo)
                {
                    case "antriesgofisico":
                        antriesgofisico1 = "X";
                        break;
                    case "antriesgomecanico":
                        antriesgomecanico1 = "X";
                        break;
                    case "antriesgoquimico":
                        antriesgoquimico1 = "X";
                        break;
                    case "antriesgobiologico":
                        antriesgobiologico1 = "X";
                        break;
                    case "antriesgoergonomico":
                        antriesgoergonomico1 = "X";
                        break;
                    case "antriesgopsicosocial":
                        antriesgopsicosocial1 = "X";
                        break;
                    default:
                        break;
                }
                antobservaciones1 = campos["txtObservacion1"];

                antempresa2 = campos["txtAntEmpresa2"];
                antpuestotrabajo2 = campos["txtAntPuestoTrabajo2"];
                antactividad2 = campos["txtAntActividades2"];
                anttiempotrabajo2 = campos["txtAntTiempoTrabajo2"];
                antriesgo2 = campos["riesgoTrabajoSelect2"];
                switch (antriesgo2)
                {
                    case "antriesgofisico":
                        antriesgofisico2 = "X";
                        break;
                    case "antriesgomecanico":
                        antriesgomecanico2 = "X";
                        break;
                    case "antriesgoquimico":
                        antriesgoquimico2 = "X";
                        break;
                    case "antriesgobiologico":
                        antriesgobiologico2 = "X";
                        break;
                    case "antriesgoergonomico":
                        antriesgoergonomico2 = "X";
                        break;
                    case "antriesgopsicosocial":
                        antriesgopsicosocial2 = "X";
                        break;
                    default:
                        break;
                }
                antobservaciones2 = campos["txtObservacion2"];

                antempresa3 = campos["txtAntEmpresa3"];
                antpuestotrabajo3 = campos["txtAntPuestoTrabajo3"];
                antactividad3 = campos["txtAntActividades3"];
                anttiempotrabajo3 = campos["txtAntTiempoTrabajo3"];
                antriesgo3 = campos["riesgoTrabajoSelect3"];
                switch (antriesgo3)
                {
                    case "antriesgofisico":
                        antriesgofisico3 = "X";
                        break;
                    case "antriesgomecanico":
                        antriesgomecanico3 = "X";
                        break;
                    case "antriesgoquimico":
                        antriesgoquimico3 = "X";
                        break;
                    case "antriesgobiologico":
                        antriesgobiologico3 = "X";
                        break;
                    case "antriesgoergonomico":
                        antriesgoergonomico3 = "X";
                        break;
                    case "antriesgopsicosocial":
                        antriesgopsicosocial3 = "X";
                        break;
                    default:
                        break;
                }
                antobservaciones3 = campos["txtObservacion3"];

                antempresa4 = campos["txtAntEmpresa4"];
                antpuestotrabajo4 = campos["txtAntPuestoTrabajo4"];
                antactividad4 = campos["txtAntActividades4"];
                anttiempotrabajo4 = campos["txtAntTiempoTrabajo4"];
                antriesgo4 = campos["riesgoTrabajoSelect4"];
                switch (antriesgo4)
                {
                    case "antriesgofisico":
                        antriesgofisico4 = "X";
                        break;
                    case "antriesgomecanico":
                        antriesgomecanico4 = "X";
                        break;
                    case "antriesgoquimico":
                        antriesgoquimico4 = "X";
                        break;
                    case "antriesgobiologico":
                        antriesgobiologico4 = "X";
                        break;
                    case "antriesgoergonomico":
                        antriesgoergonomico4 = "X";
                        break;
                    case "antriesgopsicosocial":
                        antriesgopsicosocial4 = "X";
                        break;
                    default:
                        break;
                }
                antobservaciones4 = campos["txtObservacion4"];

                if (campos["AccidentesTrabajoSelect"] == "si")
                {
                    accidentestrabajoSi = "X";
                }
                else if (campos["AccidentesTrabajoSelect"] == "no")
                {
                    accidentestrabajoNo = "X";
                }
                especificaracctrabajo = campos["txtEspecificarAccidentesTrabajoSelect"];
                acctrabajoobservaciones = campos["txtObservacionesAccTrabajo"];
                if (campos["EnfermedadesProfSelect"] == "si")
                {
                    enfermedadesprofSi = "X";
                }
                else if (campos["EnfermedadesProfSelect"] == "no")
                {
                    enfermedadesprofNo = "X";
                }
                especificarenfermedadesprof = campos["txtEspecificarEnfermedadesProfSelect"];
                enfermedadesprofobservaciones = campos["txtObservacionesEnfermedadesProf"];

                antfamiliares = campos["txtAntecedentesFamiliares"];
                actextralaborales = campos["txtActividadesExtraLaborales"];
                enfermedadactual = campos["txtEnfermedadActual"];


                factoresriesgopuestotrabajo1 = campos["txtPuestoTrabajo1"];
                factoresriesgoactividades1 = campos["txtActividadesTrabajo1"];
                switch (campos["txtFisicoSelect1"])
                {
                    case "TemperaturasAltas":
                        fisicotempaltas1 = "X";
                        break;
                    case "TemperaturasBajas":
                        fisicotempbajas1 = "X";
                        break;
                    case "RadiacionIonizante":
                        fisicoionizante1 = "X";
                        break;
                    case "RadiaciónNoIonizante":
                        fisicoNoionizante1 = "X";
                        break;
                    case "Ruido":
                        fisicoruido1 = "X";
                        break;
                    case "Vibracion":
                        fisicovibracion1 = "X";
                        break;
                    case "Iluminacion":
                        fisicoiluminacion1 = "X";
                        break;
                    case "Ventilacion":
                        fisicoventilacion1 = "X";
                        break;
                    case "FluidoElectrico":
                        fisicoelectrico1 = "X";
                        break;
                    case "OtrosFisico":
                        fisicootros1 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtMecanicoSelect1"])
                {
                    case "AtrapamientoMaquinas":
                        mecatrapmaquinas1 = "X";
                        break;
                    case "AtrapamientoSuperficies":
                        mecatrapsuperficies1 = "X";
                        break;
                    case "AtrapamientoObjetos":
                        mecatrapobjetos1 = "X";
                        break;
                    case "CaidaObjetos":
                        meccaidasdeobjetos1 = "X";
                        break;
                    case "CaidasMismoNivel":
                        meccaidasmismonivel1 = "X";
                        break;
                    case "CaidasDiferenteNivel":
                        meccaidasdiferentenivel1 = "X";
                        break;
                    case "ContactoElectrico":
                        meccontactoelectrico1 = "X";
                        break;
                    case "ContactoSuperficiesTrabajos":
                        meccontacosuperficies1 = "X";
                        break;
                    case "ProyeccionPartículas":
                        mecproyeccionparticulas1 = "X";
                        break;
                    case "ProyeccionFluidos":
                        mecproyeccionfluidos1 = "X";
                        break;
                    case "Pinchazos":
                        mecpinchazos1 = "X";
                        break;
                    case "Cortes":
                        meccortes1 = "X";
                        break;
                    case "AtropellamientoVehículo":
                        mecatropellamientovehiculo1 = "X";
                        break;
                    case "ChoquesVehicular":
                        mecchoquescolision1 = "X";
                        break;
                    case "OtrosMecanico":
                        mecanicootros1 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtQuimicoSelect1"])
                {
                    case "Solidos":
                        quimicosolidos1 = "X";
                        break;
                    case "Polvos":
                        quimicopolvos1 = "X";
                        break;
                    case "Humos":
                        quimicohumos1 = "X";
                        break;
                    case "liquidos":
                        quimicoliquidos1 = "X";
                        break;
                    case "vapores":
                        quimicovapores1 = "X";
                        break;
                    case "Aerosoles":
                        quimicoaerosoles1 = "X";
                        break;
                    case "Neblinas":
                        quimiconeblinas1 = "X";
                        break;
                    case "Gaseosos":
                        quimicogaseosos1 = "X";
                        break;
                    case "OtrosQuimico":
                        quimicootros1 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtBiologicoSelect1"])
                {
                    case "Virus":
                        biologicovirus1 = "X";
                        break;
                    case "Hongos":
                        biologicohongos1 = "X";
                        break;
                    case "Bacterias":
                        biologicobacterias1 = "X";
                        break;
                    case "Parasitos":
                        biologicoparasitos1 = "X";
                        break;
                    case "ExposicionVectores":
                        biologicoexpovectores1 = "X";
                        break;
                    case "ExposicionAnimales":
                        biologicoexpoanimales1 = "X";
                        break;
                    case "OtrosBiologico":
                        biologicootros1 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtErgonomicoSelect1"])
                {
                    case "ManejoCargas":
                        ergonomicomanejomanual1 = "X";
                        break;
                    case "MovimientoRepetitivos":
                        ergonomicomovimientorep1 = "X";
                        break;
                    case "PosturasForzadas":
                        ergonomicoposturasforzadas1 = "X";
                        break;
                    case "TrabajosPVD":
                        ergonomicotrabajopvd1 = "X";
                        break;
                    case "OtrosErgonomico":
                        ergonomicootros1 = "X";
                        break;
                    default:
                        break;
                }

                switch (campos["txtPSicosocialSelect1"])
                {
                    case "MonotoniaTrabajo ":
                        psicosocialmonotonia1 = "X";
                        break;
                    case "SobrecargaLaboral":
                        psicosocialsobrecarga1 = "X";
                        break;
                    case "MinuciosidadTarea":
                        psicosocialminuciosidad1 = "X";
                        break;
                    case "AltaResponsabilidad":
                        psicosocialaltaresponsabilidad1 = "X";
                        break;
                    case "AutonomiaDecisiones":
                        psicosocialautonomia1 = "X";
                        break;
                    case "SupervisionDeficiente":
                        psicosocialsupervision1 = "X";
                        break;
                    case "ConflictoRol":
                        psicosocialconflicto1 = "X";
                        break;
                    case "FaltaClaridadFunciones":
                        psicosocialfaltaclaridad1 = "X";
                        break;
                    case "IncorrectaDistribuciónTrabajo":
                        psicosocialincorrecta1 = "X";
                        break;
                    case "TurnosRotativos":
                        psicosocialturnos1 = "X";
                        break;
                    case "RelacionesInterpersonales":
                        psicosocialrelaciones1 = "X";
                        break;
                    case "InestabilidadLaboral":
                        psicosocialinestabilidad1 = "X";
                        break;
                    case "OtrosPSicosocial":
                        psicosocialotros1 = "X";
                        break;
                    default:
                        break;
                }
                medidaspreventivas1 = campos["txtMedidadPreventiva1"];


                factoresriesgopuestotrabajo2 = campos["txtPuestoTrabajo2"];
                factoresriesgoactividades2 = campos["txtActividadesTrabajo2"];
                switch (campos["txtFisicoSelect2"])
                {
                    case "TemperaturasAltas":
                        fisicotempaltas2 = "X";
                        break;
                    case "TemperaturasBajas":
                        fisicotempbajas2 = "X";
                        break;
                    case "RadiacionIonizante":
                        fisicoionizante2 = "X";
                        break;
                    case "RadiaciónNoIonizante":
                        fisicoNoionizante2 = "X";
                        break;
                    case "Ruido":
                        fisicoruido2 = "X";
                        break;
                    case "Vibracion":
                        fisicovibracion2 = "X";
                        break;
                    case "Iluminacion":
                        fisicoiluminacion2 = "X";
                        break;
                    case "Ventilacion":
                        fisicoventilacion2 = "X";
                        break;
                    case "FluidoElectrico":
                        fisicoelectrico2 = "X";
                        break;
                    case "OtrosFisico":
                        fisicootros2 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtMecanicoSelect2"])
                {
                    case "AtrapamientoMaquinas":
                        mecatrapmaquinas2 = "X";
                        break;
                    case "AtrapamientoSuperficies":
                        mecatrapsuperficies2 = "X";
                        break;
                    case "AtrapamientoObjetos":
                        mecatrapobjetos2 = "X";
                        break;
                    case "CaidaObjetos":
                        meccaidasdeobjetos2 = "X";
                        break;
                    case "CaidasMismoNivel":
                        meccaidasmismonivel2 = "X";
                        break;
                    case "CaidasDiferenteNivel":
                        meccaidasdiferentenivel2 = "X";
                        break;
                    case "ContactoElectrico":
                        meccontactoelectrico2 = "X";
                        break;
                    case "ContactoSuperficiesTrabajos":
                        meccontacosuperficies2 = "X";
                        break;
                    case "ProyeccionPartículas":
                        mecproyeccionparticulas2 = "X";
                        break;
                    case "ProyeccionFluidos":
                        mecproyeccionfluidos2 = "X";
                        break;
                    case "Pinchazos":
                        mecpinchazos2 = "X";
                        break;
                    case "Cortes":
                        meccortes2 = "X";
                        break;
                    case "AtropellamientoVehículo":
                        mecatropellamientovehiculo2 = "X";
                        break;
                    case "ChoquesVehicular":
                        mecchoquescolision2 = "X";
                        break;
                    case "OtrosMecanico":
                        mecanicootros2 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtQuimicoSelect2"])
                {
                    case "Solidos":
                        quimicosolidos2 = "X";
                        break;
                    case "Polvos":
                        quimicopolvos2 = "X";
                        break;
                    case "Humos":
                        quimicohumos2 = "X";
                        break;
                    case "liquidos":
                        quimicoliquidos2 = "X";
                        break;
                    case "vapores":
                        quimicovapores2 = "X";
                        break;
                    case "Aerosoles":
                        quimicoaerosoles2 = "X";
                        break;
                    case "Neblinas":
                        quimiconeblinas2 = "X";
                        break;
                    case "Gaseosos":
                        quimicogaseosos2 = "X";
                        break;
                    case "OtrosQuimico":
                        quimicootros2 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtBiologicoSelect2"])
                {
                    case "Virus":
                        biologicovirus2 = "X";
                        break;
                    case "Hongos":
                        biologicohongos2 = "X";
                        break;
                    case "Bacterias":
                        biologicobacterias2 = "X";
                        break;
                    case "Parasitos":
                        biologicoparasitos2 = "X";
                        break;
                    case "ExposicionVectores":
                        biologicoexpovectores2 = "X";
                        break;
                    case "ExposicionAnimales":
                        biologicoexpoanimales2 = "X";
                        break;
                    case "OtrosBiologico":
                        biologicootros2 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtErgonomicoSelect2"])
                {
                    case "ManejoCargas":
                        ergonomicomanejomanual2 = "X";
                        break;
                    case "MovimientoRepetitivos":
                        ergonomicomovimientorep2 = "X";
                        break;
                    case "PosturasForzadas":
                        ergonomicoposturasforzadas2 = "X";
                        break;
                    case "TrabajosPVD":
                        ergonomicotrabajopvd2 = "X";
                        break;
                    case "OtrosErgonomico":
                        ergonomicootros2 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtPSicosocialSelect2"])
                {
                    case "MonotoniaTrabajo":
                        psicosocialmonotonia2 = "X";
                        break;
                    case "SobrecargaLaboral":
                        psicosocialsobrecarga2 = "X";
                        break;
                    case "MinuciosidadTarea":
                        psicosocialminuciosidad2 = "X";
                        break;
                    case "AltaResponsabilidad":
                        psicosocialaltaresponsabilidad2 = "X";
                        break;
                    case "AutonomiaDecisiones":
                        psicosocialautonomia2 = "X";
                        break;
                    case "SupervisionDeficiente":
                        psicosocialsupervision2 = "X";
                        break;
                    case "ConflictoRol":
                        psicosocialconflicto2 = "X";
                        break;
                    case "FaltaClaridadFunciones":
                        psicosocialfaltaclaridad2 = "X";
                        break;
                    case "IncorrectaDistribuciónTrabajo":
                        psicosocialincorrecta2 = "X";
                        break;
                    case "TurnosRotativos":
                        psicosocialturnos2 = "X";
                        break;
                    case "RelacionesInterpersonales":
                        psicosocialrelaciones2 = "X";
                        break;
                    case "InestabilidadLaboral":
                        psicosocialinestabilidad2 = "X";
                        break;
                    case "OtrosPSicosocial":
                        psicosocialotros2 = "X";
                        break;
                    default:
                        break;
                }
                medidaspreventivas2 = campos["txtMedidadPreventiva2"];

                factoresriesgopuestotrabajo3 = campos["txtPuestoTrabajo3"];
                factoresriesgoactividades3 = campos["txtActividadesTrabajo3"];
                switch (campos["txtFisicoSelect3"])
                {
                    case "TemperaturasAltas":
                        fisicotempaltas3 = "X";
                        break;
                    case "TemperaturasBajas":
                        fisicotempbajas3 = "X";
                        break;
                    case "RadiacionIonizante":
                        fisicoionizante3 = "X";
                        break;
                    case "RadiaciónNoIonizante":
                        fisicoNoionizante3 = "X";
                        break;
                    case "Ruido":
                        fisicoruido3 = "X";
                        break;
                    case "Vibracion":
                        fisicovibracion3 = "X";
                        break;
                    case "Iluminacion":
                        fisicoiluminacion3 = "X";
                        break;
                    case "Ventilacion":
                        fisicoventilacion3 = "X";
                        break;
                    case "FluidoElectrico":
                        fisicoelectrico3 = "X";
                        break;
                    case "OtrosFisico":
                        fisicootros3 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtMecanicoSelect3"])
                {
                    case "AtrapamientoMaquinas":
                        mecatrapmaquinas3 = "X";
                        break;
                    case "AtrapamientoSuperficies":
                        mecatrapsuperficies3 = "X";
                        break;
                    case "AtrapamientoObjetos":
                        mecatrapobjetos3 = "X";
                        break;
                    case "CaidaObjetos":
                        meccaidasdeobjetos3 = "X";
                        break;
                    case "CaidasMismoNivel":
                        meccaidasmismonivel3 = "X";
                        break;
                    case "CaidasDiferenteNivel":
                        meccaidasdiferentenivel3 = "X";
                        break;
                    case "ContactoElectrico":
                        meccontactoelectrico3 = "X";
                        break;
                    case "ContactoSuperficiesTrabajos":
                        meccontacosuperficies3 = "X";
                        break;
                    case "ProyeccionPartículas":
                        mecproyeccionparticulas3 = "X";
                        break;
                    case "ProyeccionFluidos":
                        mecproyeccionfluidos3 = "X";
                        break;
                    case "Pinchazos":
                        mecpinchazos3 = "X";
                        break;
                    case "Cortes":
                        meccortes3 = "X";
                        break;
                    case "AtropellamientoVehículo":
                        mecatropellamientovehiculo3 = "X";
                        break;
                    case "ChoquesVehicular":
                        mecchoquescolision3 = "X";
                        break;
                    case "OtrosMecanico":
                        mecanicootros3 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtQuimicoSelect3"])
                {
                    case "Solidos":
                        quimicosolidos3 = "X";
                        break;
                    case "Polvos":
                        quimicopolvos3 = "X";
                        break;
                    case "Humos":
                        quimicohumos3 = "X";
                        break;
                    case "liquidos":
                        quimicoliquidos3 = "X";
                        break;
                    case "vapores":
                        quimicovapores3 = "X";
                        break;
                    case "Aerosoles":
                        quimicoaerosoles3 = "X";
                        break;
                    case "Neblinas":
                        quimiconeblinas3 = "X";
                        break;
                    case "Gaseosos":
                        quimicogaseosos3 = "X";
                        break;
                    case "OtrosQuimico":
                        quimicootros3 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtBiologicoSelect3"])
                {
                    case "Virus":
                        biologicovirus3 = "X";
                        break;
                    case "Hongos":
                        biologicohongos3 = "X";
                        break;
                    case "Bacterias":
                        biologicobacterias3 = "X";
                        break;
                    case "Parasitos":
                        biologicoparasitos3 = "X";
                        break;
                    case "ExposicionVectores":
                        biologicoexpovectores3 = "X";
                        break;
                    case "ExposicionAnimales":
                        biologicoexpoanimales3 = "X";
                        break;
                    case "OtrosBiologico":
                        biologicootros3 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtErgonomicoSelect3"])
                {
                    case "ManejoCargas":
                        ergonomicomanejomanual3 = "X";
                        break;
                    case "MovimientoRepetitivos":
                        ergonomicomovimientorep3 = "X";
                        break;
                    case "PosturasForzadas":
                        ergonomicoposturasforzadas3 = "X";
                        break;
                    case "TrabajosPVD":
                        ergonomicotrabajopvd3 = "X";
                        break;
                    case "OtrosErgonomico":
                        ergonomicootros3 = "X";
                        break;
                    default:
                        break;
                }
                switch (campos["txtPSicosocialSelect3"])
                {
                    case "MonotoniaTrabajo":
                        psicosocialmonotonia3 = "X";
                        break;
                    case "SobrecargaLaboral":
                        psicosocialsobrecarga3 = "X";
                        break;
                    case "MinuciosidadTarea":
                        psicosocialminuciosidad3 = "X";
                        break;
                    case "AltaResponsabilidad":
                        psicosocialaltaresponsabilidad3 = "X";
                        break;
                    case "AutonomiaDecisiones":
                        psicosocialautonomia3 = "X";
                        break;
                    case "SupervisionDeficiente":
                        psicosocialsupervision3 = "X";
                        break;
                    case "ConflictoRol":
                        psicosocialconflicto3 = "X";
                        break;
                    case "FaltaClaridadFunciones":
                        psicosocialfaltaclaridad3 = "X";
                        break;
                    case "IncorrectaDistribuciónTrabajo":
                        psicosocialincorrecta3 = "X";
                        break;
                    case "TurnosRotativos":
                        psicosocialturnos3 = "X";
                        break;
                    case "RelacionesInterpersonales":
                        psicosocialrelaciones3 = "X";
                        break;
                    case "InestabilidadLaboral":
                        psicosocialinestabilidad3 = "X";
                        break;
                    case "OtrosPSicosocial":
                        psicosocialotros3 = "X";
                        break;
                    default:
                        break;
                }
                medidaspreventivas3 = campos["txtMedidadPreventiva3"];

                switch (campos["txtPatologia"])
                {
                    case "1":
                        revisionpiel = "X";
                        break;
                    case "2":
                        revisionsentidos = "X";
                        break;
                    case "3":
                        revisionrespiratorio = "X";
                        break;
                    case "4":
                        revisioncardio = "X";
                        break;
                    case "5":
                        revisiondigestivo = "X";
                        break;
                    case "6":
                        revisiongenito = "X";
                        break;
                    case "7":
                        revisionmusculo = "X";
                        break;
                    case "8":
                        revisionendocrino = "X";
                        break;
                    case "9":
                        revisionhemo = "X";
                        break;
                    case "10":
                        revisionnervioso = "X";
                        break;
                    default:
                        break;
                }

                revisionorganosdescripcion = campos["txtRevisionOrganos"];

                constpresion = campos["txtConstPresionArterial"];
                consttemperatura = campos["txtConstTemperatura"];
                constfreccardiaca = campos["txtConstFrecuenciaCardicaca"];
                constsaturacion = campos["txtConstSaturacionOxigeno"];
                constfrecrespiratoria = campos["txtConstFrecuenciaRespiratoria"];
                constpeso = campos["txtConstPeso"];
                consttalla = campos["txtConstTalla"];
                constindice = campos["txtConstMasaCorporal"];
                constperimetro = campos["txtConstPerimetroAbdominal"];

                if (campos["formulario"] == "1" || campos["formulario"] == "2" || campos["formulario"] == "3" || campos["formulario"] == "4")
                {

                    if (campos["txtpielA"] == true)
                    {
                        examfisicopielA = "X";
                    }
                    if (campos["txtpielB"] == true)
                    {
                        examfisicopielB = "X";
                    }
                    if (campos["txtpielC"] == true)
                    {
                        examfisicopielC = "X";
                    }
                    if (campos["txtojosA"] == true)
                    {
                        examfisicoojosA = "X";
                    }
                    if (campos["txtojosB"] == true)
                    {
                        examfisicoojosB = "X";
                    }
                    if (campos["txtojosC"] == true)
                    {
                        examfisicoojosC = "X";
                    }
                    if (campos["txtojosD"] == true)
                    {
                        examfisicoojosD = "X";
                    }
                    if (campos["txtojosE"] == true)
                    {
                        examfisicoojosE = "X";
                    }
                    if (campos["txtoidoA"] == true)
                    {
                        examfisicooidoA = "X";
                    }
                    if (campos["txtoidoB"] == true)
                    {
                        examfisicooidoB = "X";
                    }
                    if (campos["txtoidoC"] == true)
                    {
                        examfisicooidoC = "X";
                    }
                    if (campos["txtoroA"] == true)
                    {
                        examfisicooroA = "X";
                    }
                    if (campos["txtoroB"] == true)
                    {
                        examfisicooroB = "X";
                    }
                    if (campos["txtoroC"] == true)
                    {
                        examfisicooroC = "X";
                    }
                    if (campos["txtoroD"] == true)
                    {
                        examfisicooroD = "X";
                    }
                    if (campos["txtoroE"] == true)
                    {
                        examfisicooroE = "X";
                    }
                    if (campos["txtnarizA"] == true)
                    {
                        examfisiconarizA = "X";
                    }
                    if (campos["txtnarizB"] == true)
                    {
                        examfisiconarizB = "X";
                    }
                    if (campos["txtnarizC"] == true)
                    {
                        examfisiconarizC = "X";
                    }
                    if (campos["txtnarizD"] == true)
                    {
                        examfisiconarizD = "X";
                    }
                    if (campos["txtcuelloA"] == true)
                    {
                        examfisicocuelloA = "X";
                    }
                    if (campos["txtcuelloB"] == true)
                    {
                        examfisicocuelloB = "X";
                    }
                    if (campos["txttoraxA"] == true)
                    {
                        examfisicotorax1A = "X";
                    }
                    if (campos["txttoraxB"] == true)
                    {
                        examfisicotorax1B = "X";
                    }
                    if (campos["txttoraxC"] == true)
                    {
                        examfisicotorax2A = "X";
                    }
                    if (campos["txtToraxD"] == true)
                    {
                        examfisicotorax2B = "X";
                    }
                    if (campos["txtabdomenA"] == true)
                    {
                        examfisicoabdomenA = "X";
                    }
                    if (campos["txtabdomenB"] == true)
                    {
                        examfisicoabdomenB = "X";
                    }
                    if (campos["txtcolumnaA"] == true)
                    {
                        examfisicocolumnaA = "X";
                    }
                    if (campos["txtcolumnaB"] == true)
                    {
                        examfisicocolumnaB = "X";
                    }
                    if (campos["txtcolumnaC"] == true)
                    {
                        examfisicocolumnaC = "X";
                    }
                    if (campos["txtpelvisA"] == true)
                    {
                        examfisicopelvisA = "X";
                    }
                    if (campos["txtpelvisB"] == true)
                    {
                        examfisicopelvisB = "X";
                    }
                    if (campos["txtextremidadesA"] == true)
                    {
                        examfisicoextremA = "X";
                    }
                    if (campos["txtextremidadesB"] == true)
                    {
                        examfisicoextremB = "X";
                    }
                    if (campos["txtextremidadesC"] == true)
                    {
                        examfisicoextremC = "X";
                    }
                    if (campos["txtneurologicoA"] == true)
                    {
                        examfisiconeuroA = "X";
                    }
                    if (campos["txtneurologicoB"] == true)
                    {
                        examfisiconeuroB = "X";
                    }
                    if (campos["txtneurologicoC"] == true)
                    {
                        examfisiconeuroC = "X";
                    }
                    if (campos["txtneurologicoD"] == true)
                    {
                        examfisiconeuroD = "X";
                    }
                }
                examfisicoobservacion = campos["txtExamFisicoObservacion"];

                examNom1 = campos["txtNomExamen1"];
                examFecha1 = campos["fechaExam1"];
                examResultado1 = campos["txtResExamen1"];
                examNom2 = campos["txtNomExamen2"];
                examFecha2 = campos["fechaExam2"];
                examResultado2 = campos["txtResExamen2"];
                examNom3 = campos["txtNomExamen3"];
                examFecha3 = campos["fechaExam3"];
                examResultado3 = campos["txtResExamen3"];
                examNom4 = campos["txtNomExamen4"];
                examFecha4 = campos["fechaExam4"];
                examResultado4 = campos["txtResExamen4"];
                examObservaciones = campos["txtExamenbservacion"];

                DiagDescripcion1 = campos["txtDiagDescripcion1"];
                DiagCIE1 = campos["txtDiagCIE1"];


                switch (campos["txtDiagnositicoSelect1"])
                {
                    case "PRE":
                        DiagPre1 = "X";
                        break;
                    case "DEF":
                        DiagDef1 = "X";
                        break;
                    default:
                        break;
                }
                DiagDescripcion2 = campos["txtDiagDescripcion2"];
                DiagCIE2 = campos["txtDiagCIE2"];
                switch (campos["txtDiagnositicoSelect2"])
                {
                    case "PRE":
                        DiagPre2 = "X";
                        break;
                    case "DEF":
                        DiagDef2 = "X";
                        break;
                    default:
                        break;
                }
                DiagDescripcion3 = campos["txtDiagDescripcion3"];
                DiagCIE3 = campos["txtDiagCIE3"];
                switch (campos["txtDiagnositicoSelect3"])
                {
                    case "PRE":
                        DiagPre3 = "X";
                        break;
                    case "DEF":
                        DiagDef3 = "X";
                        break;
                    default:
                        break;
                }

                areatrabajo = campos["txtAreaTrabajo"];

                switch (campos["txtAptitudSelect"])
                {
                    case "apto":
                        aptitudApto = "X";
                        break;
                    case "aptoObservacion":
                        aptitudObservacion = "X";
                        break;
                    case "aptoLimitacion":
                        aptitudLimitaciones = "X";
                        break;
                    case "noApto":
                        aptitudNoapto = "X";
                        break;
                    default:
                        break;
                }
                aptitudObservacionDesc = campos["txtDescObservacion"];
                aptitudLimitacionDesc = campos["txtDescLimitacion"];


                if (campos["formulario"] == "4")
                {
                    actividades1 = campos["actividades1"];
                    actividades2 = campos["actividades2"];
                    actividades3 = campos["actividades3"];

                    switch (campos["EvalRetiroSelect"])
                    {
                        case "si":
                            EvalRetiroSi = "X";
                            break;
                        case "no":
                            EvalRetiroNo = "X";
                            break;
                        default:
                            break;
                    }
                    EvalRetiroObservacion = campos["EvalRetiroObservacion"];
                }

                if (campos["formulario"] == "5")
                {
                    aptitudAptoDesc = campos["txtDescAptitud"];
                    switch (campos["txtAptitudSelect"])
                    {
                        case "apto":
                            aptitudApto = "X";
                            descAptitud = aptitudAptoDesc;
                            break;
                        case "aptoObservacion":
                            aptitudObservacion = "X";
                            descAptitud = aptitudObservacionDesc;
                            break;
                        case "aptoLimitacion":
                            aptitudLimitaciones = "X";
                            descAptitud = aptitudLimitacionDesc;
                            break;
                        case "noApto":
                            aptitudNoapto = "X";
                            descAptitud = aptitudAptoDesc;
                            break;
                        default:
                            break;
                    }

                    switch (campos["txtevalRetiro"])
                    {
                        case "si":
                            retiroSi = "X";
                            break;
                        case "no":
                            retiroNo = "X";
                            break;
                        default:
                            break;
                    }
                    switch (campos["txtdiag"])
                    {
                        case "presuntiva":
                            diagPresuntiva = "X";
                            break;
                        case "definitiva":
                            diagDefinitiva = "X";
                            break;
                        case "noAplica":
                            diagNoAplica = "X";
                            break;
                        default:
                            break;
                    }
                    switch (campos["txtrelacionTrabajo"])
                    {
                        case "si":
                            relacionSi = "X";
                            break;
                        case "no":
                            relacionNo = "X";
                            break;
                        case "noAplica":
                            relacionNoAplica = "X";
                            break;
                        default:
                            break;
                    }
                }
                descRecomendaciones = campos["txtRecomendacion"];


                var nombreDoctor = campos["nombre"];
                // Separar el nombre completo en palabras utilizando el espacio como delimitador
                var doctor = nombreDoctor.Split(' ');
                // Asegurarse de que haya al menos cuatro palabras antes de acceder a los índices
                primerApellidoDoc = doctor.Length > 0 ? doctor[0] : "";
                segundoApellidoDoc = doctor.Length > 1 ? doctor[1] : "";
                primerNombreDoc = doctor.Length > 2 ? doctor[2] : "";
                segundoNombreDoc = doctor.Length > 3 ? doctor[3] : "";

                // Creamos un DataSet para almacenar los datos de la tabla
                var ds = new DataSet();

                // Registramos las etiquetas creadas con los nombres en orden en el formulario de Excel
                if (campos["formulario"] == "1" || campos["formulario"] == "2" || campos["formulario"] == "3" || campos["formulario"] == "4" || campos["formulario"] == "5")
                {
                    info.Columns.Add("numhistoria");
                    info.Columns.Add("numarchivo");
                    info.Columns.Add("primerapellido");
                    info.Columns.Add("segundoapellido");
                    info.Columns.Add("primernombre");
                    info.Columns.Add("segundonombre");
                    info.Columns.Add("sexo");

                    if (campos["formulario"] == "1" || campos["formulario"] == "3")
                    {
                        info.Columns.Add("edad");

                        if (campos["formulario"] == "1")
                        {
                            info.Columns.Add("catolica");
                            info.Columns.Add("evangelica");
                            info.Columns.Add("testigo");
                            info.Columns.Add("mormona");
                            info.Columns.Add("otrareligion");
                            info.Columns.Add("gruposanguineo");
                            info.Columns.Add("lateralidad");
                            info.Columns.Add("orientlesbiana");
                            info.Columns.Add("orientgay");
                            info.Columns.Add("orientbisexual");
                            info.Columns.Add("orientheterosexual");
                            info.Columns.Add("orientnosabe");
                            info.Columns.Add("identfemenino");
                            info.Columns.Add("identmasculino");
                            info.Columns.Add("identransfem");
                            info.Columns.Add("identransmasc");
                            info.Columns.Add("identnosabe");
                            info.Columns.Add("discapacidadsi");
                            info.Columns.Add("discapacidadno");
                            info.Columns.Add("discapacidadtipo");
                            info.Columns.Add("discapacidadporcentaje");
                        }
                    }
                    info.Columns.Add("puestotrabajo");

                    if (campos["formulario"] == "5")
                    {
                        info.Columns.Add("fecEmision");
                        info.Columns.Add("evaIngreso");
                        info.Columns.Add("evaPeriodico");
                        info.Columns.Add("evaReintegro");
                        info.Columns.Add("evaRetiro");

                        info.Columns.Add("aptitudApto");
                        info.Columns.Add("aptitudObservacion");
                        info.Columns.Add("aptitudLimitaciones");
                        info.Columns.Add("aptitudNoapto");

                        info.Columns.Add("descAptitud");

                        info.Columns.Add("retiroSi");
                        info.Columns.Add("retiroNo");

                        info.Columns.Add("diagPresuntiva");
                        info.Columns.Add("diagDefinitiva");
                        info.Columns.Add("diagNoAplica");

                        info.Columns.Add("relacionSi");
                        info.Columns.Add("relacionNo");
                        info.Columns.Add("relacionNoAplica");
                    }
                    if (campos["formulario"] == "1")
                    {
                        info.Columns.Add("areatrabajo");
                    }
                    if (campos["formulario"] == "1" || campos["formulario"] == "2" || campos["formulario"] == "3")
                    {
                        info.Columns.Add("motivoconsulta");
                    }

                    if (campos["formulario"] == "1" || campos["formulario"] == "2" || campos["formulario"] == "4")
                    {
                        if (campos["formulario"] == "4")
                        {
                            info.Columns.Add("actividades1");
                            info.Columns.Add("factoresriesgoactividades1");
                            info.Columns.Add("actividades2");
                            info.Columns.Add("factoresriesgoactividades2");
                            info.Columns.Add("actividades3");
                            info.Columns.Add("factoresriesgoactividades3");
                        }
                        info.Columns.Add("antpersonales");
                    }
                    if (campos["formulario"] == "1")
                    {
                        info.Columns.Add("menarquia");
                        info.Columns.Add("ciclos");
                        info.Columns.Add("ultmenstruacion");
                        info.Columns.Add("gestas");
                        info.Columns.Add("partos");
                        info.Columns.Add("cesareas");
                        info.Columns.Add("abortos");
                        info.Columns.Add("hijosvivosF");
                        info.Columns.Add("hijosmuertosF");
                        info.Columns.Add("vidaSxActivaSi");
                        info.Columns.Add("vidaSxActivaNo");
                        info.Columns.Add("metodoplanfamSi1");
                        info.Columns.Add("metodoplanfamNo1");
                        info.Columns.Add("metodoplanfamTipo1");
                        info.Columns.Add("papnicolaouSi");
                        info.Columns.Add("papnicolaouNo");
                        info.Columns.Add("papnicolaouTiempo");
                        info.Columns.Add("papnicolaouResultado");
                        info.Columns.Add("ecomamaSi");
                        info.Columns.Add("ecomamaNo");
                        info.Columns.Add("ecomamaTiempo");
                        info.Columns.Add("ecomamaResultado");
                        info.Columns.Add("colposcopiaSi");
                        info.Columns.Add("colposcopiaNo");
                        info.Columns.Add("colposcopiaTiempo");
                        info.Columns.Add("colposcopiaResultado");
                        info.Columns.Add("mamografiaSi");
                        info.Columns.Add("mamografiaNo");
                        info.Columns.Add("mamografiaTiempo");
                        info.Columns.Add("mamografiaResultado");
                        info.Columns.Add("antigenoprostSi");
                        info.Columns.Add("antigenoprostNo");
                        info.Columns.Add("antigenoprostTiempo");
                        info.Columns.Add("antigenoprostResultado");
                        info.Columns.Add("ecoprostaticoSi");
                        info.Columns.Add("ecoprostaticoNo");
                        info.Columns.Add("ecoprostaticoTiempo");
                        info.Columns.Add("ecoprostaticoResultado");
                        info.Columns.Add("metodoplanfamSi2");
                        info.Columns.Add("metodoplanfamNo2");
                        info.Columns.Add("metodoplanfamTipo2");
                        info.Columns.Add("hijosvivosM");
                        info.Columns.Add("hijosmuertosM");
                    }

                    if (campos["formulario"] == "1" || campos["formulario"] == "2")
                    {
                        info.Columns.Add("tabacoSi");
                        info.Columns.Add("tabacoNo");
                        info.Columns.Add("tabacoTiempo");
                        info.Columns.Add("tabacoCantidad");
                        info.Columns.Add("tabacoExconsumidor");
                        info.Columns.Add("tabacoTiempoabst");
                        info.Columns.Add("actividadfisicaSi");
                        info.Columns.Add("actividadfisicaNo");
                        info.Columns.Add("actividadfisicaCual");
                        info.Columns.Add("tiempoActividadFisica");
                        info.Columns.Add("alcoholSi");
                        info.Columns.Add("alcoholNo");
                        info.Columns.Add("alcoholTiempo");
                        info.Columns.Add("alcoholCantidad");
                        info.Columns.Add("alcoholExconsumidor");
                        info.Columns.Add("alcoholTiempoabst");
                        info.Columns.Add("nombreOtrasDrogas");
                        info.Columns.Add("otrasdrogasSi");
                        info.Columns.Add("otrasdrogasNo");
                        info.Columns.Add("otrasdrogasTiempo");
                        info.Columns.Add("otrasdrogasCantidad");
                        info.Columns.Add("otrasdrogasExconsumidor");
                        info.Columns.Add("otrasdrogasTiempoabst");
                        info.Columns.Add("medicacionhabSi");
                        info.Columns.Add("medicacionhabNo");
                        info.Columns.Add("medicacionhabCual1");
                        info.Columns.Add("medicacionhabCual2");
                        info.Columns.Add("medicacionhabCual3");
                        info.Columns.Add("cantidadMedicamento1");
                        info.Columns.Add("cantidadMedicamento2");
                        info.Columns.Add("cantidadMedicamento3");
                    }

                    if (campos["formulario"] == "1")
                    {
                        info.Columns.Add("antempresa1");
                        info.Columns.Add("antpuestotrabajo1");
                        info.Columns.Add("antactividad1");
                        info.Columns.Add("anttiempotrabajo1");
                        info.Columns.Add("antriesgofisico1");
                        info.Columns.Add("antriesgomecanico1");
                        info.Columns.Add("antriesgoquimico1");
                        info.Columns.Add("antriesgobiologico1");
                        info.Columns.Add("antriesgoergonomico1");
                        info.Columns.Add("antriesgopsicosocial1");
                        info.Columns.Add("antobservaciones1");

                        info.Columns.Add("antempresa2");
                        info.Columns.Add("antpuestotrabajo2");
                        info.Columns.Add("antactividad2");
                        info.Columns.Add("anttiempotrabajo2");
                        info.Columns.Add("antriesgofisico2");
                        info.Columns.Add("antriesgomecanico2");
                        info.Columns.Add("antriesgoquimico2");
                        info.Columns.Add("antriesgobiologico2");
                        info.Columns.Add("antriesgoergonomico2");
                        info.Columns.Add("antriesgopsicosocial2");
                        info.Columns.Add("antobservaciones2");

                        info.Columns.Add("antempresa3");
                        info.Columns.Add("antpuestotrabajo3");
                        info.Columns.Add("antactividad3");
                        info.Columns.Add("anttiempotrabajo3");
                        info.Columns.Add("antriesgofisico3");
                        info.Columns.Add("antriesgomecanico3");
                        info.Columns.Add("antriesgoquimico3");
                        info.Columns.Add("antriesgobiologico3");
                        info.Columns.Add("antriesgoergonomico3");
                        info.Columns.Add("antriesgopsicosocial3");
                        info.Columns.Add("antobservaciones3");

                        info.Columns.Add("antempresa4");
                        info.Columns.Add("antpuestotrabajo4");
                        info.Columns.Add("antactividad4");
                        info.Columns.Add("anttiempotrabajo4");
                        info.Columns.Add("antriesgofisico4");
                        info.Columns.Add("antriesgomecanico4");
                        info.Columns.Add("antriesgoquimico4");
                        info.Columns.Add("antriesgobiologico4");
                        info.Columns.Add("antriesgoergonomico4");
                        info.Columns.Add("antriesgopsicosocial4");
                        info.Columns.Add("antobservaciones4");
                    }

                    if (campos["formulario"] == "1" || campos["formulario"] == "2" || campos["formulario"] == "4")
                    {
                        info.Columns.Add("accidentestrabajoSi");
                        info.Columns.Add("accidentestrabajoNo");
                        info.Columns.Add("especificaracctrabajo");
                        info.Columns.Add("acctrabajoobservaciones");
                        info.Columns.Add("enfermedadesprofSi");
                        info.Columns.Add("enfermedadesprofNo");
                        info.Columns.Add("especificarenfermedadesprof");
                        info.Columns.Add("enfermedadesprofobservaciones");
                    }
                    if (campos["formulario"] == "1" || campos["formulario"] == "2")
                    {
                        info.Columns.Add("antfamiliares");
                        info.Columns.Add("factoresriesgopuestotrabajo1");
                        info.Columns.Add("factoresriesgoactividades1");
                    }
                    if (campos["formulario"] == "1" || campos["formulario"] == "2")
                    {
                        info.Columns.Add("fisicotempaltas1");
                        info.Columns.Add("fisicotempbajas1");
                        info.Columns.Add("fisicoionizante1");
                        info.Columns.Add("fisicoNoionizante1");
                        info.Columns.Add("fisicoruido1");
                        info.Columns.Add("fisicovibracion1");
                        info.Columns.Add("fisicoiluminacion1");
                        info.Columns.Add("fisicoventilacion1");
                        info.Columns.Add("fisicoelectrico1");
                        info.Columns.Add("fisicootros1");
                        info.Columns.Add("mecatrapmaquinas1");
                        info.Columns.Add("mecatrapsuperficies1");
                        info.Columns.Add("mecatrapobjetos1");
                        info.Columns.Add("meccaidasdeobjetos1");
                        info.Columns.Add("meccaidasmismonivel1");
                        info.Columns.Add("meccaidasdiferentenivel1");
                        info.Columns.Add("meccontactoelectrico1");
                        info.Columns.Add("meccontacosuperficies1");
                        info.Columns.Add("mecproyeccionparticulas1");
                        info.Columns.Add("mecproyeccionfluidos1");
                        info.Columns.Add("mecpinchazos1");
                        info.Columns.Add("meccortes1");
                        info.Columns.Add("mecatropellamientovehiculo1");
                        info.Columns.Add("mecchoquescolision1");
                        info.Columns.Add("mecanicootros1");
                        info.Columns.Add("quimicosolidos1");
                        info.Columns.Add("quimicopolvos1");
                        info.Columns.Add("quimicohumos1");
                        info.Columns.Add("quimicoliquidos1");
                        info.Columns.Add("quimicovapores1");
                        info.Columns.Add("quimicoaerosoles1");
                        info.Columns.Add("quimiconeblinas1");
                        info.Columns.Add("quimicogaseosos1");
                        info.Columns.Add("quimicootros1");
                        info.Columns.Add("biologicovirus1");
                        info.Columns.Add("biologicohongos1");
                        info.Columns.Add("biologicobacterias1");
                        info.Columns.Add("biologicoparasitos1");
                        info.Columns.Add("biologicoexpovectores1");
                        info.Columns.Add("biologicoexpoanimales1");
                        info.Columns.Add("biologicootros1");
                        info.Columns.Add("ergonomicomanejomanual1");
                        info.Columns.Add("ergonomicomovimientorep1");
                        info.Columns.Add("ergonomicoposturasforzadas1");
                        info.Columns.Add("ergonomicotrabajopvd1");
                        info.Columns.Add("ergonomicootros1");
                        info.Columns.Add("psicosocialmonotonia1");
                        info.Columns.Add("psicosocialsobrecarga1");
                        info.Columns.Add("psicosocialminuciosidad1");
                        info.Columns.Add("psicosocialaltaresponsabilidad1");
                        info.Columns.Add("psicosocialautonomia1");
                        info.Columns.Add("psicosocialsupervision1");
                        info.Columns.Add("psicosocialconflicto1");
                        info.Columns.Add("psicosocialfaltaclaridad1");
                        info.Columns.Add("psicosocialincorrecta1");
                        info.Columns.Add("psicosocialturnos1");
                        info.Columns.Add("psicosocialrelaciones1");
                        info.Columns.Add("psicosocialinestabilidad1");
                        info.Columns.Add("psicosocialotros1");
                        info.Columns.Add("medidaspreventivas1");

                        info.Columns.Add("factoresriesgopuestotrabajo2");
                        info.Columns.Add("factoresriesgoactividades2");
                    }
                    if (campos["formulario"] == "1" || campos["formulario"] == "2")
                    {
                        info.Columns.Add("fisicotempaltas2");
                        info.Columns.Add("fisicotempbajas2");
                        info.Columns.Add("fisicoionizante2");
                        info.Columns.Add("fisicoNoionizante2");
                        info.Columns.Add("fisicoruido2");
                        info.Columns.Add("fisicovibracion2");
                        info.Columns.Add("fisicoiluminacion2");
                        info.Columns.Add("fisicoventilacion2");
                        info.Columns.Add("fisicoelectrico2");
                        info.Columns.Add("fisicootros2");
                        info.Columns.Add("mecatrapmaquinas2");
                        info.Columns.Add("mecatrapsuperficies2");
                        info.Columns.Add("mecatrapobjetos2");
                        info.Columns.Add("meccaidasdeobjetos2");
                        info.Columns.Add("meccaidasmismonivel2");
                        info.Columns.Add("meccaidasdiferentenivel2");
                        info.Columns.Add("meccontactoelectrico2");
                        info.Columns.Add("meccontacosuperficies2");
                        info.Columns.Add("mecproyeccionparticulas2");
                        info.Columns.Add("mecproyeccionfluidos2");
                        info.Columns.Add("mecpinchazos2");
                        info.Columns.Add("meccortes2");
                        info.Columns.Add("mecatropellamientovehiculo2");
                        info.Columns.Add("mecchoquescolision2");
                        info.Columns.Add("mecanicootros2");
                        info.Columns.Add("quimicosolidos2");
                        info.Columns.Add("quimicopolvos2");
                        info.Columns.Add("quimicohumos2");
                        info.Columns.Add("quimicoliquidos2");
                        info.Columns.Add("quimicovapores2");
                        info.Columns.Add("quimicoaerosoles2");
                        info.Columns.Add("quimiconeblinas2");
                        info.Columns.Add("quimicogaseosos2");
                        info.Columns.Add("quimicootros2");
                        info.Columns.Add("biologicovirus2");
                        info.Columns.Add("biologicohongos2");
                        info.Columns.Add("biologicobacterias2");
                        info.Columns.Add("biologicoparasitos2");
                        info.Columns.Add("biologicoexpovectores2");
                        info.Columns.Add("biologicoexpoanimales2");
                        info.Columns.Add("biologicootros2");
                        info.Columns.Add("ergonomicomanejomanual2");
                        info.Columns.Add("ergonomicomovimientorep2");
                        info.Columns.Add("ergonomicoposturasforzadas2");
                        info.Columns.Add("ergonomicotrabajopvd2");
                        info.Columns.Add("ergonomicootros2");
                        info.Columns.Add("psicosocialmonotonia2");
                        info.Columns.Add("psicosocialsobrecarga2");
                        info.Columns.Add("psicosocialminuciosidad2");
                        info.Columns.Add("psicosocialaltaresponsabilidad2");
                        info.Columns.Add("psicosocialautonomia2");
                        info.Columns.Add("psicosocialsupervision2");
                        info.Columns.Add("psicosocialconflicto2");
                        info.Columns.Add("psicosocialfaltaclaridad2");
                        info.Columns.Add("psicosocialincorrecta2");
                        info.Columns.Add("psicosocialturnos2");
                        info.Columns.Add("psicosocialrelaciones2");
                        info.Columns.Add("psicosocialinestabilidad2");
                        info.Columns.Add("psicosocialotros2");
                        info.Columns.Add("medidaspreventivas2");

                        info.Columns.Add("factoresriesgopuestotrabajo3");
                        info.Columns.Add("factoresriesgoactividades3");
                    }
                    if (campos["formulario"] == "1" || campos["formulario"] == "2")
                    {
                        info.Columns.Add("fisicotempaltas3");
                        info.Columns.Add("fisicotempbajas3");
                        info.Columns.Add("fisicoionizante3");
                        info.Columns.Add("fisicoNoionizante3");
                        info.Columns.Add("fisicoruido3");
                        info.Columns.Add("fisicovibracion3");
                        info.Columns.Add("fisicoiluminacion3");
                        info.Columns.Add("fisicoventilacion3");
                        info.Columns.Add("fisicoelectrico3");
                        info.Columns.Add("fisicootros3");
                        info.Columns.Add("mecatrapmaquinas3");
                        info.Columns.Add("mecatrapsuperficies3");
                        info.Columns.Add("mecatrapobjetos3");
                        info.Columns.Add("meccaidasdeobjetos3");
                        info.Columns.Add("meccaidasmismonivel3");
                        info.Columns.Add("meccaidasdiferentenivel3");
                        info.Columns.Add("meccontactoelectrico3");
                        info.Columns.Add("meccontacosuperficies3");
                        info.Columns.Add("mecproyeccionparticulas3");
                        info.Columns.Add("mecproyeccionfluidos3");
                        info.Columns.Add("mecpinchazos3");
                        info.Columns.Add("meccortes3");
                        info.Columns.Add("mecatropellamientovehiculo3");
                        info.Columns.Add("mecchoquescolision3");
                        info.Columns.Add("mecanicootros3");
                        info.Columns.Add("quimicosolidos3");
                        info.Columns.Add("quimicopolvos3");
                        info.Columns.Add("quimicohumos3");
                        info.Columns.Add("quimicoliquidos3");
                        info.Columns.Add("quimicovapores3");
                        info.Columns.Add("quimicoaerosoles3");
                        info.Columns.Add("quimiconeblinas3");
                        info.Columns.Add("quimicogaseosos3");
                        info.Columns.Add("quimicootros3");
                        info.Columns.Add("biologicovirus3");
                        info.Columns.Add("biologicohongos3");
                        info.Columns.Add("biologicobacterias3");
                        info.Columns.Add("biologicoparasitos3");
                        info.Columns.Add("biologicoexpovectores3");
                        info.Columns.Add("biologicoexpoanimales3");
                        info.Columns.Add("biologicootros3");
                        info.Columns.Add("ergonomicomanejomanual3");
                        info.Columns.Add("ergonomicomovimientorep3");
                        info.Columns.Add("ergonomicoposturasforzadas3");
                        info.Columns.Add("ergonomicotrabajopvd3");
                        info.Columns.Add("ergonomicootros3");
                        info.Columns.Add("psicosocialmonotonia3");
                        info.Columns.Add("psicosocialsobrecarga3");
                        info.Columns.Add("psicosocialminuciosidad3");
                        info.Columns.Add("psicosocialaltaresponsabilidad3");
                        info.Columns.Add("psicosocialautonomia3");
                        info.Columns.Add("psicosocialsupervision3");
                        info.Columns.Add("psicosocialconflicto3");
                        info.Columns.Add("psicosocialfaltaclaridad3");
                        info.Columns.Add("psicosocialincorrecta3");
                        info.Columns.Add("psicosocialturnos3");
                        info.Columns.Add("psicosocialrelaciones3");
                        info.Columns.Add("psicosocialinestabilidad3");
                        info.Columns.Add("psicosocialotros3");
                        info.Columns.Add("medidaspreventivas3");
                    }

                    if (campos["formulario"] == "1")
                    {
                        info.Columns.Add("actextralaborales");
                    }
                    if (campos["formulario"] == "1" || campos["formulario"] == "2" || campos["formulario"] == "3")
                    {
                        info.Columns.Add("enfermedadactual");
                    }
                    if (campos["formulario"] == "1" || campos["formulario"] == "2")
                    {
                        info.Columns.Add("revisionpiel");
                        info.Columns.Add("revisionsentidos");
                        info.Columns.Add("revisionrespiratorio");
                        info.Columns.Add("revisioncardio");
                        info.Columns.Add("revisiondigestivo");
                        info.Columns.Add("revisiongenito");
                        info.Columns.Add("revisionmusculo");
                        info.Columns.Add("revisionendocrino");
                        info.Columns.Add("revisionhemo");
                        info.Columns.Add("revisionnervioso");
                        info.Columns.Add("revisionorganosdescripcion");
                    }
                    if (campos["formulario"] == "1" || campos["formulario"] == "2" || campos["formulario"] == "3" || campos["formulario"] == "4")
                    {
                        info.Columns.Add("constpresion");
                        info.Columns.Add("consttemperatura");
                        info.Columns.Add("constfreccardiaca");
                        info.Columns.Add("constsaturacion");
                        info.Columns.Add("constfrecrespiratoria");
                        info.Columns.Add("constpeso");
                        info.Columns.Add("consttalla");
                        info.Columns.Add("constindice");
                        info.Columns.Add("constperimetro");

                        info.Columns.Add("examfisicopielA");
                        info.Columns.Add("examfisicopielB");
                        info.Columns.Add("examfisicopielC");
                        info.Columns.Add("examfisicoojosA");
                        info.Columns.Add("examfisicoojosB");
                        info.Columns.Add("examfisicoojosC");
                        info.Columns.Add("examfisicoojosD");
                        info.Columns.Add("examfisicoojosE");
                        info.Columns.Add("examfisicooidoA");
                        info.Columns.Add("examfisicooidoB");
                        info.Columns.Add("examfisicooidoC");
                        info.Columns.Add("examfisicooroA");
                        info.Columns.Add("examfisicooroB");
                        info.Columns.Add("examfisicooroC");
                        info.Columns.Add("examfisicooroD");
                        info.Columns.Add("examfisicooroE");
                        info.Columns.Add("examfisiconarizA");
                        info.Columns.Add("examfisiconarizB");
                        info.Columns.Add("examfisiconarizC");
                        info.Columns.Add("examfisiconarizD");
                        info.Columns.Add("examfisicocuelloA");
                        info.Columns.Add("examfisicocuelloB");
                        info.Columns.Add("examfisicotorax1A");
                        info.Columns.Add("examfisicotorax1B");
                        info.Columns.Add("examfisicotorax2A");
                        info.Columns.Add("examfisicotorax2B");
                        info.Columns.Add("examfisicoabdomenA");
                        info.Columns.Add("examfisicoabdomenB");
                        info.Columns.Add("examfisicocolumnaA");
                        info.Columns.Add("examfisicocolumnaB");
                        info.Columns.Add("examfisicocolumnaC");
                        info.Columns.Add("examfisicopelvisA");
                        info.Columns.Add("examfisicopelvisB");
                        info.Columns.Add("examfisicoextremA");
                        info.Columns.Add("examfisicoextremB");
                        info.Columns.Add("examfisicoextremC");
                        info.Columns.Add("examfisiconeuroA");
                        info.Columns.Add("examfisiconeuroB");
                        info.Columns.Add("examfisiconeuroC");
                        info.Columns.Add("examfisiconeuroD");
                        info.Columns.Add("examfisicoobservacion");

                        info.Columns.Add("examNom1");
                        info.Columns.Add("examFecha1");
                        info.Columns.Add("examResultado1");
                        info.Columns.Add("examNom2");
                        info.Columns.Add("examFecha2");
                        info.Columns.Add("examResultado2");
                    }
                    if (campos["formulario"] == "1" || campos["formulario"] == "3")
                    {

                        info.Columns.Add("examNom3");
                        info.Columns.Add("examFecha3");
                        info.Columns.Add("examResultado3");

                        if (campos["formulario"] == "1")
                        {
                            info.Columns.Add("examNom4");
                            info.Columns.Add("examFecha4");
                            info.Columns.Add("examResultado4");
                        }
                    }
                    if (campos["formulario"] == "1" || campos["formulario"] == "2" || campos["formulario"] == "3" || campos["formulario"] == "4")
                    {
                        info.Columns.Add("examObservaciones");

                        info.Columns.Add("DiagDescripcion1");
                        info.Columns.Add("DiagCIE1");
                        info.Columns.Add("DiagPre1");
                        info.Columns.Add("DiagDef1");
                        info.Columns.Add("DiagDescripcion2");
                        info.Columns.Add("DiagCIE2");
                        info.Columns.Add("DiagPre2");
                        info.Columns.Add("DiagDef2");
                        info.Columns.Add("DiagDescripcion3");
                        info.Columns.Add("DiagCIE3");
                        info.Columns.Add("DiagPre3");
                        info.Columns.Add("DiagDef3");
                    }
                    if (campos["formulario"] == "1" || campos["formulario"] == "2" || campos["formulario"] == "3")
                    {
                        info.Columns.Add("aptitudApto");
                        info.Columns.Add("aptitudObservacion");
                        info.Columns.Add("aptitudLimitaciones");
                        info.Columns.Add("aptitudNoapto");
                        info.Columns.Add("aptitudObservacionDesc");
                        info.Columns.Add("aptitudLimitacionDesc");

                    }
                    if (campos["formulario"] == "4")
                    {
                        info.Columns.Add("EvalRetiroSi");
                        info.Columns.Add("EvalRetiroNo");
                        info.Columns.Add("EvalRetiroObservacion");
                    }

                    info.Columns.Add("descRecomendaciones");



                    // FORMULARIO TIPO 1
                    if (campos["formulario"] == "1")
                    {
                        // Agregamos las variables cargadas con la informacion a las etiquetas registradas previamente en orden
                        info.Rows.Add("QWE32", "ASLS22", primerApellido, segundoApellido, primerNombre, segundoNombre, sexo, edad, catolica, evangelica, testigo, mormona, otrareligion, gruposanguineo, lateralidad,
                                 orientlesbiana, orientgay, orientbisexual, orientheterosexual, orientnosabe, identfemenino, identmasculino, identransfem, identransmasc, identnosabe, discapacidadsi,
                                 discapacidadno, discapacidadtipo, discapacidadporcentaje, puestotrabajo, areatrabajo, motivoconsulta, antpersonales, menarquia, ciclos, ultmenstruacion, gestas, partos, cesareas, abortos, hijosvivosF, hijosmuertosF, vidaSxActivaSi, vidaSxActivaNo,
                                 metodoplanfamSi1, metodoplanfamNo1, metodoplanfamTipo1, papnicolaouSi, papnicolaouNo, papnicolaouTiempo, papnicolaouResultado, ecomamaSi, ecomamaNo, ecomamaTiempo, ecomamaResultado,
                                 colposcopiaSi, colposcopiaNo, colposcopiaTiempo, colposcopiaResultado, mamografiaSi, mamografiaNo, mamografiaTiempo, mamografiaResultado, antigenoprostSi, antigenoprostNo, antigenoprostTiempo,
                                 antigenoprostResultado, ecoprostaticoSi, ecoprostaticoNo, ecoprostaticoTiempo, ecoprostaticoResultado, metodoplanfamSi2, metodoplanfamNo2, metodoplanfamTipo2, hijosvivosM, hijosmuertosM,
                                 tabacoSi, tabacoNo, tabacoTiempo, tabacoCantidad, tabacoExconsumidor, tabacoTiempoabst, actividadfisicaSi, actividadfisicaNo, actividadfisicaCual, tiempoActividadFisica, alcoholSi, alcoholNo, alcoholTiempo, alcoholCantidad, alcoholExconsumidor, alcoholTiempoabst,
                                 nombreOtrasDrogas, otrasdrogasSi, otrasdrogasNo, otrasdrogasTiempo, otrasdrogasCantidad, otrasdrogasExconsumidor, otrasdrogasTiempoabst,
                                 medicacionhabSi, medicacionhabNo, medicacionhabCual1, medicacionhabCual2, medicacionhabCual3, cantidadMedicamento1, cantidadMedicamento2, cantidadMedicamento3,
                                 antempresa1, antpuestotrabajo1, antactividad1, anttiempotrabajo1, antriesgofisico1, antriesgomecanico1, antriesgoquimico1, antriesgobiologico1, antriesgoergonomico1, antriesgopsicosocial1,
                                 antobservaciones1, antempresa2, antpuestotrabajo2, antactividad2, anttiempotrabajo2, antriesgofisico2, antriesgomecanico2, antriesgoquimico2, antriesgobiologico2, antriesgoergonomico2, antriesgopsicosocial2,
                                 antobservaciones2, antempresa3, antpuestotrabajo3, antactividad3, anttiempotrabajo3, antriesgofisico3, antriesgomecanico3, antriesgoquimico3, antriesgobiologico3, antriesgoergonomico3, antriesgopsicosocial3,
                                 antobservaciones3, antempresa4, antpuestotrabajo4, antactividad4, anttiempotrabajo4, antriesgofisico4, antriesgomecanico4, antriesgoquimico4, antriesgobiologico4, antriesgoergonomico4, antriesgopsicosocial4,
                                 antobservaciones4, accidentestrabajoSi, accidentestrabajoNo, especificaracctrabajo, acctrabajoobservaciones, enfermedadesprofSi, enfermedadesprofNo, especificarenfermedadesprof, enfermedadesprofobservaciones,
                                 antfamiliares, factoresriesgopuestotrabajo1, factoresriesgoactividades1, fisicotempaltas1, fisicotempbajas1, fisicoionizante1, fisicoNoionizante1, fisicoruido1, fisicovibracion1, fisicoiluminacion1, fisicoventilacion1,
                                 fisicoelectrico1, fisicootros1, mecatrapmaquinas1, mecatrapsuperficies1, mecatrapobjetos1, meccaidasdeobjetos1, meccaidasmismonivel1, meccaidasdiferentenivel1, meccontactoelectrico1,
                                 meccontacosuperficies1, mecproyeccionparticulas1, mecproyeccionfluidos1, mecpinchazos1, meccortes1, mecatropellamientovehiculo1, mecchoquescolision1, mecanicootros1, quimicosolidos1, quimicopolvos1, quimicohumos1,
                                 quimicoliquidos1, quimicovapores1, quimicoaerosoles1, quimiconeblinas1, quimicogaseosos1, quimicootros1, biologicovirus1, biologicohongos1, biologicobacterias1, biologicoparasitos1, biologicoexpovectores1,
                                 biologicoexpoanimales1, biologicootros1, ergonomicomanejomanual1, ergonomicomovimientorep1, ergonomicoposturasforzadas1, ergonomicotrabajopvd1, ergonomicootros1, psicosocialmonotonia1, psicosocialsobrecarga1,
                                 psicosocialminuciosidad1, psicosocialaltaresponsabilidad1, psicosocialautonomia1, psicosocialsupervision1, psicosocialconflicto1, psicosocialfaltaclaridad1, psicosocialincorrecta1, psicosocialturnos1,
                                 psicosocialrelaciones1, psicosocialinestabilidad1, psicosocialotros1, medidaspreventivas1, factoresriesgopuestotrabajo2, factoresriesgoactividades2, fisicotempaltas2, fisicotempbajas2, fisicoionizante2,
                                 fisicoNoionizante2, fisicoruido2, fisicovibracion2, fisicoiluminacion2, fisicoventilacion2, fisicoelectrico2, fisicootros2, mecatrapmaquinas2, mecatrapsuperficies2, mecatrapobjetos2, meccaidasdeobjetos2, meccaidasmismonivel2, meccaidasdiferentenivel2, meccontactoelectrico2,
                                 meccontacosuperficies2, mecproyeccionparticulas2, mecproyeccionfluidos2, mecpinchazos2, meccortes2, mecatropellamientovehiculo2, mecchoquescolision2, mecanicootros2, quimicosolidos2, quimicopolvos2, quimicohumos2,
                                 quimicoliquidos2, quimicovapores2, quimicoaerosoles2, quimiconeblinas2, quimicogaseosos2, quimicootros2, biologicovirus2, biologicohongos2, biologicobacterias2, biologicoparasitos2, biologicoexpovectores2,
                                 biologicoexpoanimales2, biologicootros2, ergonomicomanejomanual2, ergonomicomovimientorep2, ergonomicoposturasforzadas2, ergonomicotrabajopvd2, ergonomicootros2, psicosocialmonotonia2, psicosocialsobrecarga2,
                                 psicosocialminuciosidad2, psicosocialaltaresponsabilidad2, psicosocialautonomia2, psicosocialsupervision2, psicosocialconflicto2, psicosocialfaltaclaridad2, psicosocialincorrecta2, psicosocialturnos2,
                                 psicosocialrelaciones2, psicosocialinestabilidad2, psicosocialotros2, medidaspreventivas2, factoresriesgopuestotrabajo3, factoresriesgoactividades3, fisicotempaltas3, fisicotempbajas3, fisicoionizante3, fisicoNoionizante3, fisicoruido3, fisicovibracion3, fisicoiluminacion3, fisicoventilacion3,
                                 fisicoelectrico3, fisicootros3, mecatrapmaquinas3, mecatrapsuperficies3, mecatrapobjetos3, meccaidasdeobjetos3, meccaidasmismonivel3, meccaidasdiferentenivel3, meccontactoelectrico3,
                                 meccontacosuperficies3, mecproyeccionparticulas3, mecproyeccionfluidos3, mecpinchazos3, meccortes3, mecatropellamientovehiculo3, mecchoquescolision3, mecanicootros3, quimicosolidos3, quimicopolvos3, quimicohumos3,
                                 quimicoliquidos3, quimicovapores3, quimicoaerosoles3, quimiconeblinas3, quimicogaseosos3, quimicootros3, biologicovirus3, biologicohongos3, biologicobacterias3, biologicoparasitos3, biologicoexpovectores3,
                                 biologicoexpoanimales3, biologicootros3, ergonomicomanejomanual3, ergonomicomovimientorep3, ergonomicoposturasforzadas3, ergonomicotrabajopvd3, ergonomicootros3, psicosocialmonotonia3, psicosocialsobrecarga3,
                                 psicosocialminuciosidad3, psicosocialaltaresponsabilidad3, psicosocialautonomia3, psicosocialsupervision3, psicosocialconflicto3, psicosocialfaltaclaridad3, psicosocialincorrecta3, psicosocialturnos3,
                                 psicosocialrelaciones3, psicosocialinestabilidad3, psicosocialotros3, medidaspreventivas3, actextralaborales, enfermedadactual, revisionpiel, revisionsentidos, revisionrespiratorio, revisioncardio, revisiondigestivo,
                                 revisiongenito, revisionmusculo, revisionendocrino, revisionhemo, revisionnervioso, revisionorganosdescripcion, constpresion, consttemperatura, constfreccardiaca, constsaturacion, constfrecrespiratoria,
                                 constpeso, consttalla, constindice, constperimetro, examfisicopielA, examfisicopielB, examfisicopielC, examfisicoojosA, examfisicoojosB, examfisicoojosC, examfisicoojosD, examfisicoojosE, examfisicooidoA, examfisicooidoB,
                                 examfisicooidoC, examfisicooroA, examfisicooroB, examfisicooroC, examfisicooroD, examfisicooroE, examfisiconarizA, examfisiconarizB, examfisiconarizC, examfisiconarizD, examfisicocuelloA, examfisicocuelloB, examfisicotorax1A,
                                 examfisicotorax1B, examfisicotorax2A, examfisicotorax2B, examfisicoabdomenA, examfisicoabdomenB, examfisicocolumnaA, examfisicocolumnaB, examfisicocolumnaC, examfisicopelvisA, examfisicopelvisB, examfisicoextremA, examfisicoextremB,
                                 examfisicoextremC, examfisiconeuroA, examfisiconeuroB, examfisiconeuroC, examfisiconeuroD, examfisicoobservacion, examNom1, examFecha1, examResultado1, examNom2, examFecha2, examResultado2, examNom3, examFecha3, examResultado3, examNom4,
                                 examFecha4, examResultado4, examObservaciones, DiagDescripcion1, DiagCIE1, DiagPre1, DiagDef1, DiagDescripcion2, DiagCIE2, DiagPre2, DiagDef2, DiagDescripcion3, DiagCIE3, DiagPre3, DiagDef3, aptitudApto, aptitudObservacion, aptitudLimitaciones,
                                 aptitudNoapto, aptitudObservacionDesc, aptitudLimitacionDesc, descRecomendaciones);

                        // Registramos los ingresos si el formulario es tipo 1
                        info.TableName = "info";
                    }

                    // FORMULARIO TIPO 2
                    if (campos["formulario"] == "2")
                    {
                        // Agregamos las variables cargadas con la informacion a las etiquetas registradas previamente en orden
                        info.Rows.Add("QWE32", "ASLS22", primerApellido, segundoApellido, primerNombre, segundoNombre, sexo, puestotrabajo, motivoconsulta, antpersonales,
                            tabacoSi, tabacoNo, tabacoTiempo, tabacoCantidad, tabacoExconsumidor, tabacoTiempoabst, actividadfisicaSi, actividadfisicaNo, actividadfisicaCual, tiempoActividadFisica, alcoholSi, alcoholNo, alcoholTiempo, alcoholCantidad, alcoholExconsumidor, alcoholTiempoabst,
                            nombreOtrasDrogas, otrasdrogasSi, otrasdrogasNo, otrasdrogasTiempo, otrasdrogasCantidad, otrasdrogasExconsumidor, otrasdrogasTiempoabst,
                            medicacionhabSi, medicacionhabNo, medicacionhabCual1, medicacionhabCual2, medicacionhabCual3, cantidadMedicamento1, cantidadMedicamento2, cantidadMedicamento3,
                            accidentestrabajoSi, accidentestrabajoNo, especificaracctrabajo, acctrabajoobservaciones, enfermedadesprofSi, enfermedadesprofNo, especificarenfermedadesprof, enfermedadesprofobservaciones, antfamiliares,

                            factoresriesgopuestotrabajo1, factoresriesgoactividades1, fisicotempaltas1, fisicotempbajas1, fisicoionizante1, fisicoNoionizante1, fisicoruido1, fisicovibracion1, fisicoiluminacion1, fisicoventilacion1, fisicoelectrico1, fisicootros1,
                            mecatrapmaquinas1, mecatrapsuperficies1, mecatrapobjetos1, meccaidasdeobjetos1, meccaidasmismonivel1, meccaidasdiferentenivel1, meccontactoelectrico1, meccontacosuperficies1, mecproyeccionparticulas1, mecproyeccionfluidos1, mecpinchazos1, meccortes1, mecatropellamientovehiculo1, mecchoquescolision1, mecanicootros1,
                            quimicosolidos1, quimicopolvos1, quimicohumos1, quimicoliquidos1, quimicovapores1, quimicoaerosoles1, quimiconeblinas1, quimicogaseosos1, quimicootros1,
                            biologicovirus1, biologicohongos1, biologicobacterias1, biologicoparasitos1, biologicoexpovectores1, biologicoexpoanimales1, biologicootros1,
                            ergonomicomanejomanual1, ergonomicomovimientorep1, ergonomicoposturasforzadas1, ergonomicotrabajopvd1, ergonomicootros1,
                            psicosocialmonotonia1, psicosocialsobrecarga1, psicosocialminuciosidad1, psicosocialaltaresponsabilidad1, psicosocialautonomia1, psicosocialsupervision1, psicosocialconflicto1, psicosocialfaltaclaridad1, psicosocialincorrecta1, psicosocialturnos1, psicosocialrelaciones1, psicosocialinestabilidad1, psicosocialotros1, medidaspreventivas1,

                            factoresriesgopuestotrabajo2, factoresriesgoactividades2, fisicotempaltas2, fisicotempbajas2, fisicoionizante2, fisicoNoionizante2, fisicoruido2, fisicovibracion2, fisicoiluminacion2, fisicoventilacion2, fisicoelectrico2, fisicootros2,
                            mecatrapmaquinas2, mecatrapsuperficies2, mecatrapobjetos2, meccaidasdeobjetos2, meccaidasmismonivel2, meccaidasdiferentenivel2, meccontactoelectrico2, meccontacosuperficies2, mecproyeccionparticulas2, mecproyeccionfluidos2, mecpinchazos2, meccortes2, mecatropellamientovehiculo2, mecchoquescolision2, mecanicootros2,
                            quimicosolidos2, quimicopolvos2, quimicohumos2, quimicoliquidos2, quimicovapores2, quimicoaerosoles2, quimiconeblinas2, quimicogaseosos2, quimicootros2,
                            biologicovirus2, biologicohongos2, biologicobacterias2, biologicoparasitos2, biologicoexpovectores2, biologicoexpoanimales2, biologicootros2,
                            ergonomicomanejomanual2, ergonomicomovimientorep2, ergonomicoposturasforzadas2, ergonomicotrabajopvd2, ergonomicootros2,
                            psicosocialmonotonia2, psicosocialsobrecarga2, psicosocialminuciosidad2, psicosocialaltaresponsabilidad2, psicosocialautonomia2, psicosocialsupervision2, psicosocialconflicto2, psicosocialfaltaclaridad2, psicosocialincorrecta2, psicosocialturnos2, psicosocialrelaciones2, psicosocialinestabilidad2, psicosocialotros2, medidaspreventivas2,

                            factoresriesgopuestotrabajo3, factoresriesgoactividades3, fisicotempaltas3, fisicotempbajas3, fisicoionizante3, fisicoNoionizante3, fisicoruido3, fisicovibracion3, fisicoiluminacion3, fisicoventilacion3, fisicoelectrico3, fisicootros3,
                            mecatrapmaquinas3, mecatrapsuperficies3, mecatrapobjetos3, meccaidasdeobjetos3, meccaidasmismonivel3, meccaidasdiferentenivel3, meccontactoelectrico3, meccontacosuperficies3, mecproyeccionparticulas3, mecproyeccionfluidos3, mecpinchazos3, meccortes3, mecatropellamientovehiculo3, mecchoquescolision3, mecanicootros3,
                            quimicosolidos3, quimicopolvos3, quimicohumos3, quimicoliquidos3, quimicovapores3, quimicoaerosoles3, quimiconeblinas3, quimicogaseosos3, quimicootros3,
                            biologicovirus3, biologicohongos3, biologicobacterias3, biologicoparasitos3, biologicoexpovectores3, biologicoexpoanimales3, biologicootros3,
                            ergonomicomanejomanual3, ergonomicomovimientorep3, ergonomicoposturasforzadas3, ergonomicotrabajopvd3, ergonomicootros3,
                            psicosocialmonotonia3, psicosocialsobrecarga3, psicosocialminuciosidad3, psicosocialaltaresponsabilidad3, psicosocialautonomia3, psicosocialsupervision3, psicosocialconflicto3, psicosocialfaltaclaridad3, psicosocialincorrecta3, psicosocialturnos3, psicosocialrelaciones3, psicosocialinestabilidad3, psicosocialotros3, medidaspreventivas3,

                            enfermedadactual, revisionpiel, revisionsentidos, revisionrespiratorio, revisioncardio, revisiondigestivo, revisiongenito, revisionmusculo, revisionendocrino, revisionhemo, revisionnervioso, revisionorganosdescripcion,
                            constpresion, consttemperatura, constfreccardiaca, constsaturacion, constfrecrespiratoria, constpeso, consttalla, constindice, constperimetro,

                            examfisicopielA, examfisicopielB, examfisicopielC, examfisicoojosA, examfisicoojosB, examfisicoojosC, examfisicoojosD, examfisicoojosE,
                            examfisicooidoA, examfisicooidoB, examfisicooidoC, examfisicooroA, examfisicooroB, examfisicooroC, examfisicooroD, examfisicooroE,
                            examfisiconarizA, examfisiconarizB, examfisiconarizC, examfisiconarizD, examfisicocuelloA, examfisicocuelloB,
                            examfisicotorax1A, examfisicotorax1B, examfisicotorax2A, examfisicotorax2B, examfisicoabdomenA, examfisicoabdomenB,
                            examfisicocolumnaA, examfisicocolumnaB, examfisicocolumnaC, examfisicopelvisA, examfisicopelvisB,
                            examfisicoextremA, examfisicoextremB, examfisicoextremC, examfisiconeuroA, examfisiconeuroB, examfisiconeuroC, examfisiconeuroD, examfisicoobservacion,

                            examNom1, examFecha1, examResultado1, examNom2, examFecha2, examResultado2, examObservaciones,

                            DiagDescripcion1, DiagCIE1, DiagPre1, DiagDef1, DiagDescripcion2, DiagCIE2, DiagPre2, DiagDef2, DiagDescripcion3, DiagCIE3, DiagPre3, DiagDef3,

                            aptitudApto, aptitudObservacion, aptitudLimitaciones, aptitudNoapto, aptitudObservacionDesc, aptitudLimitacionDesc, descRecomendaciones);

                        // Registramos los ingresos si el formulario es tipo 2
                        info.TableName = "info";
                    }

                    // FORMULARIO TIPO 3
                    if (campos["formulario"] == "3")
                    {
                        // Agregamos las variables cargadas con la informacion a las etiquetas registradas previamente en orden
                        info.Rows.Add("QWE32", "ASLS22", primerApellido, segundoApellido, primerNombre, segundoNombre, sexo, edad, puestotrabajo, motivoconsulta, enfermedadactual, constpresion, consttemperatura, constfreccardiaca, constsaturacion, constfrecrespiratoria,
                                 constpeso, consttalla, constindice, constperimetro, examfisicopielA, examfisicopielB, examfisicopielC, examfisicoojosA, examfisicoojosB, examfisicoojosC, examfisicoojosD, examfisicoojosE, examfisicooidoA, examfisicooidoB,
                                 examfisicooidoC, examfisicooroA, examfisicooroB, examfisicooroC, examfisicooroD, examfisicooroE, examfisiconarizA, examfisiconarizB, examfisiconarizC, examfisiconarizD, examfisicocuelloA, examfisicocuelloB, examfisicotorax1A,
                                 examfisicotorax1B, examfisicotorax2A, examfisicotorax2B, examfisicoabdomenA, examfisicoabdomenB, examfisicocolumnaA, examfisicocolumnaB, examfisicocolumnaC, examfisicopelvisA, examfisicopelvisB, examfisicoextremA, examfisicoextremB,
                                 examfisicoextremC, examfisiconeuroA, examfisiconeuroB, examfisiconeuroC, examfisiconeuroD, examfisicoobservacion, examNom1, examFecha1, examResultado1, examNom2, examFecha2, examResultado2, examNom3, examFecha3, examResultado3, examObservaciones,
                                 DiagDescripcion1, DiagCIE1, DiagPre1, DiagDef1, DiagDescripcion2, DiagCIE2, DiagPre2, DiagDef2, DiagDescripcion3, DiagCIE3, DiagPre3, DiagDef3, aptitudApto, aptitudObservacion, aptitudLimitaciones, aptitudNoapto, aptitudObservacionDesc, aptitudLimitacionDesc, descRecomendaciones);

                        // Registramos los ingresos si el formulario es tipo 1
                        info.TableName = "info";
                    }


                    // FORMULARIO TIPO 4
                    if (campos["formulario"] == "4")
                    {
                        // Agregamos las variables cargadas con la informacion a las etiquetas registradas previamente en orden
                        info.Rows.Add("QWE32", "ASLS22", primerApellido, segundoApellido, primerNombre, segundoNombre, sexo, puestotrabajo, actividades1, factoresriesgoactividades1, actividades2, factoresriesgoactividades2, actividades3, factoresriesgoactividades3, antpersonales, accidentestrabajoSi, accidentestrabajoNo, especificaracctrabajo, acctrabajoobservaciones, enfermedadesprofSi, enfermedadesprofNo, especificarenfermedadesprof, enfermedadesprofobservaciones,
                            constpresion, consttemperatura, constfreccardiaca, constsaturacion, constfrecrespiratoria, constpeso, consttalla, constindice, constperimetro, examfisicopielA, examfisicopielB, examfisicopielC, examfisicoojosA, examfisicoojosB, examfisicoojosC, examfisicoojosD, examfisicoojosE, examfisicooidoA, examfisicooidoB, examfisicooidoC, examfisicooroA, examfisicooroB, examfisicooroC, examfisicooroD, examfisicooroE, examfisiconarizA, examfisiconarizB,
                            examfisiconarizC, examfisiconarizD, examfisicocuelloA, examfisicocuelloB, examfisicotorax1A, examfisicotorax1B, examfisicotorax2A, examfisicotorax2B, examfisicoabdomenA, examfisicoabdomenB, examfisicocolumnaA, examfisicocolumnaB, examfisicocolumnaC, examfisicopelvisA, examfisicopelvisB, examfisicoextremA, examfisicoextremB, examfisicoextremC, examfisiconeuroA, examfisiconeuroB, examfisiconeuroC, examfisiconeuroD, examfisicoobservacion, examNom1,
                            examFecha1, examResultado1, examNom2, examFecha2, examResultado2, examObservaciones, DiagDescripcion1, DiagCIE1, DiagPre1, DiagDef1, DiagDescripcion2, DiagCIE2, DiagPre2, DiagDef2, DiagDescripcion3, DiagCIE3, DiagPre3, DiagDef3, EvalRetiroSi, EvalRetiroNo, EvalRetiroObservacion, descRecomendaciones);

                        // Registramos los ingresos si el formulario es tipo 1
                        info.TableName = "info";
                    }

                    // FORMULARIO TIPO 4
                    if (campos["formulario"] == "5")
                    {
                        // Agregamos las variables cargadas con la informacion a las etiquetas registradas previamente en orden
                        info.Rows.Add("QWE32", "ASLS22", primerApellido, segundoApellido, primerNombre, segundoNombre, sexo, puestotrabajo, fecEmision, evaIngreso, evaPeriodico, evaReintegro, evaRetiro, aptitudApto, aptitudObservacion, aptitudLimitaciones, aptitudNoapto, descAptitud, retiroSi, retiroNo, diagPresuntiva, diagDefinitiva, diagNoAplica, relacionSi, relacionNo, relacionNoAplica, descRecomendaciones);

                        // Registramos los ingresos si el formulario es tipo 1
                        info.TableName = "info";
                    }

                }

                PDFs generarRide = new PDFs();
                ds.Tables.Add(info);
                //Creamos el QR del usuario
                string rutaQR = generarRide.GenerarCodigoQR(primerApellido + " " + segundoApellido + " " + primerNombre + " " + segundoNombre);
                //Creamos el QR del doctor
                string rutaQR2 = generarRide.GenerarCodigoQR(primerApellidoDoc + " " + segundoApellidoDoc + " " + primerNombreDoc + " " + segundoNombreDoc);

                // Obtenemos el nombre del paciente o identificador único
                string nombrePaciente = $"{primerApellido} {segundoApellido} {primerNombre} {segundoNombre}";

                // Directorio base donde se almacenan las historias clínicas
                string historiasClinicasFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HistoriasClinicas");

                // Verifica si la carpeta del paciente ya existe
                string pacienteFolder = Path.Combine(historiasClinicasFolder, nombrePaciente);
                if (!Directory.Exists(pacienteFolder))
                {
                    // La carpeta no existe, créala
                    Directory.CreateDirectory(pacienteFolder);
                }

                string nomHoja = "";
                string nomHoja2 = "";
                string tipoFormulario = "";
                string archivo = "";
                switch (campos["formulario"])
                {
                    case "1":
                        nomHoja = "077-PREOCUPA. INICIO 1-3";
                        tipoFormulario = "PREOCUPACIONAL";
                        archivo = "TemplatePreocupacional.xlsx";
                        break;
                    case "2":
                        nomHoja = "078-PERIODICA ";
                        tipoFormulario = "PERIODICA";
                        archivo = "TemplatePeriodica.xlsx";
                        break;
                    case "3":
                        nomHoja = "079-REINTEGRO";
                        tipoFormulario = "REINTEGRO";
                        archivo = "TemplateReintegro.xlsx";
                        break;
                    case "4":
                        nomHoja = "080-RETIRO 1-2";
                        //nomHoja2 = "080-RETIRO 2-2";
                        tipoFormulario = "RETIRO";
                        archivo = "TemplateRetiro.xlsx";
                        break;
                    case "5":
                        nomHoja = "CERTIFICADO DE AL";
                        //nomHoja2 = "080-RETIRO 2-2";
                        tipoFormulario = "CERTIFICADO";
                        archivo = "TemplateCertificado.xlsx";
                        break;
                    default:
                        break;
                }

                // Nombre del archivo de la historia clínica
                string nombreArchivo = $"{tipoFormulario}_{nombrePaciente}_{DateTime.Now.ToString("yyyyMMdd_HHmm")}.xlsx";

                // Ruta completa del archivo de salida, que incluye la carpeta del paciente
                string outputPath = Path.Combine(pacienteFolder, nombreArchivo);



                // Llama a un método "FillReport" para llenar un archivo Excel utilizando una plantilla ("templateDOS.xlsx") y datos proporcionados en el DataSet ("ds").
                // El archivo resultante se guardará en la ruta especificada por "outputPath".
                TemplateExcel.FillReport(outputPath, archivo, nomHoja, ds, new string[] { "{", "}" }, rutaQR, rutaQR2, nombrePaciente);

                /*switch (campos["formulario"])
                {
                    case "4":
                        TemplateExcel.FillReport(outputPath, archivo, nomHoja2, ds, new string[] { "{", "}" }, rutaQR, rutaQR2, nombrePaciente);
                        break;
                    default:
                        break;
                }*/


                // Abre el archivo recién creado utilizando la aplicación asociada en el sistema.
                Process.Start(outputPath);

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }

            return respuesta.SerializaToJson();
        }

        public string AbrirDocHistoria(dynamic campos)
        {
            var nombreCompleto = campos["nombreCarpeta"];
            var nombreArchivo = campos["nombreArchivo"];

            // Separar el nombre completo en palabras utilizando el espacio como delimitador
            var palabras = nombreCompleto.Split(' ');

            // Asegurarse de que haya al menos cuatro palabras antes de acceder a los índices
            string primerApellido = palabras.Length > 0 ? palabras[0] : "";
            string segundoApellido = palabras.Length > 1 ? palabras[1] : "";
            string primerNombre = palabras.Length > 2 ? palabras[2] : "";
            string segundoNombre = palabras.Length > 3 ? palabras[3] : "";


            PDFs generarRide = new PDFs();

            // Obtenemos el nombre del paciente o identificador único
            string nombrePaciente = $"{primerApellido} {segundoApellido} {primerNombre} {segundoNombre}";

            // Directorio base donde se almacenan las historias clínicas
            string historiasClinicasFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HistoriasClinicas");

            // Verifica si la carpeta del paciente ya existe
            string pacienteFolder = Path.Combine(historiasClinicasFolder, nombrePaciente);

            // Ruta completa del archivo de salida, que incluye la carpeta del paciente
            string outputPath = Path.Combine(pacienteFolder, nombreArchivo);


            // Abre el archivo recién creado utilizando la aplicación asociada en el sistema.
            Process.Start(outputPath);

            return "";

        }




        public string GuardarHisInmunizaciones(dynamic campos)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();

            try
            {
                string primerNombre = "";
                string segundoNombre = "";
                string primerApellido = "";
                string segundoApellido = "";


                // Guardamos los datos obtenidos del JSON generado en Departamento.js y los guardamos en las variables previamente creadas.
                var nombreCompleto = campos["txtNombre"];

                // Separar el nombre completo en palabras utilizando el espacio como delimitador
                var palabras = nombreCompleto.Split(' ');

                // Asegurarse de que haya al menos cuatro palabras antes de acceder a los índices
                primerApellido = palabras.Length > 0 ? palabras[0] : "";
                segundoApellido = palabras.Length > 1 ? palabras[1] : "";
                primerNombre = palabras.Length > 2 ? palabras[2] : "";
                segundoNombre = palabras.Length > 3 ? palabras[3] : "";

                string sexo = campos["txtSexo"];
                string puestotrabajo = campos["txtPuestoTrabajo1"];
                //string areatrabajo = "";

                string fechaTetanos1 = campos["txtfechaTetanos1"];
                string fechaTetanos2 = campos["txtfechaTetanos2"];
                string fechaTetanos3 = campos["txtfechaTetanos3"];
                string fechaTetanos4 = campos["txtfechaTetanos4"];
                string fechaTetanos5 = campos["txtfechaTetanos5"];

                string loteTetanos1 = campos["txtloteTetanos1"];
                string loteTetanos2 = campos["txtloteTetanos2"];
                string loteTetanos3 = campos["txtloteTetanos3"];
                string loteTetanos4 = campos["txtloteTetanos4"];
                string loteTetanos5 = campos["txtloteTetanos5"];

                string esquemaTetanos1 = campos["txtesquemaTetanos1"];
                string esquemaTetanos2 = campos["txtesquemaTetanos2"];
                string esquemaTetanos3 = campos["txtesquemaTetanos3"];
                string esquemaTetanos4 = campos["txtesquemaTetanos4"];
                string esquemaTetanos5 = campos["txtesquemaTetanos5"];

                if (campos["txtesquemaTetanos1"] == "SI")
                {
                    esquemaTetanos1 = "X";
                }
                if (campos["txtesquemaTetanos2"] == "SI")
                {
                    esquemaTetanos2 = "X";
                }
                if (campos["txtesquemaTetanos3"] == "SI")
                {
                    esquemaTetanos3 = "X";
                }
                if (campos["txtesquemaTetanos4"] == "SI")
                {
                    esquemaTetanos4 = "X";
                }
                if (campos["txtesquemaTetanos5"] == "SI")
                {
                    esquemaTetanos5 = "X";
                }

                string nombreTetanos1 = campos["txtnombreTetanos1"];
                string nombreTetanos2 = campos["txtnombreTetanos2"];
                string nombreTetanos3 = campos["txtnombreTetanos3"];
                string nombreTetanos4 = campos["txtnombreTetanos4"];
                string nombreTetanos5 = campos["txtnombreTetanos5"];

                string establecimientoTetanos1 = campos["txtestablecimientoTetanos1"];
                string establecimientoTetanos2 = campos["txtestablecimientoTetanos2"];
                string establecimientoTetanos3 = campos["txtestablecimientoTetanos3"];
                string establecimientoTetanos4 = campos["txtestablecimientoTetanos4"];
                string establecimientoTetanos5 = campos["txtestablecimientoTetanos5"];

                string obsTetanos1 = campos["txtobsTetanos1"];
                string obsTetanos2 = campos["txtobsTetanos2"];
                string obsTetanos3 = campos["txtobsTetanos3"];
                string obsTetanos4 = campos["txtobsTetanos4"];
                string obsTetanos5 = campos["txtobsTetanos5"];


                string fechaHepA1 = campos["txtfechaHepA1"];
                string fechaHepA2 = campos["txtfechaHepA2"];
                string fechaHepA3 = campos["txtfechaHepA3"];

                string fechaHepB1 = campos["txtfechaHepB1"];
                string fechaHepB2 = campos["txtfechaHepB2"];
                string fechaHepB3 = campos["txtfechaHepB3"];

                string loteHepA1 = campos["txtloteHepA1"];
                string loteHepA2 = campos["txtloteHepA2"];
                string loteHepA3 = campos["txtloteHepA3"];

                string loteHepB1 = campos["txtloteHepB1"];
                string loteHepB2 = campos["txtloteHepB2"];
                string loteHepB3 = campos["txtloteHepB3"];

                string esquemaHepA1 = campos["txtesquemaHepA1"];
                string esquemaHepA2 = campos["txtesquemaHepA2"];
                string esquemaHepA3 = campos["txtesquemaHepA3"];

                string esquemaHepB1 = campos["txtesquemaHepB1"];
                string esquemaHepB2 = campos["txtesquemaHepB2"];
                string esquemaHepB3 = campos["txtesquemaHepB3"];

                if (campos["txtesquemaHepA1"] == "SI")
                {
                    esquemaHepA1 = "X";
                }
                if (campos["txtesquemaHepA2"] == "SI")
                {
                    esquemaHepA2 = "X";
                }
                if (campos["txtesquemaHepA3"] == "SI")
                {
                    esquemaHepA3 = "X";
                }
                if (campos["txtesquemaHepB1"] == "SI")
                {
                    esquemaHepB1 = "X";
                }
                if (campos["txtesquemaHepB2"] == "SI")
                {
                    esquemaHepB2 = "X";
                }
                if (campos["txtesquemaHepB3"] == "SI")
                {
                    esquemaHepB3 = "X";
                }

                string nombreHepA1 = campos["txtnombreHepA1"];
                string nombreHepA2 = campos["txtnombreHepA2"];
                string nombreHepA3 = campos["txtnombreHepA3"];

                string nombreHepB1 = campos["txtnombreHepB1"];
                string nombreHepB2 = campos["txtnombreHepB2"];
                string nombreHepB3 = campos["txtnombreHepB3"];

                string establecimientoHepA1 = campos["txtestablecimientoHepA1"];
                string establecimientoHepA2 = campos["txtestablecimientoHepA2"];
                string establecimientoHepA3 = campos["txtestablecimientoHepA3"];

                string establecimientoHepB1 = campos["txtestablecimientoHepB1"];
                string establecimientoHepB2 = campos["txtestablecimientoHepB2"];
                string establecimientoHepB3 = campos["txtestablecimientoHepB3"];

                string obsHepA1 = campos["txtobsHepA1"];
                string obsHepA2 = campos["txtobsHepA2"];
                string obsHepA3 = campos["txtobsHepA3"];

                string obsHepB1 = campos["txtobsHepB1"];
                string obsHepB2 = campos["txtobsHepB2"];
                string obsHepB3 = campos["txtobsHepB3"];


                string fechaInfluenza = campos["txtfechaInfluenza"];
                string fechaFiebre = campos["txtfechaFiebre"];
                string fechaSarampion1 = campos["txtfechaSarampion1"];
                string fechaSarampion2 = campos["txtfechaSarampion2"];

                string loteInfluenza = campos["txtloteInfluenza"];
                string loteFiebre = campos["txtloteFiebre"];
                string loteSarampion1 = campos["txtloteSarampion1"];
                string loteSarampion2 = campos["txtloteSarampion2"];

                string esquemaInfluenza = campos["txtesquemaInfluenza"];
                string esquemaFiebre = campos["txtesquemaFiebre"];
                string esquemaSarampion1 = campos["txtesquemaSarampion1"];
                string esquemaSarampion2 = campos["txtesquemaSarampion2"];

                if (campos["txtesquemaInfluenza"] == "SI")
                {
                    esquemaInfluenza = "X";
                }
                if (campos["txtesquemaFiebre"] == "SI")
                {
                    esquemaFiebre = "X";
                }
                if (campos["txtesquemaSarampion1"] == "SI")
                {
                    esquemaSarampion1 = "X";
                }
                if (campos["txtesquemaSarampion2"] == "SI")
                {
                    esquemaSarampion2 = "X";
                }

                string nombreInfluenza = campos["txtnombreInfluenza"];
                string nombreFiebre = campos["txtnombreFiebre"];
                string nombreSarampion1 = campos["txtnombreSarampion1"];
                string nombreSarampion2 = campos["txtnombreSarampion2"];

                string establecimientoInfluenza = campos["txtestablecimientoInfluenza"];
                string establecimientoFiebre = campos["txtestablecimientoFiebre"];
                string establecimientoSarampion1 = campos["txtestablecimientoSarampion1"];
                string establecimientoSarampion2 = campos["txtestablecimientoSarampion2"];

                string obsInfluenza = campos["txtobsInfluenza"];
                string obsFiebre = campos["txtobsFiebre"];
                string obsSarampion1 = campos["txtobsSarampion1"];
                string obsSarampion2 = campos["txtobsSarampion2"];


                string primerApellidoDoc = "";
                string segundoApellidoDoc = "";
                string primerNombreDoc = "";
                string segundoNombreDoc = "";

                var nombreDoctor = campos["nombre"];
                // Separar el nombre completo en palabras utilizando el espacio como delimitador
                var doctor = nombreDoctor.Split(' ');
                // Asegurarse de que haya al menos cuatro palabras antes de acceder a los índices
                primerApellidoDoc = doctor.Length > 0 ? doctor[0] : "";
                segundoApellidoDoc = doctor.Length > 1 ? doctor[1] : "";
                primerNombreDoc = doctor.Length > 2 ? doctor[2] : "";
                segundoNombreDoc = doctor.Length > 3 ? doctor[3] : "";

                var info = new DataTable();

                // Creamos un DataSet para almacenar los datos de la tabla
                var ds = new DataSet();

                info.Columns.Add("numhistoria");
                info.Columns.Add("numarchivo");
                info.Columns.Add("primerapellido");
                info.Columns.Add("segundoapellido");
                info.Columns.Add("primernombre");
                info.Columns.Add("segundonombre");
                info.Columns.Add("sexo");

                info.Columns.Add("puestotrabajo");

                info.Columns.Add("fechaTetanos1");
                info.Columns.Add("fechaTetanos2");
                info.Columns.Add("fechaTetanos3");
                info.Columns.Add("fechaTetanos4");
                info.Columns.Add("fechaTetanos5");

                info.Columns.Add("loteTetanos1");
                info.Columns.Add("loteTetanos2");
                info.Columns.Add("loteTetanos3");
                info.Columns.Add("loteTetanos4");
                info.Columns.Add("loteTetanos5");

                info.Columns.Add("esquemaTetanos1");
                info.Columns.Add("esquemaTetanos2");
                info.Columns.Add("esquemaTetanos3");
                info.Columns.Add("esquemaTetanos4");
                info.Columns.Add("esquemaTetanos5");

                info.Columns.Add("nombreTetanos1");
                info.Columns.Add("nombreTetanos2");
                info.Columns.Add("nombreTetanos3");
                info.Columns.Add("nombreTetanos4");
                info.Columns.Add("nombreTetanos5");

                info.Columns.Add("establecimientoTetanos1");
                info.Columns.Add("establecimientoTetanos2");
                info.Columns.Add("establecimientoTetanos3");
                info.Columns.Add("establecimientoTetanos4");
                info.Columns.Add("establecimientoTetanos5");

                info.Columns.Add("obsTetanos1");
                info.Columns.Add("obsTetanos2");
                info.Columns.Add("obsTetanos3");
                info.Columns.Add("obsTetanos4");
                info.Columns.Add("obsTetanos5");


                info.Columns.Add("fechaHepA1");
                info.Columns.Add("fechaHepA2");
                info.Columns.Add("fechaHepA3");

                info.Columns.Add("fechaHepB1");
                info.Columns.Add("fechaHepB2");
                info.Columns.Add("fechaHepB3");

                info.Columns.Add("loteHepA1");
                info.Columns.Add("loteHepA2");
                info.Columns.Add("loteHepA3");

                info.Columns.Add("loteHepB1");
                info.Columns.Add("loteHepB2");
                info.Columns.Add("loteHepB3");

                info.Columns.Add("esquemaHepA1");
                info.Columns.Add("esquemaHepA2");
                info.Columns.Add("esquemaHepA3");

                info.Columns.Add("esquemaHepB1");
                info.Columns.Add("esquemaHepB2");
                info.Columns.Add("esquemaHepB3");

                info.Columns.Add("nombreHepA1");
                info.Columns.Add("nombreHepA2");
                info.Columns.Add("nombreHepA3");

                info.Columns.Add("nombreHepB1");
                info.Columns.Add("nombreHepB2");
                info.Columns.Add("nombreHepB3");

                info.Columns.Add("establecimientoHepA1");
                info.Columns.Add("establecimientoHepA2");
                info.Columns.Add("establecimientoHepA3");

                info.Columns.Add("establecimientoHepB1");
                info.Columns.Add("establecimientoHepB2");
                info.Columns.Add("establecimientoHepB3");

                info.Columns.Add("obsHepA1");
                info.Columns.Add("obsHepA2");
                info.Columns.Add("obsHepA3");

                info.Columns.Add("obsHepB1");
                info.Columns.Add("obsHepB2");
                info.Columns.Add("obsHepB3");


                info.Columns.Add("fechaInfluenza");
                info.Columns.Add("fechaFiebre");
                info.Columns.Add("fechaSarampion1");
                info.Columns.Add("fechaSarampion2");

                info.Columns.Add("loteInfluenza");
                info.Columns.Add("loteFiebre");
                info.Columns.Add("loteSarampion1");
                info.Columns.Add("loteSarampion2");

                info.Columns.Add("esquemaInfluenza");
                info.Columns.Add("esquemaFiebre");
                info.Columns.Add("esquemaSarampion1");
                info.Columns.Add("esquemaSarampion2");

                info.Columns.Add("nombreInfluenza");
                info.Columns.Add("nombreFiebre");
                info.Columns.Add("nombreSarampion1");
                info.Columns.Add("nombreSarampion2");

                info.Columns.Add("establecimientoInfluenza");
                info.Columns.Add("establecimientoFiebre");
                info.Columns.Add("establecimientoSarampion1");
                info.Columns.Add("establecimientoSarampion2");

                info.Columns.Add("obsInfluenza");
                info.Columns.Add("obsFiebre");
                info.Columns.Add("obsSarampion1");
                info.Columns.Add("obsSarampion2");


                // Agregamos las variables cargadas con la informacion a las etiquetas registradas previamente en orden
                info.Rows.Add("QWE32", "ASLS22", primerApellido, segundoApellido, primerNombre, segundoNombre, sexo, puestotrabajo, fechaTetanos1, fechaTetanos2, fechaTetanos3, fechaTetanos4, fechaTetanos5,
                    loteTetanos1, loteTetanos2, loteTetanos3, loteTetanos4, loteTetanos5, esquemaTetanos1, esquemaTetanos2, esquemaTetanos3, esquemaTetanos4, esquemaTetanos5, nombreTetanos1, nombreTetanos2, nombreTetanos3, nombreTetanos4,
                    nombreTetanos5, establecimientoTetanos1, establecimientoTetanos2, establecimientoTetanos3, establecimientoTetanos4, establecimientoTetanos5, obsTetanos1, obsTetanos2, obsTetanos3, obsTetanos4, obsTetanos5, fechaHepA1,
                    fechaHepA2, fechaHepA3, fechaHepB1, fechaHepB2, fechaHepB3, loteHepA1, loteHepA2, loteHepA3, loteHepB1, loteHepB2, loteHepB3, esquemaHepA1, esquemaHepA2, esquemaHepA3, esquemaHepB1, esquemaHepB2, esquemaHepB3, nombreHepA1,
                    nombreHepA2, nombreHepA3, nombreHepB1, nombreHepB2, nombreHepB3, establecimientoHepA1, establecimientoHepA2, establecimientoHepA3, establecimientoHepB1, establecimientoHepB2, establecimientoHepB3, obsHepA1, obsHepA2,
                    obsHepA3, obsHepB1, obsHepB2, obsHepB3, fechaInfluenza, fechaFiebre, fechaSarampion1, fechaSarampion2, loteInfluenza, loteFiebre, loteSarampion1, loteSarampion2, esquemaInfluenza, esquemaFiebre, esquemaSarampion1,
                    esquemaSarampion2, nombreInfluenza, nombreFiebre, nombreSarampion1, nombreSarampion2, establecimientoInfluenza, establecimientoFiebre, establecimientoSarampion1, establecimientoSarampion2, obsInfluenza, obsFiebre, obsSarampion1, obsSarampion2);

                // Registramos los ingresos si el formulario es tipo 1
                info.TableName = "info";


                PDFs generarRide = new PDFs();
                ds.Tables.Add(info);
                //Creamos el QR del usuario
                string rutaQR = generarRide.GenerarCodigoQR(primerApellido + " " + segundoApellido + " " + primerNombre + " " + segundoNombre);
                //Creamos el QR del doctor
                string rutaQR2 = generarRide.GenerarCodigoQR(primerApellidoDoc + " " + segundoApellidoDoc + " " + primerNombreDoc + " " + segundoNombreDoc);

                // Obtenemos el nombre del paciente o identificador único
                string nombrePaciente = $"{primerApellido} {segundoApellido} {primerNombre} {segundoNombre}";

                // Directorio base donde se almacenan las historias clínicas
                string historiasClinicasFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HistoriasClinicas");

                // Verifica si la carpeta del paciente ya existe
                string pacienteFolder = Path.Combine(historiasClinicasFolder, nombrePaciente);
                if (!Directory.Exists(pacienteFolder))
                {
                    // La carpeta no existe, créala
                    Directory.CreateDirectory(pacienteFolder);
                }

                string nomHoja = "";
                string nomHoja2 = "";
                string tipoFormulario = "";
                string archivo = "";

                nomHoja = "Form 083 Registro Inmunizacione";
                tipoFormulario = "INMUNIZACIONES";
                archivo = "TemplateInmunizaciones.xlsx";


                // Nombre del archivo de la historia clínica
                string nombreArchivo = $"{tipoFormulario}_{nombrePaciente}_{DateTime.Now.ToString("yyyyMMdd_HHmm")}.xlsx";

                // Ruta completa del archivo de salida, que incluye la carpeta del paciente
                string outputPath = Path.Combine(pacienteFolder, nombreArchivo);



                // Llama a un método "FillReport" para llenar un archivo Excel utilizando una plantilla ("templateDOS.xlsx") y datos proporcionados en el DataSet ("ds").
                // El archivo resultante se guardará en la ruta especificada por "outputPath".
                TemplateExcel.FillReport(outputPath, archivo, nomHoja, ds, new string[] { "{", "}" }, rutaQR, rutaQR2, nombrePaciente);

                // Abre el archivo recién creado utilizando la aplicación asociada en el sistema.
                Process.Start(outputPath);

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }
            return respuesta.SerializaToJson();
        }


        private string ConsultarProvinciasCiudades(dynamic campos)
        {
            List<EntProvinciaCiudad> Ent1 = new List<EntProvinciaCiudad>();

            int op = Convert.ToInt32(campos["opcion"]);
            string prov = campos["prov"];

            try
            {
                Ent1 = NegProvinciasCiudades.Sp_RTAConsultarProvinciasCiudades(op, prov);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al buscar los datos. " + ex.Message.ToString(), "danger", "");
            }
            //GuardarHistoria();

            // Add the selected option to the JSON response
            dynamic jsonData = new { resultados = Ent1, selectedOption = campos["opcion"] };
            return JsonConvert.SerializeObject(jsonData);
        }

    }
}
