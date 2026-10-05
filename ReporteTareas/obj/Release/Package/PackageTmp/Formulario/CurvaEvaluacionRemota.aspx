<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CurvaEvaluacionRemota.aspx.cs" Inherits="ReporteTareas.Formulario.CurvaEvaluacionRemota" ResponseEncoding="utf-8" EnableViewState="false" EnableSessionState="False" %>
<%-- UNICA pagina anonima del modulo: sin Master.Master, sin menu y sin sesion.
     La credencial es el token del enlace. Esta pagina no carga NINGUN dato en
     el servidor (no hay ViewState que filtrar): todo llega por
     CurvaEvaluacionRemota.ashx, que solo entrega lo que permite la seccion
     11.3 de la especificacion. jQuery va local, no del CDN. --%>
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="robots" content="noindex, nofollow" />
    <meta name="referrer" content="no-referrer" />
    <title>Evaluación — Curva de aprendizaje DOS</title>
    <link href="../css/curvaAprendizaje.css?v=1" rel="stylesheet" />
</head>
<body class="ca-remota">
    <div class="ca-pagina">
        <div class="ca-encabezado" id="caEncabezado">
            <p class="ca-kicker">Talento Humano · DOS</p>
            <h1 id="caTitulo">Curva de aprendizaje</h1>
            <p class="ca-sub" id="caSubtitulo"></p>
        </div>
        <div id="caEstado" class="ca-panel" style="display: none"></div>
        <div id="caFormulario"><div class="ca-vacio">Cargando…</div></div>
        <div id="caError" class="ca-aviso" style="display: none; margin-top: 12px"></div>
        <div class="ca-guardar" id="caBotonera" style="display: none">
            <button type="button" id="caEnviar" class="ca-btn ca-btn-primario">Enviar evaluación</button>
        </div>
    </div>
    <script src="../js/jquery.min.js" type="text/javascript"></script>
    <script src="../js/curvaComun.js?v=1" type="text/javascript"></script>
    <script src="../js/curvaFormulario.js?v=1" type="text/javascript"></script>
    <script src="../js/curvaRemota.js?v=1" type="text/javascript"></script>
</body>
</html>
