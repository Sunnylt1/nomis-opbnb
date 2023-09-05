// ------------------------------------------------------------------------------------------------------
// <copyright file="TokenBalancesRequest.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Nomis.BalanceChecker.Interfaces.Enums;

namespace Nomis.BalanceChecker.Interfaces.Requests
{
    /// <summary>
    /// Token balances request.
    /// </summary>
    public class TokenBalancesRequest
    {
        /// <summary>
        /// Owner wallet address.
        /// </summary>
        public string? Owner { get; set; }

        /// <summary>
        /// Blockchain.
        /// </summary>
        public BalanceCheckerChain Blockchain { get; set; }

        /// <summary>
        /// Token addresses.
        /// </summary>
        public IList<string> TokenAddresses { get; set; } = new List<string>();

        /// <summary>
        /// Use DeBank API for getting token holding.
        /// </summary>
        public bool UseDeBankApi { get; init; }
    }
}