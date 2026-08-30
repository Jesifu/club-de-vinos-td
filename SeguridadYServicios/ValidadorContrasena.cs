using System.Linq;

namespace SeguridadYServicios
{
    public static class ValidadorContrasena
    {
        public const int LargoMinimo = 6;

        public static string ObtenerError(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
                return "La contraseña no puede estar vacía.";

            if (contrasena.Length < LargoMinimo)
                return "La contraseña debe tener al menos " + LargoMinimo + " caracteres.";

            if (!contrasena.Any(char.IsUpper))
                return "La contraseña debe contener al menos una letra mayúscula.";

            if (!contrasena.Any(char.IsDigit))
                return "La contraseña debe contener al menos un número.";

            return null;
        }
    }
}
