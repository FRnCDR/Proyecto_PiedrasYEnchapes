using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Cotizacion
    {
        public int CotizacionID { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Display(Name = "Cliente")]
        public int? IdCliente { get; set; }

        [Display(Name = "Fecha de Cotización")]
        public DateTime FechaCotizacion { get; set; }

        [Display(Name = "Total")]
        public decimal Total { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; }

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; }
    }
}