// ------------------------------------------------------------------------------------------------------
// <copyright file="OpBnbBscscanSettings.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Nomis.Blockchain.Abstractions.Contracts;
using Nomis.Blockchain.Abstractions.Contracts.Settings;
using Nomis.Blockchain.Abstractions.Enums;
using Nomis.Utils.Enums;

namespace Nomis.OpBnbBscscan.Settings
{
    /// <summary>
    /// OpBnb bscscan settings.
    /// </summary>
    internal class OpBnbBscscanSettings :
        IBlockchainSettings,
        IRateLimitSettings,
        IGetFromCacheStatsSettings,
        IHttpClientLoggingSettings,
        IUseHistoricalMedianBalanceUSDSettings
    {
        /// <inheritdoc />
        public int? ItemsFetchLimitPerRequest { get; init; }

        /// <summary>
        /// API keys for OpBnbBscscan.
        /// </summary>
        public IList<string> ApiKeys { get; init; } = new List<string>();

        /// <summary>
        /// OpBnbBscscan API base URL.
        /// </summary>
        /// <remarks>
        /// <see href="https://opbnb-testnet.bscscan.com/apis"/>
        /// </remarks>
        public string? ApiBaseUrl { get; init; }

        /// <inheritdoc/>
        public uint MaxApiCallsPerSecond { get; init; }

        /// <inheritdoc />
        public IDictionary<BlockchainKind, BlockchainDescriptor> BlockchainDescriptors { get; init; } = new Dictionary<BlockchainKind, BlockchainDescriptor>();

        /// <inheritdoc/>
        public bool GetFromCacheStatsIsEnabled { get; init; }

        /// <inheritdoc/>
        public TimeSpan GetFromCacheStatsTimeLimit { get; init; }

        /// <inheritdoc/>
        public bool UseHttpClientLogging { get; init; }

        /// <inheritdoc/>
        public IDictionary<ScoringCalculationModel, bool> UseHistoricalMedianBalanceUSD { get; init; } = new Dictionary<ScoringCalculationModel, bool>();

        /// <inheritdoc/>
        public decimal MedianBalancePrecision { get; init; }

        /// <inheritdoc/>
        public TimeSpan? MedianBalanceStartFrom { get; init; }

        /// <inheritdoc/>
        public int? MedianBalanceLastCount { get; init; }
    }
}