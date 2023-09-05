// ------------------------------------------------------------------------------------------------------
// <copyright file="IWalletCommonStats.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using System.Text.Json.Serialization;

using Nomis.Utils.Enums;

namespace Nomis.Utils.Contracts.Stats
{
    /// <summary>
    /// Wallet common stats.
    /// </summary>
    public interface IWalletCommonStats<TTransactionIntervalData> :
        IWalletStats
        where TTransactionIntervalData : class, ITransactionIntervalData
    {
        /// <summary>
        /// Set wallet common stats.
        /// </summary>
        /// <typeparam name="TWalletStats">The wallet stats type.</typeparam>
        /// <param name="stats">The wallet stats.</param>
        public new void FillStatsTo<TWalletStats>(TWalletStats stats)
            where TWalletStats : class, IWalletCommonStats<TTransactionIntervalData>
        {
            stats.WalletAge = WalletAge;
            stats.TurnoverIntervals = TurnoverIntervals;
            stats.NoData = NoData;
        }

        /// <summary>
        /// Additional wallet stats scoring calculation functions.
        /// </summary>
        [JsonIgnore]
        IEnumerable<Func<ulong, ScoringCalculationModel, double>> AdditionalScores => new List<Func<ulong, ScoringCalculationModel, double>>();

        /// <summary>
        /// Wallet stats adjusting score multiplier calculation functions.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Func<ulong, ScoringCalculationModel, double>> AdjustingScoreMultipliers => new List<Func<ulong, ScoringCalculationModel, double>>();

        /// <summary>
        /// The list of properties excluded from <see cref="StatsDescriptions"/>.
        /// </summary>
        [JsonIgnore]
        static IEnumerable<string> ExcludedStatDescriptions => new List<string>
        {
            nameof(AdditionalScores),
            nameof(AdjustingScoreMultipliers),
            nameof(NoData),
            nameof(TurnoverIntervals),
            nameof(StatsDescriptions),
            nameof(ExcludedStatDescriptions)
        };

        /// <summary>
        /// Wallet age (months).
        /// </summary>
        public int WalletAge { get; set; }

        /// <summary>
        /// The intervals of funds movements on the wallet.
        /// </summary>
        public IEnumerable<TTransactionIntervalData>? TurnoverIntervals { get; set; }

        /// <summary>
        /// Wallet stats descriptions.
        /// </summary>
        public IDictionary<string, PropertyData> StatsDescriptions { get; }

        /// <summary>
        /// Calculate wallet common stats score.
        /// </summary>
        /// <param name="chainId">Blockchain id.</param>
        /// <param name="calculationModel">Scoring calculation model.</param>
        /// <returns>Returns wallet common stats score.</returns>
        public new double CalculateScore(
            ulong chainId,
            ScoringCalculationModel calculationModel)
        {
            double result = WalletAgeScore(chainId, WalletAge, calculationModel) / 100 * WalletAgePercents(chainId, calculationModel);

            return result;
        }

        private static double WalletAgePercents(
            ulong chainId,
            ScoringCalculationModel calculationModel)
        {
            switch (calculationModel)
            {
                case ScoringCalculationModel.Symbiosis:
                    return 20.1 / 100;
                case ScoringCalculationModel.XDEFI:
                    return 19.48 / 100;
                case ScoringCalculationModel.Halo:
                    return 24.37 / 100;
                case ScoringCalculationModel.Rubic:
                    return 26.62 / 100;
                case ScoringCalculationModel.HederaSybilPrevention:
                    return 26.02 / 100;
                case ScoringCalculationModel.HederaDeFi:
                    return 17.02 / 100;
                case ScoringCalculationModel.HederaNFT:
                    return 16.71 / 100;
                case ScoringCalculationModel.HederaReputation:
                    return 12.5 / 100;
                case ScoringCalculationModel.ZkSyncEra:
                case ScoringCalculationModel.CommonV3:
                    return 30.97 / 100;
                case ScoringCalculationModel.LayerZero:
                    return 34.39 / 100;
                case ScoringCalculationModel.CommonV1:
                default:
                    return 32.34 / 100;
            }
        }

        private static double WalletAgeScore(
            ulong chainId,
            int walletAgeMonths,
            ScoringCalculationModel calculationModel)
        {
            switch (calculationModel)
            {
                case ScoringCalculationModel.Symbiosis:
                case ScoringCalculationModel.XDEFI:
                case ScoringCalculationModel.Halo:
                    return walletAgeMonths switch
                    {
                        0 => 6.86,
                        < 1 => 18.59,
                        < 12 => 57.43,
                        < 24 => 75.79,
                        _ => 100
                    };
                case ScoringCalculationModel.Rubic:
                case ScoringCalculationModel.HederaSybilPrevention:
                case ScoringCalculationModel.HederaDeFi:
                case ScoringCalculationModel.HederaNFT:
                case ScoringCalculationModel.HederaReputation:
                    return walletAgeMonths switch
                    {
                        0 => 6.1,
                        < 1 => 20.91,
                        < 12 => 57.43,
                        < 24 => 75.79,
                        _ => 100
                    };
                case ScoringCalculationModel.ZkSyncEra:
                case ScoringCalculationModel.CommonV3:
                    return walletAgeMonths switch
                    {
                        0 => 7.6,
                        < 1 => 14.93,
                        < 6 => 64.6,
                        < 12 => 75.79,
                        _ => 100
                    };
                case ScoringCalculationModel.LayerZero:
                    return walletAgeMonths switch
                    {
                        0 => 15.48,
                        < 1 => 32.64,
                        < 6 => 75.79,
                        < 12 => 87.06,
                        _ => 100
                    };
                case ScoringCalculationModel.CommonV1:
                default:
                    return walletAgeMonths switch
                    {
                        < 1 => 7.14,
                        < 12 => 21.36,
                        < 24 => 54.6,
                        _ => 100
                    };
            }
        }
    }
}