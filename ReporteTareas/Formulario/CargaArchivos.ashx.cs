using System;
using System.Collections.Generic;
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
using System.IO;
using FilesHelper;

namespace JsonJQueryNetCargaArchivos
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class CargaArchivos : IHttpHandler
    {
        
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder responseAction = new StringBuilder();
            dynamic datosRecibidos;
            dynamic parametros = "";
            string tipoAjax;

            if (context.Request.ContentType.Contains(""))
            {

                var action = context.Request.Form.Get("action");
                
                if (action == null)
                {
                    var inputStream = new System.IO.StreamReader(context.Request.InputStream);
                    var inputJson = inputStream.ReadToEnd();
                    List<RespuestaJson> collectionJson = inputJson.DeserializarJsonTo<List<RespuestaJson>>();

                    JavaScriptSerializer i = new JavaScriptSerializer();
                    datosRecibidos = i.Deserialize(inputJson.ToString(), typeof(object));

                    action = datosRecibidos[0]["action"];
                    parametros = datosRecibidos[0]["parameters"];

                    tipoAjax = "json";

                }
                else
                {
                    tipoAjax = "";
                }


                Boolean existAction = false;

                if (action == "CargarArchivos" && tipoAjax == "")
                {
                    existAction = true;
                    responseAction.Append(CargarArchivosAdjuntos(context));
                }

                if (action == "ListaArchivosTarea" && tipoAjax == "json")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaArchivosTarea(parametros));
                }

                if (action == "ListaArchivosVacaciones" && tipoAjax == "json")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaArchivosVacaciones(parametros));
                }

                if (action == "ListaArchivosContrato" && tipoAjax == "json")
                {
                    existAction = true;
                    responseAction.Append(ObtenerListaArchivosContrato(parametros));
                }

                if (action == "BorrarArchivosTarea" && tipoAjax == "json")
                {
                    existAction = true;
                    responseAction.Append(BorrarArchivosTarea(parametros));
                }

                if (action == "BorrarArchivosContrato" && tipoAjax == "json")
                {
                    existAction = true;
                    responseAction.Append(BorrarArchivosContrato(parametros));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }


            }

            context.Response.ContentType = "application/json";
            context.Response.Charset = "utf-8";
            context.Response.Write(responseAction.ToString());
        }

        public string BorrarArchivosTarea(dynamic parametros)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            string IdUsuarioSession = "";

            try
            {

                string session = parametros["session"].ToString();
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                Int32 IdArchivo = Convert.ToInt32(parametros["txtCodigoItem"].ToString());

                respuesta = NegTareas.BorrarArchivosTarea(IdArchivo);


            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "").SerializaToJson();
            }


            return respuesta.SerializaToJson();

        }

        public string BorrarArchivosContrato(dynamic parametros)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            string IdUsuarioSession = "";

            try
            {

                string session = parametros["session"].ToString();
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                Int32 IdArchivo = Convert.ToInt32(parametros["txtCodigoItem"].ToString());

                respuesta = NegTareas.BorrarArchivosContrato(IdArchivo);


            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "").SerializaToJson();
            }


            return respuesta.SerializaToJson();

        }

        public string ObtenerListaArchivosTarea(dynamic parametros)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            List<EntArchivoTarea> listaArchivosTarea = new List<EntArchivoTarea>();
            string IdUsuarioSession = "";
            string respuestaCompuesta = "";
            string resultadoFinal = "";

            try
            {

                string session = parametros["session"].ToString();
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                Int32 IdTarea = Convert.ToInt32(parametros["frmTxtCodigo"].ToString());
                Int32 IdServicio = Convert.ToInt32(parametros["IdServicio"].ToString());

                listaArchivosTarea = NegTareas.ListaArchivosTareas(IdTarea, IdServicio);
                respuesta.estado = "1";
                respuesta.mensaje = "OK";
                respuesta.resultado = "@resultado@";

                respuestaCompuesta = respuesta.SerializaToJson();
                string patron = "\"@resultado@\"";
                string listaResultado = listaArchivosTarea.SerializaToJson();
                resultadoFinal = respuestaCompuesta.Replace(patron, listaResultado);
                
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "").SerializaToJson();
            }


            return resultadoFinal;

        }

        public string ObtenerListaArchivosContrato(dynamic parametros)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            List<EntArchivoTarea> listaArchivosTarea = new List<EntArchivoTarea>();
            string IdUsuarioSession = "";
            string respuestaCompuesta = "";
            string resultadoFinal = "";

            try
            {

                string session = parametros["session"].ToString();
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                Int32 IdTarea = Convert.ToInt32(parametros["frmTxtCodigo"].ToString());

                listaArchivosTarea = NegTareas.ListaArchivosContrato(IdTarea);
                respuesta.estado = "1";
                respuesta.mensaje = "OK";
                respuesta.resultado = "@resultado@";

                respuestaCompuesta = respuesta.SerializaToJson();
                string patron = "\"@resultado@\"";
                string listaResultado = listaArchivosTarea.SerializaToJson();
                resultadoFinal = respuestaCompuesta.Replace(patron, listaResultado);

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "").SerializaToJson();
            }


            return resultadoFinal;

        }

        public string ObtenerListaArchivosVacaciones(dynamic parametros)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            List<EntArchivoTarea> listaArchivosTarea = new List<EntArchivoTarea>();
            string IdUsuarioSession = "";
            string respuestaCompuesta = "";
            string resultadoFinal = "";

            try
            {

                string session = parametros["session"].ToString();
                IdUsuarioSession = seguridad.Desencripta(session.ToString());
                Int32 IdTarea = Convert.ToInt32(parametros["frmTxtCodigo"].ToString());

                listaArchivosTarea = NegSolicitud.ListaArchivosSolicitudes(IdTarea);
                respuesta.estado = "1";
                respuesta.mensaje = "OK";
                respuesta.resultado = "@resultado@";

                respuestaCompuesta = respuesta.SerializaToJson();
                string patron = "\"@resultado@\"";
                string listaResultado = listaArchivosTarea.SerializaToJson();
                resultadoFinal = respuestaCompuesta.Replace(patron, listaResultado);

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "").SerializaToJson();
            }


            return resultadoFinal;

        }

        public string CargarArchivosAdjuntos(dynamic context)
        {

            SeguridadHelper seguridad = new SeguridadHelper();
            string respuesta = "";
            string resultado = "";

            string IdUsuarioSession = "";
            string folderPath = "";
            string fileName = "";
            string mensajeRespuesta = "";
            string iconName = "";
            EntArchivoTarea registro = new EntArchivoTarea();
            EntRespuesta respuestaInsertarArchivo = new EntRespuesta();
            EntRespuesta maximaOrdenArchivosTarea = new EntRespuesta();
            int numeroArchivosOrden = 0;

            if (context.Request.Files.Count > 0)
            {

                maximaOrdenArchivosTarea = NegTareas.MaximaOrdenArchivosTarea( Convert.ToInt32(context.Request.Form.Get("Id_RegTareas")), Convert.ToInt32(context.Request.Form.Get("idServicio")));
                numeroArchivosOrden = Convert.ToInt32(maximaOrdenArchivosTarea.resultado) + 1;

                folderPath = context.Server.MapPath("~/descargas/");
                respuesta = responseMessage("1", mensajeRespuesta, "success");
                string session = context.Request.Form.Get("session");
                IdUsuarioSession = seguridad.Desencripta(session.ToString());


                for (int elemento = 0; elemento < context.Request.Files.Count; elemento++)
                {
                    HttpPostedFile postedFile = context.Request.Files.Get(elemento);
                    fileName = Path.GetFileName(postedFile.FileName);

                    // Extraigo la extensión del archivo
                    char separador = '.';
                    string[] arrArchivoExtension = fileName.Split(separador);
                    string extensionArchivo = arrArchivoExtension[arrArchivoExtension.Length - 1];

                    FileHelper fileHelper = new FileHelper();
                    iconName = fileHelper.iconFile(extensionArchivo);

                    // Registro del archivo en la base atado a la tarea principal.
                    byte[] bytesFileName = Encoding.Default.GetBytes(fileName);
                    fileName = Encoding.UTF8.GetString(bytesFileName);

                    string soloNombre = Path.GetFileNameWithoutExtension(fileName);

                    registro.Nombre_Archivo = fileName;
                    registro.Extension_Archivo = extensionArchivo;
                    registro.Codigo_Archivo = context.Request.Form.Get("Id_RegTareas") + "-" + numeroArchivosOrden.ToString();
                    registro.Nombre_ArchivoCodigo = soloNombre + "_" + registro.Codigo_Archivo + "." + extensionArchivo;
                    registro.Ruta_Archivo = folderPath;
                    registro.Descripcion_Archivo = fileName;
                    registro.Icon_Nombre = iconName;
                    registro.Id_RegDetTareas = 0;
                    registro.Id_RegTareas = Convert.ToInt32(context.Request.Form.Get("Id_RegTareas"));
                    registro.Orden_Archivo = numeroArchivosOrden;
                    registro.Usu_Modificacion = Convert.ToInt32(IdUsuarioSession);
                    registro.Ip_Modificacion = context.Request.UserHostAddress;
                    registro.idServicio = Convert.ToInt32(context.Request.Form.Get("idServicio"));
                    // Se graba el archivo en la carpeta definida y con el nombre indicado
                    postedFile.SaveAs(folderPath + registro.Nombre_ArchivoCodigo);

                    respuestaInsertarArchivo = NegTareas.RTAInsertaArchivoTarea(registro);
                    

                    if (elemento > 0)
                    {
                        resultado += "|";
                    }
                    resultado += fileName + ";" + "../descargas/" + registro.Nombre_ArchivoCodigo + ";" + registro.Icon_Nombre;

                    numeroArchivosOrden = numeroArchivosOrden + 1;
                }

                if (context.Request.Files.Count > 1)
                {
                    mensajeRespuesta = "Archivos cargados con éxito.";
                }
                else
                {
                    mensajeRespuesta = "Archivo cargado con éxito.";
                }
                
                respuesta = responseMessage("1", mensajeRespuesta, "success", resultado);

            }
            else
            {
                respuesta = responseMessage("0", "Debe seleccionar un archivo.", "danger");
            }

            

            return respuesta;

        }

        // Función para armar respuesta que se envía al ajax
        public string responseMessage(string estado, string mensaje, string tipoMensaje, string resultado = "")
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
    }
}
