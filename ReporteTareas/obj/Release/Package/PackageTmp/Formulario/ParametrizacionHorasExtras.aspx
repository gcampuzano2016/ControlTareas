<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="ParametrizacionHorasExtras.aspx.cs" Inherits="ReporteTareas.Formulario.ParametrizacionHorasExtras" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/parametrosHorasExtras.js?v=1" type="text/javascript"></script>
    <style type="text/css">
        /* Estilos propios de esta pantalla. No tocan .table: dos-tema.css ya
           define tipografía y tamaños de las tablas del sistema. */

        /* Las marcas de una fila -«Fijado por el Código del Trabajo» y
           «Cargado, todavía sin uso en el cálculo»- van debajo de la etiqueta,
           no en una columna propia: solo cuatro de las siete filas las llevan,
           y una columna vacía en tres de siete se lee como si faltara el dato.

           Ninguna de las dos se escribe aquí ni en el .js: la primera llega
           como FijadoPorLey y la segunda como Activo, las dos desde el
           handler, que a su vez las toma de NegHeParametroPantalla. La
           etiqueta del parámetro tampoco: llega como Etiqueta. Esta pantalla
           no conoce ni una sola clave de memoria. */
        .pa-marca {
            display: inline-block;
            margin-top: 3px;
            padding: 2px 8px;
            border-radius: var(--crm-r-pildora);
            font-size: 11px;
            font-weight: 600;
        }

        /* Ámbar para lo que la ley fija -«se puede, pero mírelo dos veces»- y
           neutro para lo que está cargado sin uso -«no es un aviso, es un
           dato»-. Son los dos únicos colores con los que esta pantalla dice
           algo; el resto es tabla. */
        .pa-marca-ley { background: var(--crm-ambar-tinte); color: var(--crm-ambar); }
        .pa-marca-inactivo { background: var(--crm-neutro-tinte); color: var(--crm-tinta-suave); }

        /* Misma razón que en HorasExtras.aspx: ".table > tbody > tr > td" de
           dos-tema.css tiene más especificidad que una clase sola y le gana,
           así que lo condicional se ancla al id de la tabla. */
        #tablaParametros > tbody > tr.pa-fila-seleccionada > td { background-color: var(--crm-neutro-tinte); }
        #tablaParametros > tbody > tr.pa-fila-inactiva > td.pa-col-valor { color: var(--crm-tinta-suave); }

        .pa-valor { font-weight: 600; font-variant-numeric: tabular-nums; }
        .pa-col-valor, #tablaHistorial td.pa-col-valor { text-align: right; }
        .pa-clave { display: block; font-size: 11px; color: var(--crm-tinta-suave); }

        /* El aviso de vigencia. Es lo único de esta pantalla que no se puede
           pasar por alto: dice qué se recalcula y qué no, que es justo lo que
           nadie tiene claro al cambiar un factor a mitad de mes. */
        .pa-aviso {
            border-left: 4px solid var(--crm-ambar);
            background: var(--crm-superficie-alt);
            padding: 10px 14px;
            margin-top: 12px;
            font-size: 13px;
        }

        .pa-historial-vacio { color: var(--crm-tinta-suave); }

        /* La versión vigente dentro del historial. El historial trae todas las
           versiones de la clave, la de hoy incluida, y sin esta marca las
           filas se ven todas iguales: hay que ir a mirar la columna «Hasta»
           para saber cuál es la que rige. */
        #tablaHistorial > tbody > tr.pa-fila-vigente > td { font-weight: 600; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="page-wrapper" style="padding: 0px">
        <div class="row">
            <div class="col-lg-12" style="padding: 20px">
                <div class="card card-primary">
                    <div class="card-header" style="text-align: center">
                        <h3>Parámetros de cálculo de horas extras</h3>
                    </div>
                </div>
            </div>
        </div>

        <!-- ------------------------------------------- parámetros vigentes -->
        <div class="row" style="padding: 0 15px">
            <div class="col-lg-12">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        Valores vigentes
                        <span class="pull-right text-muted" style="font-size: 11px">
                            Cambiar un valor no altera los períodos ya cerrados: crea una versión nueva.
                        </span>
                    </div>
                    <div class="panel-body">
                        <div class="table-responsive">
                            <table class="table table-bordered table-hover" id="tablaParametros">
                                <thead>
                                    <tr>
                                        <th>Parámetro</th>
                                        <th class="pa-col-valor">Valor vigente</th>
                                        <th>Rige desde</th>
                                        <th>Lo cambió</th>
                                        <th>Cuándo</th>
                                        <th style="width: 190px"></th>
                                    </tr>
                                </thead>
                                <tbody id="cuerpoParametros">
                                    <tr>
                                        <td colspan="6" class="text-center text-muted">Cargando…</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- ------------------------------------------------------ historial -->
        <div class="row" style="padding: 0 15px 20px">
            <div class="col-lg-12">
                <div class="panel panel-default">
                    <div class="panel-heading">
                        Historial de <span id="lblClaveHistorial">—</span>
                    </div>
                    <div class="panel-body">
                        <div class="table-responsive">
                            <table class="table table-bordered" id="tablaHistorial">
                                <thead>
                                    <tr>
                                        <th class="pa-col-valor">Valor</th>
                                        <th>Desde</th>
                                        <th>Hasta</th>
                                        <th>Lo cambió</th>
                                        <th>Cuándo</th>
                                    </tr>
                                </thead>
                                <tbody id="cuerpoHistorial">
                                    <tr>
                                        <td colspan="5" class="text-center pa-historial-vacio">
                                            Elija «Ver historial» en un parámetro para ver sus versiones.
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal de edición de un parámetro -->
        <div class="modal fade" id="modalEditarParametro" tabindex="-1" role="dialog" aria-labelledby="modalEditarParametroLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalEditarParametroLabel">Editar parámetro</h4>
                    </div>
                    <div class="modal-body">
                        <div class="form-group">
                            <label>Parámetro</label>
                            <p class="form-control-static">
                                <strong id="edEtiqueta"></strong>
                                <span class="pa-clave" id="edClave"></span>
                            </p>
                        </div>
                        <div class="row">
                            <div class="form-group col-lg-4">
                                <label>Valor vigente</label>
                                <p class="form-control-static pa-valor" id="edValorActual">—</p>
                            </div>
                            <div class="form-group col-lg-4">
                                <label for="edValorNuevo">Valor nuevo</label>
                                <%-- type="text" y no type="number": un input numérico con
                                     step por omisión marca 1.5 como inválido en algunos
                                     navegadores y, peor, devuelve cadena vacía cuando lo
                                     escrito no le gusta -así que el valor tecleado se
                                     perdía sin decir nada-. El servidor valida igual. --%>
                                <input type="text" class="form-control" id="edValorNuevo" autocomplete="off" />
                            </div>
                            <div class="form-group col-lg-4">
                                <label for="edDesde">Rige desde</label>
                                <input type="date" class="form-control" id="edDesde" />
                            </div>
                        </div>
                        <div class="pa-aviso" id="edAviso"></div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" class="btn btn-primary" id="btnGuardarParametro" onclick="ConfirmarGuardar()">
                            <i class="fa fa-save"></i> Guardar
                        </button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->

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

        <!-- Modal de confirmación, mismo patrón que el de HorasExtras.aspx:
             MostrarConfirmacion le cambia título, texto y botón según quién lo
             llame. No se usa confirm() del navegador porque el aviso de
             vigencia lleva la fecha en negrita y dos párrafos, y un confirm()
             solo admite una línea de texto plano. -->
        <div class="modal fade" id="modalConfirmar" tabindex="-1" role="dialog" aria-labelledby="modalConfirmarLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>
                        <h4 class="modal-title" id="modalConfirmarLabel">Confirmar</h4>
                    </div>
                    <div class="modal-body" id="textoConfirmar">
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Cancelar</button>
                        <button type="button" class="btn btn-primary" id="btnConfirmar">Sí, guardar</button>
                    </div>
                </div>
            </div>
        </div>
        <!-- /.modal -->
    </div>
</asp:Content>
