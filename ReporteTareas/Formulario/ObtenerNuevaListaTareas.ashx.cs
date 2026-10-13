using CapaEntidad;
using CapaNegocio;
using CorreoHelper;
using JSONHelperNuevo;
using Newtonsoft.Json;
using PDF;
using SeguridadAppHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;



namespace ReporteTareas.Formulario
{
    /// <summary>
    /// Descripción breve de ObtenerNuevaListaTareas
    /// </summary>
    public class ObtenerNuevaListaTareas : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "application/json; charset=utf-8";

            dynamic parametros;
            StringBuilder responseAction = new StringBuilder();
            if (context.Request.ContentType.Contains("json"))
            {
                var inputStream = new System.IO.StreamReader(context.Request.InputStream);
                var inputJson = inputStream.ReadToEnd();

                JavaScriptSerializer i = new JavaScriptSerializer();
                parametros = i.Deserialize(inputJson.ToString(), typeof(object));

                var parameters = parametros[0]["parameters"];
                var Action = parametros[0]["action"];
                Boolean existAction = false;


                /********************************************************
                 *            Keys de acceso a las funciones            *
                 ********************************************************/

                if (Action == "BuscarEmpleadoPorCedula")
                {
                    context.Response.ContentEncoding = System.Text.Encoding.UTF8;
                    context.Response.ContentType = "application/json; charset=utf-8";

                    existAction = true;
                    responseAction.Append(BuscarEmpleadoPorCedula(parameters));
                }


                if (Action == "BuscarContactoEmpleadoPorCedula") // -----> Dep Medico
                {
                    context.Response.ContentEncoding = System.Text.Encoding.UTF8;
                    context.Response.ContentType = "application/json; charset=utf-8";

                    existAction = true;
                    responseAction.Append(BuscarContactoEmpleadoPorCedula(parameters));
                }

                if (Action == "BuscarListaFormularios") // -----> Dep Medico
                {
                    existAction = true;
                    responseAction.Append(BuscarListaFormularios(parameters));
                }

                if (Action == "AbrirDocHistoria") // -----> Dep Medico
                {
                    existAction = true;
                    responseAction.Append(AbrirDocHistoria(parameters));
                }

                if (Action == "GuardarContactoEmergencia") // -----> Dep Medico
                {
                    existAction = true;
                    responseAction.Append(GuardarContactoEmergencia(parameters));
                }

                if (Action == "BuscarCodigoCIE") // -----> Dep Medico
                {
                    context.Response.ContentEncoding = System.Text.Encoding.UTF8;
                    context.Response.ContentType = "application/json; charset=utf-8";

                    existAction = true;
                    responseAction.Append(BuscarCodigoCIE(parameters));
                }

                if (Action == "GuardarNuevaHistoria") // -----> Dep Medico
                {
                    context.Response.ContentEncoding = System.Text.Encoding.UTF8;
                    context.Response.ContentType = "application/json; charset=utf-8";

                    existAction = true;
                    responseAction.Append(GuardarNuevaHistoria(parameters));
                }

                //if (Action == "InformacionMedica") // -----> Dep Medico
                //{
                //    existAction = true;
                //    responseAction.Append(InformacionMedica(parameters));
                //}

                if (Action == "GuardarHisInmunizaciones") // -----> Dep Medico
                {
                    existAction = true;
                    responseAction.Append(GuardarHisInmunizaciones(parameters));
                }


                if (Action == "InformacionMedica") // -----> Dep Medico
                {
                    existAction = true;
                    responseAction.Append(InformacionMedica(parameters));
                }

                if (Action == "BuscarListaCliente") // -----> Contratos
                {
                    existAction = true;
                    responseAction.Append(BuscarListaClientes(parameters));
                }

                if (Action == "ConsultarOrdenServicioPedido") // -----> Contratos
                {
                    existAction = true;
                    responseAction.Append(ConsultarOrdenServicioPedido(parameters));
                }
                if (Action == "BuscarContrato") // -----> Contratos
                {
                    context.Response.ContentEncoding = System.Text.Encoding.UTF8;
                    context.Response.ContentType = "application/json; charset=utf-8";

                    existAction = true;
                    responseAction.Append(BuscarContrato(parameters));
                }
                if (Action == "GuardarNuevoInfoContrato") // -----> Contratos
                {
                    existAction = true;
                    responseAction.Append(GuardarNuevoInfoContrato(parameters));
                }

                if (Action == "BuscarListaArchivosContrato") // -----> Contratos
                {
                    existAction = true;
                    responseAction.Append(BuscarListaArchivosContrato(parameters));
                }

                if (Action == "AbrirDocArchivo") // -----> Contratos
                {
                    existAction = true;
                    responseAction.Append(AbrirDocArchivo(parameters));
                }

                if (Action == "EliminarArchivo") // -----> Contratos
                {
                    existAction = true;
                    responseAction.Append(EliminarArchivo(parameters));
                }


                if (Action == "GuardarConfiguracionEncuesta")
                {
                    existAction = true;
                    responseAction.Append(GuardarConfiguracionEncuesta(parameters));
                }

                if (Action == "ReporteConsultarPolizasFiltros") // -----> Contratos
                {
                    existAction = true;
                    responseAction.Append(ObteneConsultarPolizasFiltro(parameters));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger", ""));
                }

                if (Action == "GuardarAtencionMedica") // -----> Dep Medico
                {
                    existAction = true;
                    responseAction.Append(GuardarAtencionMedica(parameters));
                }
                if (Action == "ObteneConsultarDatosPerVulnerable") // -----> Dep Medico
                {
                    existAction = true;
                    responseAction.Append(ObteneDatosPerVulnerable(parameters));
                }

                if (Action == "ObtenerBuscarListaHsCln") // -----> Dep Medico
                {
                    existAction = true;
                    responseAction.Append(BuscarListaHsCln(parameters));
                }
                if (Action == "BuscarListaArchivosGenerico") // -----> Archivos
                {
                    existAction = true;
                    responseAction.Append(BuscarListaArchivosGenerico(parameters));
                }

                if (Action == "AbrirArchivoGenerico") // -----> Archivos GTH
                {
                    existAction = true;
                    responseAction.Append(AbrirArchivoGenerico(parameters));
                }

                if (Action == "ObteneConsultarDatosPerVulnerable") // -----> Dep Medico
                {
                    existAction = true;
                    responseAction.Append(ObteneDatosPerVulnerable(parameters));
                }


            }
            context.Response.ContentType = "application/json";
            context.Response.Charset = "utf-8";
            context.Response.Write(responseAction.ToString());
        }

        /********************************************************
         *                      FUNCIONES                       *
         ********************************************************/


        public string InformacionMedica(dynamic parameters)
        {
            EntRespuesta respuesta = new EntRespuesta();
            List<string> mensajesError = new List<string>();
            string jsonResponse = string.Empty;
            try
            {
                // Obtener los parámetros
                string nomCarpeta = "HistoriasClinicas";
                string nombreArchivo = parameters["nombreArchivo"];
                string nombreHoja = parameters["nombreHoja"];
                string nombre = parameters["nombre"];

                if (parameters["campos"] == null)
                {
                    throw new ArgumentException("El parámetro 'campos' es obligatorio.");
                }

                var campos = new Dictionary<string, string>();

                foreach (var campo in parameters["campos"])
                {
                    string nombreCampo = campo["nombre"];
                    string celda = campo["celda"];
                    campos.Add(nombreCampo, celda);
                }

                // Suponiendo que los datos se reciben como una cadena de texto JSON
                var jsonData = EditExcel.ObtenerDatosFormulario(nomCarpeta, nombre, nombreArchivo, nombreHoja, campos);

                object data = "";

                if (parameters["operacion"] == "2")
                {
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        try
                        {
                            // Deserializar el JSON a una lista de EntInfoRelevanteSalud
                            var listaEntidades = JsonConvert.DeserializeObject<List<EntInfoRelevanteSalud>>(jsonData);
                            int anio = parameters["anio"];

                            if (listaEntidades != null && listaEntidades.Any())
                            {
                                // Fecha límite (variable con fecha 2024)
                                DateTime fechaLimite = new DateTime(anio, 1, 1);

                                // Transformar y filtrar listaEntidades
                                var listaDatosMed = listaEntidades
                                    .Where(entidad => entidad.datos != null && entidad.datos.fecha != null) // Asegurar que datos y fecha no sean nulos
                                    .Where(entidad => DateTime.TryParse(entidad.datos.fecha, out DateTime fechaEntidad) && fechaEntidad >= fechaLimite) // Filtrar por fecha
                                    .Select(entidad => entidad.datos)
                                    .ToList();

                                // Verificar que la transformación fue exitosa
                                if (listaDatosMed.Any())
                                {
                                    // Enviar solo los datos relevantes a la capa de negocio
                                    respuesta = NegInfoAdicionalHisClinica.Sp_InsUpdInfoRelevante(listaDatosMed, 1);
                                }
                                else
                                {
                                    respuesta.mensaje = "No se encontraron datos válidos para cargar.";
                                }
                            }
                            else
                            {
                                respuesta.mensaje = "La lista deserializada está vacía o no contiene elementos.";
                            }
                        }
                        catch (Exception ex)
                        {
                            respuesta.mensaje = "Error al deserializar el JSON: " + ex.Message;
                        }
                    }
                    else
                    {
                        respuesta.mensaje = "El JSON proporcionado está vacío o es nulo.";
                    }
                }
                else
                {
                    try
                    {
                        // Deserializar directamente en el objeto EntInmunizacionesData
                        if (!string.IsNullOrEmpty(jsonData))
                        {
                            data = JsonConvert.DeserializeObject<EntInmunizacionesData>(jsonData);
                            respuesta.mensaje = "Datos obtenidos de correctamente.";
                            respuesta.tipoMensaje = "success";
                            respuesta.resultado = data; // Asignar data a la propiedad resultado

                        }
                        else
                        {
                            respuesta.mensaje = "Se produjo un error al cargar la información de la base de datos o No existe información disponble en la base de datos.";
                            respuesta.tipoMensaje = "error";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al deserializar EntInmunizacionesData: {ex.Message}");
                        respuesta.mensaje = "Error al procesar la solicitud.";
                        respuesta.tipoMensaje = "danger";
                        respuesta.resultado = null; // O puedes asignar un valor por defecto si lo prefieres
                    }
                }

                // Convertir el objeto data a JSON para enviarlo como respuesta
                //jsonResponse = JsonConvert.SerializeObject(data, Formatting.Indented);

            }
            catch (Exception ex)
            {
                respuesta.mensaje = ex.Message;
                respuesta.tipoMensaje = "danger";
            }
            //return jsonResponse;
            return respuesta.SerializaToJson2();
        }


        public string ObteneDatosPerVulnerable(dynamic parameters)
        {
            EntHsCln Datos = null;
            EntRespuesta respuesta = new EntRespuesta();
            int op = Convert.ToInt32(parameters["op"]);
            string ciEmpleado = parameters["session"];

            try
            {
                if (op == 1) //Opcion para obtener la informacino de un solo registro 
                {
                    Datos = NegHsCln.Consulta_Sp_DatosPerVulnerable(1, ciEmpleado);

                    // Verificar si el registro fue encontrado
                    if (Datos == null)
                    {
                        return responseMessage("0", "No se encontró el registro de Per Vulnerable especificado.", "warning", "");
                    }

                    //contrato.opcion = 2; // Asignar el valor correcto
                    return Datos.SerializaToJson2(); // Retornar el contrato en formato JSON
                }
                else if (op == 2)
                {
                    // Obtener la lista de registros de Personas Vulnerables
                    List<EntHsCln> ListaRegistros = NegHsCln.ConsultaLista_Sp_DatosPerVulnerable(2, "");

                    // Verificar si la lista está vacía
                    if (ListaRegistros == null || ListaRegistros.Count == 0)
                    {
                        return responseMessage("0", "No se encontraron los registros de Personas Vulnerables.", "warning", "");
                    }

                    return JsonConvert.SerializeObject(ListaRegistros); // Retornar la lista en JSON
                }
                else
                {
                    return responseMessage("0", "Operación no válida.", "danger", "");
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al obtener los datos. Error en ashx.cs " + ex.Message, "danger", "");
            }
        }

        public string BuscarListaHsCln(dynamic parameters)
        {
            List<EntHsCln> Lista = null;
            EntHsCln archivi = new EntHsCln();
            EntRespuesta respuesta = new EntRespuesta();

            try
            {
                string ciEmpleado = parameters["session"];
                int opc = Convert.ToInt32(parameters["opc"]);
                string f_ini = parameters["fecha_ini"];
                string f_fin = parameters["fecha_fin"];

                Lista = NegHsCln.ConsultaSp_RTAConsultarTodasHsClnPorCI(ciEmpleado, opc, f_ini, f_fin);

                if (Lista != null && Lista.Count > 0)
                {
                    //respuesta.estado = "1";
                    //respuesta.mensaje = "Consulta realizada con éxito.";
                    //respuesta.tipoMensaje = "success";
                    respuesta.resultado = Lista;
                }
                else
                {
                    respuesta.estado = "0";
                    //respuesta.mensaje = "No se encontraron datos.";
                    //respuesta.tipoMensaje = "warning";
                    respuesta.resultado = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en BuscarListaHsCln: " + ex.ToString());

                respuesta.estado = "0";
                respuesta.mensaje = "Ocurrió un error al obtener los datos: " + ex.Message;
                respuesta.tipoMensaje = "danger";
                respuesta.resultado = null;
            }

            string valor = Lista.SerializaToJson2();

            return Lista.SerializaToJson2();
        }



        public string BuscarListaArchivosGenerico(dynamic parameters)
        {
            // Obtiene los parámetros necesarios del objeto 'parameters'
            string carpetaPrincipal = parameters["CarpetaPrincipal"].ToString();
            string listaCarpetasStr = parameters["listaCarpetas"].ToString();

            // Directorio base donde se almacenan los archivos (igual que contratos)
            string carpetaBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, carpetaPrincipal);

            // Procesar lista de carpetas para construir la ruta
            string outputPath = carpetaBase;
            if (!string.IsNullOrWhiteSpace(listaCarpetasStr))
            {
                // Limpiar caracteres extra que pueden venir de JavaScript
                listaCarpetasStr = listaCarpetasStr.Trim('[', ']', '"');
                var subcarpetas = listaCarpetasStr
                    .Split(',')
                    .Select(s => s.Trim().Trim('"'))
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                // Construir la ruta combinando todas las subcarpetas
                foreach (var subcarpeta in subcarpetas)
                {
                    outputPath = Path.Combine(outputPath, subcarpeta);
                }
            }

            // Log para debug (igual que en contratos)
            logs.logs.VerErrores(outputPath, "LogInfoGenerico");

            // Transformar ruta física en URL (exactamente igual que contratos)
            // string baseUrl = "http//localhost:51037/RTareas";
            string baseUrl = "https://portaldeservicios.dos.com.ec/RTareas";
            string relativePath = outputPath.Replace(AppDomain.CurrentDomain.BaseDirectory, "").Replace("\\", "/");
            string carpetaFolder = $"{baseUrl}/{relativePath.TrimStart('/')}";

            // Verifica si la carpeta existe (igual que contratos)
            if (Directory.Exists(outputPath))
            {
                // Obtiene todos los archivos de la carpeta (exactamente igual que contratos)
                var archivos = Directory.GetFiles(outputPath)
                    .Select(archivo => new
                    {
                        Nombre = Path.GetFileName(archivo),
                        Ruta = $"{carpetaFolder}/{Path.GetFileName(archivo)}"
                    })
                    .ToList();

                if (archivos.Any())
                {
                    Console.WriteLine("Documentos encontrados:");
                    foreach (var archivo in archivos)
                    {
                        Console.WriteLine(archivo);
                    }
                    // Convierte la lista en JSON
                    return JsonConvert.SerializeObject(archivos);
                }
                else
                {
                    return "No hay archivos en la carpeta"; // No hay archivos en la carpeta
                }
            }
            else
            {
                return "Carpeta no encontrada"; // Carpeta no encontrada
            }
        }




        // Método auxiliar para construir URLs
        private string ConstruirUrl(string rutaArchivo, string baseUrl)
        {
            string rutaRelativa = rutaArchivo
                .Replace(AppDomain.CurrentDomain.BaseDirectory, "")
                .Replace("\\", "/");

            return $"{baseUrl.TrimEnd('/')}/{rutaRelativa.TrimStart('/')}";
        }

        public class OpcionesBusqueda
        {
            public string CarpetaPrincipal { get; set; }
            public List<string> RutaSubcarpetas { get; set; } = new List<string>();
            public int NivelesMaximos { get; set; } = 10;
            public bool BusquedaRecursiva { get; set; } = false;
            public string[] ExtensionesPermitidas { get; set; } = null; // null = todas las extensiones
            //public string BaseUrl { get; set; } = "://localhost:51037/RTareas"; 
            public string BaseUrl { get; set; } = "https://portaldeservicios.dos.com.ec/RTareas";
        }

        private string[] ObtenerArchivos(string rutaCompleta, OpcionesBusqueda opciones)
        {
            var searchOption = opciones.BusquedaRecursiva ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            if (opciones.ExtensionesPermitidas?.Any() == true)
            {
                var archivos = new List<string>();
                foreach (var extension in opciones.ExtensionesPermitidas)
                {
                    var patron = $"*.{extension.TrimStart('.')}";
                    archivos.AddRange(Directory.GetFiles(rutaCompleta, patron, searchOption));
                }
                return archivos.ToArray();
            }
            else
            {
                return Directory.GetFiles(rutaCompleta, "*.*", searchOption);
            }
        }


        public string AbrirArchivoGenerico(dynamic campos)
        {
            EntRespuesta respuesta = new EntRespuesta();
            try
            {
                string CarpetaPrincipal = campos["nombreCarpetaPrincipal"];

                // Convertir a array si viene como string separado por algún delimitador, o manejarlo como array
                var subcarpetasTemp = campos["nombreCarpetaSecundaria"];
                string[] Subcarpetas = null;

                // Si viene como array directamente
                if (subcarpetasTemp is string[])
                {
                    Subcarpetas = (string[])subcarpetasTemp;
                }
                // Si viene como string separado por comas, barras, etc.
                else if (subcarpetasTemp is string && !string.IsNullOrEmpty(subcarpetasTemp.ToString()))
                {
                    string subcarpetasStr = subcarpetasTemp.ToString();
                    // Separar por comas para obtener cada elemento del array
                    Subcarpetas = subcarpetasStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    // Limpiar espacios en blanco de cada elemento
                    for (int i = 0; i < Subcarpetas.Length; i++)
                    {
                        Subcarpetas[i] = Subcarpetas[i].Trim();
                    }
                }

                string archivo = campos["nombreArchivo"];

                // Construir la ruta del archivo paso a paso
                // Primero agregar RTareas, luego la carpeta principal
                string rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CarpetaPrincipal);

                // Agregar las subcarpetas al path
                if (Subcarpetas != null && Subcarpetas.Length > 0)
                {
                    foreach (string subcarpeta in Subcarpetas)
                    {
                        rutaArchivo = Path.Combine(rutaArchivo, subcarpeta);
                    }
                }

                // Agregar el archivo al final
                rutaArchivo = Path.Combine(rutaArchivo, archivo);

                // Construcción de ruta física completa
                string rutaCompleta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rutaArchivo);


                // Construcción de la URL del archivo
                //string baseUrl = "://localhost:51037";  // Sin /RTareas al final
                string baseUrl = "https://portaldeservicios.dos.com.ec/RTareas";
                string relativePath = rutaCompleta.Replace(AppDomain.CurrentDomain.BaseDirectory, "").Replace("\\", "/");
                string fileUrl = $"{baseUrl}/{relativePath.TrimStart('/')}";

                // Intentar abrir el archivo
                //Process.Start(fileUrl);

                // Retornar respuesta exitosa
                respuesta.estado = "1";
                respuesta.mensaje = "Archivo encontrado";
                respuesta.tipoMensaje = "success";
                respuesta.resultado = fileUrl; // Devolver la URL del archivo
            }
            catch (Exception ex)
            {
                // En caso de error al abrir el archivo
                respuesta.estado = "0";
                respuesta.mensaje = "Error al abrir el archivo: " + ex.Message;
                respuesta.tipoMensaje = "error";
                respuesta.resultado = "";
            }
            return respuesta.SerializaToJson2();
        }


        public string GuardarAtencionMedica(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();

            string IdUsuarioSession = "";

            try
            {
                EntHsCln registro = new EntHsCln();

                registro.hsCln_ciEmpleado = campos["txtCed"];
                registro.hsCln_Identificador = campos["txtcodigoHsCln"];
                //registro.CLI_NOMBRE = seguridad.Encripta(campos["txtNomContacto"]);

                registro.hsCln_Fecha = campos["txtfechaAtencion"];
                registro.hsCln_Hora = campos["txtHoraAtencion"];
                registro.hsCln_Motivo = campos["txtMotivos"];
                registro.hsCln_Antece = campos["txtAntecedentes"];
                registro.hsCln_SigVit = campos["txtSignos"];
                registro.hsCln_Interv = campos["txtIntervencion"];
                registro.hsCln_Recom = campos["txtRecGeneral"];
                registro.hsCln_Obs = campos["txtObsGeneral"];
                registro.hsCln_Seguir = campos["txthsCln_Seguir"];

                registro.hsVul_Vulnerds = campos["txtVulnerable"];

                respuesta = NegHsCln.Sp_InsertarActualizarHsCln(registro);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");
            }
            return respuesta.SerializaToJson2();
        }



        public string ObteneConsultarPolizasFiltro(dynamic parameters)
        {
            List<EntPoliza> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            int idPedidos = 0;
            string buscar = parameters["buscar"].ToString();
            string fechaInicio = parameters["fechaInicio"].ToString();
            string fechaFinal = parameters["fechaFinal"].ToString();
            string beneficiario = parameters["beneficiario"].ToString();
            string Proceso = parameters["Proceso"].ToString();
            int idFecha = 0;

            try
            {
                Lista = NegPoliza.ConsultaSp_RTAListaPolizas(buscar, fechaInicio, fechaFinal, idPedidos, beneficiario, Proceso, idFecha);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson2();
        }

        public string BuscarEmpleadoPorCedula(dynamic parameters)
        {
            List<EntEmpleado> Lista = null;
            EntEmpleado empleado = new EntEmpleado();
            EntRespuesta respuesta = new EntRespuesta();

            SeguridadHelper seguridad = new SeguridadHelper();
            string cedula = parameters["session"].ToString();
            string IdUsuarioSession = "";
            try
            {
                Lista = NegEmpleado.Sp_RTA_ConsultarEmpleadoPorCedula(cedula);

            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }

            return Lista.SerializaToJson2();
        }


        public string BuscarContactoEmpleadoPorCedula(dynamic parameters)
        {
            List<EntInfoAdicionalHisClinica> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            SeguridadHelper seguridad = new SeguridadHelper();
            string cedula = parameters["session"].ToString();
            string IdUsuarioSession = "";
            try
            {
                Lista = NegInfoAdicionalHisClinica.Sp_RTA_ConsultarInfoPorId(cedula);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }
            return Lista.SerializaToJson2();
        }
        public string BuscarListaFormularios(dynamic parameters)
        {
            // Obtiene los parámetros necesarios del objeto 'parameters'
            string tipo = parameters["tipo"].ToString();
            var nombreCompleto = parameters["nombre"].ToString();
            string sociedad = parameters["sociedad"].ToString();
            string areaTrabajo = parameters["areaTrabajo"].ToString();
            string fecha1 = parameters["fecha1"].ToString();
            string fecha2 = parameters["fecha2"].ToString();

            // Separar el nombre completo en palabras utilizando el espacio como delimitador
            var palabras = nombreCompleto.Split(' ');
            // Asegurarse de que haya al menos cuatro palabras antes de acceder a los índices
            string primerApellido = palabras.Length > 0 ? palabras[0] : "";
            string segundoApellido = palabras.Length > 1 ? palabras[1] : "";
            string primerNombre = palabras.Length > 2 ? palabras[2] : "";
            string segundoNombre = palabras.Length > 3 ? palabras[3] : "";

            //try
            //{
            // Directorio base donde se almacenan las historias clínicas
            string historiasClinicasFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HistoriasClinicas");

            // Obtenemos el nombre del paciente o identificador único
            string nombrePaciente = $"{primerApellido} {segundoApellido} {primerNombre} {segundoNombre}";

            // Combinar el directorio base con el nombre del paciente para obtener la carpeta del paciente
            string outputPath = Path.Combine(historiasClinicasFolder, nombrePaciente);
            logs.logs.VerErrores(outputPath, "LogHistoriaClinica");

            // Transformar ruta física en URL
            //string baseUrl = "https://portaldeservicios.dos.com.ec/RTareas";
            //string relativePath = outputPath.Replace(AppDomain.CurrentDomain.BaseDirectory, "").Replace("\\", "/");
            //string pacienteFolder = $"{baseUrl}/{relativePath.TrimStart('/')}";

            // Verifica si la carpeta del paciente existe
            if (Directory.Exists(outputPath))
            {
                // Filtra los archivos de la carpeta del paciente
                var archivosFiltrados = Directory.GetFiles(outputPath)
                 .Where(archivo =>
                 {
                     string nombreDocumento = Path.GetFileName(archivo);
                     string[] partesNombre = nombreDocumento.Split('_');

                     // Verifica que el nombre del archivo tenga al menos 4 secciones
                     if (partesNombre.Length >= 4)
                     {
                         string tipoDocumento = partesNombre[0];
                         string fechaDocumentoStr = partesNombre[2];

                         // Variable para almacenar la fecha formateada
                         string fechaFormateada = "";

                         // Verifica que sea el tipo que se busca
                         if (tipoDocumento == tipo)
                         {
                             // Convierte la fecha del documento en un objeto DateTime
                             if (DateTime.TryParseExact(fechaDocumentoStr, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                             {
                                 Console.WriteLine($"Nombre del documento: {nombreDocumento}");
                                 Console.WriteLine($"Tipo del documento: {tipoDocumento}");
                                 Console.WriteLine($"Fecha del documento: {fechaDocumentoStr}");
                                 Console.WriteLine($"Fecha de inicio: {fecha1}");
                                 Console.WriteLine($"Fecha de fin: {fecha2}");
                                 Console.WriteLine("Fecha convertida: " + fecha);

                                 DateTime fechaA = DateTime.ParseExact(fecha1, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                 DateTime fechaB = DateTime.ParseExact(fecha2, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                                 // Comprueba si la fecha del documento está en el rango especificado
                                 if (fecha >= fechaA && fecha <= fechaB)
                                 {
                                     // Formatea la fecha como "yyyy/MM/dd"
                                     fechaFormateada = fecha.ToString("yyyy/MM/dd");
                                     return true;
                                 }
                                 else
                                 {
                                     Console.WriteLine("La fecha del documento no está en el rango especificado.");
                                 }
                             }
                             else
                             {
                                 // La conversión de fecha falló, puedes manejarlo aquí
                                 Console.WriteLine("No se pudo convertir la fecha.");
                             }
                         }
                         if (tipo == "Todas")
                         {
                             // Convierte la fecha del documento en un objeto DateTime
                             if (DateTime.TryParseExact(fechaDocumentoStr, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                             {
                                 Console.WriteLine($"Nombre del documento: {nombreDocumento}");
                                 Console.WriteLine($"Tipo del documento: {tipoDocumento}");
                                 Console.WriteLine($"Fecha del documento: {fechaDocumentoStr}");
                                 Console.WriteLine($"Fecha de inicio: {fecha1}");
                                 Console.WriteLine($"Fecha de fin: {fecha2}");
                                 Console.WriteLine("Fecha convertida: " + fecha);

                                 DateTime fechaA = DateTime.ParseExact(fecha1, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                 DateTime fechaB = DateTime.ParseExact(fecha2, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                                 // Comprueba si la fecha del documento está en el rango especificado
                                 if (fecha >= fechaA && fecha <= fechaB)
                                 {
                                     // Formatea la fecha como "yyyy/MM/dd"
                                     fechaFormateada = fecha.ToString("yyyy/MM/dd");
                                     return true;
                                 }
                                 else
                                 {
                                     Console.WriteLine("La fecha del documento no está en el rango especificado.");
                                 }
                             }
                         }

                         // Formatea la fecha como "yyyy/MM/dd"
                         fechaFormateada = fechaDocumentoStr.Insert(4, "/").Insert(7, "/");
                     }

                     return false;
                 })
                 .Select(archivo =>
                 {
                     string nombreDocumento = Path.GetFileName(archivo);
                     string[] partesNombre = nombreDocumento.Split('_');


                     string tipoDocumento = partesNombre[0];
                     string fechaDocumentoStr = partesNombre[2];

                     // Formatea la fecha como "yyyy/MM/dd" en el objeto anónimo
                     string fechaFormateada = fechaDocumentoStr.Insert(4, "/").Insert(7, "/");

                     // Proyecta los datos del archivo en un objeto anónimo
                     return new
                     {
                         Tipo = tipoDocumento,
                         Nombre = nombreDocumento,
                         Sociedad = sociedad,
                         AreaTrabajo = areaTrabajo,
                         Fecha = fechaFormateada
                     };
                 })
                 .ToList();


                if (archivosFiltrados.Any())
                {
                    Console.WriteLine("Documentos encontrados:");
                    foreach (var archivoFiltrado in archivosFiltrados)
                    {
                        Console.WriteLine(archivoFiltrado);
                    }

                    // Convierte la lista de objetos anónimos en una cadena JSON
                    string json = JsonConvert.SerializeObject(archivosFiltrados);
                    return json;
                }
                else
                {
                    // No se encontraron documentos que cumplan con los criterios de búsqueda.
                    return "{}";
                }
            }
            else
            {
                // Carpeta de paciente no encontrada.
                return "{}";
            }
            //}
            //catch (Exception ex)
            //{
            //    // En caso de error, retorna un JSON con el mensaje de error
            //    return JsonConvert.SerializeObject(new { error = ex.Message });
            //}
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
            //logs.logs.VerErrores(outputPath, "LogHistoriaClinica");

            // Abre el archivo recién creado utilizando la aplicación asociada en el sistema.
            //Process.Start(outputPath);

            string baseUrl = "https://portaldeservicios.dos.com.ec/RTareas";
            string relativePath = outputPath.Replace(AppDomain.CurrentDomain.BaseDirectory, "").Replace("\\", "/");
            string fileUrl = $"{baseUrl}/{relativePath.TrimStart('/')}";

            // Registrar log para verificar la URL generada (opcional)
            logs.logs.VerErrores(fileUrl, "LogHistoriaClinica");

            // Retornar la URL generada
            //Process.Start(fileUrl);
            logs.logs.VerErrores(fileUrl, "LogHistoriaClinica");
            EntRespuesta respuesta = new EntRespuesta();
            respuesta.mensaje = fileUrl;

            return respuesta.SerializaToJson2();

        }
        public string GuardarContactoEmergencia(dynamic campos)
        {
            EntRespuesta respuesta = new EntRespuesta();
            /*SeguridadHelper seguridad = new SeguridadHelper();
            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);*/
            var opc = campos["txtOpcion"];
            try
            {
                EntInfoAdicionalHisClinica reg = new EntInfoAdicionalHisClinica();
                if (opc == "1")
                {
                    reg.IdInfo = 1;
                    reg.IdEmpleado = campos["session"];
                    reg.Lentes = campos["txtLentes"];
                    reg.GruposVulnerables = campos["txtGrpVulnerables"];
                    reg.RegistraAlergias = campos["txtAlergias"];
                    reg.NombreAlergia = campos["txtNombreAlergia"];
                    reg.TipoAlergia = campos["txtTipoAlergia"];
                    reg.ReaccionesAlergia = campos["txtReaccionesAlergia"];
                    respuesta = NegInfoAdicionalHisClinica.Sp_InsUpdInfoAdicional(reg, 1);
                }
                if (opc == "2")
                {
                    reg.IdInfo = 1;
                    reg.IdEmpleado = campos["session"];
                    reg.NomContactoEmergencia = campos["txtNombreContacto"];
                    reg.TelfContactoEmergencia = campos["txtTelfContacto"];
                    reg.ParentescoContacto = campos["txtParentescoContacto"];
                    respuesta = NegInfoAdicionalHisClinica.Sp_InsUpdInfoAdicional(reg, 2);
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");
            }
            //GuardarHistoria();
            return respuesta.SerializaToJson2();
            //return "";
        }
        public string BuscarCodigoCIE(dynamic parameters)
        {
            List<EntCodigoCIE> Lista = null;
            EntRespuesta respuesta = new EntRespuesta();
            SeguridadHelper seguridad = new SeguridadHelper();
            string codigo = parameters["session"].ToString();
            try
            {
                Lista = NegCodigoCIE.Sp_RTA_ConsultarCodigoCIE(codigo);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al obtener los datos. " + ex.Message.ToString(), "danger", "");
            }
            return Lista.SerializaToJson2();
        }

        private string SiNo(string valor, string comparar = "si")
        {
            return valor?.ToLower() == comparar ? "X" : "";
        }

        public string GetOpcion(string valor, string opcion)
        {
            if (string.IsNullOrEmpty(valor))
                return "";

            return valor.Trim().ToLower() == opcion.Trim().ToLower() ? "X" : "";
        }

        // CAMBIAR el nombre del método privado
        private string CheckCampo(dynamic campos, string key)
        {
            try
            {
                if (!campos.ContainsKey(key)) return "";
                if (campos[key] == null) return "";
                string v = campos[key].ToString().ToLower();
                return (v == "true" || v == "1") ? "X" : "";
            }
            catch { return ""; }
        }

        private string Campo(dynamic campos, string key)
        {
            try
            {
                return campos.ContainsKey(key) && campos[key] != null ? campos[key].ToString() : "";
            }
            catch
            {
                return "";
            }
        }

        private string FormatFecha(dynamic campos, string key)
        {
            string val = Campo(campos, key);
            DateTime f;
            if (DateTime.TryParse(val, out f))
                return f.ToString("yyyy/MM/dd");
            return val;
        }


        public string GuardarNuevaHistoria(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();
            //EntEmpleado empleado = new EntEmpleado();

            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);

            //empleado = NegEmpleado.Sp_RTA_ConsultarEmpleadoPorId(IdUsuarioSession);

            // Variables que usamos para mejorar el catch de los errores
            string nomHojaStr = "";
            string archivoStr = "";
            string formulario = "";

            try
            {
                // ─── Nombre paciente ───────────────────────────────────────────
                string nombreCompleto = Campo(campos, "txtNombre");
                string[] palabras = nombreCompleto.Split(' ');
                string primerApellido = palabras.Length > 0 ? palabras[0] : "";
                string segundoApellido = palabras.Length > 1 ? palabras[1] : "";
                string primerNombre = palabras.Length > 2 ? palabras[2] : "";
                string segundoNombre = palabras.Length > 3 ? palabras[3] : "";

                // ─── Nombre doctor ─────────────────────────────────────────────
                string nombreDoctor = Campo(campos, "nombre");
                string[] doctor = nombreDoctor.Split(' ');
                string primerApellidoDoc = doctor.Length > 0 ? doctor[0] : "";
                string segundoApellidoDoc = doctor.Length > 1 ? doctor[1] : "";
                string primerNombreDoc = doctor.Length > 2 ? doctor[2] : "";
                string segundoNombreDoc = doctor.Length > 3 ? doctor[3] : "";

                // ─── Sexo ──────────────────────────────────────────────────────
                string sexoh = "";
                string sexom = "";
                string sexoRaw = Campo(campos, "txtSexo").ToUpper();
                if (sexoRaw == "FEMENINO")
                { sexom = "X"; }
                else if (sexoRaw == "MASCULINO")
                { sexoh = "X"; }

                string sexoLetra = "";
                if (sexoRaw == "FEMENINO")
                { sexoLetra = "F"; }
                else if (sexoRaw == "MASCULINO")
                { sexoLetra = "M"; }

                formulario = Campo(campos, "formulario");

                // ─── Diccionario de datos ──────────────────────────────────────
                Dictionary<string, string> datos = new Dictionary<string, string>();

                datos["numhistoria"] = Campo(campos, "txtNumHistoria");
                datos["numarchivo"] = Campo(campos, "txtNumArchivo");
                datos["primerapellido"] = primerApellido;
                datos["segundoapellido"] = segundoApellido;
                datos["primernombre"] = primerNombre;
                datos["segundonombre"] = segundoNombre;
                datos["sexoh"] = sexoh;
                datos["sexom"] = sexom;
                datos["sexoLetra"] = sexoLetra;

                // ─── Fecha de nacimiento ──────────────────────────────────────────────────
                string fechaNacRaw = Campo(campos, "fechaNac");
                string fecNacDia = "";
                string fecNacMes = "";
                string fecNacAnio = "";
                DateTime fecNac;
                if (DateTime.TryParse(fechaNacRaw, out fecNac))
                {
                    fecNacDia = fecNac.Day.ToString("D2");   // 01, 02... 31
                    fecNacMes = fecNac.Month.ToString("D2"); // 01, 02... 12
                    fecNacAnio = fecNac.Year.ToString();      // 2024
                }
                datos["fecNacDia"] = fecNacDia;
                datos["fecNacMes"] = fecNacMes;
                datos["fecNacAnio"] = fecNacAnio;

                datos["sociedad"] = Campo(campos, "txtSociedad");
                datos["edad"] = Campo(campos, "txtEdad");
                datos["puestotrabajo"] = Campo(campos, "txtPuestoTrabajo");
                datos["areatrabajo"] = Campo(campos, "txtAreaTrabajo");

                // Religion
                datos["catolica"] = GetOpcion(Campo(campos, "txtReligion"), "catolica");
                datos["evangelica"] = GetOpcion(Campo(campos, "txtReligion"), "evangelica");
                datos["testigo"] = GetOpcion(Campo(campos, "txtReligion"), "testigo");
                datos["mormona"] = GetOpcion(Campo(campos, "txtReligion"), "mormona");
                datos["otrareligion"] = GetOpcion(Campo(campos, "txtReligion"), "otra");

                datos["gruposanguineo"] = Campo(campos, "txtGruposanguineo");
                datos["lateralidad"] = Campo(campos, "txtLateralidad");

                // ─── Atencion prioritaria ──────────────────────────────────────────────────
                string atencionRaw = Campo(campos, "AtencionPrioritariaSelect");
                datos["atencionEmb"] = GetOpcion(atencionRaw, "atencionEmb");
                datos["atencionDis"] = GetOpcion(atencionRaw, "atencionDis");
                datos["atencionCat"] = GetOpcion(atencionRaw, "atencionCat");
                datos["atencionLac"] = GetOpcion(atencionRaw, "atencionLac");
                datos["atencionAdu"] = GetOpcion(atencionRaw, "atencionAdu");

                // Fechas 
                datos["fechaAtencion"] = FormatFecha(campos, "fechaAtencion");
                datos["fechaIngresoTrab"] = FormatFecha(campos, "fechaIngresoTrab");
                datos["fechaReintegroTrab"] = FormatFecha(campos, "fechaReintegroTrab");
                datos["fechaUltimoDiaLab"] = FormatFecha(campos, "fechaUltimoDiaLab");

                // ─── Motivo de consulta ──────────────────────────────────────────────────
                string motivoRaw = Campo(campos, "selectMotivoConsulta");
                datos["tipoconsultaIngreso"] = GetOpcion(motivoRaw, "tipoconsultaIngreso");
                datos["tipoconsultaPeriodico"] = GetOpcion(motivoRaw, "tipoconsultaPeriodico");
                datos["tipoconsultaReintegro"] = GetOpcion(motivoRaw, "tipoconsultaReintegro");
                datos["tipoconsultaRetiro"] = GetOpcion(motivoRaw, "tipoconsultaRetiro");

                // ─── Permisos de SI o NO ──────────────────────────────────────────────────
                // Autorizacion transfusion
                datos["autorizatransfusionsi"] = SiNo(Campo(campos, "txtAutorizacionBtn1"), "si");
                datos["autorizatransfusionno"] = SiNo(Campo(campos, "txtAutorizacionBtn1"), "no");
                // Tratamiento hormonal
                datos["tratamientohormonalsi"] = SiNo(Campo(campos, "txtAutorizacionBtn2"), "si");
                datos["tratamientohormonalno"] = SiNo(Campo(campos, "txtAutorizacionBtn2"), "no");
                datos["tratamientohormonalcual"] = Campo(campos, "tratamientohormonalcual");



                // Reintegro
                datos["fechaUltimoDia"] = Campo(campos, "fechaUltimoDia");
                datos["totalDias"] = Campo(campos, "txtTotalDias");
                datos["fechaReintegro"] = Campo(campos, "fechaReintegro");
                datos["causaSalida"] = Campo(campos, "txtCausaSalida");

                // Retiro
                datos["fechaInicioLab"] = Campo(campos, "fechaInicioLab");
                datos["fechaSalida"] = Campo(campos, "fechaSalida");
                datos["txtTotalMeses"] = Campo(campos, "txtTotalMeses");

                // Certificado
                datos["fecEmision"] = Campo(campos, "txtFechaEmision");

                // Evaluacion tipo 5                
                datos["evaIngreso"] = GetOpcion(Campo(campos, "txtEvaluacion"), "Ingreso");
                datos["evaPeriodico"] = GetOpcion(Campo(campos, "txtEvaluacion"), "Periódico");
                datos["evaReintegro"] = GetOpcion(Campo(campos, "txtEvaluacion"), "Reintegro");
                datos["evaRetiro"] = GetOpcion(Campo(campos, "txtEvaluacion"), "Retiro");

                // Consulta y antecedentes
                datos["motivoconsulta"] = Campo(campos, "txtMotivoConsulta");
                datos["antpersonales"] = Campo(campos, "txtAntecedentesPersonales");
                datos["antfamiliares"] = Campo(campos, "txtAntecedentesFamiliares");
                datos["enfermedadactual"] = Campo(campos, "txtEnfermedadActual");


                // Gineco
                datos["menarquia"] = Campo(campos, "txtMenarquia");
                datos["ciclos"] = Campo(campos, "txtCiclos");
                datos["ultmenstruacion"] = Campo(campos, "fechaUltimaMens");
                datos["gestas"] = Campo(campos, "txtNumGestas");
                datos["partos"] = Campo(campos, "txtNumPartos");
                datos["cesareas"] = Campo(campos, "txtNumCesareas");
                datos["abortos"] = Campo(campos, "txtNumAbortos");

                datos["metodoplanfamSi1"] = SiNo(Campo(campos, "txtPlanificacionFam1"), "si");
                datos["metodoplanfamNo1"] = SiNo(Campo(campos, "txtPlanificacionFam1"), "no");
                datos["metodoplanfamTipo1"] = Campo(campos, "txtTipoPlanificacion1");
                datos["metodoplanfamNoresponde"] = "";
                datos["metodoplanfamMasNoresponde"] = "";
                datos["metodoplanfamSi2"] = SiNo(Campo(campos, "txtPlanificacionFam2"), "si");
                datos["metodoplanfamNo2"] = SiNo(Campo(campos, "txtPlanificacionFam2"), "no");
                datos["metodoplanfamTipo2"] = Campo(campos, "txtTipoPlanificacion2");
                datos["hijosvivosM"] = Campo(campos, "txtNumHijosVivosM");
                datos["hijosmuertosM"] = Campo(campos, "txtNumHijosMuertosM");

                datos["txtNomExamen1F"] = Campo(campos, "txtNomExamen1F");
                datos["fechaExam1F"] = Campo(campos, "fechaExam1F");
                datos["txtResExamen1F"] = Campo(campos, "txtResExamen1F");
                datos["txtNomExamen2F"] = Campo(campos, "txtNomExamen2F");
                datos["fechaExam2F"] = Campo(campos, "fechaExam2F");
                datos["txtResExamen2F"] = Campo(campos, "txtResExamen2F");

                datos["txtNomExamen1M"] = Campo(campos, "txtNomExamen1M");
                datos["fechaExam1M"] = Campo(campos, "fechaExam1M");
                datos["txtResExamen1M"] = Campo(campos, "txtResExamen1M");
                datos["txtNomExamen2M"] = Campo(campos, "txtNomExamen2M");
                datos["fechaExam2M"] = Campo(campos, "fechaExam2M");
                datos["txtResExamen2M"] = Campo(campos, "txtResExamen2M");


                datos["hijosvivosF"] = Campo(campos, "txtNumHijosVivos");
                datos["hijosmuertosF"] = Campo(campos, "txtNumHijosMuertos");
                datos["vidaSxActivaSi"] = SiNo(Campo(campos, "txtvidaSxActiva"), "SI");
                datos["vidaSxActivaNo"] = SiNo(Campo(campos, "txtvidaSxActiva"), "NO");


                // Orientacion sexual
                datos["orientlesbiana"] = GetOpcion(Campo(campos, "txtOrientacionSexual"), "lesbiana");
                datos["orientgay"] = GetOpcion(Campo(campos, "txtOrientacionSexual"), "gay");
                datos["orientbisexual"] = GetOpcion(Campo(campos, "txtOrientacionSexual"), "bisexual");
                datos["orientheterosexual"] = GetOpcion(Campo(campos, "txtOrientacionSexual"), "heterosexual");
                datos["orientnosabe"] = GetOpcion(Campo(campos, "txtOrientacionSexual"), "nosabe");

                // Identidad genero
                datos["identfemenino"] = GetOpcion(Campo(campos, "txtIdentidadGenero"), "identfemenino");
                datos["identmasculino"] = GetOpcion(Campo(campos, "txtIdentidadGenero"), "identmasculino");
                datos["identransfem"] = GetOpcion(Campo(campos, "txtIdentidadGenero"), "identransfem");
                datos["identransmasc"] = GetOpcion(Campo(campos, "txtIdentidadGenero"), "identransmasc");
                datos["identnosabe"] = GetOpcion(Campo(campos, "txtIdentidadGenero"), "identnosabe");

                // Discapacidad
                datos["discapacidadsi"] = SiNo(Campo(campos, "txtDiscapacidad"), "SI");
                datos["discapacidadno"] = SiNo(Campo(campos, "txtDiscapacidad"), "NO");
                datos["discapacidadtipo"] = Campo(campos, "txtTipoDiscapacidad");
                datos["discapacidadporcentaje"] = Campo(campos, "txtPorcentajeDiscapacidad");

                datos["fechaFormulario"] = FormatFecha(campos, "txtfechaFormulario");
                datos["horaFormulario"] = Campo(campos, "txthoraFormulario");
                datos["actividadesrelevantes"] = Campo(campos, "txtActRelevantes");

                // Examenes gineco
                datos["papnicolaouSi"] = SiNo(Campo(campos, "txtExam1"), "SI");
                datos["papnicolaouNo"] = SiNo(Campo(campos, "txtExam1"), "NO");
                datos["papnicolaouTiempo"] = Campo(campos, "txtanioExam1");
                datos["papnicolaouResultado"] = Campo(campos, "txtResultadoExam1");
                datos["ecomamaSi"] = SiNo(Campo(campos, "txtExam2"), "SI");
                datos["ecomamaNo"] = SiNo(Campo(campos, "txtExam2"), "NO");
                datos["ecomamaTiempo"] = Campo(campos, "txtanioExam2");
                datos["ecomamaResultado"] = Campo(campos, "txtResultadoExam2");
                datos["colposcopiaSi"] = SiNo(Campo(campos, "txtExam3"), "SI");
                datos["colposcopiaNo"] = SiNo(Campo(campos, "txtExam3"), "NO");
                datos["colposcopiaTiempo"] = Campo(campos, "txtanioExam3");
                datos["colposcopiaResultado"] = Campo(campos, "txtResultadoExam3");
                datos["mamografiaSi"] = SiNo(Campo(campos, "txtExam4"), "SI");
                datos["mamografiaNo"] = SiNo(Campo(campos, "txtExam4"), "NO");
                datos["mamografiaTiempo"] = Campo(campos, "txtanioExam4");
                datos["mamografiaResultado"] = Campo(campos, "txtResultadoExam4");
                datos["antigenoprostSi"] = SiNo(Campo(campos, "txtExam5"), "SI");
                datos["antigenoprostNo"] = SiNo(Campo(campos, "txtExam5"), "NO");
                datos["antigenoprostTiempo"] = Campo(campos, "txtanioExam5");
                datos["antigenoprostResultado"] = Campo(campos, "txtResultadoExam5");
                datos["ecoprostaticoSi"] = SiNo(Campo(campos, "txtExam6"), "SI");
                datos["ecoprostaticoNo"] = SiNo(Campo(campos, "txtExam6"), "NO");
                datos["ecoprostaticoTiempo"] = Campo(campos, "txtanioExam6");
                datos["ecoprostaticoResultado"] = Campo(campos, "txtResultadoExam6");

                // Tabaco / Alcohol / Otras drogas
                datos["tabacoSi"] = SiNo(Campo(campos, "txtTabacoSelect"), "si");
                datos["tabacoNo"] = SiNo(Campo(campos, "txtTabacoSelect"), "no");
                datos["tabacoTiempo"] = Campo(campos, "txtTiempoconsumoTabaco");
                datos["tabacoCantidad"] = Campo(campos, "txtCantidadTabaco");
                datos["tabacoExconsumidor"] = Campo(campos, "exConsumidoraSelectTabaco");
                datos["tabacoTiempoabst"] = Campo(campos, "txtTiempoAbstinenciaTabaco");

                datos["alcoholSi"] = SiNo(Campo(campos, "txtalcoholSelect"), "si");
                datos["alcoholNo"] = SiNo(Campo(campos, "txtalcoholSelect"), "no");
                datos["alcoholTiempo"] = Campo(campos, "txtTiempoconsumoAlcohol");
                datos["alcoholCantidad"] = Campo(campos, "txtCantidadAlcohol");
                datos["alcoholExconsumidor"] = Campo(campos, "exConsumidoraSelectAlcohol");
                datos["alcoholTiempoabst"] = Campo(campos, "txtTiempoAbstinenciaAlcohol");

                datos["otrassustancias"] = Campo(campos, "txtOtraSustancia");
                datos["otrasdrogasSi"] = SiNo(Campo(campos, "txtotraSelect"), "si");
                datos["otrasdrogasNo"] = SiNo(Campo(campos, "txtotraSelect"), "no");
                datos["otrasdrogasTiempo"] = Campo(campos, "txtTiempoconsumoOtra");
                datos["otrasdrogasCantidad"] = Campo(campos, "txtCantidadOtra");
                datos["otrasdrogasExconsumidor"] = Campo(campos, "exConsumidorSelectOtra");
                datos["otrasdrogasTiempoabst"] = Campo(campos, "txtTiempoAbstinenciaOtra");

                datos["obsConsumoSustancias"] = Campo(campos, "txtHbtsIncidentes");

                // Actividad fisica / Medicacion
                datos["actividadfisicaSi"] = SiNo(Campo(campos, "txtActividadFisicaSelect"), "si");
                datos["actividadfisicaNo"] = SiNo(Campo(campos, "txtActividadFisicaSelect"), "no");
                datos["actividafisicaCual1"] = Campo(campos, "txtCualActividad1");
                datos["actividafisicaCual2"] = Campo(campos, "txtCualActividad2");
                datos["actividafisicaCual3"] = Campo(campos, "txtCualActividad3");
                datos["actividafisicaTiempo1"] = Campo(campos, "txtFrecuenciaActividad1");
                datos["actividafisicaTiempo2"] = Campo(campos, "txtFrecuenciaActividad2");
                datos["actividafisicaTiempo3"] = Campo(campos, "txtFrecuenciaActividad3");

                datos["medicacionhabSi"] = SiNo(Campo(campos, "txtMedicacionHabSelect"), "si");
                datos["medicacionhabNo"] = SiNo(Campo(campos, "txtMedicacionHabSelect"), "no");
                datos["medicacionhabCual1"] = Campo(campos, "txtCualMedicamento1");
                datos["medicacionhabCual2"] = Campo(campos, "txtCualMedicamento2");
                datos["medicacionhabCual3"] = Campo(campos, "txtCualMedicamento3");
                datos["medicacionhabCant1"] = Campo(campos, "txtCantdidadMed1");
                datos["medicacionhabCant2"] = Campo(campos, "txtCantdidadMed2");
                datos["medicacionhabCant3"] = Campo(campos, "txtCantdidadMed3");

                // Accidentes / Enfermedades
                datos["accidentestrabajoSi"] = SiNo(Campo(campos, "AccidentesTrabajoSelect"), "si");
                datos["accidentestrabajoNo"] = SiNo(Campo(campos, "AccidentesTrabajoSelect"), "no");
                datos["especificaracctrabajo"] = Campo(campos, "txtEspecificarAccidentesTrabajoSelect");
                datos["accTrabAnio"] = Campo(campos, "txtfechaAccTrabAnio");
                datos["accTrabMes"] = Campo(campos, "txtfechaAccTrabMes");
                datos["accTrabDia"] = Campo(campos, "txtfechaAccTrabDia");
                datos["acctrabajoobservaciones"] = Campo(campos, "txtObservacionesAccTrabajo");
                datos["enfermedadesprofSi"] = SiNo(Campo(campos, "EnfermedadesProfSelect"), "si");
                datos["enfermedadesprofNo"] = SiNo(Campo(campos, "EnfermedadesProfSelect"), "no");
                datos["especificarenfermedadesprof"] = Campo(campos, "txtEspecificarEnfermedadesProfSelect");
                datos["enfProfAnio"] = Campo(campos, "txtfechaEnfProfAnio");
                datos["enfProfMes"] = Campo(campos, "txtfechaEnfProfMes");
                datos["enfProfDia"] = Campo(campos, "txtfechaEnfProfDia");
                datos["enfermedadesprofobservaciones"] = Campo(campos, "txtObservacionesEnfermedadesProf");

                // Antecedentes familiares
                datos["antFam1"] = CheckCampo(campos, "AntFamA");
                datos["antFam2"] = CheckCampo(campos, "AntFamB");
                datos["antFam3"] = CheckCampo(campos, "AntFamC");
                datos["antFam4"] = CheckCampo(campos, "AntFamD");
                datos["antFam5"] = CheckCampo(campos, "AntFamE");
                datos["antFam6"] = CheckCampo(campos, "AntFamF");
                datos["antFam7"] = CheckCampo(campos, "AntFamG");
                datos["antFam8"] = CheckCampo(campos, "AntFamH");

                // Revision organos
                datos["revisionpiel"] = CheckCampo(campos, "RevicionA");
                datos["revisionsentidos"] = CheckCampo(campos, "RevicionB");
                datos["revisionrespiratorio"] = CheckCampo(campos, "RevicionC");
                datos["revisioncardio"] = CheckCampo(campos, "RevicionD");
                datos["revisiondigestivo"] = CheckCampo(campos, "RevicionE");
                datos["revisiongenito"] = CheckCampo(campos, "RevicionF");
                datos["revisionmusculo"] = CheckCampo(campos, "RevicionG");
                datos["revisionendocrino"] = CheckCampo(campos, "RevicionH");
                datos["revisionhemo"] = CheckCampo(campos, "RevicionI");
                datos["revisionnervioso"] = CheckCampo(campos, "RevicionJ");
                datos["revisionorganosdescripcion"] = Campo(campos, "txtRevisionOrganos");

                // Constantes vitales
                datos["constpresion"] = Campo(campos, "txtConstPresionArterial");
                datos["consttemperatura"] = Campo(campos, "txtConstTemperatura");
                datos["constfreccardiaca"] = Campo(campos, "txtConstFrecuenciaCardicaca");
                datos["constsaturacion"] = Campo(campos, "txtConstSaturacionOxigeno");
                datos["constfrecrespiratoria"] = Campo(campos, "txtConstFrecuenciaRespiratoria");
                datos["constpeso"] = Campo(campos, "txtConstPeso");
                datos["consttalla"] = Campo(campos, "txtConstTalla");
                datos["constindice"] = Campo(campos, "txtConstMasaCorporal");
                datos["constperimetro"] = Campo(campos, "txtConstPerimetroAbdominal");

                // Examen fisico
                datos["examfisicopielA"] = CheckCampo(campos, "txtpielA");
                datos["examfisicopielB"] = CheckCampo(campos, "txtpielB");
                datos["examfisicopielC"] = CheckCampo(campos, "txtpielC");
                datos["examfisicoojosA"] = CheckCampo(campos, "txtojosA");
                datos["examfisicoojosB"] = CheckCampo(campos, "txtojosB");
                datos["examfisicoojosC"] = CheckCampo(campos, "txtojosC");
                datos["examfisicoojosD"] = CheckCampo(campos, "txtojosD");
                datos["examfisicoojosE"] = CheckCampo(campos, "txtojosE");
                datos["examfisicooidoA"] = CheckCampo(campos, "txtoidoA");
                datos["examfisicooidoB"] = CheckCampo(campos, "txtoidoB");
                datos["examfisicooidoC"] = CheckCampo(campos, "txtoidoC");
                datos["examfisicooroA"] = CheckCampo(campos, "txtoroA");
                datos["examfisicooroB"] = CheckCampo(campos, "txtoroB");
                datos["examfisicooroC"] = CheckCampo(campos, "txtoroC");
                datos["examfisicooroD"] = CheckCampo(campos, "txtoroD");
                datos["examfisicooroE"] = CheckCampo(campos, "txtoroE");
                datos["examfisiconarizA"] = CheckCampo(campos, "txtnarizA");
                datos["examfisiconarizB"] = CheckCampo(campos, "txtnarizB");
                datos["examfisiconarizC"] = CheckCampo(campos, "txtnarizC");
                datos["examfisiconarizD"] = CheckCampo(campos, "txtnarizD");
                datos["examfisicocuelloA"] = CheckCampo(campos, "txtcuelloA");
                datos["examfisicocuelloB"] = CheckCampo(campos, "txtcuelloB");
                datos["examfisicotorax1A"] = CheckCampo(campos, "txttoraxA");
                datos["examfisicotorax1B"] = CheckCampo(campos, "txttoraxB");
                datos["examfisicotorax2A"] = CheckCampo(campos, "txttoraxC");
                datos["examfisicotorax2B"] = CheckCampo(campos, "txtToraxD");
                datos["examfisicoabdomenA"] = CheckCampo(campos, "txtabdomenA");
                datos["examfisicoabdomenB"] = CheckCampo(campos, "txtabdomenB");
                datos["examfisicocolumnaA"] = CheckCampo(campos, "txtcolumnaA");
                datos["examfisicocolumnaB"] = CheckCampo(campos, "txtcolumnaB");
                datos["examfisicocolumnaC"] = CheckCampo(campos, "txtcolumnaC");
                datos["examfisicopelvisA"] = CheckCampo(campos, "txtpelvisA");
                datos["examfisicopelvisB"] = CheckCampo(campos, "txtpelvisB");
                datos["examfisicoextremA"] = CheckCampo(campos, "txtextremidadesA");
                datos["examfisicoextremB"] = CheckCampo(campos, "txtextremidadesB");
                datos["examfisicoextremC"] = CheckCampo(campos, "txtextremidadesC");
                datos["examfisiconeuroA"] = CheckCampo(campos, "txtneurologicoA");
                datos["examfisiconeuroB"] = CheckCampo(campos, "txtneurologicoB");
                datos["examfisiconeuroC"] = CheckCampo(campos, "txtneurologicoC");
                datos["examfisiconeuroD"] = CheckCampo(campos, "txtneurologicoD");
                datos["examfisicoobservacion"] = Campo(campos, "txtExamFisicoObservacion");

                // Examenes realizados
                datos["examNom1"] = Campo(campos, "txtNomExamen1");
                datos["examFecha1"] = Campo(campos, "fechaExam1");
                datos["examResultado1"] = Campo(campos, "txtResExamen1");
                datos["examNom2"] = Campo(campos, "txtNomExamen2");
                datos["examFecha2"] = Campo(campos, "fechaExam2");
                datos["examResultado2"] = Campo(campos, "txtResExamen2");
                datos["examNom3"] = Campo(campos, "txtNomExamen3");
                datos["examFecha3"] = Campo(campos, "fechaExam3");
                datos["examResultado3"] = Campo(campos, "txtResExamen3");
                datos["examNom4"] = Campo(campos, "txtNomExamen4");
                datos["examFecha4"] = Campo(campos, "fechaExam4");
                datos["examResultado4"] = Campo(campos, "txtResExamen4");
                datos["examNom5"] = Campo(campos, "txtNomExamen5");
                datos["examFecha5"] = Campo(campos, "fechaExam5");
                datos["examResultado5"] = Campo(campos, "txtResExamen5");
                datos["examObservaciones"] = Campo(campos, "txtExamenbservacion");

                // Actividades extra laborales
                datos["actextralaborales"] = Campo(campos, "txtDescActividadExtraLab");
                datos["actextralaboralesfecha"] = Campo(campos, "fechaActExtraLab");

                // Diagnosticos
                datos["DiagDescripcion1"] = Campo(campos, "txtDiagDescripcion1");
                datos["DiagCIE1"] = Campo(campos, "txtDiagCIE1");
                datos["DiagPre1"] = GetOpcion(Campo(campos, "txtDiagnositicoSelect1"), "PRE");
                datos["DiagDef1"] = GetOpcion(Campo(campos, "txtDiagnositicoSelect1"), "DEF");
                datos["DiagDescripcion2"] = Campo(campos, "txtDiagDescripcion2");
                datos["DiagCIE2"] = Campo(campos, "txtDiagCIE2");
                datos["DiagPre2"] = GetOpcion(Campo(campos, "txtDiagnositicoSelect2"), "PRE");
                datos["DiagDef2"] = GetOpcion(Campo(campos, "txtDiagnositicoSelect2"), "DEF");
                datos["DiagDescripcion3"] = Campo(campos, "txtDiagDescripcion3");
                datos["DiagCIE3"] = Campo(campos, "txtDiagCIE3");
                datos["DiagPre3"] = GetOpcion(Campo(campos, "txtDiagnositicoSelect3"), "PRE");
                datos["DiagDef3"] = GetOpcion(Campo(campos, "txtDiagnositicoSelect3"), "DEF");

                datos["DiagDescripcion4"] = Campo(campos, "txtDiagDescripcion4");
                datos["DiagCIE4"] = Campo(campos, "txtDiagCIE4");
                datos["DiagPre4"] = GetOpcion(Campo(campos, "txtDiagnositicoSelect4"), "PRE");
                datos["DiagDef4"] = GetOpcion(Campo(campos, "txtDiagnositicoSelect4"), "DEF");
                datos["DiagDescripcion5"] = Campo(campos, "txtDiagDescripcion5");
                datos["DiagCIE5"] = Campo(campos, "txtDiagCIE5");
                datos["DiagPre5"] = GetOpcion(Campo(campos, "txtDiagnositicoSelect5"), "PRE");
                datos["DiagDef5"] = GetOpcion(Campo(campos, "txtDiagnositicoSelect5"), "DEF");
                datos["DiagDescripcion6"] = Campo(campos, "txtDiagDescripcion6");
                datos["DiagCIE6"] = Campo(campos, "txtDiagCIE6");
                datos["DiagPre6"] = GetOpcion(Campo(campos, "txtDiagnositicoSelect6"), "PRE");
                datos["DiagDef6"] = GetOpcion(Campo(campos, "txtDiagnositicoSelect6"), "DEF");


                // Aptitud
                datos["aptitudApto"] = GetOpcion(Campo(campos, "txtAptitudSelect"), "apto");
                datos["aptitudObservacion"] = GetOpcion(Campo(campos, "txtAptitudSelect"), "aptoObservacion");
                datos["aptitudLimitaciones"] = GetOpcion(Campo(campos, "txtAptitudSelect"), "aptoLimitacion");
                datos["aptitudNoapto"] = GetOpcion(Campo(campos, "txtAptitudSelect"), "noApto");
                datos["aptitudObservacionDesc"] = Campo(campos, "txtDescObservacion");
                datos["aptitudLimitacionDesc"] = Campo(campos, "txtDescLimitacion");
                datos["aptitudReubicacionDesc"] = Campo(campos, "txtDescReubicacion");
                datos["descAptitud"] = Campo(campos, "txtDescAptitud");

                // Retiro tipo 5
                datos["retiroSi"] = SiNo(Campo(campos, "txtevalRetiro"), "si");
                datos["retiroNo"] = SiNo(Campo(campos, "txtevalRetiro"), "no");
                datos["diagPresuntiva"] = GetOpcion(Campo(campos, "txtdiag"), "presuntiva");
                datos["diagDefinitiva"] = GetOpcion(Campo(campos, "txtdiag"), "definitiva");
                datos["diagNoAplica"] = GetOpcion(Campo(campos, "txtdiag"), "noAplica");
                datos["relacionSi"] = GetOpcion(Campo(campos, "txtrelacionTrabajo"), "si");
                datos["relacionNo"] = GetOpcion(Campo(campos, "txtrelacionTrabajo"), "no");
                datos["relacionNoAplica"] = GetOpcion(Campo(campos, "txtrelacionTrabajo"), "noAplica");

                // Eval retiro tipo 4
                datos["EvalRetiroSi"] = SiNo(Campo(campos, "EvalRetiroSelect"), "si");
                datos["EvalRetiroNo"] = SiNo(Campo(campos, "EvalRetiroSelect"), "no");

                datos["EvalRetiroCondicionSi"] = SiNo(Campo(campos, "CondicionRetiroSelect"), "si");
                datos["EvalRetiroCondicionNo"] = SiNo(Campo(campos, "CondicionRetiroSelect"), "no");

                datos["EvalRetiroObservacion"] = Campo(campos, "EvalRetiroObservacion");
                datos["descRecomendaciones"] = Campo(campos, "txtRecomendacion");

                // Actividades tipo 4
                datos["actividades1"] = Campo(campos, "actividades1");
                datos["actividades2"] = Campo(campos, "actividades2");
                datos["actividades3"] = Campo(campos, "actividades3");

                // Doctor
                datos["primerApellidoDoc"] = primerApellidoDoc;
                datos["segundoApellidoDoc"] = segundoApellidoDoc;
                datos["primerNombreDoc"] = primerNombreDoc;
                datos["segundoNombreDoc"] = segundoNombreDoc;

                // ─── Antecedentes laborales 1-4 ───────────────────────────────
                string[] riesgoOpciones = new string[] {
                    "antriesgofisico", "antriesgomecanico", "antriesgoquimico",
                    "antriesgobiologico", "antriesgoergonomico", "antriesgopsicosocial"
                };

                for (int i = 1; i <= 4; i++)
                {
                    string riesgo = Campo(campos, "riesgoTrabajoSelect" + i);
                    datos["antempresa" + i] = Campo(campos, "txtAntEmpresa" + i);
                    datos["antpuestotrabajo" + i] = Campo(campos, "txtAntPuestoTrabajo" + i);
                    datos["antactividad" + i] = Campo(campos, "txtAntActividades" + i);
                    datos["anttiempotrabajo" + i] = Campo(campos, "txtAntTiempoTrabajo" + i);
                    datos["antobservaciones" + i] = Campo(campos, "txtObservacion" + i);
                    foreach (string op in riesgoOpciones)
                        datos[op + i] = GetOpcion(riesgo, op);
                }

                // ─── Factores de riesgo 1-3 ───────────────────────────────────
                Dictionary<string, string> mapaColumnas = new Dictionary<string, string>();
                mapaColumnas["TemperaturasAltas"] = "fisicotempaltas";
                mapaColumnas["TemperaturasBajas"] = "fisicotempbajas";
                mapaColumnas["RadiacionIonizante"] = "fisicoionizante";
                mapaColumnas["RadiacionNoIonizante"] = "fisicoNoionizante";
                mapaColumnas["Ruido"] = "fisicoruido";
                mapaColumnas["Vibracion"] = "fisicovibracion";
                mapaColumnas["Iluminacion"] = "fisicoiluminacion";
                mapaColumnas["Ventilacion"] = "fisicoventilacion";
                mapaColumnas["FluidoElectrico"] = "fisicoelectrico";
                mapaColumnas["OtrosFisico"] = "fisicootros";

                mapaColumnas["FaltaSeñalizacion"] = "segfalta";
                mapaColumnas["AtrapamientoMaquinas"] = "mecatrapmaquinas";
                mapaColumnas["AtrapamientoSuperficies"] = "mecatrapsuperficies";
                mapaColumnas["AtrapamientoObjetos"] = "mecatrapobjetos";
                mapaColumnas["CaidaObjetos"] = "meccaidasdeobjetos";
                mapaColumnas["CaidasMismoNivel"] = "meccaidasmismonivel";
                mapaColumnas["CaidasDiferenteNivel"] = "meccaidasdiferentenivel";
                mapaColumnas["ContactoElectrico"] = "meccontactoelectrico";
                mapaColumnas["ContactoSuperficiesTrabajos"] = "meccontacosuperficies";
                mapaColumnas["ProyeccionParticulas"] = "mecproyeccionparticulas";
                mapaColumnas["ProyeccionFluidos"] = "mecproyeccionfluidos";
                mapaColumnas["Pinchazos"] = "mecpinchazos";
                mapaColumnas["Cortes"] = "meccortes";
                mapaColumnas["AtropellamientoVehiculo"] = "mecatropellamientovehiculo";
                mapaColumnas["ChoquesVehicular"] = "mecchoquescolision";
                mapaColumnas["OtrosMecanico"] = "mecanicootros";
                mapaColumnas["Solidos"] = "quimicosolidos";
                mapaColumnas["Polvos"] = "quimicopolvos";
                mapaColumnas["Humos"] = "quimicohumos";
                mapaColumnas["liquidos"] = "quimicoliquidos";
                mapaColumnas["vapores"] = "quimicovapores";
                mapaColumnas["Aerosoles"] = "quimicoaerosoles";
                mapaColumnas["Neblinas"] = "quimiconeblinas";
                mapaColumnas["Gaseosos"] = "quimicogaseosos";
                mapaColumnas["OtrosQuimico"] = "quimicootros";
                mapaColumnas["Virus"] = "biologicovirus";
                mapaColumnas["Hongos"] = "biologicohongos";
                mapaColumnas["Bacterias"] = "biologicobacterias";
                mapaColumnas["Parasitos"] = "biologicoparasitos";
                mapaColumnas["ExposicionVectores"] = "biologicoexpovectores";
                mapaColumnas["ExposicionAnimales"] = "biologicoexpoanimales";
                mapaColumnas["OtrosBiologico"] = "biologicootros";
                mapaColumnas["ManejoCargas"] = "ergonomicomanejomanual";
                mapaColumnas["MovimientoRepetitivos"] = "ergonomicomovimientorep";
                mapaColumnas["PosturasForzadas"] = "ergonomicoposturasforzadas";
                mapaColumnas["TrabajosPVD"] = "ergonomicotrabajopvd";
                mapaColumnas["DiseñoInadecuado"] = "ergonomicodiseño";
                mapaColumnas["OtrosErgonomico"] = "ergonomicootros";

                mapaColumnas["MonotoniaTrabajo"] = "psicosocialmonotonia";
                mapaColumnas["SobrecargaLaboral"] = "psicosocialsobrecarga";
                mapaColumnas["MinuciosidadTarea"] = "psicosocialminuciosidad";
                mapaColumnas["AltaResponsabilidad"] = "psicosocialaltaresponsabilidad";
                mapaColumnas["AutonomiaDecisiones"] = "psicosocialautonomia";
                mapaColumnas["SupervisionDeficiente"] = "psicosocialsupervision";
                mapaColumnas["ConflictoRol"] = "psicosocialconflicto";
                mapaColumnas["FaltaClaridadFunciones"] = "psicosocialfaltaclaridad";
                mapaColumnas["IncorrectaDistribucionTrabajo"] = "psicosocialincorrecta";
                mapaColumnas["TurnosRotativos"] = "psicosocialturnos";
                mapaColumnas["RelacionesInterpersonales"] = "psicosocialrelaciones";
                mapaColumnas["InestabilidadLaboral"] = "psicosocialinestabilidad";
                mapaColumnas["AmenazaDelincuencial"] = "psicosocialamenaza";
                mapaColumnas["OtrosPSicosocial"] = "psicosocialotros";


                string[] selectoresFisico = new string[] { "TemperaturasAltas", "TemperaturasBajas", "RadiacionIonizante", "RadiacionNoIonizante", "Ruido", "Vibracion", "Iluminacion", "Ventilacion", "FluidoElectrico", "OtrosFisico" };
                string[] selectoresMecanico = new string[] { "FaltaSeñalizacion", "AtrapamientoMaquinas", "AtrapamientoSuperficies", "AtrapamientoObjetos", "CaidaObjetos", "CaidasMismoNivel", "CaidasDiferenteNivel", "ContactoElectrico", "ContactoSuperficiesTrabajos", "ProyeccionParticulas", "ProyeccionFluidos", "Pinchazos", "Cortes", "AtropellamientoVehiculo", "ChoquesVehicular", "OtrosMecanico" };
                string[] selectoresQuimico = new string[] { "Solidos", "Polvos", "Humos", "liquidos", "vapores", "Aerosoles", "Neblinas", "Gaseosos", "OtrosQuimico" };
                string[] selectoresBiologico = new string[] { "Virus", "Hongos", "Bacterias", "Parasitos", "ExposicionVectores", "ExposicionAnimales", "OtrosBiologico" };
                string[] selectoresErgonomico = new string[] { "ManejoCargas", "MovimientoRepetitivos", "PosturasForzadas", "TrabajosPVD", "DiseñoInadecuado", "OtrosErgonomico" };
                string[] selectoresPsicosocial = new string[] { "MonotoniaTrabajo", "SobrecargaLaboral", "MinuciosidadTarea", "AltaResponsabilidad", "AutonomiaDecisiones", "SupervisionDeficiente", "ConflictoRol", "FaltaClaridadFunciones", "IncorrectaDistribucionTrabajo", "TurnosRotativos", "RelacionesInterpersonales", "InestabilidadLaboral", "AmenazaDelincuencial", "OtrosPSicosocial" };


                // Quitar selectMedidas del array todosSelectores
                string[][] todosSelectores = new string[][] { selectoresFisico, selectoresMecanico, selectoresQuimico, selectoresBiologico, selectoresErgonomico, selectoresPsicosocial };
                string[] nombresSelectores = new string[] { "Fisico", "Mecanico", "Quimico", "Biologico", "Ergonomico", "PSicosocial" };

                for (int i = 1; i <= 7; i++)
                {
                    datos["factoresriesgopuestotrabajo" + i] = Campo(campos, "txtPuestoTrabajo" + i);
                    datos["factoresriesgoactividades" + i] = Campo(campos, "txtActividadesTrabajo" + i);

                    // Medidas preventivas — texto directo, no selector
                    datos["medidasPreventivasA" + i] = Campo(campos, "txtMedidadPreventivaA" + i);
                    datos["medidasPreventivasB" + i] = Campo(campos, "txtMedidadPreventivaB" + i);
                    datos["medidasPreventivasC" + i] = Campo(campos, "txtMedidadPreventivaC" + i);


                    for (int s = 0; s < nombresSelectores.Length; s++)
                    {
                        string valorSelect = Campo(campos, "txt" + nombresSelectores[s] + "Select" + i);
                        string[] opciones = todosSelectores[s];
                        foreach (string opcion in opciones)
                        {
                            string colName;
                            if (mapaColumnas.TryGetValue(opcion, out colName))
                                datos[colName + i] = GetOpcion(valorSelect, opcion);
                        }
                    }
                }

                string actLabRowsJson = Campo(campos, "actLabRows");

                // Inicializar las 19 filas con valores vacíos
                for (int i = 1; i <= 19; i++)
                {
                    datos["antempresa" + i] = "";
                    datos["antactividad" + i] = "";
                    datos["anttrabanterior" + i] = "";
                    datos["anttrabactual" + i] = "";
                    datos["anttrabtiempo" + i] = "";
                    datos["antaccincidente" + i] = "";
                    datos["antaccaccidente" + i] = "";
                    datos["antaccenfermedad" + i] = "";
                    datos["antcalifsi" + i] = "";
                    datos["antcalifno" + i] = "";
                    datos["antcaliffecha" + i] = "";
                    datos["antcalifespecificar" + i] = "";
                    datos["antcalifobservacion" + i] = "";
                }

                // Llenar solo las filas que tienen datos
                if (!string.IsNullOrEmpty(actLabRowsJson))
                {
                    var filas = new System.Web.Script.Serialization.JavaScriptSerializer()
                                    .Deserialize<List<Dictionary<string, string>>>(actLabRowsJson);

                    for (int i = 0; i < filas.Count; i++)
                    {
                        int n = i + 1;
                        var fila = filas[i];

                        string trabajo = fila.ContainsKey("trabajo") ? fila["trabajo"] : "";
                        string accEnf = fila.ContainsKey("accEnf") ? fila["accEnf"] : "";
                        string iess = fila.ContainsKey("iess") ? fila["iess"] : "";

                        datos["antempresa" + n] = fila.ContainsKey("centro") ? fila["centro"] : "";
                        datos["antactividad" + n] = fila.ContainsKey("actividad") ? fila["actividad"] : "";
                        datos["anttrabanterior" + n] = trabajo == "Anterior" ? "X" : "";
                        datos["anttrabactual" + n] = trabajo == "Actual" ? "X" : "";
                        datos["anttrabtiempo" + n] = fila.ContainsKey("tiempo") ? fila["tiempo"] : "";
                        datos["antaccincidente" + n] = accEnf == "Incidente" ? "X" : "";
                        datos["antaccaccidente" + n] = accEnf == "Accidente" ? "X" : "";
                        datos["antaccenfermedad" + n] = accEnf == "Enfermedad Profesional" ? "X" : "";
                        datos["antcalifsi" + n] = iess == "SI" ? "X" : "";
                        datos["antcalifno" + n] = iess == "NO" ? "X" : "";
                        datos["antcaliffecha" + n] = fila.ContainsKey("fecha") ? fila["fecha"] : "";
                        datos["antcalifespecificar" + n] = fila.ContainsKey("especificar") ? fila["especificar"] : "";
                        datos["antcalifobservacion" + n] = fila.ContainsKey("observacion") ? fila["observacion"] : "";
                    }
                }

                // ─── Construir DataTable ───────────────────────────────────────
                DataTable info = new DataTable();
                info.TableName = "info";
                foreach (string key in datos.Keys)
                    info.Columns.Add(key);

                string[] valores = new string[datos.Values.Count];
                datos.Values.CopyTo(valores, 0);
                info.Rows.Add((object[])valores.ToArray<object>());

                DataSet ds = new DataSet();
                ds.Tables.Add(info);

                // ─── Mapas de archivo, hoja y tipo ────────────────────────────
                string sociedad = datos["sociedad"];

                Dictionary<string, Dictionary<string, string>> mapaArchivos = new Dictionary<string, Dictionary<string, string>>();
                mapaArchivos["1"] = new Dictionary<string, string>(); mapaArchivos["1"]["DOS S.A"] = "TemplatePreocupacional.xlsx"; mapaArchivos["1"]["AGILITY S.A"] = "TemplatePreocupacionalAgility.xlsx"; mapaArchivos["1"]["GREEN DC"] = "TemplatePreocupacionalGreen.xlsx";
                mapaArchivos["2"] = new Dictionary<string, string>(); mapaArchivos["2"]["DOS S.A"] = "TemplatePeriodica.xlsx"; mapaArchivos["2"]["AGILITY S.A"] = "TemplatePeriodicaAgility.xlsx"; mapaArchivos["2"]["GREEN DC"] = "TemplatePeriodicaGreen.xlsx";
                mapaArchivos["3"] = new Dictionary<string, string>(); mapaArchivos["3"]["DOS S.A"] = "TemplateReintegro.xlsx"; mapaArchivos["3"]["AGILITY S.A"] = "TemplateReintegroAgility.xlsx"; mapaArchivos["3"]["GREEN DC"] = "TemplateReintegroGreen.xlsx";
                mapaArchivos["4"] = new Dictionary<string, string>(); mapaArchivos["4"]["DOS S.A"] = "TemplateRetiro.xlsx"; mapaArchivos["4"]["AGILITY S.A"] = "TemplateRetiroAgility.xlsx"; mapaArchivos["4"]["GREEN DC"] = "TemplateRetiroGreen.xlsx";
                mapaArchivos["5"] = new Dictionary<string, string>(); mapaArchivos["5"]["DOS S.A"] = "TemplateCertificado.xlsx"; mapaArchivos["5"]["AGILITY S.A"] = "TemplateCertificadoAgility.xlsx"; mapaArchivos["5"]["GREEN DC"] = "TemplateCertificadoGreen.xlsx";
                mapaArchivos["6"] = new Dictionary<string, string>(); mapaArchivos["6"]["DOS S.A"] = "FORMULARIO_EVALUACIONES_OCUPACIONALES.xlsx"; mapaArchivos["6"]["AGILITY S.A"] = "FORMULARIO_EVALUACIONES_OCUPACIONALESAgility.xlsx"; mapaArchivos["6"]["GREEN DC"] = "FORMULARIO_EVALUACIONES_OCUPACIONALESGreen.xlsx";

                // Hojas simples (1 hoja por formulario)
                Dictionary<string, string> mapaHojas = new Dictionary<string, string>();
                mapaHojas["1"] = "077-PREOCUPA. INICIO 1-3";
                mapaHojas["2"] = "078-PERIODICA ";
                mapaHojas["3"] = "079-REINTEGRO";
                mapaHojas["4"] = "080-RETIRO 1-2";
                mapaHojas["5"] = "CERTIFICADO DE AL";

                // Hojas multiples (formularios con mas de una hoja)
                Dictionary<string, string[]> mapaHojasMultiples = new Dictionary<string, string[]>();
                mapaHojasMultiples["6"] = new string[] { "EVALUACION OCUPACIONAL 1-3", "EVALUACION OCUPACIONAL 2-3", "EVALUACION OCUPACIONAL 3-3" };

                Dictionary<string, string> mapaTipos = new Dictionary<string, string>();
                mapaTipos["1"] = "PREOCUPACIONAL";
                mapaTipos["2"] = "PERIODICA";
                mapaTipos["3"] = "REINTEGRO";
                mapaTipos["4"] = "RETIRO";
                mapaTipos["5"] = "CERTIFICADO";
                mapaTipos["6"] = "PREOCUPACIONAL2";

                //string nomHojaStr = "";
                string tipoFormulario = "";
                //string archivoStr = "";
                string nhVal = "";
                string tfVal = "";
                string afVal = "";

                if (mapaHojas.TryGetValue(formulario, out nhVal))
                    nomHojaStr = nhVal;

                if (mapaTipos.TryGetValue(formulario, out tfVal))
                    tipoFormulario = tfVal;

                Dictionary<string, string> archForm;
                if (mapaArchivos.TryGetValue(formulario, out archForm))
                    if (archForm.TryGetValue(sociedad, out afVal))
                        archivoStr = afVal;

                // ─── Rutas ────────────────────────────────────────────────────
                string rutaQR = "";
                string rutaQR2 = "";

                string nombrePaciente = primerApellido + " " + segundoApellido + " " + primerNombre + " " + segundoNombre;
                string historiasClinicasFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HistoriasClinicas");
                string pacienteFolder = Path.Combine(historiasClinicasFolder, nombrePaciente);

                try
                {
                    if (!Directory.Exists(pacienteFolder))
                        Directory.CreateDirectory(pacienteFolder);

                    // ================== VERSION ORIGINAL ==================
                    /*
                    string nombreArchivo = tipoFormulario + "_" + nombrePaciente + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx";
                    string outputPath = Path.Combine(pacienteFolder, nombreArchivo);
                    string outputPathEnc = outputPath + ".enc";
                    */

                    // ================== VERSION ACTUAL ==================
                    string nombreArchivo = tipoFormulario + "_" + nombrePaciente + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx";
                    string outputPath = Path.Combine(pacienteFolder, nombreArchivo);

                    // ================== DEBUG RUTAS ==================
                    logs.logs.VerErrores("=== DEBUG RUTAS ===", "LogHistoriaClinica");
                    logs.logs.VerErrores("BaseDirectory: " + AppDomain.CurrentDomain.BaseDirectory, "LogHistoriaClinica");
                    logs.logs.VerErrores("pacienteFolder: " + pacienteFolder, "LogHistoriaClinica");
                    logs.logs.VerErrores("outputPath: " + outputPath, "LogHistoriaClinica");

                    // 🔍 Validación básica
                    if (ds == null || ds.Tables.Count == 0)
                    {
                        respuesta.estado = "0";
                        respuesta.mensaje = "No existen datos para generar el archivo.";
                        respuesta.tipoMensaje = "warning";
                        return respuesta.SerializaToJson2();
                    }

                    // ------------------ GENERACIÓN EXCEL ------------------
                    if (mapaHojasMultiples.ContainsKey(formulario))
                    {
                        string[] hojas = mapaHojasMultiples[formulario];

                        foreach (string hoja in hojas)
                        {
                            TemplateExcel.FillReport(outputPath, archivoStr, hoja, ds, new string[] { "{", "}" }, rutaQR, rutaQR2, nombrePaciente);
                        }
                    }
                    else
                    {
                        TemplateExcel.FillReport(outputPath, archivoStr, nomHojaStr, ds, new string[] { "{", "}" }, rutaQR, rutaQR2, nombrePaciente);
                    }

                    logs.logs.VerErrores(outputPath, "LogHistoriaClinica");

                    // ================== VALIDAR ARCHIVO ==================
                    if (!File.Exists(outputPath))
                    {
                        respuesta.estado = "0";
                        respuesta.mensaje = "El archivo no se pudo generar.";
                        respuesta.tipoMensaje = "danger";
                        return respuesta.SerializaToJson2();
                    }

                    // ================== PARAMETROS PARA ABRIR ==================
                    var parametrosAbrir = new Dictionary<string, object>();
                    parametrosAbrir.Add("nombreArchivo", nombreArchivo);
                    parametrosAbrir.Add("nombreCarpeta", nombrePaciente);

                    // 🔥 LLAMAR TU FUNCIÓN
                    string fileUrlRaw = AbrirDocHistoria(parametrosAbrir);

                    // ================== LIMPIAR JSON ==================
                    string fileUrl = "";

                    try
                    {
                        dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(fileUrlRaw);
                        fileUrl = obj.mensaje;
                    }
                    catch
                    {
                        fileUrl = fileUrlRaw;
                    }

                    // LOG
                    logs.logs.VerErrores("URL FINAL LIMPIA: " + fileUrl, "LogHistoriaClinica");

                    // ================== VALIDACIÓN ==================
                    if (fileUrl.Contains("Formulario"))
                    {
                        respuesta.tipoMensaje = "warning";
                        respuesta.resultado = "⚠️ ERROR: La ruta contiene 'Formulario' (INCORRECTO)";
                    }
                    else if (fileUrl.Contains("HistoriasClinicas"))
                    {
                        respuesta.tipoMensaje = "success";
                        respuesta.resultado = "✅ OK: La ruta apunta a HistoriasClinicas";
                    }
                    else
                    {
                        respuesta.tipoMensaje = "info";
                        respuesta.resultado = "ℹ️ No se pudo determinar la ruta";
                    }

                    // ================== RESPUESTA ==================
                    respuesta.estado = "1";
                    respuesta.mensaje = fileUrl;   // 🔥 URL REAL (YA LIMPIA)
                    respuesta.codigoError = nombrePaciente;


                    // ================== VERSION ANTIGUA ==================
                    /*
                    TemplateExcel.FillReport(outputPath, archivoStr, nomHojaStr, ds, new string[] { "{", "}" }, rutaQR, rutaQR2, nombrePaciente);
                    logs.logs.VerErrores(outputPath, "LogHistoriaClinica");
                    respuesta.mensaje = outputPath;
                    */

                    // ================== ABRIR LOCAL ==================
                    /*
                    Process.Start(outputPath);
                    */

                    // ================== VERSION CON ENCRIPTACION ==================
                    /*
                    TemplateExcel.FillReport2(outputPath, archivoStr, nomHojaStr, ds, new string[] { "{", "}" }, rutaQR, rutaQR2, nombrePaciente);
                    logs.logs.VerErrores(outputPath, "LogHistoriaClinica");
                    respuesta.mensaje = outputPath + ".enc";
                    */

                }
                catch (Exception ex)
                {
                    respuesta.estado = "0";
                    respuesta.mensaje = "Ocurrió un error al generar el archivo.";
                    respuesta.tipoMensaje = "danger";
                    respuesta.codigoError = "500";

                    logs.logs.VerErrores(ex.Message, "ErrorGeneracionExcel");
                }

                return respuesta.SerializaToJson2();


            }
            catch (Exception ex)
            {
                string inner = ex.InnerException != null ? ex.InnerException.Message : "ninguno";

                string sugerencia = "";
                if (ex.Message.Contains("at least one worksheet"))
                    sugerencia = " | CAUSA PROBABLE: El archivo plantilla es .xls — EPPlus solo soporta .xlsx. Convierte la plantilla a formato .xlsx.";
                else if (ex.Message.Contains("not found") || ex.Message.Contains("no se encontro"))
                    sugerencia = " | CAUSA PROBABLE: No se encontro el archivo plantilla. Verifica que exista en la carpeta del proyecto.";
                else if (ex.Message.Contains("access") || ex.Message.Contains("acceso"))
                    sugerencia = " | CAUSA PROBABLE: El archivo esta abierto en Excel. Cierra el archivo e intenta de nuevo.";

                string mensajeCompleto = "Error: " + ex.Message
                                       + " | Inner: " + inner
                                       + sugerencia
                                       + " | Formulario: " + formulario
                                       + " | Archivo: " + archivoStr
                                       + " | Hoja: " + nomHojaStr;

                return responseMessage("0", mensajeCompleto, "danger", "");
            }

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

                string numhistoria = campos["txtNumHistoria"];
                string numarchivo = campos["txtNumArchivo"];
                // Guardamos los datos obtenidos del JSON generado en Departamento.js y los guardamos en las variables previamente creadas.
                var nombreCompleto = campos["txtNombre"];

                // Separar el nombre completo en palabras utilizando el espacio como delimitador
                var palabras = nombreCompleto.Split(' ');

                // Asegurarse de que haya al menos cuatro palabras antes de acceder a los índices
                primerApellido = palabras.Length > 0 ? palabras[0] : "";
                segundoApellido = palabras.Length > 1 ? palabras[1] : "";
                primerNombre = palabras.Length > 2 ? palabras[2] : "";
                segundoNombre = palabras.Length > 3 ? palabras[3] : "";

                string sexo = "";

                if (campos["txtSexo"] == "Femenino" || campos["txtSexo"] == "FEMENINO")
                {
                    sexo = "F";
                }
                else if (campos["txtSexo"] == "Masculino" || campos["txtSexo"] == "MASCULINO")
                {
                    sexo = "M";
                }

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



                // INM EXTRAS 1

                string txtNuevaDosis1 = campos["txtNuevaDosis1"];

                string fechaNuevo1 = campos["fechaNuevo1"];
                string txtNuevoLote1 = campos["txtNuevoLote1"];
                string txtNuevoNombre1 = campos["txtNuevoNombre1"];
                string txtNuevoEstablecimiento1 = campos["txtNuevoEstablecimiento1"];
                string txtNuevoObs1 = campos["txtNuevoObs1"];

                string fechaNuevo2 = campos["fechaNuevo2"];
                string txtNuevoLote2 = campos["txtNuevoLote2"];
                string txtNuevoNombre2 = campos["txtNuevoNombre2"];
                string txtNuevoEstablecimiento2 = campos["txtNuevoEstablecimiento2"];
                string txtNuevoObs2 = campos["txtNuevoObs2"];

                string fechaNuevo3 = campos["fechaNuevo3"];
                string txtNuevoLote3 = campos["txtNuevoLote3"];
                string txtNuevoNombre3 = campos["txtNuevoNombre3"];
                string txtNuevoEstablecimiento3 = campos["txtNuevoEstablecimiento3"];
                string txtNuevoObs3 = campos["txtNuevoObs3"];

                string fechaNuevo4 = campos["fechaNuevo4"];
                string txtNuevoLote4 = campos["txtNuevoLote4"];
                string txtNuevoNombre4 = campos["txtNuevoNombre4"];
                string txtNuevoEstablecimiento4 = campos["txtNuevoEstablecimiento4"];
                string txtNuevoObs4 = campos["txtNuevoObs4"];

                string fechaNuevo5 = campos["fechaNuevo5"];
                string txtNuevoLote5 = campos["txtNuevoLote5"];
                string txtNuevoNombre5 = campos["txtNuevoNombre5"];
                string txtNuevoEstablecimiento5 = campos["txtNuevoEstablecimiento5"];
                string txtNuevoObs5 = campos["txtNuevoObs5"];

                // INM EXTRAS 2
                string txtNuevaDosis12 = campos["txtNuevaDosis12"];

                string fechaNuevo1Inm2 = campos["fechaNuevo1Inm2"];
                string txtNuevoLote1Inm2 = campos["txtNuevoLote1Inm2"];
                string txtNuevoNombre1Inm2 = campos["txtNuevoNombre1Inm2"];
                string txtNuevoEstablecimiento1Inm2 = campos["txtNuevoEstablecimiento1Inm2"];
                string txtNuevoObs1Inm2 = campos["txtNuevoObs1Inm2"];

                string fechaNuevo2Inm2 = campos["fechaNuevo2Inm2"];
                string txtNuevoLote2Inm2 = campos["txtNuevoLote2Inm2"];
                string txtNuevoNombre2Inm2 = campos["txtNuevoNombre2Inm2"];
                string txtNuevoEstablecimiento2Inm2 = campos["txtNuevoEstablecimiento2Inm2"];
                string txtNuevoObs2Inm2 = campos["txtNuevoObs2Inm2"];

                string fechaNuevo3Inm2 = campos["fechaNuevo3Inm2"];
                string txtNuevoLote3Inm2 = campos["txtNuevoLote3Inm2"];
                string txtNuevoNombre3Inm2 = campos["txtNuevoNombre3Inm2"];
                string txtNuevoEstablecimiento3Inm2 = campos["txtNuevoEstablecimiento3Inm2"];
                string txtNuevoObs3Inm2 = campos["txtNuevoObs3Inm2"];

                string fechaNuevo4Inm2 = campos["fechaNuevo4Inm2"];
                string txtNuevoLote4Inm2 = campos["txtNuevoLote4Inm2"];
                string txtNuevoNombre4Inm2 = campos["txtNuevoNombre4Inm2"];
                string txtNuevoEstablecimiento4Inm2 = campos["txtNuevoEstablecimiento4Inm2"];
                string txtNuevoObs4Inm2 = campos["txtNuevoObs4Inm2"];

                string fechaNuevo5Inm2 = campos["fechaNuevo5Inm2"];
                string txtNuevoLote5Inm2 = campos["txtNuevoLote5Inm2"];
                string txtNuevoNombre5Inm2 = campos["txtNuevoNombre5Inm2"];
                string txtNuevoEstablecimiento5Inm2 = campos["txtNuevoEstablecimiento5Inm2"];
                string txtNuevoObs5Inm2 = campos["txtNuevoObs5Inm2"];

                // INM EXTRAS 3
                string txtNuevaDosis13 = campos["txtNuevaDosis13"];

                string fechaNuevo1Inm3 = campos["fechaNuevo1Inm3"];
                string txtNuevoLote1Inm3 = campos["txtNuevoLote1Inm3"];
                string txtNuevoNombre1Inm3 = campos["txtNuevoNombre1Inm3"];
                string txtNuevoEstablecimiento1Inm3 = campos["txtNuevoEstablecimiento1Inm3"];
                string txtNuevoObs1Inm3 = campos["txtNuevoObs1Inm3"];

                string fechaNuevo2Inm3 = campos["fechaNuevo2Inm3"];
                string txtNuevoLote2Inm3 = campos["txtNuevoLote2Inm3"];
                string txtNuevoNombre2Inm3 = campos["txtNuevoNombre2Inm3"];
                string txtNuevoEstablecimiento2Inm3 = campos["txtNuevoEstablecimiento2Inm3"];
                string txtNuevoObs2Inm3 = campos["txtNuevoObs2Inm3"];

                string fechaNuevo3Inm3 = campos["fechaNuevo3Inm3"];
                string txtNuevoLote3Inm3 = campos["txtNuevoLote3Inm3"];
                string txtNuevoNombre3Inm3 = campos["txtNuevoNombre3Inm3"];
                string txtNuevoEstablecimiento3Inm3 = campos["txtNuevoEstablecimiento3Inm3"];
                string txtNuevoObs3Inm3 = campos["txtNuevoObs3Inm3"];

                string fechaNuevo4Inm3 = campos["fechaNuevo4Inm3"];
                string txtNuevoLote4Inm3 = campos["txtNuevoLote4Inm3"];
                string txtNuevoNombre4Inm3 = campos["txtNuevoNombre4Inm3"];
                string txtNuevoEstablecimiento4Inm3 = campos["txtNuevoEstablecimiento4Inm3"];
                string txtNuevoObs4Inm3 = campos["txtNuevoObs4Inm3"];

                string fechaNuevo5Inm3 = campos["fechaNuevo5Inm3"];
                string txtNuevoLote5Inm3 = campos["txtNuevoLote5Inm3"];
                string txtNuevoNombre5Inm3 = campos["txtNuevoNombre5Inm3"];
                string txtNuevoEstablecimiento5Inm3 = campos["txtNuevoEstablecimiento5Inm3"];
                string txtNuevoObs5Inm3 = campos["txtNuevoObs5Inm3"];

                // INM EXTRAS 4
                string txtNuevaDosis123 = campos["txtNuevaDosis123"];

                string fechaNuevo1Inm2Inm3 = campos["fechaNuevo1Inm2Inm3"];
                string txtNuevoLote1Inm2Inm3 = campos["txtNuevoLote1Inm2Inm3"];
                string txtNuevoNombre1Inm2Inm3 = campos["txtNuevoNombre1Inm2Inm3"];
                string txtNuevoEstablecimiento1Inm2Inm3 = campos["txtNuevoEstablecimiento1Inm2Inm3"];
                string txtNuevoObs1Inm2Inm3 = campos["txtNuevoObs1Inm2Inm3"];

                string fechaNuevo2Inm2Inm3 = campos["fechaNuevo2Inm2Inm3"];
                string txtNuevoLote2Inm2Inm3 = campos["txtNuevoLote2Inm2Inm3"];
                string txtNuevoNombre2Inm2Inm3 = campos["txtNuevoNombre2Inm2Inm3"];
                string txtNuevoEstablecimiento2Inm2Inm3 = campos["txtNuevoEstablecimiento2Inm2Inm3"];
                string txtNuevoObs2Inm2Inm3 = campos["txtNuevoObs2Inm2Inm3"];

                string fechaNuevo3Inm2Inm3 = campos["fechaNuevo3Inm2Inm3"];
                string txtNuevoLote3Inm2Inm3 = campos["txtNuevoLote3Inm2Inm3"];
                string txtNuevoNombre3Inm2Inm3 = campos["txtNuevoNombre3Inm2Inm3"];
                string txtNuevoEstablecimiento3Inm2Inm3 = campos["txtNuevoEstablecimiento3Inm2Inm3"];
                string txtNuevoObs3Inm2Inm3 = campos["txtNuevoObs3Inm2Inm3"];

                string fechaNuevo4Inm2Inm3 = campos["fechaNuevo4Inm2Inm3"];
                string txtNuevoLote4Inm2Inm3 = campos["txtNuevoLote4Inm2Inm3"];
                string txtNuevoNombre4Inm2Inm3 = campos["txtNuevoNombre4Inm2Inm3"];
                string txtNuevoEstablecimiento4Inm2Inm3 = campos["txtNuevoEstablecimiento4Inm2Inm3"];
                string txtNuevoObs4Inm2Inm3 = campos["txtNuevoObs4Inm2Inm3"];

                string fechaNuevo5Inm2Inm3 = campos["fechaNuevo5Inm2Inm3"];
                string txtNuevoLote5Inm2Inm3 = campos["txtNuevoLote5Inm2Inm3"];
                string txtNuevoNombre5Inm2Inm3 = campos["txtNuevoNombre5Inm2Inm3"];
                string txtNuevoEstablecimiento5Inm2Inm3 = campos["txtNuevoEstablecimiento5Inm2Inm3"];
                string txtNuevoObs5Inm2Inm3 = campos["txtNuevoObs5Inm2Inm3"];

                string SelectNuevoEsquema1 = "";

                if (campos["txtSelectNuevoEsquema1"] == "SI")
                {
                    SelectNuevoEsquema1 = "X";
                }

                string SelectNuevoEsquema2 = "";
                if (campos["txtSelectNuevoEsquema2"] == "SI")
                {
                    SelectNuevoEsquema2 = "X";
                }

                string SelectNuevoEsquema3 = "";
                if (campos["txtSelectNuevoEsquema3"] == "SI")
                {
                    SelectNuevoEsquema3 = "X";
                }

                string SelectNuevoEsquema4 = "";
                if (campos["txtSelectNuevoEsquema4"] == "SI")
                {
                    SelectNuevoEsquema4 = "X";
                }

                string SelectNuevoEsquema5 = "";
                if (campos["txtSelectNuevoEsquema5"] == "SI")
                {
                    SelectNuevoEsquema5 = "X";
                }


                string SelectNuevoEsquema1Inm2 = "";
                if (campos["txtSelectNuevoEsquema1Inm2"] == "SI")
                {
                    SelectNuevoEsquema1Inm2 = "X";
                }

                string SelectNuevoEsquema2Inm2 = "";
                if (campos["txtSelectNuevoEsquema2Inm2"] == "SI")
                {
                    SelectNuevoEsquema2Inm2 = "X";
                }

                string SelectNuevoEsquema3Inm2 = "";
                if (campos["txtSelectNuevoEsquema3Inm2"] == "SI")
                {
                    SelectNuevoEsquema3Inm2 = "X";
                }

                string SelectNuevoEsquema4Inm2 = "";
                if (campos["txtSelectNuevoEsquema4Inm2"] == "SI")
                {
                    SelectNuevoEsquema4Inm2 = "X";
                }

                string SelectNuevoEsquema5Inm2 = "";
                if (campos["txtSelectNuevoEsquema5Inm2"] == "SI")
                {
                    SelectNuevoEsquema5Inm2 = "X";
                }


                string SelectNuevoEsquema1Inm3 = "";
                if (campos["txtSelectNuevoEsquema1Inm3"] == "SI")
                {
                    SelectNuevoEsquema1Inm3 = "X";
                }

                string SelectNuevoEsquema2Inm3 = "";
                if (campos["txtSelectNuevoEsquema2Inm3"] == "SI")
                {
                    SelectNuevoEsquema2Inm3 = "X";
                }

                string SelectNuevoEsquema3Inm3 = "";
                if (campos["txtSelectNuevoEsquema3Inm3"] == "SI")
                {
                    SelectNuevoEsquema3Inm3 = "X";
                }

                string SelectNuevoEsquema4Inm3 = "";
                if (campos["txtSelectNuevoEsquema4Inm3"] == "SI")
                {
                    SelectNuevoEsquema4Inm3 = "X";
                }

                string SelectNuevoEsquema5Inm3 = "";
                if (campos["txtSelectNuevoEsquema5Inm3"] == "SI")
                {
                    SelectNuevoEsquema5Inm3 = "X";
                }


                string SelectNuevoEsquema1Inm2Inm3 = "";
                if (campos["txtSelectNuevoEsquema1Inm2Inm3"] == "SI")
                {
                    SelectNuevoEsquema1Inm2Inm3 = "X";
                }

                string SelectNuevoEsquema2Inm2Inm3 = "";
                if (campos["txtSelectNuevoEsquema2Inm2Inm3"] == "SI")
                {
                    SelectNuevoEsquema2Inm2Inm3 = "X";
                }

                string SelectNuevoEsquema3Inm2Inm3 = "";
                if (campos["txtSelectNuevoEsquema3Inm2Inm3"] == "SI")
                {
                    SelectNuevoEsquema3Inm2Inm3 = "X";
                }

                string SelectNuevoEsquema4Inm2Inm3 = "";
                if (campos["txtSelectNuevoEsquema4Inm2Inm3"] == "SI")
                {
                    SelectNuevoEsquema4Inm2Inm3 = "X";
                }

                string SelectNuevoEsquema5Inm2Inm3 = "";
                if (campos["txtSelectNuevoEsquema5Inm2Inm3"] == "SI")
                {
                    SelectNuevoEsquema5Inm2Inm3 = "X";
                }



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

                //Nuevas Inmunizaciones

                info.Columns.Add("Inmnombre1");

                info.Columns.Add("Inm1fecha1");
                info.Columns.Add("Inm1lote1");
                info.Columns.Add("Inm1esquema1");
                info.Columns.Add("Inm1nombre1");
                info.Columns.Add("Inm1establecimiento1");
                info.Columns.Add("Inm1obs1");

                info.Columns.Add("Inm1fecha2");
                info.Columns.Add("Inm1lote2");
                info.Columns.Add("Inm1esquema2");
                info.Columns.Add("Inm1nombre2");
                info.Columns.Add("Inm1establecimiento2");
                info.Columns.Add("Inm1obs2");

                info.Columns.Add("Inm1fecha3");
                info.Columns.Add("Inm1lote3");
                info.Columns.Add("Inm1esquema3");
                info.Columns.Add("Inm1nombre3");
                info.Columns.Add("Inm1establecimiento3");
                info.Columns.Add("Inm1obs3");

                info.Columns.Add("Inm1fecha4");
                info.Columns.Add("Inm1lote4");
                info.Columns.Add("Inm1esquema4");
                info.Columns.Add("Inm1nombre4");
                info.Columns.Add("Inm1establecimiento4");
                info.Columns.Add("Inm1obs4");

                info.Columns.Add("Inm1fecha5");
                info.Columns.Add("Inm1lote5");
                info.Columns.Add("Inm1esquema5");
                info.Columns.Add("Inm1nombre5");
                info.Columns.Add("Inm1establecimiento5");
                info.Columns.Add("Inm1obs5");

                info.Columns.Add("Inmnombre2");

                info.Columns.Add("Inm2fecha1");
                info.Columns.Add("Inm2lote1");
                info.Columns.Add("Inm2esquema1");
                info.Columns.Add("Inm2nombre1");
                info.Columns.Add("Inm2establecimiento1");
                info.Columns.Add("Inm2obs1");

                info.Columns.Add("Inm2fecha2");
                info.Columns.Add("Inm2lote2");
                info.Columns.Add("Inm2esquema2");
                info.Columns.Add("Inm2nombre2");
                info.Columns.Add("Inm2establecimiento2");
                info.Columns.Add("Inm2obs2");

                info.Columns.Add("Inm2fecha3");
                info.Columns.Add("Inm2lote3");
                info.Columns.Add("Inm2esquema3");
                info.Columns.Add("Inm2nombre3");
                info.Columns.Add("Inm2establecimiento3");
                info.Columns.Add("Inm2obs3");

                info.Columns.Add("Inm2fecha4");
                info.Columns.Add("Inm2lote4");
                info.Columns.Add("Inm2esquema4");
                info.Columns.Add("Inm2nombre4");
                info.Columns.Add("Inm2establecimiento4");
                info.Columns.Add("Inm2obs4");

                info.Columns.Add("Inm2fecha5");
                info.Columns.Add("Inm2lote5");
                info.Columns.Add("Inm2esquema5");
                info.Columns.Add("Inm2nombre5");
                info.Columns.Add("Inm2establecimiento5");
                info.Columns.Add("Inm2obs5");

                info.Columns.Add("Inmnombre3");

                info.Columns.Add("Inm3fecha1");
                info.Columns.Add("Inm3lote1");
                info.Columns.Add("Inm3esquema1");
                info.Columns.Add("Inm3nombre1");
                info.Columns.Add("Inm3establecimiento1");
                info.Columns.Add("Inm3obs1");

                info.Columns.Add("Inm3fecha2");
                info.Columns.Add("Inm3lote2");
                info.Columns.Add("Inm3esquema2");
                info.Columns.Add("Inm3nombre2");
                info.Columns.Add("Inm3establecimiento2");
                info.Columns.Add("Inm3obs2");

                info.Columns.Add("Inm3fecha3");
                info.Columns.Add("Inm3lote3");
                info.Columns.Add("Inm3esquema3");
                info.Columns.Add("Inm3nombre3");
                info.Columns.Add("Inm3establecimiento3");
                info.Columns.Add("Inm3obs3");

                info.Columns.Add("Inm3fecha4");
                info.Columns.Add("Inm3lote4");
                info.Columns.Add("Inm3esquema4");
                info.Columns.Add("Inm3nombre4");
                info.Columns.Add("Inm3establecimiento4");
                info.Columns.Add("Inm3obs4");

                info.Columns.Add("Inm3fecha5");
                info.Columns.Add("Inm3lote5");
                info.Columns.Add("Inm3esquema5");
                info.Columns.Add("Inm3nombre5");
                info.Columns.Add("Inm3establecimiento5");
                info.Columns.Add("Inm3obs5");

                info.Columns.Add("Inmnombre4");

                info.Columns.Add("Inm4fecha1");
                info.Columns.Add("Inm4lote1");
                info.Columns.Add("Inm4esquema1");
                info.Columns.Add("Inm4nombre1");
                info.Columns.Add("Inm4establecimiento1");
                info.Columns.Add("Inm4obs1");

                info.Columns.Add("Inm4fecha2");
                info.Columns.Add("Inm4lote2");
                info.Columns.Add("Inm4esquema2");
                info.Columns.Add("Inm4nombre2");
                info.Columns.Add("Inm4establecimiento2");
                info.Columns.Add("Inm4obs2");

                info.Columns.Add("Inm4fecha3");
                info.Columns.Add("Inm4lote3");
                info.Columns.Add("Inm4esquema3");
                info.Columns.Add("Inm4nombre3");
                info.Columns.Add("Inm4establecimiento3");
                info.Columns.Add("Inm4obs3");

                info.Columns.Add("Inm4fecha4");
                info.Columns.Add("Inm4lote4");
                info.Columns.Add("Inm4esquema4");
                info.Columns.Add("Inm4nombre4");
                info.Columns.Add("Inm4establecimiento4");
                info.Columns.Add("Inm4obs4");

                info.Columns.Add("Inm4fecha5");
                info.Columns.Add("Inm4lote5");
                info.Columns.Add("Inm4esquema5");
                info.Columns.Add("Inm4nombre5");
                info.Columns.Add("Inm4establecimiento5");
                info.Columns.Add("Inm4obs5");


                // Agregamos las variables cargadas con la informacion a las etiquetas registradas previamente en orden
                info.Rows.Add(numhistoria, numarchivo, primerApellido, segundoApellido, primerNombre, segundoNombre, sexo, puestotrabajo, fechaTetanos1, fechaTetanos2, fechaTetanos3, fechaTetanos4, fechaTetanos5,
                    loteTetanos1, loteTetanos2, loteTetanos3, loteTetanos4, loteTetanos5, esquemaTetanos1, esquemaTetanos2, esquemaTetanos3, esquemaTetanos4, esquemaTetanos5, nombreTetanos1, nombreTetanos2, nombreTetanos3, nombreTetanos4,
                    nombreTetanos5, establecimientoTetanos1, establecimientoTetanos2, establecimientoTetanos3, establecimientoTetanos4, establecimientoTetanos5, obsTetanos1, obsTetanos2, obsTetanos3, obsTetanos4, obsTetanos5, fechaHepA1,
                    fechaHepA2, fechaHepA3, fechaHepB1, fechaHepB2, fechaHepB3, loteHepA1, loteHepA2, loteHepA3, loteHepB1, loteHepB2, loteHepB3, esquemaHepA1, esquemaHepA2, esquemaHepA3, esquemaHepB1, esquemaHepB2, esquemaHepB3, nombreHepA1,
                    nombreHepA2, nombreHepA3, nombreHepB1, nombreHepB2, nombreHepB3, establecimientoHepA1, establecimientoHepA2, establecimientoHepA3, establecimientoHepB1, establecimientoHepB2, establecimientoHepB3, obsHepA1, obsHepA2,
                    obsHepA3, obsHepB1, obsHepB2, obsHepB3, fechaInfluenza, fechaFiebre, fechaSarampion1, fechaSarampion2, loteInfluenza, loteFiebre, loteSarampion1, loteSarampion2, esquemaInfluenza, esquemaFiebre, esquemaSarampion1,
                    esquemaSarampion2, nombreInfluenza, nombreFiebre, nombreSarampion1, nombreSarampion2, establecimientoInfluenza, establecimientoFiebre, establecimientoSarampion1, establecimientoSarampion2, obsInfluenza, obsFiebre, obsSarampion1, obsSarampion2,
                    // INM EXTRAS 1
                    txtNuevaDosis1,
                    fechaNuevo1, txtNuevoLote1, SelectNuevoEsquema1, txtNuevoNombre1, txtNuevoEstablecimiento1, txtNuevoObs1,
                    fechaNuevo2, txtNuevoLote2, SelectNuevoEsquema2, txtNuevoNombre2, txtNuevoEstablecimiento2, txtNuevoObs2,
                    fechaNuevo3, txtNuevoLote3, SelectNuevoEsquema3, txtNuevoNombre3, txtNuevoEstablecimiento3, txtNuevoObs3,
                    fechaNuevo4, txtNuevoLote4, SelectNuevoEsquema4, txtNuevoNombre4, txtNuevoEstablecimiento4, txtNuevoObs4,
                    fechaNuevo5, txtNuevoLote5, SelectNuevoEsquema5, txtNuevoNombre5, txtNuevoEstablecimiento5, txtNuevoObs5,
                    // INM EXTRAS 2
                    txtNuevaDosis12,
                    fechaNuevo1Inm2, txtNuevoLote1Inm2, SelectNuevoEsquema1Inm2, txtNuevoNombre1Inm2, txtNuevoEstablecimiento1Inm2, txtNuevoObs1Inm2,
                    fechaNuevo2Inm2, txtNuevoLote2Inm2, SelectNuevoEsquema2Inm2, txtNuevoNombre2Inm2, txtNuevoEstablecimiento2Inm2, txtNuevoObs2Inm2,
                    fechaNuevo3Inm2, txtNuevoLote3Inm2, SelectNuevoEsquema3Inm2, txtNuevoNombre3Inm2, txtNuevoEstablecimiento3Inm2, txtNuevoObs3Inm2,
                    fechaNuevo4Inm2, txtNuevoLote4Inm2, SelectNuevoEsquema4Inm2, txtNuevoNombre4Inm2, txtNuevoEstablecimiento4Inm2, txtNuevoObs4Inm2,
                    fechaNuevo5Inm2, txtNuevoLote5Inm2, SelectNuevoEsquema5Inm2, txtNuevoNombre5Inm2, txtNuevoEstablecimiento5Inm2, txtNuevoObs5Inm2,
                    // INM EXTRAS 3
                    txtNuevaDosis13,
                    fechaNuevo1Inm3, txtNuevoLote1Inm3, SelectNuevoEsquema1Inm3, txtNuevoNombre1Inm3, txtNuevoEstablecimiento1Inm3, txtNuevoObs1Inm3,
                    fechaNuevo2Inm3, txtNuevoLote2Inm3, SelectNuevoEsquema2Inm3, txtNuevoNombre2Inm3, txtNuevoEstablecimiento2Inm3, txtNuevoObs2Inm3,
                    fechaNuevo3Inm3, txtNuevoLote3Inm3, SelectNuevoEsquema3Inm3, txtNuevoNombre3Inm3, txtNuevoEstablecimiento3Inm3, txtNuevoObs3Inm3,
                    fechaNuevo4Inm3, txtNuevoLote4Inm3, SelectNuevoEsquema4Inm3, txtNuevoNombre4Inm3, txtNuevoEstablecimiento4Inm3, txtNuevoObs4Inm3,
                    fechaNuevo5Inm3, txtNuevoLote5Inm3, SelectNuevoEsquema5Inm3, txtNuevoNombre5Inm3, txtNuevoEstablecimiento5Inm3, txtNuevoObs5Inm3,
                    // INM EXTRAS 4
                    txtNuevaDosis123,
                    fechaNuevo1Inm2Inm3, txtNuevoLote1Inm2Inm3, SelectNuevoEsquema1Inm2Inm3, txtNuevoNombre1Inm2Inm3, txtNuevoEstablecimiento1Inm2Inm3, txtNuevoObs1Inm2Inm3,
                    fechaNuevo2Inm2Inm3, txtNuevoLote2Inm2Inm3, SelectNuevoEsquema2Inm2Inm3, txtNuevoNombre2Inm2Inm3, txtNuevoEstablecimiento2Inm2Inm3, txtNuevoObs2Inm2Inm3,
                    fechaNuevo3Inm2Inm3, txtNuevoLote3Inm2Inm3, SelectNuevoEsquema3Inm2Inm3, txtNuevoNombre3Inm2Inm3, txtNuevoEstablecimiento3Inm2Inm3, txtNuevoObs3Inm2Inm3,
                    fechaNuevo4Inm2Inm3, txtNuevoLote4Inm2Inm3, SelectNuevoEsquema4Inm2Inm3, txtNuevoNombre4Inm2Inm3, txtNuevoEstablecimiento4Inm2Inm3, txtNuevoObs4Inm2Inm3,
                    fechaNuevo5Inm2Inm3, txtNuevoLote5Inm2Inm3, SelectNuevoEsquema5Inm2Inm3, txtNuevoNombre5Inm2Inm3, txtNuevoEstablecimiento5Inm2Inm3, txtNuevoObs5Inm2Inm3

                    );

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

                string nombreArchPaciente = campos["archivo"];
                string op = campos["operacion"];

                nomHoja = "Form 083 Registro Inmunizacione";
                tipoFormulario = "INMUNIZACIONES";
                archivo = "TemplateInmunizaciones.xlsx";

                string nombreArchivo = "";

                if (op == "0")
                {
                    // Nombre del archivo de la historia clínica
                    nombreArchivo = $"{tipoFormulario}_{nombrePaciente}_{DateTime.Now.ToString("yyyyMMdd_HHmm")}.xlsx";
                }
                else if (op == "1")
                {
                    // Nombre del archivo de la historia clínica
                    nombreArchivo = nombreArchPaciente;
                }


                // Ruta completa del archivo de salida, que incluye la carpeta del paciente
                string outputPath = Path.Combine(pacienteFolder, nombreArchivo);



                // Llama a un método "FillReport" para llenar un archivo Excel utilizando una plantilla ("templateDOS.xlsx") y datos proporcionados en el DataSet ("ds").
                // El archivo resultante se guardará en la ruta especificada por "outputPath".
                TemplateExcel.FillReport(outputPath, archivo, nomHoja, ds, new string[] { "{", "}" }, rutaQR, rutaQR2, nombrePaciente);

                // Abre el archivo recién creado utilizando la aplicación asociada en el sistema.
                //Process.Start(outputPath);

                string baseUrl = "https://portaldeservicios.dos.com.ec/RTareas";
                string relativePath = outputPath.Replace(AppDomain.CurrentDomain.BaseDirectory, "").Replace("\\", "/");
                string fileUrl = $"{baseUrl}/{relativePath.TrimStart('/')}";

                // Registrar log para verificar la URL generada (opcional)
                logs.logs.VerErrores(fileUrl, "LogHistoriaClinica");
                respuesta.mensaje = fileUrl;
                //// Ruta completa del archivo de salida, que incluye la carpeta del paciente
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");

            }
            return respuesta.SerializaToJson2();
        }

        //public string InformacionMedica(dynamic parameters)
        //{
        //    EntRespuesta respuesta = new EntRespuesta();
        //    List<string> mensajesError = new List<string>();
        //    string jsonResponse = string.Empty;
        //    try
        //    {
        //        // Obtener los parámetros
        //        string nomCarpeta = "HistoriasClinicas";
        //        string nombreArchivo = parameters["nombreArchivo"];
        //        string nombreHoja = parameters["nombreHoja"];
        //        string nombre = parameters["nombre"];

        //        if (parameters["campos"] == null)
        //        {
        //            throw new ArgumentException("El parámetro 'campos' es obligatorio.");
        //        }

        //        var campos = new Dictionary<string, string>();

        //        foreach (var campo in parameters["campos"])
        //        {
        //            string nombreCampo = campo["nombre"];
        //            string celda = campo["celda"];
        //            campos.Add(nombreCampo, celda);
        //        }

        //        // Suponiendo que los datos se reciben como una cadena de texto JSON
        //        var jsonData = EditExcel.ObtenerDatosFormulario(nomCarpeta, nombre, nombreArchivo, nombreHoja, campos);

        //        object data = "";

        //        if (parameters["operacion"] == "2")
        //        {
        //            if (!string.IsNullOrEmpty(jsonData))
        //            {
        //                try
        //                {
        //                    // Deserializar el JSON a una lista de EntInfoRelevanteSalud
        //                    var listaEntidades = JsonConvert.DeserializeObject<List<EntInfoRelevanteSalud>>(jsonData);
        //                    int anio = parameters["anio"];

        //                    if (listaEntidades != null && listaEntidades.Any())
        //                    {
        //                        // Fecha límite (variable con fecha 2024)
        //                        DateTime fechaLimite = new DateTime(anio, 1, 1);

        //                        // Transformar y filtrar listaEntidades
        //                        var listaDatosMed = listaEntidades
        //                            .Where(entidad => entidad.datos != null && entidad.datos.fecha != null) // Asegurar que datos y fecha no sean nulos
        //                            .Where(entidad => DateTime.TryParse(entidad.datos.fecha, out DateTime fechaEntidad) && fechaEntidad >= fechaLimite) // Filtrar por fecha
        //                            .Select(entidad => entidad.datos)
        //                            .ToList();

        //                        // Verificar que la transformación fue exitosa
        //                        if (listaDatosMed.Any())
        //                        {
        //                            // Enviar solo los datos relevantes a la capa de negocio
        //                            //respuesta = NegInfoAdicionalHisClinica.Sp_InsUpdInfoRelevante(listaDatosMed, 1);
        //                        }
        //                        else
        //                        {
        //                            respuesta.mensaje = "No se encontraron datos válidos para cargar.";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        respuesta.mensaje = "La lista deserializada está vacía o no contiene elementos.";
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    respuesta.mensaje = "Error al deserializar el JSON: " + ex.Message;
        //                }
        //            }
        //            else
        //            {
        //                respuesta.mensaje = "El JSON proporcionado está vacío o es nulo.";
        //            }
        //        }
        //        else
        //        {
        //            try
        //            {
        //                // Deserializar directamente en el objeto EntInmunizacionesData
        //                if (!string.IsNullOrEmpty(jsonData))
        //                {
        //                    data = JsonConvert.DeserializeObject<EntInmunizacionesData>(jsonData);
        //                    respuesta.mensaje = "Datos obtenidos de correctamente.";
        //                    respuesta.tipoMensaje = "success";
        //                    respuesta.resultado = data; // Asignar data a la propiedad resultado

        //                }
        //                else
        //                {
        //                    respuesta.mensaje = "Se produjo un error al cargar la información de la base de datos o No existe información disponble en la base de datos.";
        //                    respuesta.tipoMensaje = "error";
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Error al deserializar EntInmunizacionesData: {ex.Message}");
        //                respuesta.mensaje = "Error al procesar la solicitud.";
        //                respuesta.tipoMensaje = "danger";
        //                respuesta.resultado = null; // O puedes asignar un valor por defecto si lo prefieres
        //            }
        //        }

        //        // Convertir el objeto data a JSON para enviarlo como respuesta
        //        //jsonResponse = JsonConvert.SerializeObject(data, Formatting.Indented);

        //    }
        //    catch (Exception ex)
        //    {
        //        respuesta.mensaje = ex.Message;
        //        respuesta.tipoMensaje = "danger";
        //    }
        //    //return jsonResponse;
        //    return respuesta.SerializaToJson2();
        //}



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

            return Lista.SerializaToJson2();
        }



        public string BuscarContrato(dynamic parameters)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            int op = Convert.ToInt32(parameters["op"]);
            string numContrato = parameters["nContrato"].ToString();
            int opcBD = Convert.ToInt32(parameters["tipo"]); // Se corrigió la conversión

            try
            {
                if (op == 1) //Opcion para obtener la informacino de un solo contrato por NumContrato o NumPedido
                {
                    EntInfoContrato contrato = NegInfoContrato.Sp_RTAConsultarContratoNum(numContrato, opcBD);

                    // Verificar si el contrato fue encontrado
                    if (contrato == null)
                    {
                        return responseMessage("0", "No se encontró el contrato especificado.", "warning", "");
                    }

                    contrato.opcion = 2; // Asignar el valor correcto
                    return contrato.SerializaToJson2(); // Retornar el contrato en formato JSON
                }
                else if (op == 2)
                {
                    // Obtener la lista de contratos
                    List<EntInfoContrato> ListaContratos = NegInfoContrato.Sp_RTA_ConsultarContratos(numContrato);

                    // Verificar si la lista está vacía
                    if (ListaContratos == null || ListaContratos.Count == 0)
                    {
                        return responseMessage("0", "No se encontraron contratos.", "warning", "");
                    }

                    return JsonConvert.SerializeObject(ListaContratos); // Retornar la lista en JSON
                }
                else
                {
                    return responseMessage("0", "Operación no válida.", "danger", "");
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrió un error al obtener los datos. " + ex.Message, "danger", "");
            }
        }


        public string GuardarNuevoInfoContrato(dynamic campos)
        {
            SeguridadHelper seguridad = new SeguridadHelper();
            EntRespuesta respuesta = new EntRespuesta();

            string IdUsuarioSession = "";
            IdUsuarioSession = seguridad.Desencripta(campos["session"]);


            try
            {
                EntInfoContrato registro = new EntInfoContrato();

                int usuario = 0;
                usuario = campos["formulario"];

                registro.opcion = usuario;
                registro.CLIENTE = campos["txtCliente"];
                //registro.CLI_NOMBRE = seguridad.Encripta(campos["txtNomContacto"]);
                //registro.CLI_TELEFONO = seguridad.Encripta(campos["txtTelefono"]);
                //registro.CLI_DIRECCION = seguridad.Encripta(campos["txtDireccion"]);
                //registro.CLI_CORREO = seguridad.Encripta(campos["txtCorreo"]);
                //registro.NUM_CONTRATO = seguridad.Encripta(campos["txtNumContrato"]);
                //registro.OBJETO = seguridad.Encripta(campos["txtObjeto"]);

                registro.CLI_NOMBRE = campos["txtNomContacto"];
                registro.CLI_TELEFONO = campos["txtTelefono"];
                registro.CLI_DIRECCION = campos["txtDireccion"];
                registro.CLI_CORREO = campos["txtCorreo"];
                registro.NUM_CONTRATO = campos["txtNumContrato"];
                registro.OBJETO = campos["txtObjeto"];

                registro.NUM_PEDIDO = campos["txtNumPedido"];
                registro.VALOR_TOTAL_CONTRATO = Convert.ToDecimal(campos["txtValorContrato"], CultureInfo.InvariantCulture);
                registro.ALCANCE = campos["txtAlcance"];
                decimal? margen = null; // Permitir valores nulos

                if (!string.IsNullOrWhiteSpace(campos["txtConMargen"]))
                {
                    if (decimal.TryParse(campos["txtConMargen"], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal tempMargen))
                    {
                        margen = tempMargen;
                        registro.MARGEN = tempMargen;
                    }
                }

                registro.HARDWARE = campos["txtHardware"];
                registro.LICENCIAS = campos["txtLicencias"];
                registro.SERVICIOS_FABRICANTE = campos["txtServiciosFab"];
                registro.SERVICIO_EXTERNOS = campos["txtServiciosExt"];
                registro.POLIZAS = campos["txtPolizas"];
                registro.FORMA_PAGO = campos["selectFormaPago"];
                registro.FECHA_SUSCRIPCION_CONTRATO = campos["fechaSuscripContrato"];
                registro.FECHA_NOTIF_ANTICIPO = campos["fechaNotifAnticipo"];
                registro.FECHA_INICIO_GARANTIA = campos["fechaIniActivacion"];
                registro.FECHA_FIN_GARANTIA = campos["fechaFinActivacion"];

                registro.ITEMS = campos["txtItems"];

                registro.CORREOS_NOTIFICACION = campos["txtCorreosAdicionales"];

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
                registro.OBS_FORMA_PAGO = campos["txtFormaPagoObs"];


                respuesta = NegInfoContrato.Sp_InsertarActualizarContrato(registro);

                if (respuesta.estado == "1")
                {
                    List<EntItemValor> listaCamposCorreo = new List<EntItemValor>();
                    EnvioCorreoHelper envioCorreo = new EnvioCorreoHelper();
                    EntParametrosCorreo parametrosServidorCorreo = new EntParametrosCorreo();
                    string contenidoCorreo = "";
                    string correosAdicionales = campos["txtCorreosAdicionales"];

                    listaCamposCorreo.Add(new EntItemValor() { Item = "txtcliente", Valor = campos["txtCliente"] });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Objeto:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "txtnumContrato", Valor = campos["txtObjeto"] });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "titValorContrato", Valor = "Número de pedido:" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "txtValorContrato", Valor = campos["txtNumPedido"] });

                    //listaCamposCorreo.Add(new EntItemValor() { Item = "txtcliente", Valor = campos["txtCliente"] });
                    //listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Contrato:" });
                    //listaCamposCorreo.Add(new EntItemValor() { Item = "txtnumContrato", Valor = campos["txtNumContrato"] });
                    //listaCamposCorreo.Add(new EntItemValor() { Item = "titValorContrato", Valor = "Valor Total de Contrato:" });
                    //listaCamposCorreo.Add(new EntItemValor() { Item = "txtValorContrato", Valor = campos["txtValorContrato"] });

                    parametrosServidorCorreo.smtpAddress = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("smtpAddress");
                    parametrosServidorCorreo.emailFrom = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFrom");
                    parametrosServidorCorreo.emailFromName = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("emailFromName");
                    parametrosServidorCorreo.password = NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("password");
                    parametrosServidorCorreo.portNumber = Convert.ToInt32(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("portNumber"));
                    parametrosServidorCorreo.enableSSL = Convert.ToBoolean(NegParametrosConfiguracion.RTA_ValorParametroConfiguracion("enableSSL"));

                    bool respuestaEnvioCorreo = false;


                    /*listaCamposCorreo.Add(new EntItemValor() { Item = "txtcliente", Valor = "PLANIFICACION DE VACACIONES" });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "etiqueta2", Valor = "Estimados2. " });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "titValorContrato", Valor = "Estimados1. " });
                    listaCamposCorreo.Add(new EntItemValor() { Item = "txtValorContrato", Valor = "12543767 " });
                    */
                    //contenidoCorreo = estructuraContenidoCorreo;

                    foreach (EntItemValor parametrosContenido in listaCamposCorreo)
                    {
                        contenidoCorreo = contenidoCorreo.Replace("[" + parametrosContenido.Item + "]", parametrosContenido.Valor);
                    }


                    //respuestaEnvioCorreo=envioCorreo.EnviarCorreo(correos, "Registro de NEW TRANSFER MEETING", "HOLA MUNDO...!!", parametrosServidorCorreo);                   
                    string[] listaCorreos = correosAdicionales.Split(';');

                    foreach (string correo in listaCorreos)
                    {
                        if (!string.IsNullOrWhiteSpace(correo))
                        {
                            //respuestaEnvioCorreo = envioCorreo.EnviarCorreo("lsalazar@dos.com.ec", "Registro de un nuevo TRANSFER MEETING", "HOLA MUNDO...!!", parametrosServidorCorreo);
                            respuestaEnvioCorreo = envioCorreo.EnvioCorreo(correo, "Ingreso de nuevo contrato", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoFormatoCorreoContrato.txt"), listaCamposCorreo);
                            //respuestaEnvioCorreo = envioCorreo.EnvioCorreo("leoc@gmail.com", "Ingreso de nuevo contrato", envioCorreo.EstructuraContenidoCorreoSolicitud("contenidoFormatoCorreoContrato.txt"), listaCamposCorreo);


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Ocurrio un error al guardar los datos. " + ex.Message.ToString(), "danger", "");
            }
            return respuesta.SerializaToJson2();
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

            return Ent1.SerializaToJson2();
        }
        public string BuscarListaArchivosContrato(dynamic parameters)
        {
            // Obtiene los parámetros necesarios del objeto 'parameters'
            string numContrato = parameters["numContrato"].ToString();

            // Directorio base donde se almacenan los archivos de contratos
            string contratosFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfoContratos");

            // Construimos la ruta específica del contrato
            string outputPath = Path.Combine(contratosFolder, numContrato);
            logs.logs.VerErrores(outputPath, "LogInfoContrato");

            // Transformar ruta física en URL
            string baseUrl = "https://portaldeservicios.dos.com.ec/RTareas";
            string relativePath = outputPath.Replace(AppDomain.CurrentDomain.BaseDirectory, "").Replace("\\", "/");
            string contratoFolder = $"{baseUrl}/{relativePath.TrimStart('/')}";

            // Verifica si la carpeta del contrato existe
            if (Directory.Exists(outputPath))
            {
                // Obtiene todos los archivos de la carpeta
                var archivos = Directory.GetFiles(outputPath)
                    .Select(archivo => new
                    {
                        Nombre = Path.GetFileName(archivo),
                        Ruta = $"{contratoFolder}/{Path.GetFileName(archivo)}"
                    })
                    .ToList();

                if (archivos.Any())
                {
                    Console.WriteLine("Documentos encontrados:");
                    foreach (var archivo in archivos)
                    {
                        Console.WriteLine(archivo);
                    }

                    // Convierte la lista en JSON
                    return JsonConvert.SerializeObject(archivos);
                }
                else
                {
                    return "{}"; // No hay archivos en la carpeta
                }
            }
            else
            {
                return "{}"; // Carpeta del contrato no encontrada
            }
        }

        public string AbrirDocArchivo(dynamic campos)
        {
            EntRespuesta respuesta = new EntRespuesta();

            var nombreCompleto = campos["nombreCarpeta"];
            var nombreArchivo = campos["nombreArchivo"];

            // Directorio base donde se almacenan los archivos
            string ArchivosFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfoContratos");

            // Construcción de la ruta del archivo
            string contratoFolder = Path.Combine(ArchivosFolder, nombreCompleto);
            string outputPath = Path.Combine(contratoFolder, nombreArchivo);


            // Verificar si el archivo existe
            if (File.Exists(outputPath))
            {
                // Transformar ruta física en URL
                //string baseUrl = "http://localhost:51037/RTareas";
                string baseUrl = "https://portaldeservicios.dos.com.ec/RTareas";
                string relativePath = outputPath.Replace(AppDomain.CurrentDomain.BaseDirectory, "").Replace("\\", "/");
                string fileUrl = $"{baseUrl}/{relativePath.TrimStart('/')}";

                // Intentar abrir el archivo
                try
                {
                    //Process.Start(outputPath);
                    //Process.Start(fileUrl);

                    // Retornar respuesta exitosa
                    respuesta.estado = "1";
                    //respuesta.mensaje = outputPath;
                    respuesta.mensaje = "Archivo encontrado";
                    respuesta.tipoMensaje = "success";
                    respuesta.resultado = fileUrl;
                }
                catch (Exception ex)
                {
                    // En caso de error al abrir el archivo
                    respuesta.estado = "0";
                    respuesta.mensaje = "Error al abrir el archivo: " + ex.Message;
                    respuesta.tipoMensaje = "error";
                    respuesta.resultado = "";
                }
            }
            else
            {
                // Si el archivo no existe, devolver error
                respuesta.estado = "0";
                respuesta.mensaje = "El archivo no fue encontrado.";
                respuesta.tipoMensaje = "error";
                respuesta.resultado = "";
            }


            // Retornar la URL generada
            //respuesta.mensaje = outputPath;
            //return fileUrl.SerializaToJson2();
            return respuesta.SerializaToJson2();
        }





        //public string AbrirDocArchivo(dynamic campos)
        //{
        //    EntRespuesta respuesta = new EntRespuesta();

        //    var nombreCompleto = campos["nombreCarpeta"];
        //    var nombreArchivo = campos["nombreArchivo"];

        //    // Directorio base donde se almacenan los archivos
        //    string ArchivosFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfoContratos");

        //    // Construcción de la ruta del archivo
        //    string contratoFolder = Path.Combine(ArchivosFolder, nombreCompleto);
        //    string outputPath = Path.Combine(contratoFolder, nombreArchivo);


        //    // Verificar si el archivo existe
        //    if (File.Exists(outputPath))
        //    {
        //        // Transformar ruta física en URL
        //        //string baseUrl = "http://localhost:51037/RTareas";
        //        string baseUrl = "https://portaldeservicios.dos.com.ec/RTareas";
        //        string relativePath = outputPath.Replace(AppDomain.CurrentDomain.BaseDirectory, "").Replace("\\", "/");
        //        string fileUrl = $"{baseUrl}/{relativePath.TrimStart('/')}";

        //        // Intentar abrir el archivo
        //        try
        //        {
        //            //Process.Start(outputPath);
        //            Process.Start(fileUrl);

        //            // Retornar respuesta exitosa
        //            respuesta.estado = "1";
        //            respuesta.mensaje = outputPath;
        //            respuesta.tipoMensaje = "success";
        //            respuesta.resultado = fileUrl;
        //        }
        //        catch (Exception ex)
        //        {
        //            // En caso de error al abrir el archivo
        //            respuesta.estado = "0";
        //            respuesta.mensaje = "Error al abrir el archivo: " + ex.Message;
        //            respuesta.tipoMensaje = "error";
        //            respuesta.resultado = "";
        //        }
        //    }
        //    else
        //    {
        //        // Si el archivo no existe, devolver error
        //        respuesta.estado = "0";
        //        respuesta.mensaje = "El archivo no fue encontrado.";
        //        respuesta.tipoMensaje = "error";
        //        respuesta.resultado = "";
        //    }


        //    // Retornar la URL generada
        //    respuesta.mensaje = outputPath;
        //    //return fileUrl.SerializaToJson2();
        //    return respuesta.SerializaToJson2();
        //}

        public string EliminarArchivo(dynamic campos)
        {
            EntRespuesta respuesta = new EntRespuesta();

            var nombreCompleto = campos["nombreCarpeta"];
            var nombreArchivo = campos["nombreArchivo"];

            string ArchivosFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InfoContratos");
            string contratoFolder = Path.Combine(ArchivosFolder, nombreCompleto);
            string archivoActual = Path.Combine(contratoFolder, nombreArchivo);
            string archivoNuevo;

            try
            {
                if (File.Exists(archivoActual))
                {
                    if (nombreArchivo.StartsWith("eliminado-"))
                    {
                        // Restaurar archivo quitando "eliminado-" del nombre
                        archivoNuevo = Path.Combine(contratoFolder, nombreArchivo.Substring(10)); // Quitamos "eliminado-" (10 caracteres)
                        File.Move(archivoActual, archivoNuevo);
                        respuesta.mensaje = "Archivo restaurado correctamente.";
                    }
                    else
                    {
                        // Eliminar archivo agregando "eliminado-" al nombre
                        archivoNuevo = Path.Combine(contratoFolder, "eliminado-" + nombreArchivo);
                        File.Move(archivoActual, archivoNuevo);
                        respuesta.mensaje = "Archivo marcado como eliminado.";
                    }


                    respuesta.estado = "1";
                    respuesta.tipoMensaje = "success";
                }
                else
                {
                    respuesta.estado = "0";
                    respuesta.mensaje = "Archivo no encontrado.";
                    respuesta.tipoMensaje = "error";
                }

                // Obtener la lista de archivos, marcando los eliminados
                var archivos = Directory.GetFiles(contratoFolder)
                    .Select(Path.GetFileName)
                    .Select(nombre => new
                    {
                        Nombre = nombre,
                        Eliminado = nombre.StartsWith("eliminado-") // Indica si el archivo está eliminado
                    })
                    .ToList();

                //respuesta.resultado = archivos;
            }
            catch (Exception ex)
            {
                respuesta.estado = "0";
                respuesta.mensaje = "Error al renombrar: " + ex.Message;
                respuesta.tipoMensaje = "error";
            }

            return respuesta.SerializaToJson2();
        }




        private string GuardarConfiguracionEncuesta(dynamic parameters)
        {
            string filePath = HttpContext.Current.Server.MapPath("~/App_Data/configuracion_encuesta.json");
            File.WriteAllText(filePath, new JavaScriptSerializer().Serialize(parameters));
            return "{\"message\":\"Configuración guardada correctamente\"}";
        }



        // Función para armar respuesta que se envía al ajax
        public string responseMessage(string estado, string mensaje, string tipoMensaje, string resultado)
        {

            EntRespuesta respuesta = new EntRespuesta();
            respuesta.estado = estado;
            respuesta.mensaje = mensaje;
            respuesta.tipoMensaje = tipoMensaje;
            respuesta.resultado = resultado;

            return respuesta.SerializaToJson2().ToString();

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