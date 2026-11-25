namespace G1.Identity.Mock.Middleware
{
    /// <summary>
    /// Propaga el identificador de correlación de la petición a la respuesta y al contexto.
    /// </summary>
    internal sealed class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Inicializa el middleware con el siguiente delegado del pipeline.
        /// </summary>
        /// <param name="next">Siguiente delegado del pipeline.</param>
        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Lee o genera el identificador de correlación y lo devuelve en la respuesta.
        /// </summary>
        /// <param name="context">Contexto HTTP de la petición.</param>
        /// <returns>Una tarea que finaliza cuando la petición ha sido procesada.</returns>
        public Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers[Constants.Headers.CorrelationId].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString(@"N");
            }

            context.Items[Constants.HttpItems.CorrelationId] = correlationId;
            context.Response.Headers[Constants.Headers.CorrelationId] = correlationId;

            return _next(context);
        }
    }
}
