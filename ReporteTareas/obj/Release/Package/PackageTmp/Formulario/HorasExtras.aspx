<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="HorasExtras.aspx.cs" Inherits="ReporteTareas.Formulario.HorasExtras" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        /* Solo para mostrar u ocultar el boton de reabrir. La barrera real
           esta en AdministrarHorasExtras.ashx.cs. */
        var HE_PUEDE_REABRIR = <%= PuedeReabrir ? "true" : "false" %>;
        /* Solo para mostrar los botones de completar datos y de ajuste de sueldo. */
        var HE_PUEDE_EDITAR_MAESTRO = <%= PuedeEditarMaestro ? "true" : "false" %>;
    </script>
    <script src="../js/horasExtras.js?v=10" type="text/javascript"></script>
    <style type="text/css">
        /* Estilos propios de esta pantalla. No tocan .table: dos-tema.css ya
           define tipografía y tamaños de las tablas del sistema. */
        .he-tablero {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
            position: sticky;
            top: 0;
            z-index: 5;
            background: var(--crm-superficie);
            padding: 10px 0 16px;
        }

        .he-indicador {
            flex: 1 1 150px;
            min-width: 150px;
            background: var(--crm-superficie-alt);
            border: 1px solid var(--crm-linea);
            border-radius: 6px;
            padding: 10px 14px;
        }

        .he-indicador-etiqueta {
            display: block;
            font-size: 11px;
            letter-spacing: .06em;
            text-transform: uppercase;
            color: var(--crm-tinta-suave);
        }

        .he-indicador-valor {
            display: block;
            margin-top: 2px;
            font-weight: 600;
            color: var(--crm-tinta);
            font-variant-numeric: tabular-nums;
        }

        /* Los cuatro estilos condicionales de fila se pintan sobre las <td>,
           no sobre el <tr>, y anclados a #tablaHE. Dos razones, las dos
           encontradas en revision:

           1) Un box-shadow inset sobre un <tr> no se pinta en Chrome/Edge
              cuando la tabla tiene border-collapse -que Bootstrap 3 aplica a
              toda .table-: Blink no compone box-shadow sobre filas de tabla
              colapsadas (Firefox si). Justo las 2 personas reales en
              revision salarial quedaban sin señal visual. Pintar sobre la
              <td> con background-color o border evita el problema de raiz.
           2) El color/fondo puesto en el <tr> perdia contra reglas mas
              especificas de dos-tema.css que apuntan directo a la <td>
              (".table > tbody > tr > td", el :first-child, y el :hover de
              .table-hover que borraba el condicional al pasar el mouse). Un
              selector anclado en el id de la tabla (#tablaHE) le gana a
              cualquier combinacion de clases sin depender del orden en que
              se cargan las hojas de estilo. */
        #tablaHE > tbody > tr.he-fila-con-horas > td { background-color: var(--crm-verde-tinte); }
        #tablaHE > tbody > tr.he-fila-no-aplica > td { background-color: var(--crm-neutro-tinte); color: var(--crm-tinta-suave); }
        #tablaHE > tbody > tr.he-fila-error > td { background-color: var(--crm-rojo-tinte); }

        /* Orden de prioridad visual: con-horas (mas debil) < no-aplica < error
           (mas fuerte). El de mayor prioridad va declarado al final, para que
           gane cuando una fila cae en mas de una condicion a la vez -las tres
           reglas de arriba tienen la misma especificidad, asi que el orden de
           declaracion es lo que decide-. */

        /* La franja de "en revision" es un borde real en la primera celda, no
           un fondo: convive con cualquiera de los tres de arriba sin taparlo.
           Con border-collapse, el borde mas ancho en un borde compartido es
           el que se ve, asi que este border-left de 4px si se respeta. */
        #tablaHE > tbody > tr.he-fila-revision > td:first-child { border-left: 4px solid var(--crm-ambar); }

        .he-fila-total td { font-weight: 600; border-top: 2px solid var(--crm-linea); }

        .he-horas50, .he-horas100 { text-align: right; }

        /* Misma razon que los cuatro de arriba: ".table > tbody > tr > td"
           de dos-tema.css tiene mas especificidad que una clase sola y
           ganaba, dejando esta columna con el color de texto normal en vez
           del atenuado. */
        #tablaHE td.he-col-derivada { color: var(--crm-tinta-suave); }

        /* Origen de las horas. «Tareas» es lo normal -lo sembró el sistema
           desde las solicitudes ya aprobadas- y va en gris discreto, con el
           mismo par de colores que .label-default; «Manual» es lo excepcional
           -alguien revisó esa fila y la corrigió a mano- y va en azul, que es
           el único color de esta pantalla que no significa ni bien ni mal,
           solo «mírame».

           El azul va en hexadecimal y no en una variable porque la paleta de
           dos-tema.css no tiene ninguno: sus cinco colores son acento, verde,
           ámbar, rojo y neutro, todos con carga de significado. Se respeta la
           forma de la casa -tinte de fondo, color saturado de texto- sin
           inventar una variable global para una sola pantalla. */
        .he-origen {
            display: inline-block;
            padding: 2px 8px;
            border-radius: var(--crm-r-pildora);
            font-size: 11px;
            font-weight: 600;
        }

        .he-origen-tareas { background: var(--crm-neutro-tinte); color: var(--crm-tinta-suave); }
        .he-origen-manual { background: #E8EFF8; color: #1B4F8A; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Cálculo de horas extras 50% y 100%</h3>
                    </div>
                </div>
            </div>
        </div>

        <!-- --------------------------------------------- periodo y filtros -->
        <div class="row" style="padding: 0 15px">
            <div class="col-lg-12">
                <div class="panel panel-default">
                    <div class="panel-body">
                        <div class="row">
                            <div class="form-group col-lg-3">
                                <label>Período</label>
                                <select class="form-control" id="selPeriodo" onchange="SeleccionarPeriodo()">
                                    <option value="">Seleccione…</option>
                                </select>
                            </div>
                            <div class="form-group col-lg-2">
                                <label>Estado</label>
                                <p class="form-control-static">
                                    <span id="lblEstadoPeriodo" class="label label-default">–</span>
                                </p>
                                <p class="text-muted" id="lblInfoCierre" style="font-size: 11px; margin: 0; display: none"></p>
                            </div>
                            <!-- El período dejó de ser un mes calendario: es un rango libre, y
                                 con eso entran las quincenas, que es lo que Nómina pide de
                                 verdad. Dos input type="date", que mandan siempre yyyy-MM-dd
                                 sin importar el idioma del navegador -es justo el formato que
                                 espera Fecha() en AdministrarHorasExtras.ashx.cs-. -->
                            <div class="form-group col-lg-2">
                                <label>Desde</label>
                                <input type="date" class="form-control" id="inFechaInicio" />
                            </div>
                            <div class="form-group col-lg-2">
                                <label>Hasta</label>
                                <input type="date" class="form-control" id="inFechaFin" />
                            </div>
                            <div class="form-group col-lg-4" style="padding-top: 25px">
                                <button type="button" class="btn btn-default" id="btnAbrirPeriodo" onclick="AbrirPeriodoSeleccionado()">
                                    <i class="fa fa-folder-open"></i> Abrir / actualizar período
                                </button>
                                <%-- Corregir las fechas de un período que YA existe, sin crear otro.
                                     Hasta ahora no había forma: «Abrir» busca el período por rango
                                     exacto, así que con una fecha distinta intentaba crear uno nuevo
                                     y chocaba con la validación de solapamiento contra el período
                                     que se quería corregir.

                                     Aparece sólo con un período abierto cargado. Uno cerrado ya se
                                     pagó con ese rango; para corregirlo hay que reabrirlo primero,
                                     que queda auditado. --%>
                                <button type="button" class="btn btn-default" id="btnEditarFechas" onclick="ConfirmarEditarFechas()" style="display: none">
                                    <i class="fa fa-calendar"></i> Corregir fechas
                                </button>
                            </div>
                        </div>

                        <hr />

                        <div class="row">
                            <div class="form-group col-lg-3">
                                <label>Empresa</label>
                                <select class="form-control" id="selEmpresa" onchange="AplicarFiltros()">
                                    <option value="">Todas</option>
                                </select>
                            </div>
                            <div class="form-group col-lg-4">
                                <label>Buscar</label>
                                <input type="text" class="form-control" id="txtBuscar"
                                       placeholder="Nombre, cédula o cargo…" oninput="AplicarFiltros()" />
                            </div>
                            <div class="form-group col-lg-2" style="padding-top: 25px">
                                <div class="checkbox">
                                    <label>
                                        <input type="checkbox" id="chkSoloConHoras" onchange="AplicarFiltros()" />
                                        Solo con horas
                                    </label>
                                </div>
                            </div>
                            <div class="form-group col-lg-3" style="padding-top: 25px; text-align: right">
                                <span id="lblGuardado" class="text-muted" style="display: none; margin-right: 10px"></span>
                                <%-- Recalcular no es lo mismo que elegir el período en el desplegable:
                                     aquello sólo lee lo guardado y esto vuelve a consultar las
                                     aprobaciones. Sin este botón hay que teclear otra vez el rango en
                                     «Abrir período», que nadie adivina y además se puede escribir mal. --%>
                                <button type="button" class="btn btn-default" id="btnRecalcular" onclick="ConfirmarRecalcular()" style="display: none">
                                    <i class="fa fa-refresh"></i> Traer aprobaciones nuevas
                                </button>
                                <button type="button" class="btn btn-default" id="btnCerrarPeriodo" onclick="ConfirmarCerrarPeriodo()" style="display: none">
                                    <i class="fa fa-lock"></i> Cerrar período
                                </button>
                                <button type="button" class="btn btn-default" id="btnReabrirPeriodo" onclick="ConfirmarReabrirPeriodo()" style="display: none">
                                    <i class="fa fa-unlock"></i> Reabrir período
                                </button>
                                <button type="button" class="btn btn-default" id="btnExportarExcel" onclick="ExportarExcel()" disabled="disabled">
                                    <i class="fa fa-file-excel-o"></i> Exportar a Excel
                                </button>
                                <button type="button" class="btn btn-primary" id="btnGuardar" onclick="GuardarTodo()">
                                    <i class="fa fa-save"></i> Guardar
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- ------------------------------------------------- tablero (5.2) -->
        <div class="row" style="padding: 0 15px">
            <div class="col-lg-12">
                <div class="he-tablero">
                    <div class="he-indicador">
                        <span class="he-indicador-etiqueta">Total horas 50%</span>
                        <span class="he-indicador-valor" id="tabHoras50">0.00</span>
                    </div>
                    <div class="he-indicador">
                        <span class="he-indicador-etiqueta">Total pago 50%</span>
                        <span class="he-indicador-valor" id="tabPago50">USD 0.00</span>
                    </div>
                    <div class="he-indicador">
                        <span class="he-indicador-etiqueta">Total horas 100%</span>
                        <span class="he-indicador-valor" id="tabHoras100">0.00</span>
                    </div>
                    <div class="he-indicador">
                        <span class="he-indicador-etiqueta">Total pago 100%</span>
                        <span class="he-indicador-valor" id="tabPago100">USD 0.00</span>
                    </div>
                    <div class="he-indicador">
                        <span class="he-indicador-etiqueta">Total horas extra</span>
                        <span class="he-indicador-valor" id="tabTotalHoras">0.00</span>
                    </div>
                    <div class="he-indicador">
                        <span class="he-indicador-etiqueta">Total a pagar HE</span>
                        <span class="he-indicador-valor" id="tabTotalPagar">USD 0.00</span>
                    </div>
                </div>
            </div>
        </div>

        <!-- --------------------------------------------------------- grilla -->
        <div class="row" style="padding: 0 15px 20px">
            <div class="col-lg-12">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        Detalle por colaborador
                        <button type="button" class="btn btn-default btn-xs pull-right" onclick="AlternarColumnasDerivadas()">
                            <i class="fa fa-columns"></i> <span id="lblColumnasDerivadas">Mostrar columnas de detalle</span>
                        </button>
                    </div>
                    <div class="panel-body">
                        <!-- Personas con horas en el período que NO están en el módulo. Solo se
                             muestra si hay alguien. Sin montos: no tienen sueldo cargado. Lo
                             llena PintarFueraDelModulo en horasExtras.js, siempre con .text(). -->
                        <div class="alert alert-warning" id="recuadroFueraDelModulo" style="display: none">
                            <strong id="tituloFueraDelModulo"></strong>
                            <div id="motivoFueraDelModulo"></div>
                            <div class="table-responsive" style="margin-top: 8px">
                                <table class="table table-condensed table-bordered" style="background: #fff; margin-bottom: 0">
                                    <thead>
                                        <tr>
                                            <th>Colaborador</th>
                                            <th>Cargo</th>
                                            <th>Aprobadas 50% (h)</th>
                                            <th>Aprobadas 100% (h)</th>
                                            <th>Fuera de horario sin aprobar (h)</th>
                                            <th>Rechazado (h)</th>
                                            <th></th>
                                        </tr>
                                    </thead>
                                    <tbody id="cuerpoFueraDelModulo"></tbody>
                                </table>
                            </div>
                        </div>
                        <div class="table-responsive">
                            <table class="table table-bordered table-hover" id="tablaHE">
                                <thead>
                                    <tr>
                                        <th>Colaborador</th>
                                        <th>Cargo</th>
                                        <th>Jornada</th>
                                        <th>Aplica HE</th>
                                        <th class="he-col-derivada" style="display: none">Divisor</th>
                                        <th class="he-col-derivada" style="display: none">Valor hora ord.</th>
                                        <th>Horas 50%</th>
                                        <th class="he-col-derivada" style="display: none">Valor hora 50%</th>
                                        <th class="he-col-derivada" style="display: none">Total 50%</th>
                                        <th>Horas 100%</th>
                                        <th class="he-col-derivada" style="display: none">Valor hora 100%</th>
                                        <th class="he-col-derivada" style="display: none">Total 100%</th>
                                        <th>Total horas</th>
                                        <th>Total HE (USD)</th>
                                        <!-- Informativas: tiempo registrado fuera del horario de la
                                             persona que NO entra en el pago. Las llena
                                             CargarFueraDeHorario después de dibujar la grilla. -->
                                        <th title="Tiempo fuera de horario sin solicitar, pendiente de aprobación o aprobado sin tipo. No se paga.">Fuera de horario sin aprobar (h)</th>
                                        <th title="Tiempo fuera de horario que la jefatura rechazó. No se paga.">Rechazado (h)</th>
                                        <th>Observación</th>
                                        <!-- Columna nueva de la fase 4, al final de las que ya
                                             había: dice si las horas las sembró el sistema desde
                                             las tareas aprobadas o si las revisó una persona. -->
                                        <th>Origen</th>
                                    </tr>
                                </thead>
                                <tbody id="cuerpoHE">
                                    <tr>
                                        <td colspan="18" class="text-center text-muted">
                                            Seleccione o abra un período para comenzar.
                                        </td>
                                    </tr>
                                </tbody>
                                <tfoot>
                                    <tr class="he-fila-total">
                                        <td colspan="4">TOTAL GENERAL</td>
                                        <td class="he-col-derivada" style="display: none"></td>
                                        <td class="he-col-derivada" style="display: none"></td>
                                        <td id="pieHoras50">0.00</td>
                                        <td class="he-col-derivada" style="display: none"></td>
                                        <td class="he-col-derivada" id="piePago50" style="display: none">USD 0.00</td>
                                        <td id="pieHoras100">0.00</td>
                                        <td class="he-col-derivada" style="display: none"></td>
                                        <td class="he-col-derivada" id="piePago100" style="display: none">USD 0.00</td>
                                        <td id="pieTotalHoras">0.00</td>
                                        <td id="pieTotalHE">USD 0.00</td>
                                        <td id="pieFueraSinAprobar"></td>
                                        <td id="pieFueraRechazado"></td>
                                        <td></td>
                                        <td></td>
                                    </tr>
                                </tfoot>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Informativo -->
        <div class="modal fade" id="modalMensajeInformativo" tabindex="-1" role="dialog" aria-labelledby="modalMensajeInformativoLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header" style="background: #fcf8e3" id="modalMensajeInformativoTipo">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="myModalLabel">Informativo</h4>
                    </div>
                    <div class="modal-body" id="MensajeInformativo">
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->

        <!-- Modal de confirmacion generico de esta pantalla -MostrarConfirmacion
             en horasExtras.js le cambia titulo, texto y boton segun quien lo
             llame-, en vez de confirm()/alert() del navegador, que no dejan
             mostrar nombres en negrita ni una lista. Nacio para el pegado de
             una columna desde Excel -el reparto es por POSICION, no hay
             columna de cedula con la que verificar la correspondencia, asi
             que antes de tocar ninguna celda se le muestra a la persona en
             que colaborador empieza y en cual termina- y lo reusan cerrar y
             reabrir el periodo. -->
        <div class="modal fade" id="modalConfirmarPegado" tabindex="-1" role="dialog" aria-labelledby="modalConfirmarPegadoLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalConfirmarPegadoLabel">Confirmar pegado</h4>
                    </div>
                    <div class="modal-body" id="textoConfirmarPegado">
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" class="btn btn-primary" id="btnConfirmarPegado">Sí, aplicar</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->

        <!-- Detalle del tiempo fuera de horario de un colaborador. Solo lectura:
             sirve para saber QUÉ tramos pedirle a la persona que solicite, o cuáles
             rechazó la jefatura. Lo llena MostrarDetalleFuera en horasExtras.js,
             siempre con .text(): la actividad es texto libre de la persona. -->
        <div class="modal fade" id="modalFueraDeHorario" tabindex="-1" role="dialog" aria-labelledby="tituloFueraDeHorario" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="tituloFueraDeHorario">Tiempo fuera de horario</h4>
                    </div>
                    <div class="modal-body">
                        <p class="text-muted">Información para revisión: estas horas NO están incluidas en los totales a pagar.</p>
                        <div class="table-responsive">
                            <table class="table table-condensed table-bordered">
                                <thead>
                                    <tr>
                                        <th>Fecha</th>
                                        <th>Tramo registrado</th>
                                        <th>Fuera de horario</th>
                                        <th>Min.</th>
                                        <th>Tipo</th>
                                        <th>Estado</th>
                                        <th>Actividad</th>
                                    </tr>
                                </thead>
                                <tbody id="cuerpoFueraDeHorario"></tbody>
                            </table>
                        </div>
                        <p id="pieFueraDeHorario"></p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->

        <!-- Completar los datos de una persona que no está en el módulo. Solo perfiles
             14 y 18 (el handler lo vuelve a comprobar). -->
        <div class="modal fade" id="modalCompletarDatos" tabindex="-1" role="dialog" aria-labelledby="tituloCompletarDatos" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="tituloCompletarDatos">Completar datos</h4>
                    </div>
                    <div class="modal-body">
                        <p class="text-muted">La persona entra al módulo con estos datos. El sueldo se agrega al historial; no reemplaza ningún otro.</p>
                        <div class="form-group">
                            <label for="cdEmpresa">Empresa</label>
                            <select class="form-control" id="cdEmpresa"></select>
                        </div>
                        <div class="form-group">
                            <label for="cdCargo">Cargo</label>
                            <input type="text" class="form-control" id="cdCargo" maxlength="200" />
                        </div>
                        <div class="form-group">
                            <label for="cdJornada">Jornada (horas por día)</label>
                            <input type="number" class="form-control" id="cdJornada" min="1" max="12" value="8" />
                        </div>
                        <div class="checkbox">
                            <label><input type="checkbox" id="cdAplicaHE" checked="checked" /> Aplica horas extras</label>
                        </div>
                        <div class="form-group">
                            <label for="cdMonto">Sueldo (USD)</label>
                            <input type="text" class="form-control" id="cdMonto" maxlength="20" placeholder="Ej.: 1850 o 1850,50" />
                        </div>
                        <div class="form-group">
                            <label for="cdVigencia">Rige desde</label>
                            <input type="date" class="form-control" id="cdVigencia" />
                        </div>
                        <!-- Confirmación antes de guardar: nombre, sueldo y fecha a la vista. -->
                        <div class="alert alert-danger" id="cdConfirmacion" style="display: none"></div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" class="btn btn-primary" id="btnGuardarCompletarDatos">Guardar</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->

        <!-- Registrar un ajuste de sueldo de alguien que ya está en el módulo. Agrega una
             fila al historial: nunca borra ni modifica la anterior. -->
        <div class="modal fade" id="modalAjusteSalario" tabindex="-1" role="dialog" aria-labelledby="tituloAjusteSalario" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="tituloAjusteSalario">Registrar ajuste de sueldo</h4>
                    </div>
                    <div class="modal-body">
                        <p class="text-muted" id="textoAjusteSalario"></p>
                        <div class="form-group">
                            <label for="ajMonto">Sueldo nuevo (USD)</label>
                            <input type="text" class="form-control" id="ajMonto" maxlength="20" placeholder="Ej.: 1650 o 1650,50" />
                        </div>
                        <div class="form-group">
                            <label for="ajVigencia">Rige desde</label>
                            <input type="date" class="form-control" id="ajVigencia" />
                        </div>
                        <div class="form-group">
                            <label for="ajObservacion">Observación</label>
                            <input type="text" class="form-control" id="ajObservacion" maxlength="400" placeholder="Motivo del ajuste" />
                        </div>
                        <!-- Confirmación antes de guardar: nombre, sueldo y fecha a la vista. -->
                        <div class="alert alert-danger" id="ajConfirmacion" style="display: none"></div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" class="btn btn-primary" id="btnGuardarAjusteSalario">Guardar</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->
    </div>
</asp:Content>
