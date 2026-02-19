using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Categoria
    {
        public int CategoriaID { get; set; }  // ID de la categoría
        public string Nombre { get; set; }    // Nombre de la categoría
        public string Descripcion { get; set; } // Descripción de la categoría
    }
}
