// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using Azure.AI.VoiceLive;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveSDK
{
    /// <summary>
    ///     How a feature is exercised: as settings on a normal session, or as an avatar session.
    /// </summary>
    public enum SdkFeatureKind
    {
        /// <summary>Session settings applied to a model session (the common case).</summary>
        ModelSession,

        /// <summary>An avatar session — the feature changes what the avatar is or how its video arrives.</summary>
        AvatarSession
    }

    /// <summary>
    ///     One feature the SDK console can exercise on its own, with everything needed to present it, apply
    ///     it and explain it. Adding a feature is one entry in <see cref="SdkFeatureCatalog.All" />.
    /// </summary>
    public sealed class SdkFeature
    {
        #region Properties

        /// <summary>Gets the stable identifier.</summary>
        public string Id { get; }

        /// <summary>Gets the one-line menu description.</summary>
        public string Title { get; }

        /// <summary>Gets the action that applies this feature to the session options.</summary>
        public Action<VoiceLiveSessionOptions> Apply { get; }

        /// <summary>Gets the lines printed after connect that explain how to try it.</summary>
        public IReadOnlyList<string> HintLines { get; }

        /// <summary>Gets the kind of session this feature runs in.</summary>
        public SdkFeatureKind Kind { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SdkFeature" /> class.
        /// </summary>
        /// <param name="id">The stable identifier.</param>
        /// <param name="title">The one-line menu description.</param>
        /// <param name="apply">Applies the feature's configuration.</param>
        /// <param name="hintLines">Lines explaining how to exercise it.</param>
        /// <param name="kind">The kind of session it runs in.</param>
        public SdkFeature(string id, string title, Action<VoiceLiveSessionOptions> apply, string[] hintLines,
            SdkFeatureKind kind = SdkFeatureKind.ModelSession)
        {
            Id = id;
            Title = title;
            Apply = apply ?? (_ => { });
            HintLines = hintLines ?? Array.Empty<string>();
            Kind = kind;
        }

        #endregion
    }

    /// <summary>
    ///     The features the SDK console can demonstrate one at a time.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         These all reached GA in <c>2026-04-10</c> / <c>2026-07-15</c> and have strongly typed SDK
    ///         support in <c>Azure.AI.VoiceLive</c> 1.2.0. Several were first tried against
    ///         <c>2026-06-01-preview</c> through the wire client in <c>VoiceLiveConsoleApp</c>; what is
    ///         interesting here is how much shorter they are to express once the SDK carries the types.
    ///     </para>
    ///     <para>
    ///         The feature menu opens the session at the newest GA wire version, so everything below is
    ///         available regardless of which version first introduced it.
    ///     </para>
    /// </remarks>
    public static class SdkFeatureCatalog
    {
        #region Static Fields and Constants

        /// <summary>The wire version the feature menu connects with — the newest GA the SDK offers.</summary>
        public const VoiceLiveClientOptions.ServiceVersion FeatureServiceVersion =
            VoiceLiveClientOptions.ServiceVersion.V2026_07_15;

        /// <summary>
        ///     The standard photo avatar characters ("Talking heads"), used only to tell a standard character
        ///     from a custom one so an unknown name is sent as <c>customized</c> instead of being rejected.
        /// </summary>
        private static readonly HashSet<string> StandardTalkingHeads = new HashSet<string>
        {
            "adrian", "amara", "amira", "anika", "bianca", "camila", "carlos", "clara", "darius", "diego",
            "elise", "farhan", "faris", "gabrielle", "hyejin", "imran", "isabella", "layla", "liwei", "ling",
            "marcus", "matteo", "rahul", "rana", "ren", "riya", "sakura", "simone", "zayd", "zoe"
        };

        /// <summary>The catalog — the single source of truth for what the feature menu offers.</summary>
        public static readonly IReadOnlyList<SdkFeature> All = new[]
        {
            new SdkFeature(
                "photo_avatar",
                "Photo avatar (talking head generated from a single image by vasa-1)",
                options =>
                {
                    // A photo avatar is already a head shot, so unlike the video avatar it needs no crop and
                    // takes no style — the standard talking heads have none. It does need its base model.
                    string character = Environment.GetEnvironmentVariable("VOICELIVE_PHOTO_AVATAR_CHARACTER")?.Trim()
                                       ?? string.Empty;
                    if (character.Length == 0)
                    {
                        character = "sakura";
                    }

                    bool customized = Environment.GetEnvironmentVariable("VOICELIVE_PHOTO_AVATAR_CUSTOMIZED") == "1"
                                      || !StandardTalkingHeads.Contains(character.ToLowerInvariant());

                    Console.WriteLine($"Photo avatar: character '{character}'"
                                      + (customized ? " (custom)" : " (standard talking head)"));

                    options.Avatar = new AvatarConfiguration(character, customized)
                    {
                        AvatarKind = AvatarConfigKind.PhotoAvatar,
                        BaseMode = PhotoAvatarBaseMode.Vasa1,
                        Video = new VideoParams
                        {
                            Bitrate = 2000000,
                            Codec = "h264",
                            Background = new VideoBackground { Color = "#FFFFFFFF" }
                        }
                    };
                },
                new[]
                {
                    "  - A still portrait animated by vasa-1, rather than a pre-rendered character.",
                    "  - VOICELIVE_PHOTO_AVATAR_CHARACTER picks the talking head (default 'sakura'); a name",
                    "    that is not one of the 30 standard heads is sent as a custom avatar automatically.",
                    "  - No crop and no style: the frame is already a head shot and the standard heads have",
                    "    no styles. video.resolution is not honored either — frames keep the portrait's ratio.",
                    "  - The video window opens automatically (FFplay must be on PATH)."
                },
                SdkFeatureKind.AvatarSession),

            new SdkFeature(
                "photo_avatar_scene",
                "Photo avatar + scene (zoom / position / rotation / amplitude)",
                options =>
                {
                    ApplyPhotoAvatar(options);
                    options.Avatar!.Scene = new SceneParams
                    {
                        Zoom = 1.2f,
                        PositionX = 0.0f,
                        PositionY = 0.05f,
                        Amplitude = 0.8f
                    };
                },
                new[]
                {
                    "  - scene is photo-avatar only; the video avatar ignores it.",
                    "  - zoom 1.0 = 100%; position/rotation are offsets around centre; amplitude below 1",
                    "    damps head movement.",
                    "  - Values here (zoom 1.2, slight rise, amplitude 0.8) frame the head a little closer."
                },
                SdkFeatureKind.AvatarSession),

            new SdkFeature(
                "avatar_websocket_video",
                "Avatar with WebSocket video (frames on the session socket, no SDP/ICE)",
                options =>
                {
                    ApplyVideoAvatar(options);
                    options.Avatar!.OutputProtocol = AvatarOutputProtocol.Websocket;
                },
                new[]
                {
                    "  - output_protocol=websocket, so video arrives as response.video.delta on the same",
                    "    socket — no WebRTC negotiation at all.",
                    "  - The frames are fragmented MP4 (ftyp/moov then moof/mdat), not a raw H.264 stream.",
                    "  - Audio stays on the standard PCM path."
                },
                SdkFeatureKind.AvatarSession),

            new SdkFeature(
                "azure_personal_voice",
                "azure-personal voice (requires a provisioned personal voice)",
                options =>
                {
                    string? name = Environment.GetEnvironmentVariable("VOICELIVE_PERSONAL_VOICE");
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Console.WriteLine("VOICELIVE_PERSONAL_VOICE is not set — the default voice is used.");
                        return;
                    }

                    options.Voice = new AzurePersonalVoice(name, PersonalVoiceModels.DragonLatestNeural);
                },
                new[]
                {
                    "  - Set VOICELIVE_PERSONAL_VOICE to the SPEAKER PROFILE ID (a GUID).",
                    "  - IMPORTANT: it is not the voice name, and not the 'Profile ID' the portal displays.",
                    "    Both of those fail with \"you don't have access to this personalVoiceName\".",
                    "    The working GUID appears only in the URL of the personal voice page in the portal.",
                    "  - Voice and avatar are independent, so this composes with the photo avatar above."
                }),

            new SdkFeature(
                "semantic_eou",
                "Semantic end-of-utterance detection (multilingual)",
                options =>
                {
                    // Semantic EOU (semantic_detection_v1_multilingual) is the GA end-of-utterance model,
                    // nested in a semantic VAD rather than being a VAD type itself. It is not smart turn
                    // detection (smart_end_of_turn_detection): that model is still preview, so the SDK has
                    // no type for it and EouDetectionModel lists only the three semantic models.
                    options.TurnDetection = new AzureSemanticVadTurnDetection
                    {
                        Threshold = 0.5f,
                        PrefixPadding = TimeSpan.FromMilliseconds(300),
                        SilenceDuration = TimeSpan.FromMilliseconds(500),
                        EndOfUtteranceDetection = new AzureSemanticEouDetectionMultilingual
                        {
                            ThresholdLevel = EouThresholdLevel.Default,
                            Timeout = TimeSpan.FromMilliseconds(1000)
                        }
                    };
                },
                new[]
                {
                    "  - Say something with a deliberate pause in the middle (\"ええと……\") and keep talking.",
                    "  - Plain server VAD would answer during the pause; the semantic model judges from what",
                    "    was said whether the utterance is complete.",
                    "  - Model: semantic_detection_v1_multilingual, nested under the semantic VAD as",
                    "    end_of_utterance_detection — it is not a top-level turn_detection type.",
                    "  - This is not smart turn detection (smart_end_of_turn_detection). That model is still",
                    "    preview — even in the GA 2026-07-15 — so the SDK has no type for it. Try it in",
                    "    VoiceLiveConsoleApp under 2026-06-01-preview."
                }),

            new SdkFeature(
                "auto_truncate",
                "Auto-truncation on barge-in (stored response matches what you heard)",
                options =>
                {
                    options.TurnDetection = new AzureSemanticVadTurnDetection
                    {
                        Threshold = 0.5f,
                        PrefixPadding = TimeSpan.FromMilliseconds(300),
                        SilenceDuration = TimeSpan.FromMilliseconds(500),
                        InterruptResponse = true,
                        AutoTruncate = true
                    };
                },
                new[]
                {
                    "  - Ask for a long answer, then interrupt it half way through.",
                    "  - The service truncates the stored response to what was actually played and sends",
                    "    conversation.item.truncated; without it the transcript keeps audio you never heard."
                }),

            new SdkFeature(
                "mcp_tool",
                "MCP server (tools hosted remotely, called and executed server-side)",
                options =>
                {
                    string url = Environment.GetEnvironmentVariable("VOICELIVE_MCP_URL")?.Trim() ?? string.Empty;
                    if (url.Length == 0)
                    {
                        url = "https://mcp.deepwiki.com/mcp";
                    }

                    Console.WriteLine($"MCP server: {url}");

                    options.Tools.Add(new VoiceLiveMcpServerDefinition("deepwiki", new Uri(url))
                    {
                        RequireApproval = new RequireApprovalOption(McpApprovalKind.Never)
                    });

                    // Without this the model answers repository questions from its own knowledge and the
                    // server is never reached — which looks identical to a broken MCP configuration.
                    options.Instructions =
                        "You are a helpful assistant. Answer in the language the user speaks. "
                        + "For any question about a GitHub repository, you must call the deepwiki tools "
                        + "instead of answering from memory, even if you believe you know the answer. "
                        + "Repository arguments take the form owner/repo. "
                        + "Prefer ask_question, which returns a short answer. Do not call read_wiki_contents: "
                        + "it returns an entire wiki, which is far too much to read aloud. "
                        + "Keep the spoken answer to a few sentences.";
                },
                new[]
                {
                    "  - The tools live on an MCP server, so nothing runs on this machine: the service lists",
                    "    them, calls them and feeds the results back into the answer.",
                    "  - Default server is deepwiki; VOICELIVE_MCP_URL points somewhere else.",
                    "  - Name a repository as owner/repo — \"What is microsoft/semantic-kernel for?\" — since",
                    "    deepwiki looks repositories up by that form.",
                    "  - Watch for the [MCP] lines: one lists the server's tools, and one per call shows the",
                    "    arguments and result. No [MCP] call line means the model answered on its own.",
                    "  - The session instructions push it toward the tools; without that nudge it usually",
                    "    answers from memory and the server is never reached. They also steer it to",
                    "    ask_question, because read_wiki_contents returns a whole wiki — 600 KB in one",
                    "    result here — which then becomes the input for the spoken answer.",
                    "  - AllowedTools on the server definition is the enforcing version of that steer.",
                    "  - require_approval is 'never' here; 'always' would make the service ask first."
                }),

            new SdkFeature(
                "interim_response",
                "Interim response (filler speech while a slow tool runs)",
                options =>
                {
                    options.Tools.Add(BuildWeatherTool());
                    var interim = new LlmInterimResponseConfig
                    {
                        Triggers = { InterimResponseTrigger.Tool, InterimResponseTrigger.Latency },
                        LatencyThreshold = TimeSpan.FromMilliseconds(500),
                        Instructions = "Say a short, natural filler in the user's language while you wait.",
                        MaxCompletionTokens = 50
                    };

                    // The config is a typed model, but the option holding it is still BinaryData, so it is
                    // written out rather than assigned. That is what keeps the wire names correct.
                    options.InterimResponse = ModelReaderWriter.Write(interim);
                },
                new[]
                {
                    "  - Bridges the silence while a tool runs or the model is slow to start.",
                    "  - 'llm' generates the filler; StaticInterimResponseConfig would read from a fixed list.",
                    "  - Cascaded sessions only (a text model plus an Azure voice).",
                    "  - Ask for the weather: the filler comes first, then the real answer."
                }),

            new SdkFeature(
                "parallel_tool_calls",
                "Parallel tool calls (two sample tools in one turn)",
                options =>
                {
                    options.AllowParallelToolCalls = true;
                    options.Tools.Add(BuildWeatherTool());
                    options.Tools.Add(BuildTimeTool());
                },
                new[]
                {
                    "  - Ask something that needs both tools: \"What's the weather and time in Tokyo?\"",
                    "  - Both function calls arrive in the same turn.",
                    "  - Submit every tool output first and request the response once; creating a response",
                    "    per output makes the service answer twice."
                }),

            new SdkFeature(
                "avatar_sync_voice",
                "avatar-sync voice (a voice trained alongside a custom video avatar)",
                options =>
                {
                    string? name = Environment.GetEnvironmentVariable("VOICELIVE_AVATAR_SYNC_VOICE");
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Console.WriteLine("VOICELIVE_AVATAR_SYNC_VOICE is not set — the default voice is used.");
                        return;
                    }

                    ApplyVideoAvatar(options);
                    options.Avatar!.Character = name;
                    options.Avatar.Customized = true;
                    options.Avatar.Style = null;
                    options.Voice = new AzureAvatarSyncVoice(name);
                },
                new[]
                {
                    "  - Only for a CUSTOM VIDEO avatar: the voice is trained from the same recording as the",
                    "    avatar, so the face and the voice come from one person.",
                    "  - Set VOICELIVE_AVATAR_SYNC_VOICE to your custom avatar's name.",
                    "  - Photo avatars cannot use it — pair them with a personal voice instead, which needs",
                    "    one photo and about thirty seconds of audio rather than ten minutes of studio video."
                },
                SdkFeatureKind.AvatarSession)
        };

        #endregion

        #region Private Methods

        /// <summary>
        ///     Applies the photo avatar configuration shared by the photo-avatar features.
        /// </summary>
        /// <param name="options">The session options to configure.</param>
        private static void ApplyPhotoAvatar(VoiceLiveSessionOptions options)
        {
            foreach (SdkFeature feature in All)
            {
                if (feature.Id == "photo_avatar")
                {
                    feature.Apply(options);
                    return;
                }
            }
        }

        /// <summary>
        ///     Applies the standard video avatar: a full-body character cropped to the speaker.
        /// </summary>
        /// <param name="options">The session options to configure.</param>
        private static void ApplyVideoAvatar(VoiceLiveSessionOptions options)
        {
            options.Avatar = new AvatarConfiguration("lisa", false)
            {
                Style = "casual-sitting",
                Video = new VideoParams
                {
                    Bitrate = 2000000,
                    Codec = "h264",
                    Crop = new VideoCrop(new[] { 560, 0 }, new[] { 1360, 1080 }),
                    Resolution = new VideoResolution(1920, 1080),
                    Background = new VideoBackground { Color = "#FFFFFFFF" }
                }
            };
        }

        /// <summary>
        ///     Builds the <c>get_weather</c> sample tool.
        /// </summary>
        /// <returns>The tool definition.</returns>
        private static VoiceLiveFunctionDefinition BuildWeatherTool()
        {
            return new VoiceLiveFunctionDefinition("get_weather")
            {
                Description = "Get the current weather for a given location. The user may ask in any language.",
                Parameters = BinaryData.FromObjectAsJson(new
                {
                    type = "object",
                    properties = new
                    {
                        location = new { type = "string", description = "The city and country, e.g. 'Tokyo, Japan'" }
                    },
                    required = new[] { "location" }
                })
            };
        }

        /// <summary>
        ///     Builds the <c>get_time</c> sample tool, so a single turn can need two calls.
        /// </summary>
        /// <returns>The tool definition.</returns>
        private static VoiceLiveFunctionDefinition BuildTimeTool()
        {
            return new VoiceLiveFunctionDefinition("get_time")
            {
                Description = "Get the current local time for a given location. The user may ask in any language.",
                Parameters = BinaryData.FromObjectAsJson(new
                {
                    type = "object",
                    properties = new
                    {
                        location = new { type = "string", description = "The city and country, e.g. 'Tokyo, Japan'" }
                    },
                    required = new[] { "location" }
                })
            };
        }

        #endregion
    }
}
