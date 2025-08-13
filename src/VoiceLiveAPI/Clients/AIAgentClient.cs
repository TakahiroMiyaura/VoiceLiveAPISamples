// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Azure.Core;
using Azure.Identity;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Sessions;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients;

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
    ///     Gets the agent access token (for API key authentication).
    /// </summary>
    public string AgentAccessToken { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the AIAgentClient with API key authentication using individual parameters.
    /// </summary>
    /// <param name="endpoint">The Azure AI endpoint URL.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="projectName">The AI Foundry project name.</param>
    /// <param name="agentId">The agent identifier.</param>
    /// <param name="agentAccessToken">The agent access token.</param>
    /// <param name="apiVersion">The API version (default: "2025-05-01-preview").</param>
    public AIAgentClient(string endpoint, string apiKey, string projectName, string agentId,
        string? agentAccessToken = null, string apiVersion = "2025-05-01-preview")
    {
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
        AgentId = agentId ?? throw new ArgumentNullException(nameof(agentId));
        AgentAccessToken = agentAccessToken ?? ApiKey;
        ApiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
        AuthMethod = AuthenticationMethod.ApiKey;
    }

    /// <summary>
    ///     Initializes a new instance of the AIAgentClient with Entra ID authentication using DefaultAzureCredential.
    /// </summary>
    /// <param name="endpoint">The Azure AI endpoint URL.</param>
    /// <param name="requestContext">The token request context for authentication.</param>
    /// <param name="projectName">The AI Foundry project name.</param>
    /// <param name="agentId">The agent identifier.</param>
    /// <param name="apiVersion">The API version (default: "2025-05-01-preview").</param>
    public AIAgentClient(string endpoint, TokenRequestContext requestContext, string projectName, string agentId,
        string apiVersion = "2025-05-01-preview")
    {
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        RequestContext = requestContext;
        ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
        AgentId = agentId ?? throw new ArgumentNullException(nameof(agentId));
        ApiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
        AuthMethod = AuthenticationMethod.EntraId;
        TokenCredential = new DefaultAzureCredential();
        AgentAccessToken = string.Empty;
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

        return
            $"{agentBaseUri}/voice-live/realtime?api-version={ApiVersion}&agent-project-name={ProjectName}&agent-id={AgentId}&agent-access-token={AgentAccessToken}";
    }

    /// <summary>
    ///     Sets up authentication for AI Agent mode.
    /// </summary>
    /// <returns>A task representing the asynchronous authentication setup.</returns>
    protected override async Task SetupAuthenticationAsync()
    {
        switch (AuthMethod)
        {
            case AuthenticationMethod.ApiKey:
                if (string.IsNullOrEmpty(ApiKey))
                    throw new InvalidOperationException("API key is required for API key authentication");
                WebSocket.Options.SetRequestHeader("api-key", ApiKey);
                break;

            case AuthenticationMethod.EntraId:
                TokenCredential ??= new DefaultAzureCredential();
                var tokenResult = await TokenCredential.GetTokenAsync(RequestContext, CancellationTokenSource.Token);
                AccessToken = tokenResult.Token;
                AgentAccessToken = AccessToken;
                WebSocket.Options.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
                break;

            default:
                throw new InvalidOperationException($"Unsupported authentication method: {AuthMethod}");
        }
    }

    #endregion
}