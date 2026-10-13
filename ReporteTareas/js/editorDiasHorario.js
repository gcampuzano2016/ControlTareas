/* ============================================================================
   Editor de los siete dias de un horario.

   Lo usan dos pantallas: el catalogo (ParametrizacionHorario.aspx) y el horario
   propio de una persona (ParametrizacionHorarioUsuario.aspx). Por eso vive
   aparte y todo va con un prefijo: en una misma pagina pueden convivir dos
   editores sin pisarse los id.

   El formato de ida y vuelta es el mismo que entienden los SPs:
       [{ ds: 1, lab: 1, ini: "08:30", fin: "17:30" }, ...]
   ============================================================================ */

var DIAS_SEMANA = [
    { ds: 1, nombre: "Lunes" },
    { ds: 2, nombre: "Martes" },
    { ds: 3, nombre: "Miércoles" },
    { ds: 4, nombre: "Jueves" },
    { ds: 5, nombre: "Viernes" },
    { ds: 6, nombre: "Sábado" },
    { ds: 7, nombre: "Domingo" }
];

/* Dibuja las siete filas dentro del tbody indicado. Se llama una sola vez. */
function DibujarEditorDias(idTbody, prefijo) {
    var filas = "";

    $.each(DIAS_SEMANA, function (i, dia) {
        filas += "<tr>";
        filas += "<td style='vertical-align:middle'>" + dia.nombre + "</td>";
        filas += "<td style='text-align:center;vertical-align:middle'>";
        filas += "<input type='checkbox' id='" + prefijo + "_lab_" + dia.ds + "'";
        filas += " onchange=\"AlternarDiaEditor('" + prefijo + "'," + dia.ds + ")\" />";
        filas += "</td>";
        filas += "<td><input type='time' class='form-control' id='" + prefijo + "_ini_" + dia.ds + "' /></td>";
        filas += "<td><input type='time' class='form-control' id='" + prefijo + "_fin_" + dia.ds + "' /></td>";
        filas += "</tr>";
    });

    $("#" + idTbody).html(filas);
}

/* Un dia no laborable no tiene horas que llenar. */
function AlternarDiaEditor(prefijo, ds) {
    var laborable = $("#" + prefijo + "_lab_" + ds).is(":checked");

    $("#" + prefijo + "_ini_" + ds).prop("disabled", !laborable);
    $("#" + prefijo + "_fin_" + ds).prop("disabled", !laborable);

    if (!laborable) {
        $("#" + prefijo + "_ini_" + ds).val("");
        $("#" + prefijo + "_fin_" + ds).val("");
    }
}

/* Deja el editor como una jornada de lunes a viernes en blanco. */
function LimpiarEditorDias(prefijo) {
    $.each(DIAS_SEMANA, function (i, dia) {
        var laborable = (dia.ds <= 5);

        $("#" + prefijo + "_lab_" + dia.ds).prop("checked", laborable);
        $("#" + prefijo + "_ini_" + dia.ds).val("");
        $("#" + prefijo + "_fin_" + dia.ds).val("");

        AlternarDiaEditor(prefijo, dia.ds);
    });
}

/* Carga lo que devolvio Sp_RTA_ObtenerHorario. */
function LlenarEditorDias(prefijo, lista) {
    LimpiarEditorDias(prefijo);

    if (lista == null) { return; }

    $.each(lista, function (i, item) {
        var ds = item.DiaSemana;
        var laborable = (item.EsLaborable == 1);

        $("#" + prefijo + "_lab_" + ds).prop("checked", laborable);
        AlternarDiaEditor(prefijo, ds);

        if (laborable) {
            $("#" + prefijo + "_ini_" + ds).val(item.HoraInicio);
            $("#" + prefijo + "_fin_" + ds).val(item.HoraFin);
        }
    });
}

/* Copia las horas del lunes al resto de la semana laboral. */
function AplicarLunesAViernes(prefijo) {
    var ini = $("#" + prefijo + "_ini_1").val();
    var fin = $("#" + prefijo + "_fin_1").val();

    if (ini === "" || fin === "") {
        return "Llene primero la hora de entrada y de salida del lunes.";
    }

    for (var ds = 2; ds <= 5; ds++) {
        $("#" + prefijo + "_lab_" + ds).prop("checked", true);
        AlternarDiaEditor(prefijo, ds);
        $("#" + prefijo + "_ini_" + ds).val(ini);
        $("#" + prefijo + "_fin_" + ds).val(fin);
    }

    $("#" + prefijo + "_lab_1").prop("checked", true);
    AlternarDiaEditor(prefijo, 1);
    $("#" + prefijo + "_ini_1").val(ini);
    $("#" + prefijo + "_fin_1").val(fin);

    return "";
}

/* Los siete dias listos para mandar al handler. */
function LeerEditorDias(prefijo) {
    var dias = [];

    $.each(DIAS_SEMANA, function (i, dia) {
        var laborable = $("#" + prefijo + "_lab_" + dia.ds).is(":checked");

        dias.push({
            "ds": dia.ds,
            "lab": laborable ? 1 : 0,
            "ini": laborable ? $("#" + prefijo + "_ini_" + dia.ds).val() : "",
            "fin": laborable ? $("#" + prefijo + "_fin_" + dia.ds).val() : ""
        });
    });

    return dias;
}

/* Las mismas reglas que valida el SP, pero sin ir al servidor.
   Devuelve "" cuando esta todo bien. */
function ValidarEditorDias(prefijo) {
    var dias = LeerEditorDias(prefijo);
    var hayLaborable = false;
    var error = "";

    $.each(dias, function (i, dia) {
        if (dia.lab != 1) { return; }

        hayLaborable = true;

        var nombre = DIAS_SEMANA[i].nombre;

        if (dia.ini === "" || dia.fin === "") {
            error = "Falta la hora de entrada o de salida del " + nombre + ".";
            return false;
        }

        if (dia.ini >= dia.fin) {
            error = "En " + nombre + " la hora de entrada debe ser anterior a la de salida.";
            return false;
        }
    });

    if (error !== "") { return error; }

    if (!hayLaborable) {
        return "El horario debe tener al menos un día laborable.";
    }

    return "";
}
