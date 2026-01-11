// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Authentication
{
    /// <summary>
    ///     Represents a credential for authenticating with the VoiceLive API.
    /// </summary>
    /// <remarks>
    ///     This class provides a unified abstraction for both API key and token-based authentication,
    ///     supporting automatic token refresh for <see cref="TokenCredential" /> based authentication.
    /// </remarks>
    public sealed class VoiceLiveCredential
    {
        #region Private Fields

        private readonly AzureKeyCredential keyCredential;
        private readonly TokenCredential tokenCredential;
        private readonly string[] scopes;
        private AccessToken? cachedToken;
        private readonly object tokenLock = new object();

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a value indicating whether this credential uses API key authentication.
        /// </summary>
        public bool IsApiKeyCredential => keyCredential != null;

        /// <summary>
        ///     Gets a value indicating whether this credential uses token-based authentication.
        /// </summary>
        public bool IsTokenCredential => tokenCredential != null;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceLiveCredential" /> class with an API key.
        /// </summary>
        /// <param name="keyCredential">The Azure key credential containing the API key.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyCredential" /> is null.</exception>
        public VoiceLiveCredential(AzureKeyCredential keyCredential)
        {
            this.keyCredential = keyCredential ?? throw new ArgumentNullException(nameof(keyCredential));
            this.tokenCredential = null;
            this.scopes = null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceLiveCredential" /> class with a token credential.
        /// </summary>
        /// <param name="tokenCredential">The Azure token credential for authentication.</param>
        /// <param name="scopes">
        ///     The scopes to request when acquiring tokens.
        ///     Defaults to the Cognitive Services scope if not specified.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokenCredential" /> is null.</exception>
        public VoiceLiveCredential(TokenCredential tokenCredential, string[] scopes = null)
        {
            this.tokenCredential = tokenCredential ?? throw new ArgumentNullException(nameof(tokenCredential));
            this.keyCredential = null;
            this.scopes = scopes ?? new[] { "https://cognitiveservices.azure.com/.default" };
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Applies authentication headers to the WebSocket connection.
        /// </summary>
        /// <param name="webSocket">The WebSocket client to configure.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="webSocket" /> is null.</exception>
        public async Task ApplyAuthenticationAsync(ClientWebSocket webSocket,
            CancellationToken cancellationToken = default)
        {
            if (webSocket == null)
            {
                throw new ArgumentNullException(nameof(webSocket));
            }

            if (keyCredential != null)
            {
                webSocket.Options.SetRequestHeader("api-key", keyCredential.Key);
            }
            else if (tokenCredential != null)
            {
                var token = await GetTokenAsync(cancellationToken).ConfigureAwait(false);
                webSocket.Options.SetRequestHeader("Authorization", $"Bearer {token}");
            }
        }

        /// <summary>
        ///     Gets a valid access token, refreshing if necessary.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that returns the access token string.</returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when this credential is not a token credential.
        /// </exception>
        public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            if (tokenCredential == null)
            {
                throw new InvalidOperationException("This credential does not support token-based authentication.");
            }

            // Check if cached token is still valid (with 5 minute buffer)
            lock (tokenLock)
            {
                if (cachedToken.HasValue && cachedToken.Value.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    return cachedToken.Value.Token;
                }
            }

            // Get new token
            var tokenRequestContext = new TokenRequestContext(scopes);
            var newToken = await tokenCredential.GetTokenAsync(tokenRequestContext, cancellationToken)
                .ConfigureAwait(false);

            lock (tokenLock)
            {
                cachedToken = newToken;
            }

            return newToken.Token;
        }

        /// <summary>
        ///     Invalidates the cached token, forcing a refresh on the next request.
        /// </summary>
        public void InvalidateToken()
        {
            lock (tokenLock)
            {
                cachedToken = null;
            }
        }

        #endregion
    }
}