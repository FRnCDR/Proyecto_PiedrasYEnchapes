using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    /// <summary>
    /// Arma el comprobante de compra en PDF.
    ///
    /// Nota: es un comprobante interno, NO una factura electrónica autorizada por
    /// el Ministerio de Hacienda (eso exige clave numérica, consecutivo autorizado,
    /// XML firmado y aprobación de Hacienda, que este sistema no emite).
    /// </summary>
    public static class ComprobantePdfService
    {
        // Datos del emisor. Se leen de Web.config cuando existen para no quemarlos aquí.
        private static string Empresa =>
            ConfigurationManager.AppSettings["EmpresaNombre"] ?? "Gustavo Fallas Piedras";

        private static string Actividad =>
            ConfigurationManager.AppSettings["EmpresaActividad"] ?? "Piedras decorativas y enchapes naturales";

        private static string Telefono =>
            ConfigurationManager.AppSettings["EmpresaTelefono"] ?? "+506 8884 3892";

        private static string Correo =>
            ConfigurationManager.AppSettings["EmpresaCorreo"] ?? "piedrasyenchapes@gmail.com";

        private static string Cedula =>
            ConfigurationManager.AppSettings["EmpresaCedula"];

        private static string Direccion =>
            ConfigurationManager.AppSettings["EmpresaDireccion"];

        // Paleta alineada con el sitio.
        private static readonly BaseColor Tinta = new BaseColor(17, 24, 39);       // #111827
        private static readonly BaseColor Suave = new BaseColor(107, 114, 128);    // #6B7280
        private static readonly BaseColor Linea = new BaseColor(229, 231, 235);    // #E5E7EB
        private static readonly BaseColor Cebra = new BaseColor(249, 250, 251);    // #F9FAFB
        private static readonly BaseColor Acento = new BaseColor(255, 193, 7);     // #FFC107

        // Colones con separador de miles "." y decimales "," (formato de Costa Rica).
        private static readonly NumberFormatInfo FormatoCRC = new NumberFormatInfo
        {
            NumberGroupSeparator = ".",
            NumberDecimalSeparator = ",",
            NumberDecimalDigits = 2
        };

        private static readonly CultureInfo CulturaFecha = new CultureInfo("es-CR");

        private static string Money(decimal valor)
        {
            return "₡ " + valor.ToString("N2", FormatoCRC);
        }

        /// <summary>
        /// Carga DejaVu Sans embebida. Las fuentes base de PDF no traen el símbolo
        /// del colón (₡), por eso se incrusta una con cobertura Unicode.
        /// </summary>
        private static BaseFont CargarBaseFont(bool negrita)
        {
            var archivo = negrita ? "DejaVuSans-Bold.ttf" : "DejaVuSans.ttf";
            var ruta = HttpContext.Current.Server.MapPath("~/StaticFiles/fonts/" + archivo);

            return BaseFont.CreateFont(ruta, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        }

        public static byte[] Generar(
            int compraId,
            DateTime fechaCompra,
            string estado,
            string clienteNombre,
            string clienteIdentificacion,
            string clienteCorreo,
            List<DetalleCompraModel> detalle)
        {
            var baseNormal = CargarBaseFont(false);
            var baseNegrita = CargarBaseFont(true);

            var fTitulo = new Font(baseNegrita, 15.5f, Font.NORMAL, Tinta);
            var fEmpresa = new Font(baseNegrita, 14, Font.NORMAL, Tinta);
            var fTexto = new Font(baseNormal, 9.5f, Font.NORMAL, Tinta);
            var fTextoSuave = new Font(baseNormal, 9.5f, Font.NORMAL, Suave);
            var fEtiqueta = new Font(baseNegrita, 8, Font.NORMAL, Suave);
            var fCabecera = new Font(baseNegrita, 9, Font.NORMAL, BaseColor.WHITE);
            var fCelda = new Font(baseNormal, 9.5f, Font.NORMAL, Tinta);
            var fTotal = new Font(baseNegrita, 14, Font.NORMAL, Tinta);
            var fPie = new Font(baseNormal, 7.5f, Font.NORMAL, Suave);

            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 42, 42, 40, 60);
                var writer = PdfWriter.GetInstance(doc, ms);
                writer.PageEvent = new PieDePagina(baseNormal);

                doc.AddTitle("Comprobante de compra " + Consecutivo(compraId));
                doc.AddCreator(Empresa);

                doc.Open();

                doc.Add(Encabezado(compraId, fechaCompra, estado, fTitulo, fEmpresa, fTexto, fTextoSuave, fEtiqueta));
                doc.Add(Separador());
                doc.Add(BloqueCliente(clienteNombre, clienteIdentificacion, clienteCorreo, fEtiqueta, fTexto));
                doc.Add(TablaDetalle(detalle, fCabecera, fCelda));
                doc.Add(BloqueTotales(detalle, fEtiqueta, fTexto, fTotal));
                doc.Add(NotaPago(fEtiqueta, fTexto, fTextoSuave));

                doc.Close();
                return ms.ToArray();
            }
        }

        // Número de comprobante legible y estable: COMP-000010
        private static string Consecutivo(int compraId)
        {
            return "COMP-" + compraId.ToString("D6");
        }

        private static IElement Encabezado(
            int compraId, DateTime fecha, string estado,
            Font fTitulo, Font fEmpresa, Font fTexto, Font fTextoSuave, Font fEtiqueta)
        {
            var tabla = new PdfPTable(2) { WidthPercentage = 100 };
            tabla.SetWidths(new float[] { 50f, 50f });

            // Emisor
            var emisor = new PdfPCell { Border = Rectangle.NO_BORDER, PaddingBottom = 6 };
            emisor.AddElement(new Paragraph(Empresa, fEmpresa));
            emisor.AddElement(new Paragraph(Actividad, fTextoSuave));

            if (!string.IsNullOrWhiteSpace(Cedula))
                emisor.AddElement(new Paragraph("Cédula jurídica: " + Cedula, fTextoSuave));

            if (!string.IsNullOrWhiteSpace(Direccion))
                emisor.AddElement(new Paragraph(Direccion, fTextoSuave));

            emisor.AddElement(new Paragraph("Tel. " + Telefono, fTextoSuave));
            emisor.AddElement(new Paragraph(Correo, fTextoSuave));
            tabla.AddCell(emisor);

            // Datos del documento
            var datos = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };

            var titulo = new Paragraph("COMPROBANTE DE COMPRA", fTitulo)
            {
                Alignment = Element.ALIGN_RIGHT,
                SpacingAfter = 8
            };
            datos.AddElement(titulo);

            datos.AddElement(ParDerecha("N.º de comprobante", Consecutivo(compraId), fEtiqueta, fTexto));
            datos.AddElement(ParDerecha("Fecha de emisión",
                fecha.ToString("dd/MM/yyyy HH:mm", CulturaFecha), fEtiqueta, fTexto));
            datos.AddElement(ParDerecha("Estado", estado ?? "Pendiente", fEtiqueta, fTexto));
            datos.AddElement(ParDerecha("Moneda", "Colón costarricense (CRC)", fEtiqueta, fTexto));

            tabla.AddCell(datos);
            return tabla;
        }

        private static Paragraph ParDerecha(string etiqueta, string valor, Font fEtiqueta, Font fValor)
        {
            var p = new Paragraph { Alignment = Element.ALIGN_RIGHT, SpacingAfter = 3 };
            p.Add(new Chunk(etiqueta.ToUpperInvariant() + "   ", fEtiqueta));
            p.Add(new Chunk(valor, fValor));
            return p;
        }

        private static IElement Separador()
        {
            var tabla = new PdfPTable(1) { WidthPercentage = 100, SpacingBefore = 10, SpacingAfter = 14 };
            var celda = new PdfPCell
            {
                Border = Rectangle.BOTTOM_BORDER,
                BorderColorBottom = Acento,
                BorderWidthBottom = 2.5f,
                FixedHeight = 2f
            };
            tabla.AddCell(celda);
            return tabla;
        }

        private static IElement BloqueCliente(string nombre, string identificacion, string correo,
            Font fEtiqueta, Font fTexto)
        {
            var tabla = new PdfPTable(3) { WidthPercentage = 100, SpacingAfter = 16 };
            tabla.SetWidths(new float[] { 40f, 30f, 30f });

            tabla.AddCell(CeldaDato("Cliente", string.IsNullOrWhiteSpace(nombre) ? "—" : nombre, fEtiqueta, fTexto));
            tabla.AddCell(CeldaDato("Identificación", string.IsNullOrWhiteSpace(identificacion) ? "—" : identificacion, fEtiqueta, fTexto));
            tabla.AddCell(CeldaDato("Correo electrónico", string.IsNullOrWhiteSpace(correo) ? "—" : correo, fEtiqueta, fTexto));

            return tabla;
        }

        private static PdfPCell CeldaDato(string etiqueta, string valor, Font fEtiqueta, Font fTexto)
        {
            var celda = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = Cebra,
                Padding = 10,
                PaddingBottom = 12
            };
            celda.AddElement(new Paragraph(etiqueta.ToUpperInvariant(), fEtiqueta) { SpacingAfter = 3 });
            celda.AddElement(new Paragraph(valor, fTexto));
            return celda;
        }

        private static IElement TablaDetalle(List<DetalleCompraModel> detalle, Font fCabecera, Font fCelda)
        {
            var tabla = new PdfPTable(5) { WidthPercentage = 100, SpacingAfter = 14 };
            tabla.SetWidths(new float[] { 7f, 47f, 13f, 16.5f, 16.5f });
            tabla.HeaderRows = 1;

            foreach (var titulo in new[] { "#", "Descripción", "Cant.", "Precio unit.", "Importe" })
            {
                var alineacion = titulo == "#" || titulo == "Descripción"
                    ? Element.ALIGN_LEFT
                    : (titulo == "Cant." ? Element.ALIGN_CENTER : Element.ALIGN_RIGHT);

                tabla.AddCell(new PdfPCell(new Phrase(titulo, fCabecera))
                {
                    BackgroundColor = Tinta,
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = alineacion,
                    Padding = 8
                });
            }

            var fila = 0;
            foreach (var item in detalle)
            {
                fila++;
                var fondo = fila % 2 == 0 ? Cebra : BaseColor.WHITE;

                tabla.AddCell(CeldaDetalle(fila.ToString(), fCelda, fondo, Element.ALIGN_LEFT));
                tabla.AddCell(CeldaDetalle(item.NombreProducto, fCelda, fondo, Element.ALIGN_LEFT));
                tabla.AddCell(CeldaDetalle(item.Cantidad.ToString(), fCelda, fondo, Element.ALIGN_CENTER));
                tabla.AddCell(CeldaDetalle(Money(item.PrecioUnitario), fCelda, fondo, Element.ALIGN_RIGHT));
                tabla.AddCell(CeldaDetalle(Money(item.Subtotal), fCelda, fondo, Element.ALIGN_RIGHT));
            }

            if (fila == 0)
            {
                tabla.AddCell(new PdfPCell(new Phrase("Esta compra no tiene productos registrados.", fCelda))
                {
                    Colspan = 5,
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColorBottom = Linea,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 12
                });
            }

            return tabla;
        }

        private static PdfPCell CeldaDetalle(string texto, Font fuente, BaseColor fondo, int alineacion)
        {
            return new PdfPCell(new Phrase(texto, fuente))
            {
                BackgroundColor = fondo,
                Border = Rectangle.BOTTOM_BORDER,
                BorderColorBottom = Linea,
                BorderWidthBottom = 0.7f,
                HorizontalAlignment = alineacion,
                Padding = 8
            };
        }

        private static IElement BloqueTotales(List<DetalleCompraModel> detalle,
            Font fEtiqueta, Font fTexto, Font fTotal)
        {
            var total = detalle.Sum(d => d.Subtotal);
            var unidades = detalle.Sum(d => d.Cantidad);

            var contenedor = new PdfPTable(2) { WidthPercentage = 100, SpacingAfter = 18 };
            contenedor.SetWidths(new float[] { 55f, 45f });

            var izquierda = new PdfPCell { Border = Rectangle.NO_BORDER };
            izquierda.AddElement(new Paragraph(
                "Líneas: " + detalle.Count + "    Unidades: " + unidades, fTexto));
            contenedor.AddCell(izquierda);

            var totales = new PdfPTable(2) { WidthPercentage = 100 };
            totales.SetWidths(new float[] { 50f, 50f });

            totales.AddCell(CeldaTotal("Subtotal", fEtiqueta, Element.ALIGN_LEFT, false));
            totales.AddCell(CeldaTotal(Money(total), fTexto, Element.ALIGN_RIGHT, false));

            totales.AddCell(CeldaTotal("TOTAL A PAGAR", fEtiqueta, Element.ALIGN_LEFT, true));
            totales.AddCell(CeldaTotal(Money(total), fTotal, Element.ALIGN_RIGHT, true));

            var derecha = new PdfPCell(totales) { Border = Rectangle.NO_BORDER };
            contenedor.AddCell(derecha);

            return contenedor;
        }

        private static PdfPCell CeldaTotal(string texto, Font fuente, int alineacion, bool destacado)
        {
            return new PdfPCell(new Phrase(texto, fuente))
            {
                Border = destacado ? Rectangle.TOP_BORDER : Rectangle.NO_BORDER,
                BorderColorTop = Tinta,
                BorderWidthTop = 1.2f,
                HorizontalAlignment = alineacion,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = destacado ? 8 : 4,
                PaddingBottom = destacado ? 4 : 4
            };
        }

        private static IElement NotaPago(Font fEtiqueta, Font fTexto, Font fTextoSuave)
        {
            var tabla = new PdfPTable(1) { WidthPercentage = 100 };

            var celda = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = Cebra,
                Padding = 12
            };

            celda.AddElement(new Paragraph("COORDINACIÓN DE PAGO Y ENTREGA", fEtiqueta) { SpacingAfter = 5 });
            celda.AddElement(new Paragraph(
                "El pago y la entrega se coordinan directamente con la empresa por WhatsApp al " +
                Telefono + " o al correo " + Correo + ", indicando el número de comprobante.",
                fTexto));

            tabla.AddCell(celda);
            return tabla;
        }

        /// <summary>
        /// Pie de página repetido: numeración y la aclaración de que el documento
        /// no sustituye una factura electrónica autorizada.
        /// </summary>
        private class PieDePagina : PdfPageEventHelper
        {
            private readonly Font _fuente;

            public PieDePagina(BaseFont baseFont)
            {
                _fuente = new Font(baseFont, 7.5f, Font.NORMAL, Suave);
            }

            public override void OnEndPage(PdfWriter writer, Document doc)
            {
                var cb = writer.DirectContent;

                // Línea superior del pie
                cb.SetColorStroke(Linea);
                cb.SetLineWidth(0.7f);
                cb.MoveTo(doc.LeftMargin, doc.BottomMargin - 14);
                cb.LineTo(doc.PageSize.Width - doc.RightMargin, doc.BottomMargin - 14);
                cb.Stroke();

                var aviso = new Phrase(
                    "Documento informativo sin validez tributaria. No sustituye una factura electrónica " +
                    "autorizada por el Ministerio de Hacienda.", _fuente);

                ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, aviso,
                    doc.LeftMargin, doc.BottomMargin - 27, 0);

                var pagina = new Phrase("Página " + writer.PageNumber, _fuente);
                ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, pagina,
                    doc.PageSize.Width - doc.RightMargin, doc.BottomMargin - 27, 0);
            }
        }
    }
}
