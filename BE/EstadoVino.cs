namespace BE
{
    public enum EstadoVino
    {
        Activo,
        Descontinuado,
        // Ciclo de vida con solicitud de autorización: se agregan al final para no alterar los valores existentes.
        Pendiente,
        Rechazado
    }
}
