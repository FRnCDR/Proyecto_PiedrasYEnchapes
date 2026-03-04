// EditarPerfil.js

console.log("EditarPerfil.js cargado. urlConsultarCedulaEditarPerfil =",
    typeof urlConsultarCedulaEditarPerfil !== "undefined" ? urlConsultarCedulaEditarPerfil : "NO DEFINIDA"
);

function consultarCedulaEditarPerfil(identificacion) {
    $.ajax({
        url: urlConsultarCedulaEditarPerfil, // variable definida en la vista
        type: 'GET',
        data: { id: identificacion },
        dataType: 'json',
        success: function (result) {

            if (result.results && result.results.length > 0) {
                var nombreCompleto = result.results[0].fullname;
                $("#Nombre").val(nombreCompleto);
            } else {
                console.log("No se encontró el nombre");
                $("#Nombre").val("No encontrado");
            }
        },
        error: function () {
            console.log("Error en la consulta");
            $("#Nombre").val("Error al consultar");
        }
    });
}

$(document).ready(function () {

    let timer = null;

    $("#Identificacion").on("keyup", function () {
        clearTimeout(timer);

        var identificacion = $(this).val().trim();

        // Espera un toque para no disparar 200 llamadas mientras escribe
        timer = setTimeout(function () {
            if (identificacion.length >= 8) { // 8 o 9, según tu regla
                console.log("Consultando cédula:", identificacion);
                consultarCedulaEditarPerfil(identificacion);
            } else {
                // si es muy corta, limpia el nombre
                $("#Nombre").val("");
            }
        }, 350);
    });

});