﻿// ------------------------------------------------------------------------------------------------------
// <copyright file="IWalletContractStats.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Nomis.Utils.Contracts.Stats;
using Nomis.Utils.Enums;

namespace Nomis.Blockchain.Abstractions.Stats
{
    /// <summary>
    /// Wallet contract stats.
    /// </summary>
    public interface IWalletContractStats :
        IWalletStats
    {
        /// <summary>
        /// Set wallet contract stats.
        /// </summary>
        /// <typeparam name="TWalletStats">The wallet stats type.</typeparam>
        /// <param name="stats">The wallet stats.</param>
        public new void FillStatsTo<TWalletStats>(TWalletStats stats)
            where TWalletStats : class, IWalletContractStats
        {
            stats.DeployedContracts = DeployedContracts;
        }

        /// <summary>
        /// Amount of deployed smart-contracts.
        /// </summary>
        public int DeployedContracts { get; set; }

        /// <summary>
        /// Calculate wallet contract stats score.
        /// </summary>
        /// <param name="chainId">Blockchain id.</param>
        /// <param name="calculationModel">Scoring calculation model.</param>
        /// <returns>Returns wallet contract stats score.</returns>
        public new double CalculateScore(
            ulong chainId,
            ScoringCalculationModel calculationModel)
        {
            double result = DeployedContractsScore(chainId, DeployedContracts, calculationModel) / 100 * DeployedContractsPercents(chainId, calculationModel);

            return result;
        }

        private static double DeployedContractsPercents(
            ulong chainId,
            ScoringCalculationModel calculationModel)
        {
            switch (calculationModel)
            {
                case ScoringCalculationModel.Symbiosis:
                    return 2.8 / 100;
                case ScoringCalculationModel.XDEFI:
                    return 3.18 / 100;
                case ScoringCalculationModel.Halo:
                    return 4.7 / 100;
                case ScoringCalculationModel.CommonV2:
                case ScoringCalculationModel.CommonV3:
                    return 11.25 / 100;
                case ScoringCalculationModel.Rubic:
                    return 2.03 / 100;
                case ScoringCalculationModel.HederaSybilPrevention:
                    return 3.68 / 100;
                case ScoringCalculationModel.HederaDeFi:
                    return 3.22 / 100;
                case ScoringCalculationModel.HederaNFT:
                    return 6.49 / 100;
                case ScoringCalculationModel.HederaReputation:
                    return 12.5 / 100;
                case ScoringCalculationModel.ZkSyncEra:
                    return 11.25 / 100;
                case ScoringCalculationModel.LayerZero:
                    return 0;
                case ScoringCalculationModel.CommonV1:
                default:
                    return 2.77 / 100;
            }
        }

        private static double DeployedContractsScore(
            ulong chainId,
            int deployedContracts,
            ScoringCalculationModel calculationModel)
        {
            switch (calculationModel)
            {
                case ScoringCalculationModel.Symbiosis:
                case ScoringCalculationModel.XDEFI:
                case ScoringCalculationModel.Halo:
                case ScoringCalculationModel.Rubic:
                    return deployedContracts switch
                    {
                        0 => 12.99,
                        1 => 64.6,
                        >= 2 and < 5 => 68.43,
                        < 10 => 87.06,
                        _ => 100
                    };
                case ScoringCalculationModel.HederaSybilPrevention:
                case ScoringCalculationModel.HederaDeFi:
                case ScoringCalculationModel.HederaNFT:
                case ScoringCalculationModel.HederaReputation:
                case ScoringCalculationModel.CommonV2:
                case ScoringCalculationModel.CommonV3:
                case ScoringCalculationModel.ZkSyncEra:
                    return deployedContracts switch
                    {
                        0 => 12.44,
                        1 => 75.79,
                        >= 2 and < 5 => 87.06,
                        < 10 => 87.06,
                        _ => 100
                    };
                case ScoringCalculationModel.LayerZero:
                    return 0;
                case ScoringCalculationModel.CommonV1:
                default:
                    return deployedContracts switch
                    {
                        < 1 => 8.93,
                        < 5 => 17.8,
                        < 10 => 36.74,
                        < 20 => 61.18,
                        _ => 100
                    };
            }
        }
    }
}