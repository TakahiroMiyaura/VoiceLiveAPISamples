// Copyright (c) 2025 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using Azure.Core;
using Azure.Identity;
using Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients.Messages.Sessions;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI.Clients;

/// <summary>
///     Azure AI Foundry VoiceInfo Live API client for AI Model mode.
///     Provides direct connection to AI models (e.g., GPT-4o) with real-time audio communication.
/// </summary>
public class AIModelClient : VoiceLiveAPIClientBase
{
    #region Public Properties

    /// <summary>
    ///     Gets the AI model name.
    /// </summary>
    public string Model { get; }

    #endregion

    #region Public Methods

    /// <summary>
    ///     Establishes a WebSocket connection to the VoiceInfo Live API in AI Model mode.
    /// </summary>
    /// <returns>A task representing the asynchronous connect operation.</returns>
    public override async Task ConnectAsync(ClientSessionUpdate sessionUpdated)
    {
        LogMessage($"Connecting to AI Model: {Model}");
        await base.ConnectAsync(sessionUpdated);
    }

    #endregion

    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the AIModelClient with API key authentication.
    /// </summary>
    /// <param name="endpoint">The Azure AI endpoint URL.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="model">The AI model name (default: "gpt-4o").</param>
    /// <param name="apiVersion">The API version (default: "2025-05-01-preview").</param>
    public AIModelClient(string endpoint, string apiKey, string model = "gpt-4o",
        string apiVersion = "2025-05-01-preview")
    {
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        ApiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
        AuthMethod = AuthenticationMethod.ApiKey;
    }

    /// <summary>
    ///     Initializes a new instance of the AIModelClient with Entra ID authentication using DefaultAzureCredential.
    /// </summary>
    /// <param name="endpoint">The Azure AI endpoint URL.</param>
    /// <param name="requestContext">The token request context for authentication.</param>
    /// <param name="model">The AI model name (default: "gpt-4o").</param>
    /// <param name="apiVersion">The API version (default: "2025-05-01-preview").</param>
    public AIModelClient(string endpoint, TokenRequestContext requestContext, string model = "gpt-4o",
        string apiVersion = "2025-05-01-preview")
    {
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        RequestContext = requestContext;
        Model = model ?? throw new ArgumentNullException(nameof(model));
        ApiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
        AuthMethod = AuthenticationMethod.EntraId;
        TokenCredential = new DefaultAzureCredential();
    }

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
                WebSocket.Options.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
                break;

            default:
                throw new InvalidOperationException($"Unsupported authentication method: {AuthMethod}");
        }
    }

    #endregion
}