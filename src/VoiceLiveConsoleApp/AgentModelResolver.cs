// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{
    /// <summary>
    ///     Reports which model actually answered a Foundry agent turn, without holding up the conversation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The session never names the model behind an agent: <c>response.done</c> reports this session's
    ///         model, and when the agent runs on a Model Router deployment even tracing only shows the
    ///         deployment name. The response object does name it, so this reads
    ///         <c>{project endpoint}/openai/v1/responses/{id}</c> and prints the <c>model</c> field — which is
    ///         what makes the router's per-turn choices visible while you talk.
    ///     </para>
    ///     <para>
    ///         Opt in with <c>--resolve-agent-model</c> (it costs an extra authenticated request per turn) and
    ///         set the project endpoint. Every lookup runs detached and swallows its failures: this is a
    ///         diagnostic, and it must never be able to disturb the session it is observing.
    ///     </para>
    /// </remarks>
    public static class AgentModelResolver
    {
        #region Static Fields and Constants

        /// <summary>The scope the Responses API is read with.</summary>
        private const string Scope = "https://ai.azure.com/.default";

        /// <summary>
        ///     Ids already looked up, so a turn reported through more than one event resolves once.
        /// </summary>
        private static readonly HashSet<string> ReportedIds = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        ///     One client for the process. Created per call previously, which churned sockets for no reason.
        /// </summary>
        private static readonly HttpClient Http = new HttpClient();

        /// <summary>
        ///     One credential for the process, so its token cache is actually reused across turns.
        ///     Managed identity is excluded because this runs on a developer machine, where probing IMDS
        ///     only adds a timeout.
        /// </summary>
        private static readonly DefaultAzureCredential Credential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions { ExcludeManagedIdentityCredential = true });

        /// <summary>Whether reporting is switched on.</summary>
        private static bool enabled;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Turns reporting on or off. Off by default.
        /// </summary>
        /// <param name="isEnabled">Whether each agent turn should be resolved.</param>
        public static void Enable(bool isEnabled)
        {
            enabled = isEnabled;
        }

        /// <summary>
        ///     Starts a detached lookup for an agent turn and returns immediately. Does nothing when reporting
        ///     is off, the id is absent, or that id was already reported.
        /// </summary>
        /// <param name="id">
        ///     A response id (<c>resp_…</c>) from the agent-as-tool path, or a conversation id (<c>conv_…</c>)
        ///     from an agent session. May be null.
        /// </param>
        public static void Report(string? id)
        {
            if (!enabled || string.IsNullOrEmpty(id))
            {
                return;
            }

            lock (ReportedIds)
            {
                if (!ReportedIds.Add(id!))
                {
                    return;
                }
            }

            string? projectEndpoint = ConsoleSettings.Get("FoundryProjectEndpoint");
            if (string.IsNullOrWhiteSpace(projectEndpoint))
            {
                Console.WriteLine("[agent] set FoundryProjectEndpoint to resolve which model answered.");
                return;
            }

            _ = Task.Run(() => ResolveAsync(id!, projectEndpoint!.TrimEnd('/')));
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Reads the response behind an agent turn and prints the model that produced it.
        /// </summary>
        /// <param name="id">The response or conversation id.</param>
        /// <param name="projectEndpoint">The project endpoint, without a trailing slash.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task ResolveAsync(string id, string projectEndpoint)
        {
            try
            {
                AccessToken token = await Credential.GetTokenAsync(new TokenRequestContext(new[] { Scope }));

                // Two shapes of id reach us. The agent-as-tool path hands over the agent's response id, so read
                // that response directly. An agent session only reports the conversation, so list the recent
                // responses and match on the conversation each one carries (the listing's ?conversation=
                // filter is not honored, hence the client-side match). The route is project-scoped; the
                // resource-level path returns 404.
                bool byConversation = id.StartsWith("conv_", StringComparison.OrdinalIgnoreCase);
                string url = byConversation
                    ? $"{projectEndpoint}/openai/v1/responses?limit=20"
                    : $"{projectEndpoint}/openai/v1/responses/{id}";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                using HttpResponseMessage response = await Http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[agent] could not read {id}: " +
                                      $"{(int)response.StatusCode} {response.ReasonPhrase}");
                    return;
                }

                using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                JsonElement result = doc.RootElement;

                if (byConversation && !TryFindByConversation(result, id, out result))
                {
                    return;
                }

                Console.WriteLine($"[agent] answered by {ReadModel(result)}{ReadReasoning(result)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[agent] model lookup failed: {ex.Message}");
            }
        }

        /// <summary>
        ///     Finds the response belonging to a conversation within a listing.
        /// </summary>
        /// <param name="listing">The listing payload.</param>
        /// <param name="conversationId">The conversation to match.</param>
        /// <param name="match">The matching response, when found.</param>
        /// <returns><see langword="true" /> when a response was found.</returns>
        private static bool TryFindByConversation(JsonElement listing, string conversationId, out JsonElement match)
        {
            match = default;

            if (!listing.TryGetProperty("data", out JsonElement data) || data.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (JsonElement candidate in data.EnumerateArray())
            {
                if (candidate.TryGetProperty("conversation", out JsonElement conversation)
                    && conversation.ValueKind == JsonValueKind.Object
                    && conversation.TryGetProperty("id", out JsonElement id)
                    && string.Equals(id.GetString(), conversationId, StringComparison.Ordinal))
                {
                    match = candidate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Reads the model name from a response.
        /// </summary>
        /// <param name="response">The response payload.</param>
        /// <returns>The model name, or a placeholder describing what was missing.</returns>
        private static string ReadModel(JsonElement response)
        {
            return response.TryGetProperty("model", out JsonElement model)
                ? model.GetString() ?? "(null)"
                : "(no model field)";
        }

        /// <summary>
        ///     Reads the reasoning token count, which tells you the router picked a reasoning model and that it
        ///     actually reasoned.
        /// </summary>
        /// <param name="response">The response payload.</param>
        /// <returns>A suffix describing the reasoning tokens, or an empty string.</returns>
        private static string ReadReasoning(JsonElement response)
        {
            // The listing nests reasoning under output_token_details; a directly-read response uses
            // output_tokens_details. Accept either.
            if (response.TryGetProperty("usage", out JsonElement usage)
                && (usage.TryGetProperty("output_token_details", out JsonElement details)
                    || usage.TryGetProperty("output_tokens_details", out details))
                && details.ValueKind == JsonValueKind.Object
                && details.TryGetProperty("reasoning_tokens", out JsonElement tokens)
                && tokens.ValueKind == JsonValueKind.Number
                && tokens.GetInt32() > 0)
            {
                return $", reasoning {tokens.GetInt32()} tokens";
            }

            return string.Empty;
        }

        #endregion
    }
}
