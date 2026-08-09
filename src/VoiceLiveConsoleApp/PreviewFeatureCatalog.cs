// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core.Commons.Messages.Parts;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{
    /// <summary>
    ///     How a preview feature is exercised: as settings on a model session, or as a transport that starts a
    ///     session of its own.
    /// </summary>
    public enum PreviewSessionKind
    {
        /// <summary>Session settings applied to a model session (the common case).</summary>
        ModelSession,

        /// <summary>A WebRTC voice call over <c>/voice-live/realtime/calls</c> instead of the WebSocket audio path.</summary>
        WebRtcVoice,

        /// <summary>An avatar session whose video arrives as <c>response.video.delta</c> over the WebSocket.</summary>
        AvatarWebSocketVideo,

        /// <summary>
        ///     An avatar session driven by a photo avatar (<c>type=photo-avatar</c>, base model <c>vasa-1</c>)
        ///     instead of the pre-rendered video avatar. Media still arrives over WebRTC.
        /// </summary>
        PhotoAvatar
    }

    /// <summary>
    ///     A single, self-contained preview feature that the console can exercise one at a time. Everything the
    ///     console needs to present, scope, configure, and explain a feature lives on this object, so adding a
    ///     new preview feature is a matter of adding one entry to
    ///     <see cref="PreviewFeatureCatalog.All" /> — no scattered <c>switch</c> statements to update.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <b>How to add a new feature check:</b> add a new <see cref="PreviewFeatureCheck" /> to
    ///         <see cref="PreviewFeatureCatalog.All" /> with (1) a stable <see cref="Id" />, (2) a
    ///         <see cref="Title" /> for the menu, (3) the <see cref="MinApiVersion" /> the feature first
    ///         appears in, (4) an <see cref="Apply" /> that mutates the session options, and (5)
    ///         <see cref="HintLines" /> that tell the user how to try it. Set the behavior flags only when the
    ///         feature needs the console to change its input handling.
    ///     </para>
    /// </remarks>
    public sealed class PreviewFeatureCheck
    {
        #region Properties

        /// <summary>Gets the stable identifier (used for logging and equality).</summary>
        public string Id { get; }

        /// <summary>Gets the one-line menu description.</summary>
        public string Title { get; }

        /// <summary>Gets the minimum API version at which this feature becomes available.</summary>
        public string MinApiVersion { get; }

        /// <summary>Gets the lines printed after connect to explain how to exercise the feature.</summary>
        public IReadOnlyList<string> HintLines { get; }

        /// <summary>Gets the action that applies this feature's configuration to the session options.</summary>
        public Action<VoiceLiveSessionOptions> Apply { get; }

        /// <summary>
        ///     Gets a value indicating whether the microphone must stay on (the client does not auto-stop on
        ///     the VAD's <c>speech_stopped</c>) so a server-side end-of-turn model can decide across pauses.
        /// </summary>
        public bool KeepMicOpen { get; }

        /// <summary>
        ///     Gets a value indicating whether the 'X' key should stream text via
        ///     <c>input_text.delta</c>/<c>.done</c> instead of a single <c>conversation.item.create</c>.
        /// </summary>
        public bool UseStreamingTextInput { get; }

        /// <summary>
        ///     Gets a value indicating whether a second sample tool (<c>get_time</c>) should be added so the
        ///     model can issue two tool calls in one turn.
        /// </summary>
        public bool IncludeParallelToolSample { get; }

        /// <summary>
        ///     Gets a value indicating whether the assistant should speak a proactive greeting once the session
        ///     is ready (before the user says anything).
        /// </summary>
        public bool SendProactiveGreeting { get; }

        /// <summary>
        ///     Gets a value indicating whether the microphone capture must be sent as interleaved stereo PCM16
        ///     (mic on channel 0, speaker-playback echo reference on channel 1) for the client-side echo
        ///     cancellation reference feature.
        /// </summary>
        public bool UseStereoEcReference { get; }

        /// <summary>
        ///     Gets the preview feature flags to append to the realtime WebSocket URL (via
        ///     <c>VoiceLiveClientOptions.Features</c>), e.g. <c>client_ec_reference:true</c>. Empty when the
        ///     feature needs no URL flag.
        /// </summary>
        public string[] WireFeatures { get; }

        /// <summary>
        ///     Gets the kind of session this feature runs in. Most preview features are session settings
        ///     applied to a model session; a few are transports of their own and start a different session.
        /// </summary>
        public PreviewSessionKind SessionKind { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="PreviewFeatureCheck" /> class.
        /// </summary>
        /// <param name="id">The stable identifier.</param>
        /// <param name="title">The one-line menu description.</param>
        /// <param name="minApiVersion">The minimum API version the feature appears in.</param>
        /// <param name="apply">The action that applies the feature's session configuration.</param>
        /// <param name="hintLines">The lines explaining how to exercise the feature.</param>
        /// <param name="keepMicOpen">Whether the mic must stay on for this feature.</param>
        /// <param name="useStreamingTextInput">Whether the 'X' key should stream text input.</param>
        /// <param name="includeParallelToolSample">Whether to add the second sample tool.</param>
        /// <param name="sendProactiveGreeting">Whether the assistant greets first on session ready.</param>
        /// <param name="useStereoEcReference">Whether mic capture must be sent as interleaved stereo (EC reference).</param>
        /// <param name="wireFeatures">Preview feature flags to append to the WebSocket URL.</param>
        /// <param name="sessionKind">The kind of session this feature runs in.</param>
        public PreviewFeatureCheck(string id, string title, string minApiVersion,
            Action<VoiceLiveSessionOptions> apply, string[] hintLines, bool keepMicOpen = false,
            bool useStreamingTextInput = false, bool includeParallelToolSample = false,
            bool sendProactiveGreeting = false, bool useStereoEcReference = false,
            string[]? wireFeatures = null,
            PreviewSessionKind sessionKind = PreviewSessionKind.ModelSession)
        {
            SessionKind = sessionKind;
            Id = id;
            Title = title;
            MinApiVersion = minApiVersion;
            Apply = apply ?? (_ => { });
            HintLines = hintLines ?? Array.Empty<string>();
            KeepMicOpen = keepMicOpen;
            UseStreamingTextInput = useStreamingTextInput;
            IncludeParallelToolSample = includeParallelToolSample;
            SendProactiveGreeting = sendProactiveGreeting;
            UseStereoEcReference = useStereoEcReference;
            WireFeatures = wireFeatures ?? Array.Empty<string>();
        }

        #endregion
    }

    /// <summary>
    ///     The catalog of preview feature checks, ordered by the API version they first appear in. The console
    ///     builds its <c>feature Check</c> menu from <see cref="ForVersion" />, so this list is the single
    ///     source of truth for what can be checked at each version.
    /// </summary>
    public static class PreviewFeatureCatalog
    {
        #region Static Fields and Constants

        /// <summary>All preview feature checks, oldest API version first.</summary>
        public static readonly IReadOnlyList<PreviewFeatureCheck> All = new[]
        {
            // ---- 2026-01-01-preview ----
            new PreviewFeatureCheck(
                "auto_truncate",
                "Auto-truncation (turn_detection.auto_truncate on barge-in)",
                "2026-01-01-preview",
                options =>
                {
                    // Auto truncation keeps the stored context aligned with the audio the user actually heard
                    // when they interrupt. It needs a VAD that supports it (azure_semantic_vad) with
                    // interrupt_response enabled.
                    options.TurnDetection = new TurnDetection
                    {
                        Type = "azure_semantic_vad",
                        Threshold = 0.5f,
                        SilenceDurationMs = 500,
                        InterruptResponse = true,
                        AutoTruncate = true,
                        CreateResponse = true
                    };
                },
                new[]
                {
                    "  - Ask something that yields a long spoken answer, then INTERRUPT (barge in) mid-answer.",
                    "  - The service truncates the stored response to what you heard and sends conversation.item.truncated.",
                    "  - Without this, the logged text would include audio you never heard."
                }),

            new PreviewFeatureCheck(
                "webrtc_voice",
                "WebRTC voice (RTP audio over /calls instead of the WebSocket)",
                "2026-01-01-preview",
                options => { /* Configured by the WebRTC call itself, not by session options here. */ },
                new[]
                {
                    "  - Audio flows over WebRTC RTP; the WebSocket only carries signaling (rtc.call.*).",
                    "  - Pinned to 2026-01-01-preview: /calls answers 401 on 2026-06-01-preview with either",
                    "    auth method. Override with VOICELIVE_WEBRTC_API_VERSION.",
                    "  - Not combinable with avatar output."
                },
                sessionKind: PreviewSessionKind.WebRtcVoice),

            // ---- 2026-06-01-preview ----
            new PreviewFeatureCheck(
                "avatar_websocket_video",
                "Avatar with WebSocket video (response.video.delta, no SDP/ICE)",
                "2026-06-01-preview",
                options => { /* The avatar block is built by the console's avatar configuration. */ },
                new[]
                {
                    "  - Sets output_protocol=websocket, so avatar video arrives as response.video.delta",
                    "    (base64 fMP4/H.264) on the same WebSocket — no WebRTC negotiation.",
                    "  - Audio stays on the standard 24 kHz PCM path.",
                    "  - The video window (FFplay) opens automatically once frames arrive."
                },
                sessionKind: PreviewSessionKind.AvatarWebSocketVideo),

            new PreviewFeatureCheck(
                "photo_avatar",
                "Photo avatar (talking head generated from a single image by vasa-1)",
                "2026-06-01-preview",
                options => { /* The avatar block is built by the console's avatar configuration. */ },
                new[]
                {
                    "  - Sets avatar type=photo-avatar with base model vasa-1, so the service animates a still",
                    "    portrait rather than streaming a pre-rendered video avatar.",
                    "  - Character comes from the 'Talking heads' list (VOICELIVE_PHOTO_AVATAR_CHARACTER,",
                    "    default 'sakura'). Set VOICELIVE_PHOTO_AVATAR_CUSTOMIZED=1 to use your own photo avatar.",
                    "  - Media arrives over WebRTC, the same path as the video avatar.",
                    "  - Needs a text-to-speech-avatar region; see the Speech service regions table.",
                    "  - The video window (FFplay) opens automatically once frames arrive."
                },
                sessionKind: PreviewSessionKind.PhotoAvatar),

            new PreviewFeatureCheck(
                "smart_end_of_turn",
                "Smart end-of-turn detection (turn_detection = smart_end_of_turn_detection)",
                "2026-06-01-preview",
                options =>
                {
                    // Smart end-of-turn is an end-of-utterance config nested in a semantic VAD
                    // (model = smart_end_of_turn_detection). It is NOT a top-level turn_detection type.
                    options.TurnDetection = new TurnDetection
                    {
                        Type = "azure_semantic_vad",
                        Threshold = 0.5f,
                        SilenceDurationMs = 500,
                        EndOfUtteranceDetection = new
                        {
                            model = "smart_end_of_turn_detection",
                            threshold_level = "medium",
                            timeout_ms = 1000
                        },
                        CreateResponse = true
                    };
                },
                new[]
                {
                    "  - The mic stays on (no auto-stop) so the smart end-of-turn model can decide.",
                    "  - Speak with a SHORT pause mid-sentence (< timeout_ms=1000): the turn is held.",
                    "  - A longer pause ends the turn and a response is generated. Press 'R' to stop the mic."
                },
                keepMicOpen: true),

            new PreviewFeatureCheck(
                "parallel_tool_calls",
                "Parallel tool calls (parallel_tool_calls, with two sample tools)",
                "2026-06-01-preview",
                options => options.ParallelToolCalls = true,
                new[]
                {
                    "  - Ask for two things at once, e.g. 'What's the weather AND the time in Tokyo?'.",
                    "  - Watch for two [Function Call] lines (get_weather + get_time) in one turn."
                },
                includeParallelToolSample: true),

            new PreviewFeatureCheck(
                "native_voice",
                "azure-realtime-native voice (requires an azure-realtime model)",
                "2026-06-01-preview",
                options =>
                {
                    // azure-realtime-native voice requires the azure-realtime model (native speech-to-speech,
                    // region-gated). That model does not use the cascaded Azure-TTS options and rejects tools,
                    // so clear them to isolate just model + voice.
                    options.Model = "azure-realtime";
                    options.Voice = new Voice { Type = "azure-realtime-native", Name = "ava" };
                    options.InputAudioNoiseReduction = null;
                    options.Animation = null;
                    options.OutputAudioTimestampTypes = null;
                    options.Tools = null;
                    options.ToolChoice = null;
                    options.ParallelToolCalls = null;
                },
                new[]
                {
                    "  - Uses model 'azure-realtime' + voice { type: azure-realtime-native, name: ava }.",
                    "  - Requires an azure-realtime-capable region (e.g. swedencentral, eastus2, westus2).",
                    "  - Cascaded options (noise reduction / viseme / word timestamps) are cleared here."
                }),

            new PreviewFeatureCheck(
                "client_ec_reference",
                "Client-side echo cancellation reference (stereo mic+playback)",
                "2026-06-01-preview",
                options =>
                    // Tell the server to use channel 1 of the stereo input as the echo reference instead of its
                    // internal TTS loopback. Requires channels=2 + pcm16 + the client_ec_reference URL flag; the
                    // console then sends interleaved stereo [mic, played-audio].
                    options.InputAudioEchoCancellation = new AudioInputEchoCancellationSettings
                    {
                        Type = "server_echo_cancellation",
                        ReferenceSource = "client",
                        Channels = 2
                    },
                new[]
                {
                    "  - Sends interleaved stereo PCM16: ch0 = mic, ch1 = actual speaker playback (echo ref).",
                    "  - Verifies the wire (URL &features=client_ec_reference:true + channels=2 accepted).",
                    "  - Alignment is best-effort (constant-delay FIFO); true AEC quality needs an echo setup.",
                    "  - Talk while the assistant is speaking; the server EC uses your played audio as reference."
                },
                keepMicOpen: true,
                useStereoEcReference: true,
                wireFeatures: new[] { "client_ec_reference:true" }),

            new PreviewFeatureCheck(
                "streaming_text_input",
                "Pre-generated assistant message (predefined text spoken via 'X' key)",
                // The one-shot pre_generated_assistant_message used here is documented from
                // 2026-01-01-preview onwards. (The incremental variant, input_text.delta/.done, is the
                // 2026-06-01-preview addition — see the hint lines: it is not reachable from a client.)
                "2026-01-01-preview",
                options => { /* No session config; exercised via the 'X' key. */ },
                new[]
                {
                    "  - Press 'X' and type text; the ASSISTANT speaks it verbatim (TTS), not a reply.",
                    "  - Sent as response.create { pre_generated_assistant_message } (one text entry),",
                    "    the one-shot form documented from 2026-01-01-preview.",
                    "  - NOTE: the incremental variant input_text.delta/.done (added in 2026-06-01-preview)",
                    "    appends to an *incomplete* pre_generated_assistant_message, which a client cannot",
                    "    open (content is mandatory) — a preview gap, so the one-shot form is used instead."
                },
                useStreamingTextInput: true),

            // ---- 2026-01-01-preview (added) ----
            new PreviewFeatureCheck(
                "interim_response",
                "Interim response (spoken filler during tool calls / high latency)",
                "2026-01-01-preview",
                options =>
                    // Bridges tool-call / latency gaps with short spoken filler. Model mode requires a cascaded
                    // text LLM (e.g. gpt-4o) + Azure voice — the FeatureCheck defaults already satisfy this.
                    // NOTE: the wire uses snake_case ("llm_interim_response") — the API-reference JSON's
                    // hyphenated "llm-interim-response" is a doc typo (the service rejects the hyphen form).
                    options.InterimResponse = new
                    {
                        type = "llm_interim_response",
                        triggers = new[] { "latency", "tool" },
                        latency_threshold_ms = 2000,
                        model = "gpt-4.1-mini",
                        instructions = "Generate a brief, friendly acknowledgment that you're working on it.",
                        max_completion_tokens = 30
                    },
                new[]
                {
                    "  - Ask something that triggers get_weather; a short filler is spoken while the tool runs.",
                    "  - Requires a cascaded text model + Azure voice (default gpt-4o works; azure-realtime does NOT)."
                }),

            new PreviewFeatureCheck(
                "azure_personal_voice",
                "azure-personal voice (requires a provisioned personal voice)",
                "2026-01-01-preview",
                options =>
                {
                    Voice? personalVoice = TryBuildPersonalVoice();
                    if (personalVoice != null)
                    {
                        options.Voice = personalVoice;
                    }
                },
                new[]
                {
                    "  - Pass --personal-voice <speaker profile GUID> (or set VOICELIVE_PERSONAL_VOICE).",
                    "    The base model defaults to DragonLatestNeural; --personal-voice-model changes it.",
                    "  - The same setting applies to avatar sessions, so a custom photo avatar can speak",
                    "    with your personal voice (avatar and voice are independent settings).",
                    "  - IMPORTANT: voice.name takes the SPEAKER PROFILE ID (a GUID), not the voice name and",
                    "    not the 'Profile ID' shown on the portal's model page. Both of those fail with",
                    "    \"you don't have access to this personalVoiceName or it's not available\".",
                    "    The working GUID appears in the URL of the personal voice page in the portal.",
                    "  - Without the env var the default Azure voice is used (no personal voice applied)."
                }),

            new PreviewFeatureCheck(
                "proactive_greeting",
                "Proactive greeting (assistant speaks first on session ready)",
                "2026-01-01-preview",
                options => { /* No session config; the console sends the greeting on session.updated. */ },
                new[]
                {
                    "  - The assistant greets you first, once the session is ready, before you speak.",
                    "  - Implemented client-side: a system 'greet the user' item + response.create on session.updated."
                },
                sendProactiveGreeting: true),

            new PreviewFeatureCheck(
                "foundry_agent_tool",
                "Foundry agent as a tool (chat-supervisor; delegates to your agent)",
                "2026-01-01-preview",
                options =>
                {
                    // Expose a Foundry agent as a tool of this model session ("chat-supervisor" pattern): the
                    // realtime model handles the small talk and delegates the hard question to the agent. The
                    // service invokes the agent itself, so no function_call reaches the client — progress shows
                    // up as response.foundry_agent_call.* events. Reuses the same agent/project the AI Agent
                    // mode uses (VOICELIVE_AGENT_NAME / VOICELIVE_AGENT_PROJECT or user secrets).
                    var agentName = ConsoleSettings.Get("AgentName");
                    var projectName = ConsoleSettings.Get("AgentProjectName");
                    if (string.IsNullOrWhiteSpace(agentName) || string.IsNullOrWhiteSpace(projectName))
                    {
                        return;
                    }

                    options.Tools = new ToolDefinition[]
                    {
                        new FoundryAgentToolConfig
                        {
                            AgentName = agentName,
                            ProjectName = projectName,
                            Description = ConsoleSettings.Get("AgentToolDescription")
                                          ?? "Delegate detailed or complex questions to this specialist agent."
                        }
                    };
                    options.ToolChoice = "auto";
                    options.ParallelToolCalls = null;
                },
                new[]
                {
                    "  - Declares your Foundry agent as a tool (type=foundry_agent) of this model session.",
                    "  - Needs Entra ID auth and VOICELIVE_AGENT_NAME / VOICELIVE_AGENT_PROJECT (same values",
                    "    as the AI Agent conversation mode); without them no tool is declared.",
                    "  - Ask something the agent specializes in; the service calls the agent server-side, so",
                    "    watch for [FoundryAgentCall] in_progress/completed instead of a local function call.",
                    "  - Override the tool description with VOICELIVE_AGENT_TOOL_DESCRIPTION."
                }),

            // ---- 2026-04-10+ (shown when 2026-06-01-preview is selected) ----
            new PreviewFeatureCheck(
                "mcp_server",
                "MCP server integration (managed tools; default deepwiki)",
                "2026-04-10",
                options =>
                {
                    // MCP tools are managed/executed by the service. Default to the public deepwiki MCP server
                    // so it works without setup; override via env. require_approval=never for auto-execution.
                    var url = ConsoleSettings.GetOr("McpUrl", "https://mcp.deepwiki.com/mcp");
                    var label = ConsoleSettings.GetOr("McpLabel", "deepwiki");
                    options.Tools = new ToolDefinition[]
                    {
                        new McpToolConfig
                        {
                            ServerLabel = label,
                            ServerUrl = url,
                            RequireApproval = JsonSerializer.SerializeToElement("never")
                        }
                    };
                    options.ToolChoice = "auto";
                    options.ParallelToolCalls = null;
                },
                new[]
                {
                    "  - Uses a managed MCP server (default deepwiki https://mcp.deepwiki.com/mcp, require_approval=never).",
                    "  - Ask e.g. 'What is the structure of the microsoft/vscode repository?'.",
                    "  - Override with VOICELIVE_MCP_URL / VOICELIVE_MCP_LABEL. Requires api-version 2026-04-10+.",
                    "  - Watch for mcp_list_tools / response.mcp_call events."
                })
        };

        #endregion

        #region Public Methods

        /// <summary>
        ///     Builds the <c>azure-personal</c> voice from the <c>PersonalVoice</c> setting, or returns
        ///     <see langword="null" /> when that variable is unset. Voice and avatar are independent session
        ///     settings, so this is shared with the avatar path: a custom photo avatar can speak with a
        ///     personal voice.
        /// </summary>
        /// <returns>The personal voice, or <see langword="null" /> to keep the caller's default voice.</returns>
        public static Voice? TryBuildPersonalVoice()
        {
            string? name = ConsoleSettings.Get("PersonalVoice");
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            string model = ConsoleSettings.Get("PersonalVoiceModel")
                           ?? "DragonLatestNeural";

            return new Voice { Type = "azure-personal", Model = model, Name = name };
        }

        /// <summary>
        ///     Returns the feature checks available at the given API version. Version strings sort
        ///     chronologically, so an ordinal comparison scopes the list (a later version is a superset).
        /// </summary>
        /// <param name="apiVersion">The selected API version.</param>
        /// <returns>The feature checks valid for that version, in catalog order.</returns>
        public static IReadOnlyList<PreviewFeatureCheck> ForVersion(string apiVersion)
        {
            return All.Where(f => string.CompareOrdinal(apiVersion, f.MinApiVersion) >= 0).ToList();
        }

        #endregion
    }
}
