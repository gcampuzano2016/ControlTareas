<%@ Page Title="" Language="C#" MasterPageFile="~/Formulario/Master.Master" AutoEventWireup="true" CodeBehind="HorasExtras.aspx.cs" Inherits="ReporteTareas.Formulario.HorasExtras" ResponseEncoding="utf-8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="../js/horasExtras.js?v=1" type="text/javascript"></script>
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

        /* Orden de prioridad visual: con-horas (mas debil) < no-aplica < error
           (mas fuerte). El de mayor prioridad va declarado al final, para que
           gane cuando una fila cae en mas de una condicion a la vez. */
        .he-fila-con-horas { background-color: var(--crm-verde-tinte); }
        .he-fila-no-aplica  { background-color: var(--crm-neutro-tinte); color: var(--crm-tinta-suave); }
        .he-fila-error      { background-color: var(--crm-rojo-tinte); }

        /* La franja de "en revision" es un borde, no un fondo: convive con
           cualquiera de los tres de arriba sin taparlo. */
        .he-fila-revision { box-shadow: inset 4px 0 0 0 var(--crm-ambar); }

        .he-fila-total td { font-weight: 600; border-top: 2px solid var(--crm-linea); }

        .he-horas50, .he-horas100 { text-align: right; }
        .he-col-derivada { color: var(--crm-tinta-suave); }
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
                            </div>
                            <div class="form-group col-lg-2">
                                <label>Abrir año</label>
                                <input type="number" class="form-control" id="inAnioAbrir" min="2020" max="2100" />
                            </div>
                            <div class="form-group col-lg-2">
                                <label>Abrir mes</label>
                                <select class="form-control" id="inMesAbrir">
                                    <option value="1">Enero</option>
                                    <option value="2">Febrero</option>
                                    <option value="3">Marzo</option>
                                    <option value="4">Abril</option>
                                    <option value="5">Mayo</option>
                                    <option value="6">Junio</option>
                                    <option value="7">Julio</option>
                                    <option value="8">Agosto</option>
                                    <option value="9">Septiembre</option>
                                    <option value="10">Octubre</option>
                                    <option value="11">Noviembre</option>
                                    <option value="12">Diciembre</option>
                                </select>
                            </div>
                            <div class="form-group col-lg-3" style="padding-top: 25px">
                                <button type="button" class="btn btn-default" onclick="AbrirPeriodoSeleccionado()">
                                    <i class="fa fa-folder-open"></i> Abrir / actualizar período
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
                                        <th>Observación</th>
                                    </tr>
                                </thead>
                                <tbody id="cuerpoHE">
                                    <tr>
                                        <td colspan="15" class="text-center text-muted">
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
    </div>
</asp:Content>
