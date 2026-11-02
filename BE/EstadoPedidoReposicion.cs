namespace BE
{
    // Ciclo de vida del Pedido de Reposición (N03): Pendiente -> Aprobado | Rechazado (terminal);
    // Aprobado -> Recibido (terminal). Sin cancelación.
    public enum EstadoPedidoReposicion
    {
        Pendiente,
        Aprobado,
        Rechazado,
        Recibido
    }
}
