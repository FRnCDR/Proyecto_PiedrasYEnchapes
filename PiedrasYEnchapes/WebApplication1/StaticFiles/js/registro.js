$(document).ready(function () {

    // Cuando el usuario escribe en identificación
    $("#Identificacion").on("keyup", function () {
        var identificacion = $(this).val().trim();

        if (identificacion.length >= 8) {
            $("#Nombre").val("Consultando...");
            $("#Nombre")
                .closest(".input-group-modern")
                .addClass("is-focused is-filled");

            consultarCedula(identificacion);
        } else {
            $("#Nombre").val("");
            $("#Nombre")
                .closest(".input-group-modern")
                .removeClass("is-focused is-filled");
        }
    });

    function consultarCedula(identificacion) {
        $.ajax({
            url: urlConsultarCedula, // definida en la vista
            type: 'GET',
            data: { id: identificacion },
            dataType: 'json',
            success: function (result) {
                if (result.results && result.results.length > 0) {
                    var nombreCompleto = result.results[0].fullname;
                    $("#Nombre").val(nombreCompleto);
                    $("#Nombre")
                        .closest(".input-group-modern")
                        .addClass("is-focused is-filled");
                }
            },
            error: function () {
                $("#Nombre").val("Error al consultar");
                $("#Nombre")
                    .closest(".input-group-modern")
                    .addClass("is-focused is-filled");
            }
        });
    }

});

$(function () {
    $("#FormRegistro").validate({

        errorElement: 'span',
        errorClass: 'text-danger text-sm',

        errorPlacement: function (error, element) {
            if (element.parent(".input-group-modern").length) {
                element.parent().after(error);
            } else {
                error.insertAfter(element);
            }
        },

        highlight: function (element) {
            $(element)
                .closest('.input-group-modern')
                .addClass('is-invalid');
        },

        unhighlight: function (element) {
            $(element)
                .closest('.input-group-modern')
                .removeClass('is-invalid');
        },

        rules: {
            Identificacion: {
                required: true,
                minlength: 9
            },
            CorreoElectronico: {
                required: true,
                email: true
            },
            Contrasenna: {
                required: true,
                minlength: 6
            }
        },
        messages: {
            Identificacion: {
                required: "* Requerido",
                minlength: "* Debe tener al menos 9 dígitos"
            },
            CorreoElectronico: {
                required: "* Requerido",
                email: "* Ingresa un correo válido"
            },
            Contrasenna: {
                required: "* Requerido",
                minlength: "* Mínimo 6 caracteres"
            }
        }
    });
});
