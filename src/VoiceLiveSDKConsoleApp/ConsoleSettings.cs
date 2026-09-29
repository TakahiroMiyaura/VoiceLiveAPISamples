// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveSDK
{
    /// <summary>
    ///     What a setting is for, which decides how it is normally supplied.
    /// </summary>
    public enum SettingCategory
    {
        /// <summary>
        ///     Where to connect and what to authenticate with. Fixed per environment, so it belongs in user
        ///     secrets and is overridden with an environment variable (a shell profile, a CI secret).
        /// </summary>
        Connection,

        /// <summary>
        ///     What a feature operates on — a voice, an avatar, an MCP server. Changed from run to run, so it
        ///     is overridden with a command-line argument.
        /// </summary>
        FeatureInput,

        /// <summary>
        ///     How the console itself behaves while it runs (logging, tracing). Only ever wanted for a single
        ///     run, so it is a command-line argument.
        /// </summary>
        Diagnostic
    }

    /// <summary>
    ///     One configurable value and every way it can be supplied. Keeping the sources on the setting itself
    ///     means a new setting is one entry here rather than a read scattered into the code that needs it, and
    ///     it lets <see cref="ConsoleSettings.PrintHelp" /> list them all without a second list to keep in step.
    /// </summary>
    public sealed class ConsoleSetting
    {
        #region Properties

        /// <summary>Gets the stable name used to look the setting up.</summary>
        public string Name { get; }

        /// <summary>Gets what the setting is for, which decides how it is normally supplied.</summary>
        public SettingCategory Category { get; }

        /// <summary>Gets the user-secrets key, or <see langword="null" /> when the setting has none.</summary>
        public string? SecretKey { get; }

        /// <summary>Gets the environment variable name, or <see langword="null" /> when it has none.</summary>
        public string? EnvironmentVariable { get; }

        /// <summary>Gets the command-line switch (including the leading dashes), or <see langword="null" />.</summary>
        public string? Argument { get; }

        /// <summary>Gets the value used when no source supplies one.</summary>
        public string? DefaultValue { get; }

        /// <summary>Gets a value indicating whether the setting is a flag (present means true).</summary>
        public bool IsFlag { get; }

        /// <summary>Gets the one-line description shown by <see cref="ConsoleSettings.PrintHelp" />.</summary>
        public string Description { get; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConsoleSetting" /> class.
        /// </summary>
        /// <param name="name">The stable lookup name.</param>
        /// <param name="category">What the setting is for.</param>
        /// <param name="description">The one-line description.</param>
        /// <param name="secretKey">The user-secrets key, if any.</param>
        /// <param name="environmentVariable">The environment variable name, if any.</param>
        /// <param name="argument">The command-line switch, if any.</param>
        /// <param name="defaultValue">The value used when nothing supplies one.</param>
        /// <param name="isFlag">Whether the setting is a presence flag rather than a value.</param>
        public ConsoleSetting(string name, SettingCategory category, string description,
            string? secretKey = null, string? environmentVariable = null, string? argument = null,
            string? defaultValue = null, bool isFlag = false)
        {
            Name = name;
            Category = category;
            Description = description;
            SecretKey = secretKey;
            EnvironmentVariable = environmentVariable;
            Argument = argument;
            DefaultValue = defaultValue;
            IsFlag = isFlag;
        }

        #endregion
    }

    /// <summary>
    ///     The SDK console's settings: one catalog of every configurable value, and one place that resolves them.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The same design as <c>ConsoleSettings</c> in VoiceLiveConsoleApp. Values are resolved
    ///         <b>default → user secrets → environment variable → command-line argument</b>, so the narrower the
    ///         scope, the higher it wins.
    ///     </para>
    ///     <para>
    ///         Both consoles read the same user-secrets store (they share a <c>UserSecretsId</c>), so a setting
    ///         they have in common uses the same secret key and a value set for one is seen by the other.
    ///     </para>
    /// </remarks>
    public static class ConsoleSettings
    {
        #region Static Fields and Constants

        /// <summary>The settings catalog — the single source of truth for what the console can be given.</summary>
        public static readonly IReadOnlyList<ConsoleSetting> All = new[]
        {
            // ---- Connection and credentials (user secrets, overridden by environment variables) ----
            new ConsoleSetting("Endpoint", SettingCategory.Connection,
                "Azure AI Services endpoint of the Voice Live resource.",
                secretKey: "VoiceLiveAPI:AzureEndpoint", environmentVariable: "VOICELIVE_ENDPOINT"),

            new ConsoleSetting("ApiKey", SettingCategory.Connection,
                "API key for key-based authentication (Entra ID is used otherwise).",
                secretKey: "AzureAIFoundry:ApiKey", environmentVariable: "VOICELIVE_APIKEY"),

            new ConsoleSetting("Model", SettingCategory.Connection,
                "Model to run a model session against (not every region offers every model).",
                secretKey: "VoiceLiveAPI:Model", environmentVariable: "VOICELIVE_MODEL", defaultValue: "gpt-4o"),

            new ConsoleSetting("AgentName", SettingCategory.Connection,
                "Foundry agent to talk to in AI Agent mode.",
                secretKey: "AzureAIFoundry:AgentName", environmentVariable: "VOICELIVE_AGENT_NAME"),

            new ConsoleSetting("AgentProjectName", SettingCategory.Connection,
                "Foundry project that hosts the agent.",
                secretKey: "AzureAIFoundry:AgentProjectName", environmentVariable: "VOICELIVE_AGENT_PROJECT"),

            new ConsoleSetting("ServiceVersion", SettingCategory.Connection,
                "Wire version to connect with, overriding the per-mode default (V2026_07_15 or 2026-07-15).",
                secretKey: "VoiceLiveAPI:SdkServiceVersion", environmentVariable: "VOICELIVE_SDK_SERVICE_VERSION"),

            // ---- Feature inputs (user secrets, overridden by command-line arguments) ----
            new ConsoleSetting("Voice", SettingCategory.FeatureInput,
                "Voice for spoken output: an Azure voice name, or an OpenAI voice for realtime models.",
                secretKey: "VoiceLiveAPI:Voice", environmentVariable: "VOICELIVE_VOICE",
                argument: "--voice", defaultValue: "ja-JP-Nanami:DragonHDLatestNeural"),

            new ConsoleSetting("AvatarBackend", SettingCategory.FeatureInput,
                "Default for what drives an avatar session: 'agent' or 'model'.",
                secretKey: "VoiceLiveAPI:AvatarBackend", environmentVariable: "VOICELIVE_AVATAR_BACKEND",
                argument: "--avatar-backend", defaultValue: "agent"),

            new ConsoleSetting("PersonalVoice", SettingCategory.FeatureInput,
                "Personal voice speaker profile ID (the GUID from the portal page URL).",
                secretKey: "VoiceLiveAPI:PersonalVoice", environmentVariable: "VOICELIVE_PERSONAL_VOICE",
                argument: "--personal-voice"),

            new ConsoleSetting("PhotoAvatarCharacter", SettingCategory.FeatureInput,
                "Photo avatar character: a standard talking head, or your custom avatar's name.",
                secretKey: "VoiceLiveAPI:PhotoAvatarCharacter",
                environmentVariable: "VOICELIVE_PHOTO_AVATAR_CHARACTER",
                argument: "--photo-avatar", defaultValue: "sakura"),

            new ConsoleSetting("PhotoAvatarCustomized", SettingCategory.FeatureInput,
                "Force the photo avatar to resolve as custom (inferred from the name otherwise).",
                secretKey: "VoiceLiveAPI:PhotoAvatarCustomized",
                environmentVariable: "VOICELIVE_PHOTO_AVATAR_CUSTOMIZED",
                argument: "--photo-avatar-customized", isFlag: true),

            new ConsoleSetting("SceneZoom", SettingCategory.FeatureInput,
                "Photo avatar scene zoom. Only values below 1 have an effect (zoom out).",
                secretKey: "VoiceLiveAPI:PhotoAvatarSceneZoom",
                environmentVariable: "VOICELIVE_PHOTO_AVATAR_SCENE_ZOOM",
                argument: "--scene-zoom", defaultValue: "0.8"),

            new ConsoleSetting("ScenePositionX", SettingCategory.FeatureInput,
                "Photo avatar horizontal offset, -1 to 1 of the frame width (positive moves right).",
                secretKey: "VoiceLiveAPI:PhotoAvatarScenePositionX",
                environmentVariable: "VOICELIVE_PHOTO_AVATAR_SCENE_POSITION_X",
                argument: "--scene-position-x", defaultValue: "0"),

            new ConsoleSetting("ScenePositionY", SettingCategory.FeatureInput,
                "Photo avatar vertical offset, -1 to 1 of the frame height (positive moves down).",
                secretKey: "VoiceLiveAPI:PhotoAvatarScenePositionY",
                environmentVariable: "VOICELIVE_PHOTO_AVATAR_SCENE_POSITION_Y",
                argument: "--scene-position-y", defaultValue: "0.05"),

            new ConsoleSetting("SceneAmplitude", SettingCategory.FeatureInput,
                "Photo avatar head movement, (0, 1] (below 1 damps it).",
                secretKey: "VoiceLiveAPI:PhotoAvatarSceneAmplitude",
                environmentVariable: "VOICELIVE_PHOTO_AVATAR_SCENE_AMPLITUDE",
                argument: "--scene-amplitude", defaultValue: "0.8"),

            new ConsoleSetting("McpUrl", SettingCategory.FeatureInput,
                "MCP server to attach to the session.",
                secretKey: "VoiceLiveAPI:McpUrl", environmentVariable: "VOICELIVE_MCP_URL",
                argument: "--mcp-url", defaultValue: "https://mcp.deepwiki.com/mcp"),

            new ConsoleSetting("AvatarSyncAvatar", SettingCategory.FeatureInput,
                "Custom video avatar whose voice sync to use: the avatar's name, not an ID.",
                secretKey: "VoiceLiveAPI:AvatarSyncAvatar", environmentVariable: "VOICELIVE_AVATAR_SYNC_VOICE",
                argument: "--avatar-sync"),

            new ConsoleSetting("AvatarSyncVoiceModel", SettingCategory.FeatureInput,
                "Base model behind the avatar's voice sync.",
                secretKey: "VoiceLiveAPI:AvatarSyncVoiceModel",
                environmentVariable: "VOICELIVE_AVATAR_SYNC_VOICE_MODEL",
                argument: "--avatar-sync-model", defaultValue: "DragonHDOmniLatestNeural"),

            new ConsoleSetting("AvatarSyncStyle", SettingCategory.FeatureInput,
                "Style of the custom video avatar, if it was trained with more than one.",
                secretKey: "VoiceLiveAPI:AvatarSyncStyle", environmentVariable: "VOICELIVE_AVATAR_SYNC_STYLE",
                argument: "--avatar-sync-style"),

            new ConsoleSetting("InterimLatencyMs", SettingCategory.FeatureInput,
                "Interim response: how long a reply may lag before the latency trigger fires.",
                secretKey: "VoiceLiveAPI:InterimLatencyMs", environmentVariable: "VOICELIVE_INTERIM_LATENCY_MS",
                argument: "--interim-latency-ms", defaultValue: "2000"),

            new ConsoleSetting("ToolDelayMs", SettingCategory.FeatureInput,
                "Interim response: how long the sample tool waits. Above zero, the filler is no longer spoken.",
                secretKey: "VoiceLiveAPI:ToolDelayMs", environmentVariable: "VOICELIVE_TOOL_DELAY_MS",
                argument: "--tool-delay-ms", defaultValue: "0"),

            // ---- Diagnostics (command-line arguments) ----
            new ConsoleSetting("LogLevel", SettingCategory.Diagnostic,
                "Minimum log level (Trace/Debug/Information/Warning/Error). Default Error keeps output readable.",
                secretKey: "VoiceLiveAPI:LogLevel", environmentVariable: "VOICELIVE_LOG_LEVEL",
                argument: "--log-level", defaultValue: "Error"),

            new ConsoleSetting("OpenTelemetry", SettingCategory.Diagnostic,
                "Print the SDK's OpenTelemetry spans ([OTel], one line per event: token usage, latency).",
                secretKey: "VoiceLiveAPI:OpenTelemetry", environmentVariable: "VOICELIVE_OTEL",
                argument: "--otel", isFlag: true)
        };

        /// <summary>Values that read as true, from any source.</summary>
        private static readonly string[] TruthyValues = { "1", "true", "yes", "on" };

        /// <summary>The user secrets, set by <see cref="Initialize" />.</summary>
        private static IConfiguration? configuration;

        /// <summary>The parsed command line, set by <see cref="Initialize" />.</summary>
        private static IReadOnlyDictionary<string, string> arguments =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Public Methods

        /// <summary>
        ///     Binds the settings to the user secrets and the command line. Call once at startup.
        /// </summary>
        /// <param name="config">The configuration holding the user secrets.</param>
        /// <param name="args">The raw command-line arguments.</param>
        public static void Initialize(IConfiguration config, string[] args)
        {
            configuration = config;
            arguments = ParseArguments(args);
        }

        /// <summary>
        ///     Resolves a setting: default, then user secrets, then environment variable, then command-line
        ///     argument, with later sources winning.
        /// </summary>
        /// <param name="name">The setting's <see cref="ConsoleSetting.Name" />.</param>
        /// <returns>The resolved value, or <see langword="null" /> when nothing supplies one.</returns>
        public static string? Get(string name)
        {
            ConsoleSetting setting = Find(name);
            string? value = setting.DefaultValue;

            if (setting.SecretKey != null && configuration != null)
            {
                value = Coalesce(configuration[setting.SecretKey], value);
            }

            if (setting.EnvironmentVariable != null)
            {
                value = Coalesce(Environment.GetEnvironmentVariable(setting.EnvironmentVariable), value);
            }

            if (setting.Argument != null && arguments.TryGetValue(setting.Argument, out string? fromArgs))
            {
                value = Coalesce(fromArgs, value);
            }

            return value?.Trim();
        }

        /// <summary>
        ///     Resolves a boolean setting. Accepts <c>1</c>, <c>true</c>, <c>yes</c> and <c>on</c> from any
        ///     source; a flag argument given without a value counts as true.
        /// </summary>
        /// <param name="name">The setting's <see cref="ConsoleSetting.Name" />.</param>
        /// <returns><see langword="true" /> when the setting is on.</returns>
        public static bool GetFlag(string name)
        {
            string? value = Get(name);
            return !string.IsNullOrEmpty(value) && TruthyValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Resolves a numeric setting, falling back to its default when the value is not a number.
        /// </summary>
        /// <param name="name">The setting's <see cref="ConsoleSetting.Name" />.</param>
        /// <returns>The number.</returns>
        public static float GetNumber(string name)
        {
            if (float.TryParse(Get(name), NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                return value;
            }

            Console.WriteLine($"[Config] {name} is not a number; using {Find(name).DefaultValue}.");
            return float.Parse(Find(name).DefaultValue ?? "0", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Describes how a setting can be supplied, for messages that tell the user something is missing.
        /// </summary>
        /// <param name="name">The setting's <see cref="ConsoleSetting.Name" />.</param>
        /// <returns>The ways to supply it, e.g. "env VOICELIVE_ENDPOINT | secret VoiceLiveAPI:AzureEndpoint".</returns>
        public static string DescribeSources(string name)
        {
            ConsoleSetting setting = Find(name);
            var sources = new List<string>();

            if (setting.Argument != null)
            {
                sources.Add(setting.IsFlag ? setting.Argument : $"{setting.Argument} <value>");
            }

            if (setting.EnvironmentVariable != null)
            {
                sources.Add($"env {setting.EnvironmentVariable}");
            }

            if (setting.SecretKey != null)
            {
                sources.Add($"secret {setting.SecretKey}");
            }

            return string.Join(" | ", sources);
        }

        /// <summary>
        ///     Reports whether the command line asked for the settings listing (<c>--help</c> / <c>-h</c>).
        /// </summary>
        /// <returns><see langword="true" /> when help was requested.</returns>
        public static bool HelpRequested()
        {
            return arguments.ContainsKey("--help") || arguments.ContainsKey("-h");
        }

        /// <summary>
        ///     Prints every setting grouped by category, with the ways it can be supplied and its current value.
        /// </summary>
        public static void PrintHelp()
        {
            Console.WriteLine("Azure VoiceLive SDK Console — settings");
            Console.WriteLine();
            Console.WriteLine("  Resolved as: default -> user secrets -> environment variable -> argument.");
            Console.WriteLine("  Flags accept 1/true/yes/on, or the bare switch on the command line.");

            foreach (SettingCategory category in new[]
                     {
                         SettingCategory.Connection, SettingCategory.FeatureInput, SettingCategory.Diagnostic
                     })
            {
                Console.WriteLine();
                Console.WriteLine(Describe(category));

                foreach (ConsoleSetting setting in All.Where(s => s.Category == category))
                {
                    Console.WriteLine($"  {setting.Name}");
                    Console.WriteLine($"      {setting.Description}");
                    Console.WriteLine($"      set with: {DescribeSources(setting.Name)}");

                    string? current = Get(setting.Name);
                    if (!string.IsNullOrEmpty(current))
                    {
                        Console.WriteLine($"      current : {Redact(setting, current)}");
                    }
                }
            }

            Console.WriteLine();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Looks a setting up by name.
        /// </summary>
        /// <param name="name">The setting's name.</param>
        /// <returns>The setting.</returns>
        /// <exception cref="ArgumentException">The name is not in the catalog.</exception>
        private static ConsoleSetting Find(string name)
        {
            return All.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase))
                   ?? throw new ArgumentException($"Unknown setting '{name}'.", nameof(name));
        }

        /// <summary>
        ///     Parses <c>--name value</c>, <c>--name=value</c> and bare <c>--flag</c> forms.
        /// </summary>
        /// <param name="args">The raw command-line arguments.</param>
        /// <returns>The switches and their values (a bare flag maps to "true").</returns>
        private static IReadOnlyDictionary<string, string> ParseArguments(string[] args)
        {
            var parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (!arg.StartsWith("-", StringComparison.Ordinal))
                {
                    continue;
                }

                int equals = arg.IndexOf('=');
                if (equals > 0)
                {
                    parsed[arg.Substring(0, equals)] = arg.Substring(equals + 1);
                    continue;
                }

                // A switch takes the next token as its value unless that token is itself a switch, which is
                // what makes bare flags (--otel) work without a placeholder value. A negative number is a
                // value, not a switch, so that --scene-position-y -0.1 works.
                bool hasValue = i + 1 < args.Length
                                && (!args[i + 1].StartsWith("-", StringComparison.Ordinal)
                                    || float.TryParse(args[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out _));

                parsed[arg] = hasValue ? args[++i] : "true";
            }

            return parsed;
        }

        /// <summary>
        ///     Returns <paramref name="candidate" /> when it holds a value, otherwise keeps the current one.
        /// </summary>
        /// <param name="candidate">The value from a higher-priority source.</param>
        /// <param name="current">The value resolved so far.</param>
        /// <returns>The value to carry forward.</returns>
        private static string? Coalesce(string? candidate, string? current)
        {
            return string.IsNullOrWhiteSpace(candidate) ? current : candidate;
        }

        /// <summary>
        ///     Masks values that should not be printed in full.
        /// </summary>
        /// <param name="setting">The setting being printed.</param>
        /// <param name="value">Its current value.</param>
        /// <returns>The value, redacted when it is a credential.</returns>
        private static string Redact(ConsoleSetting setting, string value)
        {
            return setting.Name.IndexOf("Key", StringComparison.OrdinalIgnoreCase) >= 0 ? "***" : value;
        }

        /// <summary>
        ///     Returns the heading for a category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>The heading line.</returns>
        private static string Describe(SettingCategory category)
        {
            switch (category)
            {
                case SettingCategory.Connection:
                    return "Connection and credentials (fixed per environment — secrets, or environment variables):";
                case SettingCategory.FeatureInput:
                    return "Feature inputs (change per run — secrets, or command-line arguments):";
                default:
                    return "Diagnostics (single run — command-line arguments):";
            }
        }

        #endregion
    }
}
