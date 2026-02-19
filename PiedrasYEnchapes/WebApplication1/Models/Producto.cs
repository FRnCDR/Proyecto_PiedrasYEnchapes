using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Producto
    {
        public int ProductoID { get; set; }  // Clave primaria

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }  // Nombre del producto

        [Required]
        [StringLength(255)]
        public string Descripcion { get; set; }  // Descripción del producto

        [Required]
        public int Stock { get; set; }  // Cantidad disponible en stock

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser un valor positivo.")]
        public decimal Precio { get; set; }  // Precio del producto

        public string Imagen { get; set; }  // Ruta o nombre de la imagen

        // Relación con la categoría
        public int? CategoriaID { get; set; }  // Guardar el ID de la categoría
        public virtual Categoria Categoria { get; set; }
    }
}
