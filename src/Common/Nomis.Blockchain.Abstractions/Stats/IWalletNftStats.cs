﻿// ------------------------------------------------------------------------------------------------------
// <copyright file="IWalletNftStats.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Nomis.Utils.Contracts.Stats;
using Nomis.Utils.Enums;

namespace Nomis.Blockchain.Abstractions.Stats
{
    /// <summary>
    /// Wallet NFT stats.
    /// </summary>
    public interface IWalletNftStats :
        IWalletStats
    {
        /// <summary>
        /// Set wallet NFT stats.
        /// </summary>
        /// <typeparam name="TWalletStats">The wallet stats type.</typeparam>
        /// <param name="stats">The wallet stats.</param>
        public new void FillStatsTo<TWalletStats>(TWalletStats stats)
            where TWalletStats : class, IWalletNftStats
        {
            stats.NftHolding = NftHolding;
            stats.NftTrading = NftTrading;
            stats.NftWorth = NftWorth;
        }

        /// <summary>
        /// Total NFTs on wallet (number).
        /// </summary>
        public int NftHolding { get; set; }

        /// <summary>
        /// NFT trading activity (Native token).
        /// </summary>
        public decimal NftTrading { get; set; }

        /// <summary>
        /// NFT worth on wallet (Native token).
        /// </summary>
        public decimal NftWorth { get; set; }

        /// <summary>
        /// Calculate wallet NFT stats score.
        /// </summary>
        /// <param name="chainId">Blockchain id.</param>
        /// <param name="calculationModel">Scoring calculation model.</param>
        /// <returns>Returns wallet NFT stats score.</returns>
        public new double CalculateScore(
            ulong chainId,
            ScoringCalculationModel calculationModel)
        {
            double result = 0.0;
            double nft = 0.0;
            nft += NftHoldingScore(chainId, NftHolding, calculationModel) / 100 * NftHoldingPercents(chainId, calculationModel);
            nft += NftTradingScore(chainId, NftTrading, calculationModel) / 100 * NftTradingPercents(chainId, calculationModel);
            nft += NftWorthScore(chainId, NftWorth, calculationModel) / 100 * NftWorthPercents(chainId, calculationModel);
            result += nft * NftPercents(chainId, calculationModel);

            return result;
        }

        private static double NftPercents(
            ulong chainId,
            ScoringCalculationModel calculationModel)
        {
            switch (calculationModel)
            {
                case ScoringCalculationModel.Symbiosis:
                    return 2.06 / 100;
                case ScoringCalculationModel.XDEFI:
                    return 7.95 / 100;
                case ScoringCalculationModel.Halo:
                    return 15.93 / 100;
                case ScoringCalculationModel.CommonV2:
                case ScoringCalculationModel.CommonV3:
                    return 6.65 / 100;
                case ScoringCalculationModel.Rubic:
                    return 2.28 / 100;
                case ScoringCalculationModel.HederaSybilPrevention:
                    return 3.92 / 100;
                case ScoringCalculationModel.HederaDeFi:
                    return 4.88 / 100;
                case ScoringCalculationModel.HederaNFT:
                    return 30.15 / 100;
                case ScoringCalculationModel.HederaReputation:
                    return 12.5 / 100;
                case ScoringCalculationModel.ZkSyncEra:
                    return 6.65 / 100;
                case ScoringCalculationModel.LayerZero:
                    return 1.98 / 100;
                case ScoringCalculationModel.CommonV1:
                default:
                    return 2.15 / 100;
            }
        }

        private static double NftHoldingPercents(
            ulong chainId,
            ScoringCalculationModel calculationModel)
        {
            switch (calculationModel)
            {
                case ScoringCalculationModel.Symbiosis:
                case ScoringCalculationModel.XDEFI:
                case ScoringCalculationModel.Halo:
                case ScoringCalculationModel.CommonV2:
                case ScoringCalculationModel.CommonV3:
                case ScoringCalculationModel.Rubic:
                    return 45.72 / 100;
                case ScoringCalculationModel.HederaSybilPrevention:
                case ScoringCalculationModel.HederaDeFi:
                case ScoringCalculationModel.HederaNFT:
                case ScoringCalculationModel.HederaReputation:
                case ScoringCalculationModel.ZkSyncEra:
                    return 53.96 / 100;
                case ScoringCalculationModel.LayerZero:
                    return 100.0 / 100;
                case ScoringCalculationModel.CommonV1:
                default:
                    return 6.52 / 100;
            }
        }

        private static double NftTradingPercents(
            ulong chainId,
            ScoringCalculationModel calculationModel)
        {
            switch (calculationModel)
            {
                case ScoringCalculationModel.Symbiosis:
                case ScoringCalculationModel.XDEFI:
                case ScoringCalculationModel.Halo:
                case ScoringCalculationModel.CommonV2:
                case ScoringCalculationModel.CommonV3:
                case ScoringCalculationModel.Rubic:
                    return 22.33 / 100;
                case ScoringCalculationModel.HederaSybilPrevention:
                case ScoringCalculationModel.HederaDeFi:
                case ScoringCalculationModel.HederaNFT:
                case ScoringCalculationModel.HederaReputation:
                case ScoringCalculationModel.ZkSyncEra:
                    return 16.34 / 100;
                case ScoringCalculationModel.LayerZero:
                    return 0;
                case ScoringCalculationModel.CommonV1:
                default:
                    return 16.38 / 100;
            }
        }

        private static double NftWorthPercents(
            ulong chainId,
            ScoringCalculationModel calculationModel)
        {
            switch (calculationModel)
            {
                case ScoringCalculationModel.Symbiosis:
                case ScoringCalculationModel.XDEFI:
                case ScoringCalculationModel.Halo:
                case ScoringCalculationModel.CommonV2:
                case ScoringCalculationModel.CommonV3:
                case ScoringCalculationModel.Rubic:
                    return 31.95 / 100;
                case ScoringCalculationModel.HederaSybilPrevention:
                case ScoringCalculationModel.HederaDeFi:
                case ScoringCalculationModel.HederaNFT:
                case ScoringCalculationModel.HederaReputation:
                case ScoringCalculationModel.ZkSyncEra:
                    return 29.7 / 100;
                case ScoringCalculationModel.LayerZero:
                    return 0;
                case ScoringCalculationModel.CommonV1:
                default:
                    return 23.75 / 100;
            }
        }

        private static double NftHoldingScore(
            ulong chainId,
            int value,
            ScoringCalculationModel calculationModel)
        {
            switch (calculationModel)
            {
                case ScoringCalculationModel.Symbiosis:
                case ScoringCalculationModel.XDEFI:
                case ScoringCalculationModel.Halo:
                case ScoringCalculationModel.CommonV2:
                case ScoringCalculationModel.CommonV3:
                case ScoringCalculationModel.Rubic:
                case ScoringCalculationModel.HederaSybilPrevention:
                case ScoringCalculationModel.HederaDeFi:
                case ScoringCalculationModel.HederaNFT:
                case ScoringCalculationModel.HederaReputation:
                case ScoringCalculationModel.ZkSyncEra:
                    return value switch
                    {
                        <= 2 => 11.89,
                        <= 5 => 38.22,
                        <= 10 => 88.01,
                        _ => 100
                    };
                case ScoringCalculationModel.LayerZero:
                    return value switch
                    {
                        <= 25 => 11.89,
                        <= 50 => 38.22,
                        <= 100 => 88.01,
                        _ => 100
                    };
                case ScoringCalculationModel.CommonV1:
                default:
                    return value switch
                    {
                        <= 10 => 7.91,
                        <= 100 => 22.36,
                        <= 500 => 70.71,
                        _ => 100
                    };
            }
        }

        private static double NftTradingScore(
            ulong chainId,
            decimal value,
            ScoringCalculationModel calculationModel)
        {
            switch (calculationModel)
            {
                case ScoringCalculationModel.Symbiosis:
                case ScoringCalculationModel.XDEFI:
                case ScoringCalculationModel.Halo:
                case ScoringCalculationModel.CommonV2:
                case ScoringCalculationModel.CommonV3:
                case ScoringCalculationModel.Rubic:
                case ScoringCalculationModel.HederaSybilPrevention:
                case ScoringCalculationModel.HederaDeFi:
                case ScoringCalculationModel.HederaNFT:
                case ScoringCalculationModel.HederaReputation:
                case ScoringCalculationModel.ZkSyncEra:
                    return value switch
                    {
                        <= 2 => 11.89,
                        <= 5 => 38.22,
                        <= 10 => 88.01,
                        _ => 100
                    };
                case ScoringCalculationModel.LayerZero:
                    return 0;
                case ScoringCalculationModel.CommonV1:
                default:
                    return value switch
                    {
                        <= 1 => 4.59,
                        <= 10 => 10.62,
                        <= 50 => 24.49,
                        <= 100 => 55.38,
                        _ => 100
                    };
            }
        }

        private static double NftWorthScore(
            ulong chainId,
            decimal value,
            ScoringCalculationModel calculationModel)
        {
            switch (calculationModel)
            {
                case ScoringCalculationModel.Symbiosis:
                case ScoringCalculationModel.XDEFI:
                case ScoringCalculationModel.Halo:
                case ScoringCalculationModel.CommonV2:
                case ScoringCalculationModel.CommonV3:
                case ScoringCalculationModel.Rubic:
                case ScoringCalculationModel.HederaSybilPrevention:
                case ScoringCalculationModel.HederaDeFi:
                case ScoringCalculationModel.HederaNFT:
                case ScoringCalculationModel.HederaReputation:
                case ScoringCalculationModel.ZkSyncEra:
                    return value switch
                    {
                        <= 0.5M => 14.7,
                        <= 1 => 21.44,
                        <= 2 => 40.66,
                        <= 5 => 75.79,
                        _ => 100
                    };
                case ScoringCalculationModel.LayerZero:
                    return 0;
                case ScoringCalculationModel.CommonV1:
                default:
                    return value switch
                    {
                        <= 1 => 4.59,
                        <= 10 => 10.62,
                        <= 50 => 24.49,
                        <= 100 => 55.38,
                        _ => 100
                    };
            }
        }
    }
}