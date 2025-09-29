// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Logs;
using Microsoft.Extensions.Logging;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Clients
{
    /// <summary>
    ///     Authentication methods for VoiceLive API clients.
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        ///     API key authentication.
        /// </summary>
        ApiKey,

        /// <summary>
        ///     Bearer token authentication.
        /// </summary>
        BearerToken
    }

    /// <summary>
    ///     Azure AI Foundry VoiceInfo Live API client for AI Model mode.
    ///     Provides direct connection to AI models (e.g., GPT-4o) with real-time audio communication.
    /// </summary>
    public class AIModelClient : VoiceLiveAPIClientBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the AIModelClient with explicit authentication type.
        /// </summary>
        /// <param name="endpoint">The Azure AI endpoint URL.</param>
        /// <param name="accessToken">The access token for authentication.</param>
        /// <param name="authType">The authentication type (ApiKey or BearerToken).</param>
        /// <param name="model">The AI model name (default: "gpt-4o").</param>
        /// <param name="apiVersion">The API version (default: "2025-05-01-preview").</param>
        public AIModelClient(string endpoint, string accessToken, AuthenticationType authType, string model = "gpt-4o",
            string apiVersion = "2025-05-01-preview")
        {
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            AuthType = authType;
            Model = model ?? throw new ArgumentNullException(nameof(model));
            ApiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets or sets the logger instance for this client.
        /// </summary>
        public override ILogger Logger { set; get; } = LoggerFactoryManager.CreateLogger<AIModelClient>();

        /// <summary>
        ///     Establishes a WebSocket connection to the VoiceInfo Live API in AI Model mode.
        /// </summary>
        /// <returns>A task representing the asynchronous connect operation.</returns>
        public override async Task ConnectAsync(IClientSessionUpdate sessionUpdated)
        {
            Log(LogLevel.Information, $"Connecting to AI Model: {Model}");
            await base.ConnectAsync(sessionUpdated);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the AI model name.
        /// </summary>
        public string Model { get; }

        /// <summary>
        ///     Gets the authentication type.
        /// </summary>
        public AuthenticationType AuthType { get; }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Builds the WebSocket connection URI for AI Model mode.
        /// </summary>
        /// <returns>The connection URI string.</returns>
        protected override string BuildConnectionUri()
        {
            if (string.IsNullOrEmpty(Endpoint))
                throw new InvalidOperationException("Endpoint is required for AI Model mode");

            var baseUri = Endpoint.TrimEnd('/').Replace("https://", "wss://");
            return $"{baseUri}/voice-live/realtime?api-version={ApiVersion}&model={Model}";
        }

        /// <summary>
        ///     Sets up authentication for AI Model mode.
        /// </summary>
        /// <returns>A task representing the asynchronous authentication setup.</returns>
        protected override Task SetupAuthenticationAsync()
        {
            if (string.IsNullOrEmpty(AccessToken))
                throw new InvalidOperationException("Access token is required for authentication");

            switch (AuthType)
            {
                case AuthenticationType.ApiKey:
                    WebSocket.Options.SetRequestHeader("api-key", AccessToken);
                    Logger.LogDebug("Using API Key authentication");
                    break;

                case AuthenticationType.BearerToken:
                    WebSocket.Options.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
                    Logger.LogDebug("Using Bearer Token authentication");
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported authentication type: {AuthType}");
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}