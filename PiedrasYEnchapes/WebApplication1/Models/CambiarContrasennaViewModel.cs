using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;




namespace WebApplication1.Models
{
    public class CambiarContrasennaViewModel
    {
        [Required(ErrorMessage = "Debe ingresar la contraseña actual")]
        public string ContrasennaActual { get; set; }

        [Required(ErrorMessage = "Debe ingresar la nueva contraseña")]
        public string NuevaContrasenna { get; set; }

        [Required(ErrorMessage = "Debe confirmar la nueva contraseña")]
        [Compare("NuevaContrasenna", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasenna { get; set; }
    }
}