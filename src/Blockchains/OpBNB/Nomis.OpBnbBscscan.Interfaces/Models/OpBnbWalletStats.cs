// ------------------------------------------------------------------------------------------------------
// <copyright file="OpBnbWalletStats.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

using Nomis.Blockchain.Abstractions.Stats;

namespace Nomis.OpBnbBscscan.Interfaces.Models
{
    /// <summary>
    /// opBNB wallet stats.
    /// </summary>
    public sealed class OpBnbWalletStats :
        BaseEvmWalletStats<OpBnbTransactionIntervalData>,
        IWalletNftStats
    {
        /// <inheritdoc/>
        public override string NativeToken => "BNB";

        /// <inheritdoc/>
        [Display(Description = "Total NFTs on wallet", GroupName = "number")]
        public int NftHolding { get; set; }

        /// <inheritdoc/>
        [Display(Description = "NFT trading activity", GroupName = "Native token")]
        public decimal NftTrading { get; set; }

        /// <inheritdoc/>
        [Display(Description = "NFT relative turnover", GroupName = "Native token")]
        public decimal NftWorth { get; set; }
    }
}