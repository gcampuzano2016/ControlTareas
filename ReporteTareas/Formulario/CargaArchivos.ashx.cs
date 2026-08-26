using CapaEntidad;
using CapaNegocio;
using FilesHelper;
using JSONHelper;
using SeguridadAppHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

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

                if (action == "CargarArchivosInfoContratos")
                {
                    existAction = true;
                    responseAction.Append(CargarArchivosInfoContratos(context));
                }

                if (action == "CargarArchivosAtencionMed")
                {
                    existAction = true;
                    responseAction.Append(CargarArchivosAtencionMed(context));
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

        public string CargarArchivosAtencionMed(dynamic context)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            string respuesta = "";
            string mensajeRespuesta = "";
            EntArchivoTarea registro = new EntArchivoTarea();

            try
            {
                // Obtener los datos del formulario
                string nombreCompleto = context.Request.Form.Get("nombrePaciente") ?? "";
                string codigoIdentificador = context.Request.Form.Get("codigoPaciente") ?? "";
                string cedula = context.Request.Form.Get("cedula") ?? "";
                string fecha = context.Request.Form.Get("fecha") ?? "";

                // Validar que tengamos los datos necesarios
                if (string.IsNullOrEmpty(nombreCompleto) || string.IsNullOrEmpty(codigoIdentificador))
                {
                    return responseMessage("0", "Faltan datos: nombre del paciente o código identificador.", "danger");
                }

                if (string.IsNullOrEmpty(cedula))
                {
                    return responseMessage("0", "La cédula del paciente es requerida.", "danger");
                }

                // Procesar el nombre del paciente
                string primerNombre = "";
                string segundoNombre = "";
                string primerApellido = "";
                string segundoApellido = "";

                // Separar el nombre completo en palabras utilizando el espacio como delimitador
                var palabras = nombreCompleto.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Asegurarse de que haya al menos palabras antes de acceder a los índices
                primerApellido = palabras.Length > 0 ? palabras[0] : "";
                segundoApellido = palabras.Length > 1 ? palabras[1] : "";
                primerNombre = palabras.Length > 2 ? palabras[2] : "";
                segundoNombre = palabras.Length > 3 ? palabras[3] : "";

                // Crear el nombre del paciente para la carpeta
                string nombrePaciente = $"{primerApellido} {segundoApellido} {primerNombre} {segundoNombre}".Trim();

                // Limpiar el nombre del paciente para evitar caracteres problemáticos en nombres de carpeta
                nombrePaciente = LimpiarNombreCarpeta(nombrePaciente);
                codigoIdentificador = LimpiarNombreCarpeta(codigoIdentificador);
                cedula = LimpiarNombreCarpeta(cedula);

                if (context.Request.Files.Count > 0)
                {
                    // Crear la estructura de carpetas: HistoriasClinicas/{NombrePaciente}/{CodigoIdentificador}/
                    string historiasClinicasFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HistoriasClinicas");
                    string pacienteFolder = Path.Combine(historiasClinicasFolder, nombrePaciente);
                    string codigoFolder = Path.Combine(pacienteFolder, codigoIdentificador);

                    // Crear todas las carpetas necesarias si no existen
                    Directory.CreateDirectory(historiasClinicasFolder);
                    Directory.CreateDirectory(pacienteFolder);
                    Directory.CreateDirectory(codigoFolder);

                    int archivosGuardados = 0;
                    List<string> erroresArchivos = new List<string>();

                    // Iterar sobre todos los archivos subidos
                    for (int elemento = 0; elemento < context.Request.Files.Count; elemento++)
                    {
                        HttpPostedFile postedFile = context.Request.Files.Get(elemento);

                        if (postedFile != null && postedFile.ContentLength > 0)
                        {
                            try
                            {
                                string fileName = Path.GetFileName(postedFile.FileName);

                                // Validar que el archivo tenga nombre
                                if (string.IsNullOrEmpty(fileName))
                                {
                                    erroresArchivos.Add($"Archivo {elemento + 1}: nombre de archivo vacío");
                                    continue;
                                }

                                // Extraer la extensión del archivo
                                string extensionArchivo = Path.GetExtension(fileName);
                                FileHelper fileHelper = new FileHelper();
                                string iconName = fileHelper.iconFile(extensionArchivo);

                                // Convertir el nombre del archivo para evitar problemas de codificación
                                byte[] bytesFileName = Encoding.Default.GetBytes(fileName);
                                fileName = Encoding.UTF8.GetString(bytesFileName);

                                // Limpiar el nombre del archivo
                                fileName = LimpiarNombreArchivo(fileName);

                                string soloNombre = Path.GetFileNameWithoutExtension(fileName);
                                registro.Nombre_ArchivoCodigo = soloNombre + extensionArchivo;

                                // Crear nombre único si el archivo ya existe
                                string rutaCompleta = Path.Combine(codigoFolder, registro.Nombre_ArchivoCodigo);

                                if (File.Exists(rutaCompleta))
                                {
                                    string nuevoNombreBase = soloNombre + "_reemplazado";
                                    string nuevoNombre = nuevoNombreBase + extensionArchivo;
                                    rutaCompleta = Path.Combine(codigoFolder, nuevoNombre);

                                    int contador = 1;
                                    while (File.Exists(rutaCompleta))
                                    {
                                        nuevoNombre = $"{nuevoNombreBase}{contador}{extensionArchivo}";
                                        rutaCompleta = Path.Combine(codigoFolder, nuevoNombre);
                                        contador++;
                                    }

                                    registro.Nombre_ArchivoCodigo = nuevoNombre;
                                }

                                // Guardar el archivo en la ruta especificada
                                postedFile.SaveAs(rutaCompleta);
                                archivosGuardados++;
                            }
                            catch (Exception exArchivo)
                            {
                                erroresArchivos.Add($"Error con archivo {elemento + 1}: {exArchivo.Message}");
                            }
                        }
                    }

                    // Construir mensaje de respuesta
                    if (archivosGuardados > 0)
                    {
                        mensajeRespuesta = archivosGuardados > 1
                            ? $"Se cargaron {archivosGuardados} archivos con éxito en la carpeta: {nombrePaciente}/{codigoIdentificador}"
                            : $"Archivo cargado con éxito en la carpeta: {nombrePaciente}/{codigoIdentificador}";

                        if (erroresArchivos.Count > 0)
                        {
                            mensajeRespuesta += $" (Con {erroresArchivos.Count} errores)";
                        }

                        respuesta = responseMessage("1", mensajeRespuesta, "success");
                    }
                    else
                    {
                        string errorMessage = "No se pudo cargar ningún archivo.";
                        if (erroresArchivos.Count > 0)
                        {
                            errorMessage += " Errores: " + string.Join("; ", erroresArchivos);
                        }
                        respuesta = responseMessage("0", errorMessage, "danger");
                    }
                }
                else
                {
                    respuesta = responseMessage("0", "Debe seleccionar al menos un archivo.", "danger");
                }
            }
            catch (Exception ex)
            {
                respuesta = responseMessage("0", $"Error al cargar archivos: {ex.Message}", "danger");
            }

            return respuesta;
        }

        // Método auxiliar para limpiar nombres de carpetas
        private string LimpiarNombreCarpeta(string nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return "";

            // Caracteres no permitidos en nombres de carpetas
            char[] caracteresProhibidos = Path.GetInvalidPathChars()
                .Concat(Path.GetInvalidFileNameChars())
                .Concat(new char[] { '<', '>', ':', '"', '|', '?', '*' })
                .Distinct()
                .ToArray();

            foreach (char c in caracteresProhibidos)
            {
                nombre = nombre.Replace(c, '_');
            }

            // Remover espacios múltiples y al inicio/final
            nombre = System.Text.RegularExpressions.Regex.Replace(nombre, @"\s+", " ").Trim();

            return nombre;
        }

        // Método auxiliar para limpiar nombres de archivos
        private string LimpiarNombreArchivo(string nombreArchivo)
        {
            if (string.IsNullOrEmpty(nombreArchivo)) return "";

            // Diccionario de reemplazos para caracteres con tilde y especiales
            var reemplazos = new Dictionary<char, char>
            {
                {'á', 'a'}, {'à', 'a'}, {'ä', 'a'}, {'â', 'a'},
                {'é', 'e'}, {'è', 'e'}, {'ë', 'e'}, {'ê', 'e'},
                {'í', 'i'}, {'ì', 'i'}, {'ï', 'i'}, {'î', 'i'},
                {'ó', 'o'}, {'ò', 'o'}, {'ö', 'o'}, {'ô', 'o'},
                {'ú', 'u'}, {'ù', 'u'}, {'ü', 'u'}, {'û', 'u'},
                {'ñ', 'n'}, {'ç', 'c'},
                {'Á', 'A'}, {'À', 'A'}, {'Ä', 'A'}, {'Â', 'A'},
                {'É', 'E'}, {'È', 'E'}, {'Ë', 'E'}, {'Ê', 'E'},
                {'Í', 'I'}, {'Ì', 'I'}, {'Ï', 'I'}, {'Î', 'I'},
                {'Ó', 'O'}, {'Ò', 'O'}, {'Ö', 'O'}, {'Ô', 'O'},
                {'Ú', 'U'}, {'Ù', 'U'}, {'Ü', 'U'}, {'Û', 'U'},
                {'Ñ', 'N'}, {'Ç', 'C'}
            };

            StringBuilder resultado = new StringBuilder();

            foreach (char c in nombreArchivo)
            {
                if (reemplazos.ContainsKey(c))
                {
                    resultado.Append(reemplazos[c]);
                }
                else if (char.IsLetterOrDigit(c) || c == ' ' || c == '.' || c == '-')
                {
                    resultado.Append(c);
                }
                else
                {
                    resultado.Append('_');
                }
            }

            // Limpia múltiples guiones bajos consecutivos
            string nombreLimpio = Regex.Replace(resultado.ToString(), @"_+", "_");

            // Elimina guiones bajos al inicio y final
            return nombreLimpio.Trim('_');
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

                maximaOrdenArchivosTarea = NegTareas.MaximaOrdenArchivosTarea(Convert.ToInt32(context.Request.Form.Get("Id_RegTareas")), Convert.ToInt32(context.Request.Form.Get("idServicio")));
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


        public string CargarArchivosInfoContratos(dynamic context)

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

                maximaOrdenArchivosTarea = NegTareas.MaximaOrdenArchivosTarea(Convert.ToInt32(context.Request.Form.Get("Id_RegTareas")), Convert.ToInt32(context.Request.Form.Get("idServicio")));

                numeroArchivosOrden = Convert.ToInt32(maximaOrdenArchivosTarea.resultado) + 1;

                string codContrato = context.Request.Form.Get("codContrato");

                //folderPath = context.Server.MapPath("~/descargas/");

                folderPath = context.Server.MapPath("~/InfoContratos/") + codContrato + "/";

                // Verificar si la carpeta no existe y crearla

                if (!Directory.Exists(folderPath))

                {

                    Directory.CreateDirectory(folderPath);

                }

                respuesta = responseMessage("1", mensajeRespuesta, "success");

                string session = context.Request.Form.Get("session");

                //IdUsuarioSession = seguridad.Desencripta(session.ToString());


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

                    registro.Nombre_ArchivoCodigo = soloNombre + "." + extensionArchivo;

                    // Se graba el archivo en la carpeta definida y con el nombre indicado

                    postedFile.SaveAs(folderPath + registro.Nombre_ArchivoCodigo);

                }

                if (context.Request.Files.Count > 1)

                {

                    mensajeRespuesta = "Archivos cargados con éxito.";

                }

                else

                {

                    //respuesta = respuestaInsertarArchivo.SerializaToJson();

                }

                respuesta = responseMessage("1", mensajeRespuesta, "success", resultado);

            }

            else

            {

                respuesta = responseMessage("0", "Debe seleccionar un archivo.", "danger");

            }

            return respuesta;

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
