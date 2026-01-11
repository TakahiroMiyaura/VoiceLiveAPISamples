// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Authentication;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Clients;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core
{
    /// <summary>
    ///     Represents a client for connecting to the Azure AI VoiceLive service.
    /// </summary>
    /// <remarks>
    ///     This class serves as the entry point for creating VoiceLive sessions.
    ///     It handles authentication and connection setup, delegating session management
    ///     to <see cref="VoiceLiveSession" />.
    /// </remarks>
    public class VoiceLiveClient : ILogOutputClass
    {
        #region Private Fields

        private readonly string endpoint;
        private readonly VoiceLiveCredential voiceLiveCredential;
        private readonly VoiceLiveClientOptions options;

        // String-based authentication fields (for environments without Azure SDK, e.g., Unity)
        private readonly string stringCredential;
        private readonly AuthenticationType stringAuthType;
        private readonly bool useStringAuth;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the logger instance.
        /// </summary>
        public ILogger Logger { get; set; } = LoggerFactoryManager.CreateLogger<VoiceLiveClient>();

        /// <summary>
        ///     Gets the endpoint URI.
        /// </summary>
        public string Endpoint => endpoint;

        /// <summary>
        ///     Gets the client options.
        /// </summary>
        public VoiceLiveClientOptions Options => options;

        /// <summary>
        ///     Gets or sets the AI Agent project name for agent mode.
        /// </summary>
        public string AgentProjectName { get; set; }

        /// <summary>
        ///     Gets or sets the AI Agent ID for agent mode.
        /// </summary>
        public string AgentId { get; set; }

        /// <summary>
        ///     Gets or sets the AI Agent access token for agent mode.
        /// </summary>
        public string AgentAccessToken { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceLiveClient" /> class with an API key credential.
        /// </summary>
        /// <param name="endpoint">The Azure AI endpoint URL.</param>
        /// <param name="credential">The Azure key credential containing the API key.</param>
        /// <param name="options">Optional client options.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="endpoint" /> or <paramref name="credential" /> is null.
        /// </exception>
        /// <example>
        ///     <code>
        /// var client = new VoiceLiveClient(
        ///     "https://your-resource.cognitiveservices.azure.com",
        ///     new AzureKeyCredential("your-api-key"));
        /// </code>
        /// </example>
        public VoiceLiveClient(string endpoint, AzureKeyCredential credential, VoiceLiveClientOptions options = null)
        {
            this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            if (credential == null) throw new ArgumentNullException(nameof(credential));

            voiceLiveCredential = new VoiceLiveCredential(credential);
            this.options = options ?? new VoiceLiveClientOptions();
            useStringAuth = false;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceLiveClient" /> class with a token credential.
        /// </summary>
        /// <param name="endpoint">The Azure AI endpoint URL.</param>
        /// <param name="credential">The Azure token credential for authentication (e.g., DefaultAzureCredential).</param>
        /// <param name="options">Optional client options.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="endpoint" /> or <paramref name="credential" /> is null.
        /// </exception>
        /// <remarks>
        ///     This constructor supports automatic token refresh. The token will be automatically
        ///     refreshed when it expires or is about to expire.
        /// </remarks>
        /// <example>
        ///     <code>
        /// // Using DefaultAzureCredential
        /// var client = new VoiceLiveClient(
        ///     "https://your-resource.cognitiveservices.azure.com",
        ///     new DefaultAzureCredential());
        ///
        /// // Using ManagedIdentityCredential
        /// var client = new VoiceLiveClient(
        ///     "https://your-resource.cognitiveservices.azure.com",
        ///     new ManagedIdentityCredential());
        /// </code>
        /// </example>
        public VoiceLiveClient(string endpoint, TokenCredential credential, VoiceLiveClientOptions options = null)
        {
            this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            if (credential == null) throw new ArgumentNullException(nameof(credential));

            voiceLiveCredential = new VoiceLiveCredential(credential);
            this.options = options ?? new VoiceLiveClientOptions();
            useStringAuth = false;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceLiveClient" /> class with a token credential
        ///     and custom scopes.
        /// </summary>
        /// <param name="endpoint">The Azure AI endpoint URL.</param>
        /// <param name="credential">The Azure token credential for authentication.</param>
        /// <param name="scopes">The custom scopes to request when acquiring tokens.</param>
        /// <param name="options">Optional client options.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="endpoint" />, <paramref name="credential" />, or <paramref name="scopes" /> is null.
        /// </exception>
        public VoiceLiveClient(string endpoint, TokenCredential credential, string[] scopes,
            VoiceLiveClientOptions options = null)
        {
            this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            if (credential == null) throw new ArgumentNullException(nameof(credential));
            if (scopes == null) throw new ArgumentNullException(nameof(scopes));

            voiceLiveCredential = new VoiceLiveCredential(credential, scopes);
            this.options = options ?? new VoiceLiveClientOptions();
            useStringAuth = false;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceLiveClient" /> class with API key authentication.
        /// </summary>
        /// <param name="endpoint">The Azure AI endpoint URL.</param>
        /// <param name="apiKey">The API key for authentication.</param>
        /// <param name="options">Optional client options.</param>
        /// <remarks>
        ///     This constructor is provided for convenience when Azure SDK is not available.
        ///     For environments with Azure SDK support, consider using the
        ///     <see cref="VoiceLiveClient(string, AzureKeyCredential, VoiceLiveClientOptions)" /> constructor.
        /// </remarks>
        /// <example>
        ///     <code>
        /// var client = new VoiceLiveClient(
        ///     "https://your-resource.cognitiveservices.azure.com",
        ///     "your-api-key");
        /// </code>
        /// </example>
        public VoiceLiveClient(string endpoint, string apiKey, VoiceLiveClientOptions options = null)
            : this(endpoint, apiKey, AuthenticationType.ApiKey, options)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceLiveClient" /> class with specified authentication type.
        /// </summary>
        /// <param name="endpoint">The Azure AI endpoint URL.</param>
        /// <param name="credential">The credential (API key or Bearer token).</param>
        /// <param name="authType">The authentication type.</param>
        /// <param name="options">Optional client options.</param>
        /// <remarks>
        ///     <para>
        ///         This constructor is designed for environments where Azure SDK is not available,
        ///         such as Unity or other platforms that don't support Azure.Core.
        ///     </para>
        ///     <para>
        ///         For Bearer token authentication, the token can be obtained from external authentication
        ///         systems (e.g., UnityOIDC, custom OAuth implementations) and passed directly as a string.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// // API Key authentication
        /// var client = new VoiceLiveClient(
        ///     "https://your-resource.cognitiveservices.azure.com",
        ///     "your-api-key",
        ///     AuthenticationType.ApiKey);
        /// 
        /// // Bearer token authentication (e.g., from UnityOIDC)
        /// var client = new VoiceLiveClient(
        ///     "https://your-resource.cognitiveservices.azure.com",
        ///     bearerToken,
        ///     AuthenticationType.BearerToken);
        /// </code>
        /// </example>
        public VoiceLiveClient(string endpoint, string credential, AuthenticationType authType,
            VoiceLiveClientOptions options = null)
        {
            this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            stringCredential = credential ?? throw new ArgumentNullException(nameof(credential));
            stringAuthType = authType;
            this.options = options ?? new VoiceLiveClientOptions();
            useStringAuth = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Starts a new VoiceLive session with the specified model.
        /// </summary>
        /// <param name="model">The AI model to use (e.g., "gpt-4o").</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that returns the created <see cref="VoiceLiveSession" />.</returns>
        public async Task<VoiceLiveSession> StartSessionAsync(string model,
            CancellationToken cancellationToken = default)
        {
            var options = VoiceLiveSessionOptions.CreateDefault();
            options.Model = model;
            return await StartSessionAsync(options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Starts a new VoiceLive session with the specified options.
        /// </summary>
        /// <param name="sessionOptions">The session configuration options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that returns the created <see cref="VoiceLiveSession" />.</returns>
        public async Task<VoiceLiveSession> StartSessionAsync(VoiceLiveSessionOptions sessionOptions,
            CancellationToken cancellationToken = default)
        {
            if (sessionOptions == null)
            {
                throw new ArgumentNullException(nameof(sessionOptions));
            }

            var uri = BuildConnectionUri(sessionOptions.Model);
            var session = new VoiceLiveSession(uri, sessionOptions);

            Logger?.LogInformation("Starting session with model: {model}", sessionOptions.Model);

            await session.ConnectAsync(SetupAuthenticationAsync, cancellationToken).ConfigureAwait(false);

            return session;
        }

        /// <summary>
        ///     Starts a new VoiceLive session for AI Agent mode.
        /// </summary>
        /// <param name="projectName">The AI Agent project name.</param>
        /// <param name="agentId">The AI Agent ID.</param>
        /// <param name="sessionOptions">Optional session configuration options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that returns the created <see cref="VoiceLiveSession" />.</returns>
        public async Task<VoiceLiveSession> StartAgentSessionAsync(string projectName, string agentId,
            VoiceLiveSessionOptions sessionOptions = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                throw new ArgumentNullException(nameof(projectName));
            }

            if (string.IsNullOrEmpty(agentId))
            {
                throw new ArgumentNullException(nameof(agentId));
            }

            AgentProjectName = projectName;
            AgentId = agentId;

            var options = sessionOptions ?? VoiceLiveSessionOptions.CreateDefault();
            var uri = BuildAgentConnectionUri();
            var session = new VoiceLiveSession(uri, options);

            Logger?.LogInformation("Starting agent session - Project: {project}, Agent: {agent}",
                projectName, agentId);

            await session.ConnectAsync(SetupAuthenticationAsync, cancellationToken).ConfigureAwait(false);

            return session;
        }

        #endregion

        #region Private Methods

        private Uri BuildConnectionUri(string model)
        {
            var baseUri = endpoint.TrimEnd('/').Replace("https://", "wss://").Replace("http://", "ws://");
            var uri = $"{baseUri}/voice-live/realtime?api-version={options.ApiVersion}&model={model}";
            return new Uri(uri);
        }

        private Uri BuildAgentConnectionUri()
        {
            var baseUri = endpoint.TrimEnd('/').Replace("https://", "wss://").Replace("http://", "ws://");
            var uri =
                $"{baseUri}/voice-live/realtime?api-version={options.ApiVersion}&agent-project-name={AgentProjectName}&agent-id={AgentId}";
            return new Uri(uri);
        }

        private async Task SetupAuthenticationAsync(ClientWebSocket webSocket)
        {
            if (webSocket == null)
            {
                throw new ArgumentNullException(nameof(webSocket));
            }

            if (useStringAuth)
            {
                // String-based authentication path (for environments without Azure SDK, e.g., Unity)
                switch (stringAuthType)
                {
                    case AuthenticationType.ApiKey:
                        webSocket.Options.SetRequestHeader("api-key", stringCredential);
                        Logger?.LogDebug("Using API Key authentication (string-based)");
                        break;

                    case AuthenticationType.BearerToken:
                        webSocket.Options.SetRequestHeader("Authorization", $"Bearer {stringCredential}");
                        Logger?.LogDebug("Using Bearer Token authentication (string-based)");
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported authentication type: {stringAuthType}");
                }
            }
            else if (voiceLiveCredential != null)
            {
                // New authentication path with automatic token refresh
                await voiceLiveCredential.ApplyAuthenticationAsync(webSocket).ConfigureAwait(false);

                if (voiceLiveCredential.IsApiKeyCredential)
                {
                    Logger?.LogDebug("Using AzureKeyCredential authentication");
                }
                else if (voiceLiveCredential.IsTokenCredential)
                {
                    Logger?.LogDebug("Using TokenCredential authentication with automatic refresh");
                }
            }

            // Set agent access token if available
            if (!string.IsNullOrEmpty(AgentAccessToken))
            {
                webSocket.Options.SetRequestHeader("x-agent-access-token", AgentAccessToken);
            }
        }

        #endregion
    }
}