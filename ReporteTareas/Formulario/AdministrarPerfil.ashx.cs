using CapaEntidad;
using CapaNegocio;
using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace JsonJQueryNetPerfil
{
    /// <summary>
    /// Handler de la pantalla "Mi perfil".
    ///
    /// IRequiresSessionState no es decorativo: sin el, context.Session es null
    /// en un IHttpHandler y no habria de donde sacar la identidad.
    ///
    /// La estructura viene de AdministrarHorarioUsuario.ashx, pero NO su manejo
    /// de identidad. Aquel recibe codUsuario del cliente; aca eso seria que
    /// cualquiera lea y sobrescriba el perfil de cualquiera. Se sigue el patron
    /// de AdministrarUsuarios.ashx: la identidad sale de la sesion, siempre.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AdministrarPerfil : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            StringBuilder responseAction = new StringBuilder();

            if (context.Session == null || context.Session["UserLogin"] == null)
            {
                responseAction.Append(responseMessage("0", "Su sesión expiró. Vuelva a iniciar sesión.", "danger"));
            }
            else if (context.Request.ContentType != null && context.Request.ContentType.Contains("json"))
            {
                var inputStream = new System.IO.StreamReader(context.Request.InputStream);
                var inputJson = inputStream.ReadToEnd();

                JavaScriptSerializer i = new JavaScriptSerializer();
                dynamic parametros = i.Deserialize(inputJson.ToString(), typeof(object));

                var Action = parametros[0]["action"];
                bool existAction = false;

                if (Action == "CargarPerfil")
                {
                    existAction = true;
                    responseAction.Append(CargarPerfil(context));
                }

                if (Action == "GuardarContacto")
                {
                    existAction = true;
                    responseAction.Append(GuardarContacto(context, parametros[0]["parameters"]));
                }

                if (Action == "GuardarEmergencia")
                {
                    existAction = true;
                    responseAction.Append(GuardarEmergencia(context, parametros[0]["parameters"]));
                }

                if (Action == "EliminarEmergencia")
                {
                    existAction = true;
                    responseAction.Append(EliminarEmergencia(context, parametros[0]["parameters"]));
                }

                if (Action == "GuardarEstudio")
                {
                    existAction = true;
                    responseAction.Append(GuardarEstudio(context, parametros[0]["parameters"]));
                }

                if (Action == "EliminarEstudio")
                {
                    existAction = true;
                    responseAction.Append(EliminarEstudio(context, parametros[0]["parameters"]));
                }

                if (Action == "GuardarCertificacion")
                {
                    existAction = true;
                    responseAction.Append(GuardarCertificacion(context, parametros[0]["parameters"]));
                }

                if (Action == "EliminarCertificacion")
                {
                    existAction = true;
                    responseAction.Append(EliminarCertificacion(context, parametros[0]["parameters"]));
                }

                if (Action == "GuardarExperiencia")
                {
                    existAction = true;
                    responseAction.Append(GuardarExperiencia(context, parametros[0]["parameters"]));
                }

                if (Action == "EliminarExperiencia")
                {
                    existAction = true;
                    responseAction.Append(EliminarExperiencia(context, parametros[0]["parameters"]));
                }

                if (Action == "GuardarCargaFamiliar")
                {
                    existAction = true;
                    responseAction.Append(GuardarCargaFamiliar(context, parametros[0]["parameters"]));
                }

                if (Action == "EliminarCargaFamiliar")
                {
                    existAction = true;
                    responseAction.Append(EliminarCargaFamiliar(context, parametros[0]["parameters"]));
                }

                if (Action == "GuardarFoto")
                {
                    existAction = true;
                    responseAction.Append(GuardarFoto(context, parametros[0]["parameters"]));
                }

                if (Action == "EliminarFoto")
                {
                    existAction = true;
                    responseAction.Append(EliminarFoto(context));
                }

                if (Action == "EliminarDocumento")
                {
                    existAction = true;
                    responseAction.Append(EliminarDocumento(context, parametros[0]["parameters"]));
                }

                if (Action == "ListaEquipo")
                {
                    existAction = true;
                    responseAction.Append(ListaEquipo(context, parametros[0]["parameters"]));
                }

                if (Action == "PerfilEquipo")
                {
                    existAction = true;
                    responseAction.Append(PerfilEquipo(context, parametros[0]["parameters"]));
                }

                if (!existAction)
                {
                    responseAction.Append(responseMessage("0", "No existe la acción solicitada.", "danger"));
                }
            }
            else if (context.Request.Files.Count > 0)
            {
                /* La subida de documentos es lo unico que no viaja como JSON: un
                   archivo necesita multipart. Va en ESTE handler y no en
                   CargaArchivos.ashx -que es donde vive la subida del resto del
                   sistema- porque aquel se declara sin IRequiresSessionState: alli
                   context.Session es null y la identidad la manda el cliente en un
                   campo cifrado del formulario. Todo este modulo descansa en lo
                   contrario. */
                responseAction.Append(SubirDocumento(context));
            }
            else
            {
                /* Sesion valida pero ContentType que no es JSON: no cae en ningun
                   else if de arriba y, sin esta rama, salia con 200 y cuerpo vacio.
                   El cliente lee eso como si el servidor no hubiera respondido nada
                   -no como un error de negocio con mensaje-, asi que se devuelve un
                   responseMessage explicito en vez de dejar el StringBuilder vacio. */
                responseAction.Append(responseMessage("0", "La solicitud no tiene el formato esperado.", "danger"));
            }

            context.Response.ContentType = "application/json";
            // La app corre en windows-1252; se fuerza UTF-8 en los bytes para que
            // coincidan con el charset declarado y las tildes lleguen intactas.
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Charset = "utf-8";
            context.Response.Write(responseAction.ToString());
        }

        /// <summary>
        /// El perfil de quien esta conectado. No recibe parametros a proposito:
        /// no hay nada que el cliente pueda decir sobre de quien es este perfil.
        /// </summary>
        private string CargarPerfil(HttpContext context)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);

                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                return ToJson(NegPerfil.CargarPerfil(codUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar el perfil. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Guarda el contacto del usuario de la sesion.
        ///
        /// El payload pasa por NegPerfilCampos.LeerContacto, que solo lee las
        /// cuatro claves permitidas -eso filtra claves ajenas, como cargo o
        /// cedula, que simplemente no tienen donde aterrizar-, y despues por
        /// NegPerfilCampos.ValidarContacto, que filtra VALORES: sin esto, un
        /// POST directo podia escribir cualquier texto en Empleados.EstadoCivil.
        /// </summary>
        private string GuardarContacto(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                var diccionario = campos as System.Collections.Generic.IDictionary<string, object>;
                EntPerfilContacto contacto = NegPerfilCampos.LeerContacto(diccionario);

                string error = NegPerfilCampos.ValidarContacto(contacto);
                if (error != "")
                {
                    return responseMessage("0", error, "warning");
                }

                return ToJson(NegPerfil.GuardarContacto(codUsuario, contacto,
                                                        context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar el contacto. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Guarda un contacto de emergencia del usuario de la sesion.
        ///
        /// IdContacto = 0 es alta; cualquier otro valor, edicion. El navegador
        /// no valida nada -AgregarEmergencia() en miPerfil.js envia lo que
        /// haya en los campos, tal cual-, asi que esta es la unica validacion
        /// que existe: NegPerfilCampos.ValidarEmergencia. El handler ademas es
        /// alcanzable por HTTP directo, asi que aunque el cliente validara,
        /// esta seguiria siendo obligatoria.
        /// </summary>
        private string GuardarEmergencia(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                EntPerfilEmergencia contacto = new EntPerfilEmergencia
                {
                    IdContacto = Convert.ToInt32(Texto(campos, "idContacto", "0")),
                    Nombre     = Texto(campos, "nombre", ""),
                    Parentesco = Texto(campos, "parentesco", ""),
                    Telefono   = Texto(campos, "telefono", "")
                };

                /* Se valida en el servidor y no solo en el navegador: el handler
                   es alcanzable por HTTP directo. */
                string error = NegPerfilCampos.ValidarEmergencia(contacto);
                if (error != "")
                {
                    return responseMessage("0", error, "warning");
                }

                return ToJson(NegPerfil.GuardarEmergencia(codUsuario, contacto,
                                                          context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar el contacto de emergencia. " + ex.Message, "danger");
            }
        }

        /// <summary>Elimina (borrado logico) un contacto de emergencia del usuario de la sesion.</summary>
        private string EliminarEmergencia(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                int idContacto = Convert.ToInt32(Texto(campos, "idContacto", "0"));

                return ToJson(NegPerfil.EliminarEmergencia(codUsuario, idContacto,
                                                           context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al eliminar el contacto. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Las ocho escrituras de la hoja de vida comparten la misma forma:
        /// identidad de la sesion, validacion en el servidor, y recien entonces
        /// la base. La validacion va aqui y no solo en el navegador porque este
        /// handler es alcanzable por HTTP directo.
        /// </summary>
        private string GuardarEstudio(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                EntPerfilEstudio estudio = new EntPerfilEstudio
                {
                    IdEstudio      = Convert.ToInt32(Texto(campos, "idEstudio", "0")),
                    Nivel          = Texto(campos, "nivel", ""),
                    Institucion    = Texto(campos, "institucion", ""),
                    Titulo         = Texto(campos, "titulo", ""),
                    AnioGraduacion = EnteroNuloDelPayload(campos, "anioGraduacion")
                };

                string error = NegPerfilCampos.ValidarEstudio(estudio);
                if (error != "") { return responseMessage("0", error, "warning"); }

                return ToJson(NegPerfil.GuardarEstudio(codUsuario, estudio, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar el estudio. " + ex.Message, "danger");
            }
        }

        private string EliminarEstudio(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                int id = Convert.ToInt32(Texto(campos, "idEstudio", "0"));
                return ToJson(NegPerfil.EliminarEstudio(codUsuario, id, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al eliminar el estudio. " + ex.Message, "danger");
            }
        }

        private string GuardarCertificacion(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                EntPerfilCertificacion cert = new EntPerfilCertificacion
                {
                    IdCertificacion = Convert.ToInt32(Texto(campos, "idCertificacion", "0")),
                    Nombre          = Texto(campos, "nombre", ""),
                    Entidad         = Texto(campos, "entidad", ""),
                    FechaObtencion  = Texto(campos, "fechaObtencion", "")
                };

                string error = NegPerfilCampos.ValidarCertificacion(cert);
                if (error != "") { return responseMessage("0", error, "warning"); }

                return ToJson(NegPerfil.GuardarCertificacion(codUsuario, cert, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar la certificación. " + ex.Message, "danger");
            }
        }

        private string EliminarCertificacion(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                int id = Convert.ToInt32(Texto(campos, "idCertificacion", "0"));
                return ToJson(NegPerfil.EliminarCertificacion(codUsuario, id, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al eliminar la certificación. " + ex.Message, "danger");
            }
        }

        private string GuardarExperiencia(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                EntPerfilExperiencia exp = new EntPerfilExperiencia
                {
                    IdExperiencia = Convert.ToInt32(Texto(campos, "idExperiencia", "0")),
                    Empresa       = Texto(campos, "empresa", ""),
                    Cargo         = Texto(campos, "cargo", ""),
                    AnioDesde     = EnteroNuloDelPayload(campos, "anioDesde"),
                    AnioHasta     = EnteroNuloDelPayload(campos, "anioHasta"),
                    Funciones     = Texto(campos, "funciones", "")
                };

                string error = NegPerfilCampos.ValidarExperiencia(exp);
                if (error != "") { return responseMessage("0", error, "warning"); }

                return ToJson(NegPerfil.GuardarExperiencia(codUsuario, exp, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar la experiencia. " + ex.Message, "danger");
            }
        }

        private string EliminarExperiencia(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                int id = Convert.ToInt32(Texto(campos, "idExperiencia", "0"));
                return ToJson(NegPerfil.EliminarExperiencia(codUsuario, id, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al eliminar la experiencia. " + ex.Message, "danger");
            }
        }

        private string GuardarCargaFamiliar(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                EntPerfilCargaFamiliar carga = new EntPerfilCargaFamiliar
                {
                    IdCargaFam      = Convert.ToInt32(Texto(campos, "idCargaFam", "0")),
                    Nombre          = Texto(campos, "nombre", ""),
                    Parentesco      = Texto(campos, "parentesco", ""),
                    FechaNacimiento = Texto(campos, "fechaNacimiento", "")
                };

                string error = NegPerfilCampos.ValidarCargaFamiliar(carga);
                if (error != "") { return responseMessage("0", error, "warning"); }

                return ToJson(NegPerfil.GuardarCargaFamiliar(codUsuario, carga, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar la carga familiar. " + ex.Message, "danger");
            }
        }

        private string EliminarCargaFamiliar(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                int id = Convert.ToInt32(Texto(campos, "idCargaFam", "0"));
                return ToJson(NegPerfil.EliminarCargaFamiliar(codUsuario, id, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al eliminar la carga familiar. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Guarda la foto del usuario de la sesion.
        ///
        /// El navegador reduce la imagen a 256x256 y la manda en base64 por el
        /// mismo canal JSON que todo lo demas, en vez de por multipart: son unos
        /// 25 KB y no hay archivo que guardar en disco -la foto vive en la base,
        /// como la firma-, asi que un multipart solo agregaria un camino mas.
        /// </summary>
        private string GuardarFoto(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                EntPerfilFoto foto = new EntPerfilFoto
                {
                    Base64 = Texto(campos, "base64", ""),
                    Tipo   = Texto(campos, "tipo", "")
                };

                string error = NegPerfilCampos.ValidarFoto(foto);
                if (error != "") { return responseMessage("0", error, "warning"); }

                return ToJson(NegPerfil.GuardarFoto(codUsuario, foto, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al guardar la foto. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Quita la foto. No recibe parametros: solo se puede quitar la propia.
        /// </summary>
        private string EliminarFoto(HttpContext context)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                return ToJson(NegPerfil.EliminarFoto(codUsuario, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al quitar la foto. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Recibe un documento de respaldo y lo cuelga de una certificacion o de
        /// una carga familiar.
        ///
        /// El orden importa: primero la base y despues el disco. Si se escribiera
        /// el archivo antes y la base lo rechazara -porque el IdOrigen no es de
        /// esta persona, o porque su codigo esta repetido-, quedaria un archivo
        /// huerfano en el servidor que nada volveria a nombrar. Ese orden no se
        /// invierte; en cambio, si el disco falla DESPUES de la base, la fila
        /// recien insertada se compensa dandola de baja (ver el catch del SaveAs
        /// mas abajo), para no dejar un documento listado sin archivo detras.
        /// </summary>
        private string SubirDocumento(HttpContext context)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                string origen = (context.Request.Form.Get("origen") ?? "").Trim().ToUpperInvariant();

                int idOrigen;
                if (!int.TryParse(context.Request.Form.Get("idOrigen"), out idOrigen)) { idOrigen = 0; }

                HttpPostedFile archivo = context.Request.Files.Get(0);

                /* Path.GetFileName descarta cualquier ruta que venga en el nombre.
                   Sin esto, un nombre como "..\..\Formulario\algo.pdf" saldria del
                   directorio previsto. */
                string nombreArchivo = System.IO.Path.GetFileName(archivo.FileName ?? "");

                string error = NegPerfilCampos.ValidarDocumento(origen, idOrigen, nombreArchivo,
                                                                archivo.ContentLength);
                if (error != "") { return responseMessage("0", error, "warning"); }

                string extension = System.IO.Path.GetExtension(nombreArchivo).TrimStart('.').ToLowerInvariant();

                /* El nombre en disco NO lo elige el cliente: lo arma el servidor con
                   un GUID. Asi dos personas que suban "cedula.pdf" no se pisan, y la
                   direccion del archivo no se puede adivinar desde otra sesion. */
                string nombreCodigo = "Perfil_" + Guid.NewGuid().ToString("N") + "." + extension;

                const string rutaApp = "~/descargas/perfil/";
                string carpeta = context.Server.MapPath(rutaApp);

                if (!System.IO.Directory.Exists(carpeta))
                {
                    System.IO.Directory.CreateDirectory(carpeta);
                }

                /* El web.config de esta carpeta -<handlers><clear /></handlers>- es
                   lo unico que impide que IIS sirva estos documentos como archivos
                   estaticos a quien acierte el nombre. Si la carpeta no viajo en el
                   despliegue o alguien la borro en una limpieza, CreateDirectory de
                   arriba la recrea SIN esa proteccion y nada lo avisa; se repone
                   aca, en la primera subida, para que la carpeta nunca quede
                   desprotegida en produccion. */
                string rutaWebConfig = System.IO.Path.Combine(carpeta, "web.config");
                if (!System.IO.File.Exists(rutaWebConfig))
                {
                    System.IO.File.WriteAllText(rutaWebConfig,
@"<?xml version=""1.0"" encoding=""utf-8""?>
<!--
  Esta carpeta guarda respaldos personales: partidas de nacimiento, titulos,
  cedulas. Sin este archivo, IIS los serviria como archivos estaticos a
  cualquiera que acertara el nombre, sin preguntarle nada a nadie.

  <clear /> deja la carpeta sin ningun handler, de modo que IIS no tiene con
  que responder a una peticion directa. Los archivos se siguen leyendo del
  disco desde DescargarPerfil.ashx, que primero le pregunta a la base de quien
  es el documento; ese camino no pasa por los handlers de esta carpeta y no se
  ve afectado.
-->
<configuration>
  <system.webServer>
    <handlers>
      <clear />
    </handlers>
  </system.webServer>
</configuration>
");
                }

                EntPerfilDocumento doc = new EntPerfilDocumento
                {
                    Origen              = origen,
                    IdOrigen            = idOrigen,
                    NombreArchivo       = nombreArchivo,
                    NombreArchivoCodigo = nombreCodigo,
                    /* Se guarda la ruta de la aplicacion y no la fisica: asi el
                       registro sigue sirviendo si el sitio cambia de carpeta. */
                    Ruta                = rutaApp
                };

                EntRespuesta respuesta = NegPerfil.GuardarDocumento(codUsuario, doc,
                                                                    context.Request.UserHostAddress);

                if (respuesta.estado != "1") { return ToJson(respuesta); }

                try
                {
                    archivo.SaveAs(System.IO.Path.Combine(carpeta, nombreCodigo));
                }
                catch
                {
                    /* La fila ya existe y el archivo no. El caso habitual es que la
                       identidad del grupo de aplicaciones no tenga permiso de
                       escritura sobre la carpeta la primera vez que se despliega.
                       Sin esto, la fila queda activa: el usuario ve un error y cree
                       que no paso nada, pero en la siguiente carga el documento
                       aparece en su lista con un enlace que al pulsarlo dice "El
                       archivo ya no esta disponible en el servidor", y se queda asi
                       hasta que alguien lo borre a mano. Se da de baja para que eso
                       no ocurra. */
                    try
                    {
                        NegPerfil.EliminarDocumento(codUsuario, doc.IdDocumento,
                                                    context.Request.UserHostAddress);
                    }
                    catch
                    {
                        /* Si tambien falla el borrado de compensacion no hay mas que
                           hacer aca dentro: lo que si hay que evitar es que ese
                           segundo fallo se trague el mensaje y el usuario se quede
                           sin ninguna respuesta. */
                    }

                    return responseMessage("0", "No se pudo guardar el archivo en el servidor. Intente nuevamente.", "danger");
                }

                return ToJson(respuesta);
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al subir el documento. " + ex.Message, "danger");
            }
        }

        /// <summary>Borrado logico de un documento del usuario de la sesion.</summary>
        private string EliminarDocumento(HttpContext context, dynamic campos)
        {
            try
            {
                string codUsuario = CodUsuarioSesion(context);
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                int id = Convert.ToInt32(Texto(campos, "idDocumento", "0"));

                return ToJson(NegPerfil.EliminarDocumento(codUsuario, id, context.Request.UserHostAddress));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al quitar el documento. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// El equipo directo de quien esta conectado.
        ///
        /// El unico parametro que se acepta del cliente es el texto del
        /// buscador. De quien es el equipo lo dice la sesion: no hay forma de
        /// pedir el equipo de otra persona porque no hay donde decirlo.
        /// </summary>
        private string ListaEquipo(HttpContext context, dynamic campos)
        {
            try
            {
                string codJefe = CodUsuarioSesion(context);
                if (codJefe == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                string filtro = Texto(campos, "filtro", "");

                return ToJson(NegPerfil.ListaEquipo(codJefe, filtro));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar su equipo. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// El perfil recortado de alguien del equipo.
        ///
        /// Esta accion es la UNICA de todo el modulo que acepta del cliente el
        /// codigo de otra persona. Por eso no se comprueba aqui si esa persona
        /// es subordinada: se le pasa al procedimiento junto con el codigo del
        /// jefe, que sale de la sesion, y es el procedimiento el que decide. Una
        /// comprobacion en este metodo seria una segunda verdad que algun dia
        /// discreparia de la primera.
        ///
        /// Cuando no es subordinado, PerfilEncontrado vuelve en false y el
        /// mensaje es el mismo que cuando el codigo no existe. Distinguirlos le
        /// confirmaria a quien esta probando codigos cual de ellos es real.
        /// </summary>
        private string PerfilEquipo(HttpContext context, dynamic campos)
        {
            try
            {
                string codJefe = CodUsuarioSesion(context);
                if (codJefe == "")
                {
                    return responseMessage("0", "No se pudo identificar al usuario de la sesión.", "danger");
                }

                string codUsuario = Texto(campos, "codUsuario", "");
                if (codUsuario == "")
                {
                    return responseMessage("0", "No se indicó a quién consultar.", "warning");
                }

                return ToJson(NegPerfil.PerfilEquipo(codJefe, codUsuario));
            }
            catch (Exception ex)
            {
                return responseMessage("0", "Error al cargar el perfil. " + ex.Message, "danger");
            }
        }

        /// <summary>
        /// Un entero opcional del payload. Devuelve null cuando la clave no vino,
        /// vino vacia o no es un numero: para un anio, el cero significaria "anio
        /// cero" y no "no lo se".
        /// </summary>
        private static int? EnteroNuloDelPayload(dynamic campos, string clave)
        {
            string texto = Texto(campos, clave, "");
            if (texto == "") { return null; }

            int valor;
            if (!int.TryParse(texto, out valor)) { return null; }

            return valor;
        }

        /// <summary>Lee una clave del payload dinamico, con valor por omision.</summary>
        private static string Texto(dynamic campos, string clave, string omision)
        {
            var d = campos as System.Collections.Generic.IDictionary<string, object>;
            if (d == null) { return omision; }

            object valor;
            if (!d.TryGetValue(clave, out valor) || valor == null) { return omision; }

            string texto = valor.ToString().Trim();
            return texto == "" ? omision : texto;
        }

        /// <summary>De quien es este perfil. Sale de la sesion, nunca del cliente.</summary>
        private string CodUsuarioSesion(HttpContext context)
        {
            if (context.Session != null && context.Session["Cod_Usuario"] != null)
            {
                return context.Session["Cod_Usuario"].ToString().Trim();
            }
            return "";
        }

        private string responseMessage(string estado, string mensaje, string tipoMensaje, string resultado = "")
        {
            EntRespuesta respuesta = new EntRespuesta();
            respuesta.estado = estado;
            respuesta.mensaje = mensaje;
            respuesta.tipoMensaje = tipoMensaje;
            respuesta.resultado = resultado;

            return ToJson(respuesta);
        }

        /// <summary>
        /// Serializa escapando lo no ASCII como \uXXXX. El helper compartido usa
        /// Encoding.Default y rompe las tildes.
        /// </summary>
        private static string ToJson(object obj)
        {
            if (obj == null) return string.Empty;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            return serializer.Serialize(obj);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
