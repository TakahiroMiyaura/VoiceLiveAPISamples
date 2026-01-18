// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Clients
{
    /// <summary>
    ///     Authentication methods for VoiceLive API clients.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This enum is used with string-based credential constructors of <see cref="VoiceLiveClient" />,
    ///         primarily for environments where Azure SDK is not available (e.g., Unity).
    ///     </para>
    ///     <para>
    ///         For environments with Azure SDK support, consider using <see cref="Azure.AzureKeyCredential" /> or
    ///         <see cref="Azure.Core.TokenCredential" /> constructors instead, which provide automatic token refresh
    ///         and better integration with Azure services.
    ///     </para>
    /// </remarks>
    public enum AuthenticationType
    {
        /// <summary>
        ///     API key authentication using the "api-key" header.
        /// </summary>
        ApiKey,

        /// <summary>
        ///     Bearer token authentication using the "Authorization: Bearer" header.
        ///     Tokens can be obtained from external authentication systems (e.g., UnityOIDC).
        /// </summary>
        BearerToken
    }
}