using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "La identificación es obligatoria.")]
        [StringLength(20)]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100)]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(100)]
        public string CorreoElectronico { get; set; }

        [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
        [StringLength(20)]
        public string Telefono { get; set; }

        [StringLength(255)]
        public string Direccion { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }

        // Propiedad calculada
        public string NombreCompleto
        {
            get { return $"{Nombre} {Apellidos}"; }
        }
    }
}