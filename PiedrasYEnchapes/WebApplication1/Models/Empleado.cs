using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Empleado
    {
        public int IdEmpleado { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellidos { get; set; }

        [Required]
        [EmailAddress]
        public string Correo { get; set; }

        [Phone]
        public string Telefono { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }

        // OPCIONAL: Nombre completo para mostrar en tablas o desplegables
        public string NombreCompleto
        {
            get { return $"{Nombre} {Apellidos}"; }
        }
    }
}
