using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;

namespace G1.Identity.Mock.Endpoints
{
    /// <summary>
    /// Vincula el contexto de autorización opaco con el navegador que inició el flujo.
    /// </summary>
    internal interface IAuthorizationContextBinder
    {
        /// <summary>
        /// Registra el contexto actual y entrega al navegador un nonce aleatorio en una cookie HttpOnly.
        /// </summary>
        /// <param name="context">Contexto HTTP que inicia el flujo.</param>
        /// <param name="request">Solicitud OpenIddict ya validada por el servidor.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns><see langword="true"/> si el contexto se ha vinculado; en caso contrario, <see langword="false"/>.</returns>
        /// <remarks>
        /// El nombre evita colisionar con el <c>BindAsync</c> estático que Minimal API busca en los parámetros.
        /// </remarks>
        Task<bool> BindContextAsync(HttpContext context, OpenIddictRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// Comprueba que el contexto presentado pertenece al navegador actual, sin consumirlo.
        /// </summary>
        /// <param name="context">Contexto HTTP de la petición.</param>
        /// <param name="requestUri">Referencia opaca recibida.</param>
        /// <param name="clientId">Identificador de cliente recibido.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns><see langword="true"/> cuando la vinculación coincide y no ha caducado.</returns>
        Task<bool> MatchesAsync(
            HttpContext context,
            string requestUri,
            string clientId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Consume la vinculación una sola vez, de forma atómica frente a peticiones concurrentes.
        /// </summary>
        /// <param name="context">Contexto HTTP de la petición.</param>
        /// <param name="requestUri">Referencia opaca recibida.</param>
        /// <param name="clientId">Identificador de cliente recibido.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns><see langword="true"/> solo para el primero consumidor.</returns>
        Task<bool> TryConsumeAsync(
            HttpContext context,
            string requestUri,
            string clientId,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Enlace en memoria entre un request token de OpenIddict y la cookie del navegador que lo inició.
    /// </summary>
    internal sealed class AuthorizationContextBinder : IAuthorizationContextBinder
    {
        private readonly ConcurrentDictionary<string, FlowBinding> _bindings = new(StringComparer.Ordinal);
        private readonly TimeProvider _time;

        /// <summary>
        /// Inicializa una nueva instancia del enlazador.
        /// </summary>
        /// <param name="time">Proveedor de hora utilizado para la expiración.</param>
        public AuthorizationContextBinder(TimeProvider time)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
        }

        /// <inheritdoc />
        public Task<bool> BindContextAsync(
            HttpContext context,
            OpenIddictRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(request);

            if (!TryGetIdentifier(request.RequestUri, out var identifier)
                || string.IsNullOrWhiteSpace(request.ClientId)
                || request.ClientId.Length > Constants.Selector.MaximumIdentifierLength)
            {
                return Task.FromResult(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            RemoveExpired();

            var nonce = RandomNumberGenerator.GetBytes(32);
            var binding = new FlowBinding(
                SHA256.HashData(nonce),
                request.ClientId,
                _time.GetUtcNow().Add(Constants.Selector.BindingLifetime));

            if (!_bindings.TryAdd(identifier, binding))
            {
                return Task.FromResult(false);
            }

            // Una cookie por solicitud evita que un segundo flujo del mismo navegador
            // sobrescriba el nonce del primero.
            context.Response.Cookies.Append(
                Constants.Selector.BuildFlowCookieName(identifier),
                Convert.ToBase64String(nonce),
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    MaxAge = Constants.Selector.BindingLifetime,
                    IsEssential = true,
                });

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> MatchesAsync(
            HttpContext context,
            string requestUri,
            string clientId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context);
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(TryGetBinding(context, requestUri, clientId, out _));
        }

        /// <inheritdoc />
        public Task<bool> TryConsumeAsync(
            HttpContext context,
            string requestUri,
            string clientId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context);
            cancellationToken.ThrowIfCancellationRequested();

            if (!TryGetBinding(context, requestUri, clientId, out var identifier))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(_bindings.TryRemove(identifier, out _));
        }

        /// <summary>
        /// Extrae el identificador opaco de una referencia de solicitud de OpenIddict.
        /// </summary>
        /// <param name="requestUri">Referencia recibida.</param>
        /// <param name="identifier">Identificador extraído.</param>
        /// <returns><see langword="true"/> si la referencia tiene el prefijo y el formato esperados.</returns>
        internal static bool TryGetIdentifier(string? requestUri, out string identifier)
        {
            identifier = string.Empty;

            if (string.IsNullOrEmpty(requestUri)
                || !requestUri.StartsWith(Constants.Selector.RequestUriPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            var value = requestUri[Constants.Selector.RequestUriPrefix.Length..];
            if (value.Length is 0 or > Constants.Selector.MaximumIdentifierLength
                || !value.All(static character =>
                    char.IsAsciiLetterOrDigit(character) || character is '-' or '_'))
            {
                return false;
            }

            identifier = value;
            return true;
        }

        private bool TryGetBinding(
            HttpContext context,
            string requestUri,
            string clientId,
            out string identifier)
        {
            identifier = string.Empty;

            if (!TryGetIdentifier(requestUri, out identifier)
                || string.IsNullOrWhiteSpace(clientId)
                || !_bindings.TryGetValue(identifier, out var binding)
                || binding.ExpiresAt <= _time.GetUtcNow()
                || !string.Equals(binding.ClientId, clientId, StringComparison.Ordinal))
            {
                return false;
            }

            var cookie = context.Request.Cookies[Constants.Selector.BuildFlowCookieName(identifier)];
            if (string.IsNullOrEmpty(cookie))
            {
                return false;
            }

            byte[] supplied;
            try
            {
                supplied = Convert.FromBase64String(cookie);
            }
            catch (FormatException)
            {
                return false;
            }

            // La cookie contiene el nonce en claro; el almacén conserva únicamente su hash.
            return CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(supplied),
                binding.NonceHash);
        }

        private void RemoveExpired()
        {
            var now = _time.GetUtcNow();
            foreach (var pair in _bindings)
            {
                if (pair.Value.ExpiresAt <= now)
                {
                    _bindings.TryRemove(pair.Key, out _);
                }
            }
        }

        private sealed record FlowBinding(byte[] NonceHash, string ClientId, DateTimeOffset ExpiresAt);
    }

    /// <summary>
    /// Valida un contexto opaco contra el almacén de tokens de OpenIddict antes de crear una sesión.
    /// </summary>
    internal interface IAuthorizationContextValidator
    {
        /// <summary>
        /// Comprueba que la referencia corresponda a una solicitud de autorización válida del cliente.
        /// </summary>
        /// <param name="requestUri">Referencia opaca.</param>
        /// <param name="clientId">Identificador de cliente declarado.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns><see langword="true"/> solo si OpenIddict acepta el contexto.</returns>
        Task<bool> IsValidAsync(
            string requestUri,
            string clientId,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Valida request tokens mediante los administradores nativos de OpenIddict.
    /// </summary>
    /// <remarks>
    /// Los administradores se resuelven en el ámbito de la petición y de forma opcional: mientras
    /// DEV2/DEV3 no publiquen el registro de OpenIddict, el selector falla cerrado sin impedir
    /// que el host arranque ni que el resto de endpoints funcionen.
    /// </remarks>
    internal sealed class AuthorizationContextValidator : IAuthorizationContextValidator
    {
        private readonly IServiceProvider _services;
        private readonly TimeProvider _time;

        /// <summary>
        /// Inicializa una nueva instancia del validador.
        /// </summary>
        /// <param name="services">Proveedor de servicios del ámbito actual.</param>
        /// <param name="time">Proveedor de hora utilizado para comprobar la caducidad.</param>
        public AuthorizationContextValidator(IServiceProvider services, TimeProvider time)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _time = time ?? throw new ArgumentNullException(nameof(time));
        }

        /// <inheritdoc />
        public async Task<bool> IsValidAsync(
            string requestUri,
            string clientId,
            CancellationToken cancellationToken)
        {
            if (!AuthorizationContextBinder.TryGetIdentifier(requestUri, out var identifier)
                || string.IsNullOrWhiteSpace(clientId)
                || clientId.Length > Constants.Selector.MaximumIdentifierLength)
            {
                return false;
            }

            var tokens = _services.GetService<IOpenIddictTokenManager>();
            var applications = _services.GetService<IOpenIddictApplicationManager>();
            if (tokens is null || applications is null)
            {
                return false;
            }

            var token = await tokens.FindByIdAsync(identifier, cancellationToken).ConfigureAwait(false);
            if (token is null
                || !await tokens.HasTypeAsync(
                        token,
                        OpenIddictConstants.TokenTypeIdentifiers.Private.RequestToken,
                        cancellationToken).ConfigureAwait(false)
                || !await tokens.HasStatusAsync(
                        token,
                        OpenIddictConstants.Statuses.Valid,
                        cancellationToken).ConfigureAwait(false))
            {
                return false;
            }

            var expiration = await tokens.GetExpirationDateAsync(token, cancellationToken).ConfigureAwait(false);
            if (expiration is null || expiration <= _time.GetUtcNow())
            {
                return false;
            }

            var applicationId = await tokens.GetApplicationIdAsync(token, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(applicationId))
            {
                return false;
            }

            var application = await applications.FindByIdAsync(applicationId, cancellationToken).ConfigureAwait(false);
            if (application is null)
            {
                return false;
            }

            var boundClientId = await applications.GetClientIdAsync(application, cancellationToken).ConfigureAwait(false);
            return string.Equals(boundClientId, clientId, StringComparison.Ordinal);
        }
    }
}
