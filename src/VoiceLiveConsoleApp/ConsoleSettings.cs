// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Com.Reseul.Azure.AI.VoiceLiveAPI.Core;
using Microsoft.Extensions.Configuration;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
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
        ///     How the console itself behaves while it runs (logging, extra lookups). Only ever wanted for a
        ///     single run, so it is a command-line argument.
        /// </summary>
        Diagnostic
    }

    /// <summary>
    ///     One configurable value and every way it can be supplied. Keeping the sources on the setting itself
    ///     means a new setting is one entry here rather than a read scattered into the code that happens to
    ///     need it — and it lets <see cref="ConsoleSettings.PrintHelp" /> list them all without a second list
    ///     that could fall out of date.
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
    ///     The console's settings: one catalog of every configurable value, and one place that resolves them.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Values are resolved <b>default → user secrets → environment variable → command-line argument</b>,
    ///         so the narrower the scope, the higher it wins. Every setting can be put in user secrets; what
    ///         differs is the override that suits it, and that follows from
    ///         <see cref="SettingCategory" />: connection details are fixed per environment and so take an
    ///         environment variable, while feature inputs and diagnostics change from run to run and so take a
    ///         command-line argument.
    ///     </para>
    ///     <para>
    ///         Booleans accept <c>1</c>, <c>true</c>, <c>yes</c> and <c>on</c> (case-insensitive) from every
    ///         source. They used to be parsed at each call site, where some accepted only <c>1</c> and others
    ///         also <c>true</c> — a difference that failed silently.
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
                "Model to run a model session against.",
                secretKey: "VoiceLiveAPI:Model", environmentVariable: "VOICELIVE_MODEL",
                defaultValue: VoiceLiveSessionOptions.DefaultModel),

            new ConsoleSetting("AgentName", SettingCategory.Connection,
                "Foundry agent to talk to in AI Agent mode.",
                secretKey: "AzureAIFoundry:AgentName", environmentVariable: "VOICELIVE_AGENT_NAME"),

            new ConsoleSetting("AgentProjectName", SettingCategory.Connection,
                "Foundry project that hosts the agent.",
                secretKey: "AzureAIFoundry:AgentProjectName", environmentVariable: "VOICELIVE_AGENT_PROJECT"),

            new ConsoleSetting("AgentId", SettingCategory.Connection,
                "Classic agent id (agent-name supersedes it; retired 2026-08-31).",
                secretKey: "AzureAIFoundry:AgentId"),

            new ConsoleSetting("AgentAccessToken", SettingCategory.Connection,
                "Token for the classic agent connection.",
                secretKey: "AzureAIFoundry:AgentAccessToken"),

            new ConsoleSetting("IdentityEndpoint", SettingCategory.Connection,
                "Scope used when requesting an Entra ID token.",
                secretKey: "Identity:AzureEndpoint"),

            new ConsoleSetting("FoundryProjectEndpoint", SettingCategory.Connection,
                "Project endpoint used to resolve which model answered (Responses API).",
                secretKey: "AzureAIFoundry:ProjectEndpoint",
                environmentVariable: "FOUNDRY_PROJECT_ENDPOINT"),

            new ConsoleSetting("ApiVersion", SettingCategory.Connection,
                "Wire API version to open the session with.",
                secretKey: "VoiceLiveAPI:ApiVersion", environmentVariable: "VOICELIVE_API_VERSION"),

            new ConsoleSetting("WebRtcApiVersion", SettingCategory.Connection,
                "API version for the WebRTC /calls endpoint (pinned; /calls answers 401 on newer versions).",
                secretKey: "VoiceLiveAPI:WebRtcApiVersion",
                environmentVariable: "VOICELIVE_WEBRTC_API_VERSION"),

            // ---- Feature inputs (user secrets, overridden by command-line arguments) ----
            new ConsoleSetting("Voice", SettingCategory.FeatureInput,
                "Azure voice name for spoken output.",
                secretKey: "VoiceLiveAPI:Voice", environmentVariable: "VOICELIVE_VOICE",
                argument: "--voice"),

            new ConsoleSetting("PersonalVoice", SettingCategory.FeatureInput,
                "Personal voice speaker profile ID (the GUID from the portal page URL).",
                secretKey: "VoiceLiveAPI:PersonalVoice", environmentVariable: "VOICELIVE_PERSONAL_VOICE",
                argument: "--personal-voice"),

            new ConsoleSetting("PersonalVoiceModel", SettingCategory.FeatureInput,
                "Base model behind the personal voice.",
                secretKey: "VoiceLiveAPI:PersonalVoiceModel",
                environmentVariable: "VOICELIVE_PERSONAL_VOICE_MODEL",
                argument: "--personal-voice-model", defaultValue: "DragonLatestNeural"),

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

            new ConsoleSetting("AvatarBackend", SettingCategory.FeatureInput,
                "What drives an avatar session: 'agent' or 'model'.",
                secretKey: "VoiceLiveAPI:AvatarBackend", environmentVariable: "VOICELIVE_AVATAR_BACKEND",
                argument: "--avatar-backend", defaultValue: "agent"),

            new ConsoleSetting("McpUrl", SettingCategory.FeatureInput,
                "MCP server to attach to the session.",
                secretKey: "VoiceLiveAPI:McpUrl", environmentVariable: "VOICELIVE_MCP_URL",
                argument: "--mcp-url", defaultValue: "https://mcp.deepwiki.com/mcp"),

            new ConsoleSetting("McpLabel", SettingCategory.FeatureInput,
                "Label the model sees for the MCP server.",
                secretKey: "VoiceLiveAPI:McpLabel", environmentVariable: "VOICELIVE_MCP_LABEL",
                argument: "--mcp-label", defaultValue: "deepwiki"),

            new ConsoleSetting("Greeting", SettingCategory.FeatureInput,
                "What the assistant says first when the proactive greeting feature is on.",
                secretKey: "VoiceLiveAPI:Greeting", environmentVariable: "VOICELIVE_GREETING",
                argument: "--greeting"),

            new ConsoleSetting("AgentToolDescription", SettingCategory.FeatureInput,
                "Description given to the model for the foundry_agent tool (when to delegate).",
                secretKey: "VoiceLiveAPI:AgentToolDescription",
                environmentVariable: "VOICELIVE_AGENT_TOOL_DESCRIPTION",
                argument: "--agent-tool-description"),

            // ---- Diagnostics (command-line arguments) ----
            new ConsoleSetting("WireDebug", SettingCategory.Diagnostic,
                "Log every wire message, including the outgoing session.update JSON.",
                secretKey: "VoiceLiveAPI:WireDebug", environmentVariable: "VOICELIVE_WIRE_DEBUG",
                argument: "--wire-debug", isFlag: true),

            new ConsoleSetting("LogLevel", SettingCategory.Diagnostic,
                "Minimum log level (Trace/Debug/Information/Warning/Error). Default Error keeps output readable.",
                secretKey: "VoiceLiveAPI:LogLevel", environmentVariable: "VOICELIVE_LOG_LEVEL",
                argument: "--log-level"),

            new ConsoleSetting("ResolveAgentModel", SettingCategory.Diagnostic,
                "After each agent turn, look up which model actually answered (needs FoundryProjectEndpoint).",
                secretKey: "VoiceLiveAPI:ResolveAgentModel",
                environmentVariable: "VOICELIVE_RESOLVE_AGENT_MODEL",
                argument: "--resolve-agent-model", isFlag: true),

            new ConsoleSetting("StreamingText", SettingCategory.Diagnostic,
                "Send typed text as input_text.delta/.done instead of conversation.item.create. "
                + "Kept for re-testing: the delta events are rejected by the service today.",
                secretKey: "VoiceLiveAPI:StreamingText", environmentVariable: "VOICELIVE_STREAMING_TEXT",
                argument: "--streaming-text", isFlag: true)
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

            return value;
        }

        /// <summary>
        ///     Resolves a setting, falling back when no source supplies a non-empty value.
        /// </summary>
        /// <param name="name">The setting's <see cref="ConsoleSetting.Name" />.</param>
        /// <param name="fallback">The value to use instead.</param>
        /// <returns>The resolved value, or <paramref name="fallback" />.</returns>
        public static string GetOr(string name, string fallback)
        {
            string? value = Get(name);
            return string.IsNullOrWhiteSpace(value) ? fallback : value!;
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
            return !string.IsNullOrWhiteSpace(value)
                   && TruthyValues.Contains(value!.Trim(), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Describes how a setting can be supplied, for messages that tell the user something is missing.
        ///     Naming the setting alone is not actionable — what they need is the switch or variable to set.
        /// </summary>
        /// <param name="name">The setting's <see cref="ConsoleSetting.Name" />.</param>
        /// <returns>The ways to supply it, e.g. "env FOUNDRY_PROJECT_ENDPOINT | secret AzureAIFoundry:ProjectEndpoint".</returns>
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
        ///     Prints every setting grouped by category, with the ways it can be supplied and where its value
        ///     is currently coming from.
        /// </summary>
        public static void PrintHelp()
        {
            Console.WriteLine("Azure VoiceLive API Console — settings");
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
                    if (!string.IsNullOrWhiteSpace(current))
                    {
                        Console.WriteLine($"      current : {Redact(setting, current!)}");
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
            ConsoleSetting? setting = All.FirstOrDefault(
                s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

            if (setting == null)
            {
                throw new ArgumentException($"Unknown setting '{name}'.", nameof(name));
            }

            return setting;
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
                // what makes bare flags (--wire-debug) work without a placeholder value.
                bool hasValue = i + 1 < args.Length
                                && !args[i + 1].StartsWith("-", StringComparison.Ordinal);

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
            bool secretish = setting.Name.IndexOf("Key", StringComparison.OrdinalIgnoreCase) >= 0
                             || setting.Name.IndexOf("Token", StringComparison.OrdinalIgnoreCase) >= 0;

            return secretish ? "***" : value;
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
