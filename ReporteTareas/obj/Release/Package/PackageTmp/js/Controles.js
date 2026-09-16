function myCreateFunction() {

    var table = document.getElementById("myTable");
    var row = table.insertRow(0);
    var cell1 = row.insertCell(0);
    var cell2 = row.insertCell(1);
    // CREO UN ELEMENTO DEL TIPO INPUT CON document.createElement("NOMBRE TAG HTML QUE QUIERO CREAR");



    var input = document.createElement("input");
    input.type = "text";
    input.className = "urlresp";
    input.value = "Usuario";
    input.style.height = "20px";
    input.style.width = "100px";

    // Creo un segundo elemento Input


    var input2 = document.createElement("input");
    input2.type = "text";
    input2.className = "ptss";
    input2.value = "0";
    input2.autofocus;
    input2.style.height = "20px";
    input2.style.width = "30px";


    // CON EL METODO appendChild(); LOS AGREGO A LA CELDA QUE QUIERO
    cell1.appendChild(input);
    cell2.appendChild(input2);

    document.getElementById("input2").onFocus();

}



function myDeleteFunction() {
    document.getElementById("myTable").deleteRow(0);
}