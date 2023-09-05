// ------------------------------------------------------------------------------------------------------
// <copyright file="ReferralsData.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

namespace Nomis.Utils.Contracts.Referrals
{
    /// <summary>
    /// Referrals data.
    /// </summary>
    public class ReferralsData
    {
        /// <summary>
        /// Referral count.
        /// </summary>
        public int ReferralCount { get; init; }

        /// <summary>
        /// Referral wallets.
        /// </summary>
        public IList<string> ReferralWallets { get; set; } = new List<string>();
    }
}