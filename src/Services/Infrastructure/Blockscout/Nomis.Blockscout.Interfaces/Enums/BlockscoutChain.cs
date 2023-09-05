// ------------------------------------------------------------------------------------------------------
// <copyright file="BlockscoutChain.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

namespace Nomis.Blockscout.Interfaces.Enums
{
    /// <summary>
    /// Blockscout blockchain.
    /// </summary>
    public enum BlockscoutChain :
        ulong
    {
        /// <summary>
        /// Ethereum Mainnet.
        /// </summary>
        Ethereum = 1,

        /// <summary>
        /// Optimism.
        /// </summary>
        Optimism = 10,

        /// <summary>
        /// Gnosis.
        /// </summary>
        Gnosis = 100,

        /// <summary>
        /// Polygon Mainnet.
        /// </summary>
        Polygon = 137,

        /// <summary>
        /// Base.
        /// </summary>
        Base = 8453,

        // TODO - add chains
    }
}