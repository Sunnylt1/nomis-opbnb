// ------------------------------------------------------------------------------------------------------
// <copyright file="BalanceCheckerSettings.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using System.Net;

using Nomis.BalanceChecker.Contracts;
using Nomis.Blockchain.Abstractions.Contracts.Settings;
using Nomis.Utils.Contracts.Common;
using Nomis.Utils.Contracts.Settings;

namespace Nomis.BalanceChecker.Settings
{
    /// <summary>
    /// Balance checker settings.
    /// </summary>
    internal class BalanceCheckerSettings :
        ISettings,
        IRateLimitSettings,
        IHttpClientRetryingSettings
    {
        /// <summary>
        /// Use DeBank API.
        /// </summary>
        public bool UseDeBankApi { get; init; }

        /// <summary>
        /// DeBank API base URL.
        /// </summary>
        public string DeBankApiBaseUrl { get; init; } = null!;

        /// <summary>
        /// DeBank API access key.
        /// </summary>
        public string DeBankApiAccessKey { get; init; } = null!;

        /// <summary>
        /// DeBank use caching.
        /// </summary>
        public bool DeBankUseCaching { get; init; }

        /// <summary>
        /// Check DeBank units.
        /// </summary>
        public bool CheckDeBankUnits { get; init; }

        /// <summary>
        /// DeBank responses cache duration.
        /// </summary>
        public TimeSpan DeBankResponsesCacheDuration { get; init; }

        /// <summary>
        /// List of Balance checker data feed.
        /// </summary>
        public List<BalanceCheckerDataFeed> DataFeeds { get; init; } = new();

        /// <inheritdoc/>
        public uint MaxApiCallsPerSecond { get; init; }

        /// <summary>
        /// Http client timeout.
        /// </summary>
        public TimeSpan HttpClientTimeout { get; init; }

        #region IHttpClientRetryingSettings

        /// <inheritdoc/>
        public bool UseHttpClientRetrying { get; init; }

        /// <inheritdoc/>
        public int MaxRetries { get; init; }

        /// <inheritdoc/>
        public bool UseDefaultRetryTimeout { get; init; }

        /// <inheritdoc/>
        public TimeSpan DefaultRetryTimeout { get; init; }

        /// <inheritdoc/>
        public IDictionary<HttpStatusCode, TimeSpan> RetryTimeouts { get; init; } = new Dictionary<HttpStatusCode, TimeSpan>();

        #endregion #region IHttpClientRetryingSettings
    }
}