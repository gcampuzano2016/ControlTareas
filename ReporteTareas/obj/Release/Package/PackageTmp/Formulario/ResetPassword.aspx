<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="ReporteTareas.Formulario.ResetPassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <!--//////////////////////////////////////////////////////////////////////////////////////////////////////////////-->
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="description" content="">
    <meta name="author" content="DOS">
    <title class="">Control de Tareas</title>
    <link rel="icon" type="image/png" href="../Img/logo_ct.png" sizes="16x16">
	
	<script src="../js/ResetProceso.js?v=4" type="text/javascript"></script>
	
    <link href="../bower_components/bootstrap/dist/css/bootstrap.min.css" rel="stylesheet">
    <link href="../bower_components/metisMenu/dist/metisMenu.min.css" rel="stylesheet">
    <link href="../dist/css/sb-admin-2.css" rel="stylesheet">
    <link href="../bower_components/font-awesome/css/font-awesome.min.css" rel="stylesheet" type="text/css">
	
    <link href="../bower_components/sweetalert/css/sweetalert.css" rel="stylesheet" />
    <script src="../bower_components/sweetalert/js/sweetalert.min.js"></script>
    <script src="../bower_components/sweetalert/js/sweetalert.init.js"></script>
	
	<link rel="stylesheet" href="https://code.jquery.com/ui/1.10.3/themes/smoothness/jquery-ui.css" />
    <script src="https://code.jquery.com/jquery-1.9.1.js"></script>
    <script src="https://code.jquery.com/ui/1.10.3/jquery-ui.js"></script>

	
    <!--//////////////////////////////////////////////////////////////////////////////////////////////////////////////-->
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <div class="container">
                <div class="row">
                    <div class="col-md-4 col-md-offset-4">
                        <div class="login-panel panel panel-info">
							<div class="panel-heading" style="text-align: center">
								<img src="../Img/logo_ct.png" height="50" /><br/>
								<h2 class="text-center panel-collapse">Actualizar Clave</h2>Versión 2.5
							</div>						
                            <div class="panel-body">
                                <fieldset>
                                    <div class="form-group">
                                        <input type="text" class="form-control" id="txtUsuario" placeholder="Usuario">
                                    </div>
                                    <div class="form-group" style="display: none">
                                        <input type="Password" class="form-control" id="txtcodigoConfirmacion" placeholder="codigo Confirmacion">
                                    </div>									
                                    <div class="form-group">
                                        <input type="Password" class="form-control" id="txtClaveNueva" placeholder="Nueva clave">
                                    </div>
                                    <div class="form-group">
                                        <input type="Password" class="form-control" id="txtConfirmarClave" placeholder="Confirmar clave">
                                    </div>
                                    <div class="form-group" style="text-align: center;">
                                        <button id="btnGuardar" onclick="ResetUsuario()" type="button" class="btn btn-lg btn-primary btn-block">Aceptar</button>
                                    </div>
                                </fieldset>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <script src="../bower_components/jquery/dist/jquery.min.js"></script>
            <script src="../bower_components/bootstrap/dist/js/bootstrap.min.js"></script>
            <script src="../bower_components/metisMenu/dist/metisMenu.min.js"></script>
            <script src="../dist/js/sb-admin-2.js"></script>
        </div>
		<div class="modal modal-static fade" id="processing-modal" role="dialog" aria-hidden="true">
			<div class="modal-dialog">
				<div class="modal-content">
					<div class="modal-body">
						<div class="text-center">
							<img src="../Img/loading.gif" class="icon" />
							<h5><span class="modal-text">Procesando, Espere por favor... </span></h5>
						</div>
					</div>
				</div>
			</div>
		</div>		
    </form>
</body>
</html>
