
// Mensajes de Exito o Error con SweetAlert2
function MensajeIncorrecto(resultado) {
    Swal.fire({
        //error
        type: 'error',
        title: 'Error',
        text: '¡Algo salió mal...!   ' + resultado,
        width: '700px',
        height: '300px'
    });
}
function MensajeCorrecto(resultado) {
    Swal.fire({
        type: 'success',
        title: 'Éxito',
        text: '' + resultado,
        width: '700px',
        height: '300px'
    });
}
function MensajeAlerta(resultado) {
    let timerInterval;
    Swal.fire({
        type: 'warning',
        title: "Importante...!!!",
        text: resultado,
        icon: "warning",
        timer: 3000,
        timerProgressBar: true,
        didOpen: () => {
            Swal.showLoading();
            const timer = Swal.getPopup().querySelector("b");
            timerInterval = setInterval(() => {
                timer.textContent = `${Swal.getTimerLeft()}`;
            }, 100);
        },
        willClose: () => {
            clearInterval(timerInterval);
        }
    });
}

function alerta(respuesta) {
    Swal.fire({
        type: 'info',
        title: "Advertencia...!!!",
        text: respuesta,
        icon: "info",
        timer: 3000,
        width: '800px',
        heightAuto: false,
    });
}



// Cambia el color de las pestañas
function cambiarColorHover(pestaniaId) {
    var pestania1 = document.getElementById(pestaniaId);
    pestania1.style.backgroundColor = "#F4F4F4"; // Nuevo color de fondo al pasar el mouse
    pestania1.style.color = "#07BCC8"; // Nuevo color del texto al pasar el mouse
}
function restaurarColor(pestaniaId) {
    var pestania = document.getElementById(pestaniaId);
    pestania.style.backgroundColor = "#A90505"; // Restaurar color de fondo original
    pestania.style.color = "#EDEDED"; // Restaurar color del texto original
}

//cambios---------------------------------------------------------------------------------------------------------------------
function seleccionarPestania(pestaniaId) {
    var pestanias = document.querySelectorAll('.nav-link');
    pestanias.forEach(function (pestania) {
        pestania.classList.remove('tab-seleccionada');
        // Ejecutas una función específica según la pestaña seleccionada
        switch (pestaniaId) {
            case 'pestania1':
                VerAtencionesMedicas();
                break;
            case 'pestania2':
                break;
            case 'pestania3':
                break;
            default:
                break;
        }
    });

    var pestaniaSeleccionada = document.getElementById(pestaniaId);
    pestaniaSeleccionada.classList.add('tab-seleccionada');
}
//cambios---------------------------------------------------------------------------------------------------------------------


//----------------  Activar Pestaña 1 ------------------------
function VerAtencionesMedicas() {
    //IdEmpleado = id;
    document.getElementById("pestaniaAtencionesMedicas").style.display = "block";
    //document.getElementById("pestaniaConsulta").style.display = "none";
    //document.getElementById("pestaniaTrabajo").style.display = "none";
    ObtenerDatosHsCln("", "", "");
}



//function CargarPagina(div, url, datos, tipoControl, extra, boton) {

//    if (div != undefined) {
//        $.ajax({
//            type: "POST",
//            url: url,
//            data: datos,
//            contentType: "application/json; charset=utf-8",
//            dataType: "json",
//            beforeSend: function (respuesta) {
//                $("#divMensajes").html("Cargando Información...");
//            },
//            success: function (respuesta) {
//                if (respuesta != null) {
//                    var idTotalRegistro = respuesta.length;
//                    if (respuesta.length == 0) {
//                        $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, extra));
//                    }
//                    else {
//                        $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, extra));
//                    }
//                } else {
//                    $(div).html("No existen datos para esta consulta.");
//                }

//                $("#divMensajes").html("");

//            },
//            error: function (xhr, textStatus, errorThrown) {
//                console.error("Error en AJAX:", {
//                    status: xhr.status,
//                    statusText: xhr.statusText,
//                    responseText: xhr.responseText,
//                    textStatus: textStatus,
//                    errorThrown: errorThrown
//                });

//                // Intenta mostrar el mensaje personalizado del backend si viene en el response
//                var mensajeError = "Ocurrió un error al procesar la solicitud.";

//                try {
//                    var respuesta = JSON.parse(xhr.responseText);
//                    if (respuesta && respuesta.mensaje) {
//                        mensajeError = respuesta.mensaje;
//                    }
//                } catch (e) {
//                    mensajeError = xhr.responseText || mensajeError;
//                }

//                MensajeIncorrecto(mensajeError);
//            }

//        });
//    }
//}


function CargarPaginaAdicional(div, url, datos, tipoControl, boton, idSeleccionado) {

    if (div != undefined) {
        $.ajax({
            type: "POST",
            url: url,
            data: datos,
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            beforeSend: function () {
                $("#divMensajes").html("Cargando Información...");
            },
            success: function (respuesta) {

                // Separar los dos bloques JSON
                const index = respuesta.indexOf('['); // posición donde empieza el array
                const parte1 = respuesta.substring(0, index).trim();
                const parte2 = respuesta.substring(index).trim();
                //const json1 = JSON.parse(parte1);
                const json2 = JSON.parse(parte2);
                //console.log("Primer bloque JSON:", json1);
                console.log("Segundo bloque JSON (menú):", json2);

                if (json2 != null) {
                    if (typeof respuesta.estado == "undefined") {
                        $(div).html(RecorreJSON(div, json2, tipoControl, boton, idSeleccionado));
                    } else {
                        MensajeAlerta(respuesta.mensaje);
                        //MostrarMensajeDialogo("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", respuesta.mensaje, respuesta.tipoMensaje);
                    }

                } else {
                    $(div).html("No existen datos para esta consulta.");
                }

                $("#divMensajes").html("");


            },
            error: function (objeto, msgError, objError) {
                var mesnajeError = "La busqueda de la información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
                MensajeIncorrecto("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
            }
        });
    }
}

function CargarPagina(div, url, datos, tipoControl, boton, idSeleccionado) {

    if (div != undefined) {
        $.ajax({
            type: "POST",
            url: url,
            data: datos,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            beforeSend: function () {
                $("#divMensajes").html("Cargando Información...");
            },
            success: function (respuesta) {
                if (respuesta.estado == "1") {
                    var idTotalRegistro = respuesta.length;
                    if (respuesta.length == 0) {
                        $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, boton));
                    }
                    else {
                        $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, boton));
                    }
                } else if (respuesta.estado == "0") {
                    MensajeIncorrecto(respuesta.mensaje);
                }

                $("#divMensajes").html("");

            },
            error: function (objeto, msgError, objError) {
                var mesnajeError = "La busqueda de la información está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
                MensajeIncorrecto("#modalMensajeInformativoTipo", "#MensajeInformativo", "#modalMensajeInformativo", mesnajeError, 'danger');
            }
        });
    }
}


/* =================================================== *
 *          Mostrar las atenciones al Historial        *
 * =================================================== */
// Crear tarjetas dinámicamente
function crearTarjetasDesdeJSON(datosJSON, contenedorId = 'contenedorTarjetas') {
    const contenedor = document.getElementById(contenedorId);
    //var nombre = $("#txtNombre").val();

    if (!contenedor) {
        console.error('No se encontró el contenedor con ID:', contenedorId);
        return;
    }

    // Limpiar contenedor antes de crear nuevas tarjetas
    contenedor.innerHTML = '';

    // Crear una tarjeta por cada registro en el JSON
    datosJSON.forEach((registro, index) => {
        const idTarjeta = `tarjeta_${index}`;
        const subCarpetasEspecificas = [registro.hsCln_Nombre, registro.hsCln_Identificador];

        // Crear el HTML de la tarjeta
        const tarjetaHTML = `
            <div class="card fade-in" id="${idTarjeta}" style="margin-bottom:4rem;">
                <!-- Parte superior de la tarjeta -->
                <div class="card-top">
                    <div class="patient-info" style="flex: 1;">
                        <div class="patient-name">${registro.hsCln_Nombre || '------'}</div>
                    </div>
                    <div class="status-indicator" style="display: flex; align-items: center; gap: 8px;">
                        <div class="status-dot completed"></div>
                        <span>Consulta activa</span>
                        <button type="button" class="expand-btn" onclick="toggleTarjeta('${idTarjeta}')" style="margin-left: 2rem;">
                            <i class="fas fa-chevron-down" id="icono_${idTarjeta}"></i>
                        </button>
                    </div>
                </div>

                <!-- Detalles de la cita -->
                <div class="appointment-details">
                    <div class="detail-box date-box">
                        <div class="detail-label">
                            <i class="fas fa-calendar-alt"></i>
                            Fecha de atención
                        </div>
                        <div class="detail-value">${registro.hsCln_Fecha || '------'}</div>
                    </div>
                    <div class="detail-box time-box">
                        <div class="detail-label">
                            <i class="fas fa-clock"></i>
                            Hora
                        </div>
                        <div class="detail-value">${registro.hsCln_Hora || '------'}</div>
                    </div>
                </div>

                <!-- Parte inferior de la tarjeta -->
                <div class="card-bottom"></div>

            
                    <!-- Contenido de la tarjeta (minimizable) -->
                    <div id="contenido_${idTarjeta}" class="medical-card" style="background: white; border-radius: 15px; padding: 1rem 3rem 2rem 3rem; margin: 1rem 0.5rem; border-radius: 15px; border: 1px solid rgb(243 243 243); box-shadow: 0 6px 25px rgba(0, 0, 0, 0.08); border: 1px solid #e8ecef; transition: all 0.3s ease; position: relative; ">
                        <div class="seccion" style="margin-top: 1.5rem;">
                            <label for="disabledTextInput"  >Motivo de consulta</label>
                            <div class="checkbox-list" id="motivo_${idTarjeta}" style="margin-left:1rem;"></div>
                        </div>
                        <div class="seccion" style="margin-top: 1.5rem;">
                            <label for="disabledTextInput" >Intervención y Recomendación</label>
                            <div class="checkbox-list" id="intervencion_${idTarjeta}" style="margin-left:1rem;"></div>
                        </div>
                        <div class="seccion" style="margin-top: 1.5rem;">
                            <label for="disabledTextInput" >Recomendación:</label>
                            <div class="texto" id="recomendacion_${idTarjeta}" style="margin-left:1rem; background-color: #86d4c824; padding: 1rem; border-left: 3px solid #19c3b5; border-radius: 8px;transition: background 0.2s ease;">${registro.hsCln_Recom || '------'}</div>
                        </div>
                        <div class="seccion" style="margin-top: 1.5rem;">
                            <label for="disabledTextInput" >Observación:</label>
                            <div class="texto" id="observacion_${idTarjeta}" style="margin-left:1rem; background-color: #95ce4a26; padding: 1rem; border-left: 3px solid #01b728; border-radius: 8px;transition: background 0.2s ease;">${registro.hsCln_Obs || '------'}</div>
                        </div>

                        <!-- Tabla de consulta de Documentos Cargados -->
                        <div class="well tabla-documentos-cargados" id="tablaDocsCargados_${idTarjeta}" style="padding:0;">
                          <div>
                            <!-- Header de la tabla con botón de recarga -->
                            <div class="tabla-header horizontal-group-simple" >
                              <label class="box-title letra-bold" style="color:#2a2a2a;">Lista de archivos</label>
                              <button type="button"
                                onclick="toggleTablaDocumentos('${registro.hsCln_Nombre}','${idTarjeta}', ['${registro.hsCln_Nombre}', '${registro.hsCln_Identificador}'], '${registro.hsCln_Identificador}')"
                                class="btn-recargar-tabla"
                                title="Recargar tabla de documentos"
                                style=" background-color: #34343400; border: none; color:black;">
                                <span id="iconoTabla_${idTarjeta}" class="glyphicon glyphicon-eye-open"></span>
                              </button>
                            </div>

                            <!-- Contenido de la tabla -->
                            <div class="contenido-tabla">
                              <table id="tbl_Docs_${registro.hsCln_Identificador}" class="tablaDocsCargados dynamic-table table-hover sm-12">
                                <thead class="tabla-header-color">
                                  <tr>
                                    <th style="width: 85%; text-align: center;">Archivos</th>
                                    <th style="width: 15%; text-align: center;">Descargar</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  <!-- DATA POR MEDIO DE AJAX -->
                                </tbody>
                              </table>
                            </div>
                          </div>
                        </div>

                    </div>                    

                </div>
            </div>
        `;

        // Agregar la tarjeta al contenedor
        contenedor.insertAdjacentHTML('beforeend', tarjetaHTML);

        // Llenar los checkboxes de motivos
        if (registro.hsCln_Motivo && registro.hsCln_Motivo.trim().length > 0) {
            const motivoContainer = document.getElementById(`motivo_${idTarjeta}`);

            // Dividir el texto por punto y coma
            const motivosArray = registro.hsCln_Motivo.split(';');

            motivosArray.forEach((motivoCompleto, idx) => {
                // Limpiar espacios en blanco
                motivoCompleto = motivoCompleto.trim();

                if (motivoCompleto.length > 0) {
                    let textoMotivo = '';
                    let textoAdicional = '';

                    // Verificar si contiene texto entre corchetes
                    const regex = /^(.+?)\s*-\s*\[(.+?)\]$/;
                    const match = motivoCompleto.match(regex);

                    if (match) {
                        // Si hay texto entre corchetes
                        textoMotivo = match[1].trim();
                        textoAdicional = match[2].trim();
                    } else {
                        // Si no hay corchetes, todo es el texto del motivo
                        textoMotivo = motivoCompleto;
                    }

                    // Crear el HTML del checkbox con el texto adicional si existe
                    const checkboxHTML = `
                <div class="checkbox-item checked1" style="margin-bottom: 0.5rem; display: flex; align-items: center;">
                    <input type="checkbox" id="motivo_${idTarjeta}_${idx}" checked disabled>
                    <span for="motivo_${idTarjeta}_${idx}" style="margin-left: 0.5rem;">${textoMotivo}</span>
                    ${textoAdicional ? `<span style="margin-left: 0.5rem; color: #666; font-style: italic;">[${textoAdicional}]</span>` : ''}
                </div>
            `;

                    motivoContainer.insertAdjacentHTML('beforeend', checkboxHTML);
                }
            });
        }

        // Llenar los checkboxes de intervenciones
        if (registro.hsCln_Interv && registro.hsCln_Interv.trim().length > 0) {
            const intervencionContainer = document.getElementById(`intervencion_${idTarjeta}`);

            // Dividir el texto por punto y coma
            const intervencionesArray = registro.hsCln_Interv.split(';');

            intervencionesArray.forEach((intervencionCompleta, idx) => {
                // Limpiar espacios en blanco
                intervencionCompleta = intervencionCompleta.trim();

                if (intervencionCompleta.length > 0) {
                    let textoIntervencion = '';
                    let textoAdicional = '';

                    // Verificar si contiene texto entre corchetes
                    const regex = /^(.+?)\s*-\s*\[(.+?)\]$/;
                    const match = intervencionCompleta.match(regex);

                    if (match) {
                        // Si hay texto entre corchetes
                        textoIntervencion = match[1].trim();
                        textoAdicional = match[2].trim();
                    } else {
                        // Si no hay corchetes, todo es el texto de la intervención
                        textoIntervencion = intervencionCompleta;
                    }

                    // Crear el HTML del checkbox con el texto adicional si existe
                    const checkboxHTML = `
                <div class="checkbox-item checked2" style="margin-bottom: 0.5rem; display: flex; align-items: center;">
                    <input type="checkbox" id="intervencion_${idTarjeta}_${idx}" checked disabled>
                    <span for="intervencion_${idTarjeta}_${idx}" style="margin-left: 0.5rem;">${textoIntervencion}</span>
                    ${textoAdicional ? `<span style="margin-left: 0.5rem; color: #666; font-style: italic;">[${textoAdicional}]</span>` : ''}
                </div>
            `;

                    intervencionContainer.insertAdjacentHTML('beforeend', checkboxHTML);
                }
            });
        }
        minimizarTodasLasTarjetas();
    });
}
// Función para minimizar/expandir tarjetas
function toggleTarjeta(idTarjeta) {
    const contenido = document.getElementById(`contenido_${idTarjeta}`);
    const icono = document.getElementById(`icono_${idTarjeta}`);

    if (contenido && icono) {
        if (contenido.style.display === 'none') {
            // Expandir
            contenido.style.display = 'block';
            icono.className = 'glyphicon glyphicon-minus';
        } else {
            // Minimizar
            contenido.style.display = 'none';
            icono.className = 'glyphicon glyphicon-menu-down';
        }
    }
}
// Función para minimizar todas las tarjetas
function minimizarTodasLasTarjetas() {
    const tarjetas = document.querySelectorAll('[id^="contenido_tarjeta_"]');
    const iconos = document.querySelectorAll('[id^="icono_tarjeta_"]');

    tarjetas.forEach(tarjeta => {
        tarjeta.style.display = 'none';
    });

    iconos.forEach(icono => {
        icono.className = 'glyphicon glyphicon-menu-down';
    });
}
// Función para expandir todas las tarjetas
function expandirTodasLasTarjetas() {
    const tarjetas = document.querySelectorAll('[id^="contenido_tarjeta_"]');
    const iconos = document.querySelectorAll('[id^="icono_tarjeta_"]');

    tarjetas.forEach(tarjeta => {
        tarjeta.style.display = 'block';
    });

    iconos.forEach(icono => {
        icono.className = 'glyphicon glyphicon-minus';
    });
}

// Función para recargar documentos con cambio de icono
function toggleTablaDocumentos(nomPersona, idTarjeta, subCarpetasEspecificas, identificador) {
    const iconoTabla = document.getElementById(`iconoTabla_${idTarjeta}`);
    const tabla = "#tbl_Docs_" + identificador;

    let persona = [];
    persona = [ nomPersona , identificador];


    // Cambiar icono a "cargando" temporalmente
    if (iconoTabla) {
        iconoTabla.className = 'glyphicon glyphicon-refresh gly-spin';
    }

    // Ejecutar la función de obtener documentos
    ObtenerListaDocsCargados("HistoriasClinicas", persona, 3, subCarpetasEspecificas, tabla);

    // Restaurar icono después de un momento (opcional)
    setTimeout(() => {
        if (iconoTabla) {
            iconoTabla.className = 'glyphicon glyphicon-refresh';
        }
    }, 1000);
}


/*      FUNCION PARA OBTENER LOS DATOS HSCLN    */
function ObtenerDatosHsCln(cedula, fec_ini, fec_fin) {

    var Datos = "[{ \"action\": \"ObtenerBuscarListaHsCln\", \"parameters\" : { fecha_ini: \"" + fec_ini + "\", fecha_fin: \"" + fec_fin + "\", opc: \"" + 1 + "\", session: \"" + "" + "\"} }]";

    CargarPaginaAdicional('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', Datos, "tableSelectBusquedaHsCln", "");
}
function RecorreJSONBusquedaHsCln(json, boton, idSeleccionado) {
    cargarHistorialAtenciones(json);
}
// Función para llamar desde tu pestaña
function cargarHistorialAtenciones(json) {
    // Crear las tarjetas
    crearTarjetasDesdeJSON(json, 'contenedorTarjetas');    
}
//function cargarHistorialAtenciones(json) {
//    if (json && Array.isArray(json.resultado)) {
//        crearTarjetasDesdeJSON(json.resultado, 'contenedorTarjetas');
//    } else {
//        console.warn('No hay historial de atenciones para mostrar.');
//    }
//}


/*==================================================================================*
 *    Función para obtener todos los archivos cargados de las atenciones medicas    *
 *==================================================================================*/
//function ObtenerListaDocsCargados(CarpetaPrincipal, nomPersona, niveles, listaCarpetas, tabla) {
//    var DatosLF = "[{ \"action\": \"BuscarListaArchivosGenerico\", \"parameters\" : { CarpetaPrincipal: \"" + CarpetaPrincipal + "\", listaCarpetas: \"" + listaCarpetas + "\", niveles: \"" + niveles + "\", session: \"" + "" + "\"} }]";

//    CargarPagina(nomPersona, 'ObtenerNuevaListaTareas.ashx', DatosLF, "tableSelectDocsCargados", tabla);
//}

function ObtenerListaDocsCargados(CarpetaPrincipal, nomPersona, niveles, listaCarpetas, tabla) {
    var DatosLF = "[{ \"action\": \"BuscarListaArchivosGenerico\", \"parameters\" : { CarpetaPrincipal: \"" + CarpetaPrincipal + "\", listaCarpetas: \"" + listaCarpetas + "\", niveles: \"" + niveles + "\", session: \"" + "" + "\"} }]";

    CargarPaginaSimple(nomPersona, 'ObtenerNuevaListaTareas.ashx', DatosLF, "tableSelectDocsCargados", tabla);
}


function CargarPaginaSimple(div, url, datos, tipoControl, boton, idSeleccionado) {

    if (div != undefined) {
        $.ajax({
            type: "POST",
            url: url,
            data: datos,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            beforeSend: function () {
                $("#divMensajes").html("Cargando Información...");
            },
            success: function (respuesta) {

                console.log("🔍 Respuesta del servidor:", respuesta); //borrar

                if (!respuesta) {
                    console.error("❌ La respuesta del servidor está vacía o es nula.");
                    return;
                }

                if (typeof respuesta === "string") {
                    try {
                        respuesta = JSON.parse(respuesta);
                    } catch (error) {
                        console.error("❌ No se pudo parsear la respuesta como JSON:", error);
                        console.error("❌ Respuesta recibida:", respuesta);
                        return;
                    }
                }

                if (respuesta != null) {
                    var idTotalRegistro = respuesta.length;
                    if (respuesta.length == 0) {
                        $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, boton));
                    }
                    else {
                        $(div).html(RecorreJSON(div, respuesta, tipoControl, idTotalRegistro, boton));
                    }
                } else {
                    $(div).html("No existen datos para esta consulta.");
                }

                $("#divMensajes").html("");

            },
            error: function (xhr, textStatus, errorThrown) {
                console.error("Error en AJAX:", {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    responseText: xhr.responseText,
                    textStatus: textStatus,
                    errorThrown: errorThrown
                });

                MensajeIncorrecto("Error al buscar los archivos.");
            }
        });
    }
}


function RecorreJSON(div, json, tipoControl, boton, extra) {
    var contenido = "";

    if (tipoControl == "tableSelectBusquedaHsCln") {
        contenido = RecorreJSONBusquedaHsCln(json, boton, extra);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectDocsCargados") {
        contenido = RecorreJSONTableSelectDocsCargados(div, json, boton, extra);
        $(div).html(contenido);
    }
    if (tipoControl == "tableSelectArchivos") {
        contenido = RecorreJSONTableSelectArchivo(json, boton, idSeleccionado);
        $(div).html(contenido);
    }

    return contenido;
}
function RecorreJSONTableSelectDocsCargados(nomPersona, json, boton, tabla) {
    //if (json && json.length > 0) {
    //document.getElementById("menuActionProyectos").style.display = "none";
    //document.getElementById("tablaDocsCargados").style.display = "block";
    dtDocsCargados(nomPersona, json, tabla);
}

function Create3(tabla) {
    if (!tabla.startsWith("#")) tabla = "#" + tabla;

    if ($.fn.DataTable.isDataTable(tabla)) {
        $(tabla).DataTable().clear().destroy();
    }

    $(tabla + " tbody").empty();

    // Reaplicar clases necesarias si se perdieron
    $(tabla).addClass("tablaDocsCargados dynamic-table table-hover sm-12");
}


function dtDocsCargados(nomPersona, json, tabla) {
    console.log("🔍 JSON recibido:", json);

    tabla = tabla.trim();
    if (!tabla.startsWith("#")) tabla = "#" + tabla;

    if (!Array.isArray(json)) {
        console.error("❌ JSON no es un array válido");
        return;
    }

    if (!$(tabla).length) {
        console.error("❌ La tabla no existe:", tabla);
        return;
    }

    // Limpiar tabla sin romper estructura
    Create3(tabla);

    // Reasegura que el thead mantenga la clase para estilos
    $(tabla + " thead").addClass("tabla-header-color");

    // Crear el DataTable sin perder el diseño
    const tablaInstancia = $(tabla).DataTable({
        data: json,
        columns: [
            {
                data: "Nombre",
                render: function (data) {
                    return data || "Archivo sin nombre";
                }
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    return `<a title='Descargar archivo' class='btn btn-abrirFormulario btn-xs'
                        style='text-align:center;'
                        data-ruta='${row.Ruta || ""}'
                        data-nombre='${row.Nombre || ""}'
                        data-persona='${nomPersona || ""}'>
                        <i class='glyphicon glyphicon-download-alt' style='color:#2a2a2a; text-align:center;'></i>
                    </a>`;
                }
            }
        ],
        language: {
            decimal: ",",
            thousands: ".",
            emptyTable: "No hay archivos disponibles",
            infoEmpty: "Mostrando 0 to 0 of 0 Entradas",
            loadingRecords: "Cargando...",
            processing: "Procesando...",
            search: "Buscar:",
            zeroRecords: "Sin archivos encontrados"
        },
        orderCellsTop: false,
        fixedHeader: true,
        lengthChange: false,
        paging: false,
        info: false,
        searching: false,
        responsive: true,
        autoWidth: false
    });
    console.log("✅ Tabla creada exitosamente con", json.length, "archivos");
}


/*=================================================================*
 *      Descargamos o abrimos el archivo seleccionado              *
 *=================================================================*/
$(document).on('click', '.btn-abrirFormulario', function () {

    const nombreArchivo = $(this).data('nombre');
    const rutaArchivo = $(this).data('ruta');
    const persona = $(this).data('persona');

    // Construye la URL completa al archivo
    var DatosA = "[{ \"action\": \"AbrirArchivoGenerico\", \"parameters\" : { nombreCarpetaPrincipal : \"" + "HistoriasClinicas" + "\", nombreCarpetaSecundaria : \"" + persona + "\", nombreArchivo: \"" + nombreArchivo + "\"} }]";
    CargarAbrirArchivo('#datosTablaPrincipal2', 'ObtenerNuevaListaTareas.ashx', DatosA, "");

});

function CargarAbrirArchivo(div, url, datos, tipoControl, boton, idSeleccionado) {

    if (div != undefined) {
        $.ajax({
            type: "POST",
            url: url,
            data: datos,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            beforeSend: function (respuesta) {
                $("#divMensajes").html("Cargando Información...");
            },
            success: function (respuesta) {
                var mensaje = "";
                if (respuesta.estado == "1") {
                    MensajeCorrecto("El archivo se ha descargado con exito.");

                    $("#divMensajes").html("");
                    window.open(respuesta.resultado);
                }
                else if (respuesta.estado == "0") {
                    MensajeIncorrecto("Ha ocurrido un error al descargar el archivo: ");
                }
            },
            error: function (objeto, msgError, objError) {
                var mesnajeError = "La acción de está tomando demasiado tiempo, la Red podría estar saturada, vuelva a intentarlo en unos segundos.";
                MensajeIncorrecto(mesnajeError);
            }
        });
    }
}



$(function () {
    ObtenerDatosHsCln("", "", "");

    
});