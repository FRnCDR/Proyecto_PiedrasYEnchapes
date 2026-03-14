using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class CotizacionLista
    {
        public int CotizacionID { get; set; }
        public int? IdCliente { get; set; }
        public string NombreCliente { get; set; }
        public DateTime FechaCotizacion { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
    }
}