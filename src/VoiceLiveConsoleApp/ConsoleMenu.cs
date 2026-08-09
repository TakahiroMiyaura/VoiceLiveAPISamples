// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;

namespace Com.Reseul.Azure.AI.Samples.VoiceLiveAPI
{
    /// <summary>
    ///     Everything the startup prompts decided. Returning the answers instead of writing them into the
    ///     caller's fields keeps the prompts free of the session state they used to reach into, and makes what
    ///     a run is configured with readable in one place.
    /// </summary>
    public sealed class MenuSelection
    {
        #region Properties

        /// <summary>Gets or sets the mode the session runs in.</summary>
        public ConnectionMode Mode { get; set; }

        /// <summary>Gets or sets the wire API version to open the session with.</summary>
        public string ApiVersion { get; set; } = string.Empty;

        /// <summary>Gets or sets the preview feature to exercise, when one was chosen.</summary>
        public PreviewFeatureCheck? Feature { get; set; }

        /// <summary>Gets or sets what drives an avatar session: <c>agent</c> or <c>model</c>.</summary>
        public string AvatarBackend { get; set; } = "agent";

        /// <summary>Gets or sets a value indicating whether avatar video arrives over the WebSocket.</summary>
        public bool AvatarUseWebSocketVideo { get; set; }

        /// <summary>Gets or sets a value indicating whether the avatar is a photo avatar.</summary>
        public bool AvatarUsePhoto { get; set; }

        #endregion
    }

    /// <summary>
    ///     The startup prompts: what to run, on which API version, and how to authenticate.
    /// </summary>
    public static class ConsoleMenu
    {
        #region Public Methods

        /// <summary>
        ///     Runs the startup prompts and reports what was chosen.
        /// </summary>
        /// <param name="supportedApiVersions">The wire versions on offer.</param>
        /// <param name="defaultApiVersion">The version pre-selected in the prompt.</param>
        /// <param name="defaultAvatarBackend">The avatar backend pre-selected in the prompt.</param>
        /// <param name="pinnedWebRtcApiVersion">The version WebRTC voice is pinned to.</param>
        /// <returns>The selection.</returns>
        public static MenuSelection Choose(string[] supportedApiVersions, string defaultApiVersion,
            string defaultAvatarBackend, string pinnedWebRtcApiVersion)
        {
            var selection = new MenuSelection
            {
                ApiVersion = defaultApiVersion,
                AvatarBackend = defaultAvatarBackend
            };

            // The split is what a user actually decides between: use the stable feature set, or try one of the
            // preview additions. Everything preview-only lives on the second branch — including the ones that
            // are transports rather than settings (WebRTC voice, WebSocket avatar video).
            Console.WriteLine();
            Console.WriteLine("Choose:");
            Console.WriteLine("1. Standard features  (GA — talk to a model, an agent, or an avatar)");
            Console.WriteLine("2. Preview features   (try one addition of a preview API version)");

            switch (Prompt("Enter your choice (1 or 2): ", 2))
            {
                case 1:
                    ChooseStandard(selection);
                    break;
                default:
                    ChoosePreview(selection, supportedApiVersions, pinnedWebRtcApiVersion);
                    break;
            }

            return selection;
        }

        /// <summary>
        ///     Prompts for the authentication method.
        /// </summary>
        /// <returns><see langword="true" /> for an API key, <see langword="false" /> for Entra ID.</returns>
        public static bool ChooseUseApiKey()
        {
            Console.WriteLine("Choose authentication method:");
            Console.WriteLine("1. API Key");
            Console.WriteLine("2. Entra ID (DefaultAzureCredential)");

            return Prompt("Enter your choice (1 or 2): ", 2) == 1;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Prompts for one of the generally available conversation modes. These run on the default wire
        ///     version, so there is no version prompt here.
        /// </summary>
        /// <param name="selection">The selection being built.</param>
        private static void ChooseStandard(MenuSelection selection)
        {
            Console.WriteLine("Choose a standard mode:");
            Console.WriteLine("1. AI Model   (talk to a model)");
            Console.WriteLine("2. AI Agent   (talk to a Foundry agent)");
            Console.WriteLine("3. Avatar     (model or agent, with WebRTC video)");

            switch (Prompt("Enter your choice (1-3): ", 3))
            {
                case 1:
                    Console.WriteLine("Selected: AI Model");
                    selection.Mode = ConnectionMode.AIModel;
                    break;
                case 2:
                    Console.WriteLine("Selected: AI Agent");
                    selection.Mode = ConnectionMode.AIAgent;
                    break;
                default:
                    Console.WriteLine("Selected: Avatar");
                    selection.Mode = ConnectionMode.Avatar;
                    ChooseAvatarBackend(selection);
                    break;
            }
        }

        /// <summary>
        ///     Prompts for the API version (which scopes the list) and then the preview feature to try. Most
        ///     features are session settings on a model session; a few start a session of their own, which
        ///     <see cref="PreviewFeatureCheck.SessionKind" /> decides.
        /// </summary>
        /// <param name="selection">The selection being built.</param>
        /// <param name="supportedApiVersions">The wire versions on offer.</param>
        /// <param name="pinnedWebRtcApiVersion">The version WebRTC voice is pinned to.</param>
        private static void ChoosePreview(MenuSelection selection, string[] supportedApiVersions,
            string pinnedWebRtcApiVersion)
        {
            selection.ApiVersion = ChooseApiVersion(supportedApiVersions, selection.ApiVersion);

            IReadOnlyList<PreviewFeatureCheck> features = PreviewFeatureCatalog.ForVersion(selection.ApiVersion);
            if (features.Count == 0)
            {
                Console.WriteLine($"No preview features are available for {selection.ApiVersion}.");
                Console.WriteLine("Falling back to a standard mode.");
                ChooseStandard(selection);
                return;
            }

            selection.Feature = ChooseFeature(features, selection.ApiVersion);

            switch (selection.Feature.SessionKind)
            {
                case PreviewSessionKind.WebRtcVoice:
                    // The documented /calls samples all use 2026-01-01-preview, and 2026-06-01-preview fails
                    // the handshake with HTTP 401 under either auth method, so pin the wire version regardless
                    // of what was selected above.
                    selection.ApiVersion = pinnedWebRtcApiVersion;
                    Console.WriteLine($"WebRTC voice api-version: {selection.ApiVersion}");
                    selection.Mode = ConnectionMode.WebRtcVoice;
                    break;

                case PreviewSessionKind.AvatarWebSocketVideo:
                    selection.AvatarUseWebSocketVideo = true;
                    selection.Mode = ConnectionMode.Avatar;
                    ChooseAvatarBackend(selection);
                    break;

                case PreviewSessionKind.PhotoAvatar:
                    // Only the avatar config changes; keep WebRTC so a failure here points at the photo avatar
                    // itself rather than at the newer WebSocket video transport.
                    selection.AvatarUsePhoto = true;
                    selection.AvatarUseWebSocketVideo = false;
                    selection.Mode = ConnectionMode.Avatar;
                    ChooseAvatarBackend(selection);
                    break;

                default:
                    selection.Mode = ConnectionMode.FeatureCheck;
                    break;
            }
        }

        /// <summary>
        ///     Prompts for the preview wire version.
        /// </summary>
        /// <param name="supportedApiVersions">The versions on offer.</param>
        /// <param name="current">The version pre-selected in the prompt.</param>
        /// <returns>The chosen version.</returns>
        private static string ChooseApiVersion(string[] supportedApiVersions, string current)
        {
            Console.WriteLine("Choose API version (VoiceLiveConsoleApp targets preview wire versions):");
            for (var i = 0; i < supportedApiVersions.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {supportedApiVersions[i]}");
            }

            int defaultIndex = Array.IndexOf(supportedApiVersions, current);
            if (defaultIndex < 0)
            {
                defaultIndex = supportedApiVersions.Length - 1;
            }

            int choice = Prompt(
                $"Enter your choice (1-{supportedApiVersions.Length}) [default: {defaultIndex + 1}]: ",
                supportedApiVersions.Length, defaultIndex + 1);

            string selected = supportedApiVersions[choice - 1];
            Console.WriteLine($"Selected API version: {selected}");
            return selected;
        }

        /// <summary>
        ///     Prompts for a single preview feature to exercise.
        /// </summary>
        /// <param name="features">The features available at the selected version.</param>
        /// <param name="apiVersion">The selected version, named in the prompt.</param>
        /// <returns>The chosen feature.</returns>
        private static PreviewFeatureCheck ChooseFeature(IReadOnlyList<PreviewFeatureCheck> features,
            string apiVersion)
        {
            Console.WriteLine($"Choose a {apiVersion} feature to check (runs as an AI Model session):");
            for (var i = 0; i < features.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {features[i].Title}");
            }

            int choice = Prompt($"Enter your choice (1-{features.Count}): ", features.Count);

            PreviewFeatureCheck feature = features[choice - 1];
            Console.WriteLine($"Selected feature: {feature.Id}");
            return feature;
        }

        /// <summary>
        ///     Prompts for the session backend used underneath avatar output. Agent (default) manages the
        ///     conversation server-side (Entra ID required); Model runs on a direct model session, which
        ///     enables model-only features such as image input.
        /// </summary>
        /// <param name="selection">The selection being built.</param>
        private static void ChooseAvatarBackend(MenuSelection selection)
        {
            bool defaultIsModel = string.Equals(selection.AvatarBackend, "model",
                StringComparison.OrdinalIgnoreCase);

            Console.WriteLine("Choose Avatar session backend:");
            Console.WriteLine("1. Agent (Foundry agent, Entra ID required)");
            Console.WriteLine("2. Model (direct model session, enables image input)");

            int choice = Prompt($"Enter your choice (1 or 2) [default: {(defaultIsModel ? "2" : "1")}]: ",
                2, defaultIsModel ? 2 : 1);

            selection.AvatarBackend = choice == 2 ? "model" : "agent";
            Console.WriteLine($"Avatar backend: {(choice == 2 ? "Model" : "Agent")}");
        }

        /// <summary>
        ///     Reads a number in <c>1..max</c>, re-prompting until it gets one. Every prompt used to carry its
        ///     own copy of this loop.
        /// </summary>
        /// <param name="prompt">The prompt to print.</param>
        /// <param name="max">The highest accepted choice.</param>
        /// <param name="defaultChoice">What an empty line means, or 0 to reject empty input.</param>
        /// <returns>The chosen number.</returns>
        private static int Prompt(string prompt, int max, int defaultChoice = 0)
        {
            Console.Write(prompt);

            while (true)
            {
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input) && defaultChoice > 0)
                {
                    return defaultChoice;
                }

                if (int.TryParse(input?.Trim(), out int choice) && choice >= 1 && choice <= max)
                {
                    return choice;
                }

                Console.Write($"Invalid choice. Please enter 1-{max}: ");
            }
        }

        #endregion
    }
}
