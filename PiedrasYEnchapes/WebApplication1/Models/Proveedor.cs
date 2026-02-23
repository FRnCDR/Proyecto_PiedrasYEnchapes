using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Proveedor
    {
        public int ProveedorID { get; set; }

        [Required(ErrorMessage = "El nombre de la empresa es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre de la empresa no puede exceder los 150 caracteres.")]
        public string NombreEmpresa { get; set; }

        [Required(ErrorMessage = "El contacto es obligatorio.")]
        [StringLength(100, ErrorMessage = "El contacto no puede exceder los 100 caracteres.")]
        public string Contacto { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres.")]
        public string CorreoElectronico { get; set; }

        [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
        public string Telefono { get; set; }

        [StringLength(255, ErrorMessage = "La dirección no puede exceder los 255 caracteres.")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El tipo de proveedor es obligatorio.")]
        [StringLength(20, ErrorMessage = "El tipo no puede exceder los 20 caracteres.")]
        public string Tipo { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}