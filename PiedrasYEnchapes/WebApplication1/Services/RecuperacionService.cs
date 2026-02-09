using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web;

namespace WebApplication1.Services
{
    public static class RecuperacionService
    {
        public static string GenerarContrasenna()
        {
            int longitud = 8;
            const string caracteres = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ0123456789";
            char[] resultado = new char[longitud];

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] buffer = new byte[sizeof(uint)];

                for (int i = 0; i < longitud; i++)
                {
                    rng.GetBytes(buffer);
                    uint num = BitConverter.ToUInt32(buffer, 0);
                    resultado[i] = caracteres[(int)(num % (uint)caracteres.Length)];
                }
            }

            return new string(resultado);
        }

        public static void EnviarCorreoRecuperacion(string asunto, string contrasenna, string destinatario)
        {
            var correoSMTP = ConfigurationManager.AppSettings["CorreoSMTP"];
            var contrasennaSMTP = ConfigurationManager.AppSettings["ContrasennaSMTP"];

            if (string.IsNullOrWhiteSpace(correoSMTP))
                throw new InvalidOperationException("AppSetting 'CorreoSMTP' no está configurado o está vacío.");

            if (string.IsNullOrWhiteSpace(contrasennaSMTP))
                throw new InvalidOperationException("AppSetting 'ContrasennaSMTP' no está configurado o está vacío.");

            if (string.IsNullOrWhiteSpace(destinatario))
                throw new ArgumentException("El correo destinatario está vacío o es nulo.", nameof(destinatario));

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(correoSMTP, contrasennaSMTP)
            };


            string cuerpoHtml = GetPlantillaRecuperacion()
                                .Replace("{{PASSWORD}}", contrasenna);

            var mensaje = new MailMessage
            {
                From = new MailAddress(correoSMTP, "Piedras Decorativas"),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mensaje.To.Add(destinatario);
            smtp.Send(mensaje);
        }

        private static string GetPlantillaRecuperacion()
        {
            var ruta = HttpContext.Current.Server.MapPath("~/Templates/templateRecuperacion.html");
            return System.IO.File.ReadAllText(ruta);
        }
    }
}
