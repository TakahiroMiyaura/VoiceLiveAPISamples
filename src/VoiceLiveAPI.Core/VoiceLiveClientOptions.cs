// Copyright (c) 2026 Takahiro Miyaura
// Released under the Boost Software License 1.0
// https://opensource.org/license/bsl-1-0

using System;
using System.Collections.Generic;

namespace Com.Reseul.Azure.AI.VoiceLiveAPI.Core
{
    /// <summary>
    ///     Represents the options for configuring a <see cref="VoiceLiveClient" />.
    /// </summary>
    /// <remarks>
    ///     This class provides configuration options for the VoiceLive client,
    ///     including API version and retry policy settings.
    /// </remarks>
    public class VoiceLiveClientOptions
    {
        #region Static Fields and Constants

        /// <summary>
        ///     The default API version used when not specified.
        /// </summary>
        /// <remarks>
        ///     Set to <c>2026-01-01-preview</c> to match the self-hosted MCP support (the MCP tool
        ///     configuration requires API version 2025-10-01 or later). The previous default
        ///     (<c>2025-05-01-preview</c>) predated that requirement and was inconsistent with the
        ///     built-in MCP feature.
        /// </remarks>
        public const string DefaultApiVersion = "2026-01-01-preview";

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the API version to use for requests.
        /// </summary>
        /// <value>
        ///     The API version string. Defaults to <see cref="DefaultApiVersion" />.
        /// </value>
        public string ApiVersion { get; set; } = DefaultApiVersion;

        /// <summary>
        ///     Gets or sets the maximum number of retry attempts for failed requests.
        /// </summary>
        /// <value>
        ///     The maximum number of retries. Defaults to 3.
        /// </value>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        ///     Gets or sets the timeout duration for WebSocket operations.
        /// </summary>
        /// <value>
        ///     The timeout as a <see cref="TimeSpan" />. Defaults to 30 seconds.
        /// </value>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     Gets or sets a value indicating whether to enable diagnostic logging.
        /// </summary>
        /// <value>
        ///     <c>true</c> to enable diagnostics; otherwise, <c>false</c>. Defaults to <c>false</c>.
        /// </value>
        public bool EnableDiagnostics { get; set; } = false;

        /// <summary>
        ///     Gets or sets preview feature flags appended to the realtime WebSocket URL as
        ///     <c>&amp;features=name:value,name:value</c>. Used to opt into gated preview capabilities such as
        ///     <c>client_ec_reference:true</c> (client-side echo cancellation reference). Empty or null adds
        ///     nothing.
        /// </summary>
        public IReadOnlyList<string> Features { get; set; } = null;

        #endregion
    }
}