// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Threading.Tasks;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Sessions;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients
{

    /// <summary>
    ///     Azure AI Foundry VoiceInfo Live API client for AI Agent mode.
    ///     Provides connection to custom AI agents with real-time audio communication.
    /// </summary>
    public class AIAgentClient : VoiceLiveAPIClientBase
    {
        #region Public Methods

        /// <summary>
        ///     Establishes a WebSocket connection to the VoiceInfo Live API in AI Agent mode.
        /// </summary>
        /// <returns>A task representing the asynchronous connect operation.</returns>
        public override async Task ConnectAsync(ClientSessionUpdate sessionUpdated)
        {
            LogMessage($"Connecting to AI Agent - Project: {ProjectName}, Agent: {AgentId}");
            await base.ConnectAsync(sessionUpdated);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the AI Foundry project name.
        /// </summary>
        public string ProjectName { get; }

        /// <summary>
        ///     Gets the agent identifier.
        /// </summary>
        public string AgentId { get; }
        
        /// <summary>
        ///     Gets the authentication type.
        /// </summary>
        public AuthenticationType AuthType { get; private set; }

        /// <summary>
        ///     Gets the agent access token (for API key authentication).
        /// </summary>
        public string AgentAccessToken { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the AIAgentClient with explicit authentication type.
        /// </summary>
        /// <param name="endpoint">The Azure AI endpoint URL.</param>
        /// <param name="accessToken">The access token for authentication.</param>
        /// <param name="authType">The authentication type (ApiKey or BearerToken).</param>
        /// <param name="projectName">The AI Foundry project name.</param>
        /// <param name="agentId">The agent identifier.</param>
        /// <param name="agentAccessToken">The agent access token (defaults to accessToken if not provided).</param>
        /// <param name="apiVersion">The API version (default: "2025-05-01-preview").</param>
        public AIAgentClient(string endpoint, string accessToken, AuthenticationType authType, string projectName, string agentId,
            string agentAccessToken = null, string apiVersion = "2025-05-01-preview")
        {
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            AuthType = authType;
            ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
            AgentId = agentId ?? throw new ArgumentNullException(nameof(agentId));
            AgentAccessToken = agentAccessToken ?? accessToken;
            ApiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
        }

        #endregion

        #region Protected Methods

        /// <summary>
        ///     Builds the WebSocket connection URI for AI Agent mode.
        /// </summary>
        /// <returns>The connection URI string.</returns>
        protected override string BuildConnectionUri()
        {
            if (string.IsNullOrEmpty(Endpoint))
                throw new InvalidOperationException("Endpoint is required for AI Agent mode");

            var agentBaseUri = Endpoint.TrimEnd('/').Replace("https://", "wss://");


            if (string.IsNullOrEmpty(ProjectName) || string.IsNullOrEmpty(AgentId))
                throw new InvalidOperationException(
                    "ProjectName and AgentId are required when not using connection string");

            if (string.IsNullOrEmpty(AgentAccessToken))
                throw new InvalidOperationException(
                    "AgentAccessToken is required for API key authentication when not using connection string");

            // Mask token for logging (show first/last few characters only)
            var maskedToken = AgentAccessToken.Length > 10 
                ? $"{AgentAccessToken.Substring(0, 5)}...{AgentAccessToken.Substring(AgentAccessToken.Length - 5)}"
                : "***";
                
            // URL encode the agent access token to handle special characters
            var encodedToken = AgentAccessToken;
            var uri = $"{agentBaseUri}/voice-live/realtime?api-version={ApiVersion}&agent-project-name={ProjectName}&agent-id={AgentId}&agent-access-token={encodedToken}";

            // Debug logging for troubleshooting
            Console.WriteLine($"[DEBUG] Agent Access Token: {maskedToken} (length: {AgentAccessToken.Length})");
            
            return uri;
        }

        /// <summary>
        ///     Sets up authentication for AI Agent mode.
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
                    Console.WriteLine($"[DEBUG] Using API Key authentication");
                    break;
                    
                case AuthenticationType.BearerToken:
                    WebSocket.Options.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
                    Console.WriteLine($"[DEBUG] Using Bearer Token authentication");
                    break;
                    
                default:
                    throw new InvalidOperationException($"Unsupported authentication type: {AuthType}");
            }
            
            return Task.CompletedTask;
        }

        #endregion
    }
}