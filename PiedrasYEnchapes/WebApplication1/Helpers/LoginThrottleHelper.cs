using System;
using System.Collections.Concurrent;

namespace WebApplication1.Helpers
{
    /// <summary>
    /// Controla la cantidad de intentos fallidos de inicio de sesión para
    /// mitigar ataques de fuerza bruta. El conteo se mantiene en memoria por
    /// clave (correo + IP), por lo que no requiere cambios en la base de datos.
    /// </summary>
    public static class LoginThrottleHelper
    {
        // Máximo de intentos fallidos permitidos antes de bloquear.
        private const int MaxIntentos = 5;

        // Tiempo que dura el bloqueo una vez alcanzado el máximo de intentos.
        private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

        // Ventana en la que se acumulan los intentos fallidos. Si pasa este
        // tiempo sin fallos, el contador se reinicia solo.
        private static readonly TimeSpan VentanaIntentos = TimeSpan.FromMinutes(15);

        private class Registro
        {
            public int Intentos;
            public DateTime PrimerIntentoUtc;
            public DateTime? BloqueadoHastaUtc;
        }

        private static readonly ConcurrentDictionary<string, Registro> _registros =
            new ConcurrentDictionary<string, Registro>(StringComparer.OrdinalIgnoreCase);

        private static string Clave(string correo, string ip)
        {
            return (correo ?? "").Trim().ToLowerInvariant() + "|" + (ip ?? "");
        }

        /// <summary>
        /// Indica si la combinación correo/IP está bloqueada actualmente.
        /// Devuelve también los minutos restantes de bloqueo.
        /// </summary>
        public static bool EstaBloqueado(string correo, string ip, out int minutosRestantes)
        {
            minutosRestantes = 0;

            if (_registros.TryGetValue(Clave(correo, ip), out var reg))
            {
                if (reg.BloqueadoHastaUtc.HasValue)
                {
                    if (reg.BloqueadoHastaUtc.Value > DateTime.UtcNow)
                    {
                        var restante = reg.BloqueadoHastaUtc.Value - DateTime.UtcNow;
                        minutosRestantes = Math.Max(1, (int)Math.Ceiling(restante.TotalMinutes));
                        return true;
                    }

                    // El bloqueo expiró: se limpia el registro.
                    _registros.TryRemove(Clave(correo, ip), out _);
                }
            }

            return false;
        }

        /// <summary>
        /// Registra un intento fallido. Al llegar al máximo se activa el bloqueo.
        /// </summary>
        public static void RegistrarFallo(string correo, string ip)
        {
            var clave = Clave(correo, ip);

            _registros.AddOrUpdate(
                clave,
                _ => new Registro
                {
                    Intentos = 1,
                    PrimerIntentoUtc = DateTime.UtcNow
                },
                (_, reg) =>
                {
                    // Si la ventana venció, se reinicia el conteo.
                    if (DateTime.UtcNow - reg.PrimerIntentoUtc > VentanaIntentos)
                    {
                        reg.Intentos = 1;
                        reg.PrimerIntentoUtc = DateTime.UtcNow;
                        reg.BloqueadoHastaUtc = null;
                    }
                    else
                    {
                        reg.Intentos++;
                    }

                    if (reg.Intentos >= MaxIntentos)
                    {
                        reg.BloqueadoHastaUtc = DateTime.UtcNow.Add(DuracionBloqueo);
                    }

                    return reg;
                });
        }

        /// <summary>
        /// Limpia el registro tras un inicio de sesión exitoso.
        /// </summary>
        public static void RegistrarExito(string correo, string ip)
        {
            _registros.TryRemove(Clave(correo, ip), out _);
        }
    }
}
