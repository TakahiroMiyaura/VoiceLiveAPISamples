// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Azure.Core;
using Azure.Identity;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{
    /// <summary>
    ///     Helper class for handling authentication methods for VoiceLive API.
    /// </summary>
    public class AuthenticationHelper
    {
        #region Public Enums

        /// <summary>
        ///     Authentication methods supported by the console application.
        /// </summary>
        public enum AuthenticationMethod
        {
            /// <summary>
            ///     API key-based authentication.
            /// </summary>
            ApiKey,

            /// <summary>
            ///     Microsoft Entra ID (keyless) authentication.
            /// </summary>
            EntraId
        }

        #endregion

        private static ILogger Logger { get; } = LoggerFactoryManager.CreateLogger<AuthenticationHelper>();

        #region Public methods

        /// <summary>
        ///     Gets an access token using API key authentication.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <returns>The API key as the access token.</returns>
        public static string GetApiKeyToken(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            return apiKey;
        }

        /// <summary>
        ///     Gets an access token using Entra ID authentication.
        /// </summary>
        /// <param name="tokenRequestUrl">The token request URL scope.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>The Bearer token from Entra ID.</returns>
        public static async Task<string> GetEntraIdTokenAsync(string tokenRequestUrl,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tokenRequestUrl))
                throw new ArgumentNullException(nameof(tokenRequestUrl));

            DefaultAzureCredential credential = new();
            TokenRequestContext requestContext = new(new[] { tokenRequestUrl });

            AccessToken tokenResult = await credential.GetTokenAsync(requestContext, cancellationToken);

            Logger.LogDebug("Token acquired successfully (length: {tokenResult.Token.Length})",
                tokenResult.Token.Length);
            Logger.LogDebug("Token expires: {tokenResult.ExpiresOn}", tokenResult.ExpiresOn);

            return tokenResult.Token;
        }

        /// <summary>
        ///     Gets an access token based on the specified authentication method.
        /// </summary>
        /// <param name="authMethod">The authentication method to use.</param>
        /// <param name="apiKey">The API key (required for ApiKey method).</param>
        /// <param name="tokenRequestUrl">The token request URL scope (required for EntraId method).</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>The access token for the specified authentication method.</returns>
        public static async Task<string> GetAccessTokenAsync(AuthenticationMethod authMethod,
            string? apiKey = null, string? tokenRequestUrl = null,
            CancellationToken cancellationToken = default)
        {
            return authMethod switch
            {
                AuthenticationMethod.ApiKey => GetApiKeyToken(apiKey ??
                                                              throw new ArgumentNullException(nameof(apiKey),
                                                                  "API key is required for ApiKey authentication")),
                AuthenticationMethod.EntraId => await GetEntraIdTokenAsync(
                    tokenRequestUrl ?? throw new ArgumentNullException(nameof(tokenRequestUrl),
                        "Token request URL is required for EntraId authentication"), cancellationToken),
                _ => throw new ArgumentException($"Unsupported authentication method: {authMethod}")
            };
        }

        #endregion
    }
}