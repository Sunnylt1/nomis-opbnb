// ------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Nomis.Blockchain.Abstractions;
using Nomis.Blockchain.Abstractions.Enums;
using Nomis.Utils.Contracts.Common;
using Nomis.Utils.Contracts.Requests;
using Nomis.Utils.Enums;

namespace Nomis.DexProviderService.Interfaces.Extensions
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Get the blockchain descriptor in which the score will be minted.
        /// </summary>
        /// <param name="service"><see cref="IDexProviderService"/>.</param>
        /// <param name="request"><see cref="WalletStatsRequest"/>.</param>
        /// <param name="scoredChain">Scored chain.</param>
        /// <returns>Returns the blockchain descriptor in which the score will be minted.</returns>
        public static IBlockchainDescriptor? MintChain(
            this IDexProviderService service,
            IHasMintChain request,
            ulong scoredChain)
        {
            var mintChain = request.MintChain;
            var supportedBlockchains = service.Blockchains(BlockchainType.EVM, true);
            if (mintChain == Utils.Enums.MintChain.Native)
            {
                var scoredChainDescriptor = supportedBlockchains.Data.Find(b => b.ChainId == scoredChain);
                if (request.MintBlockchainType == MintChainType.Testnet)
                {
                    scoredChainDescriptor = supportedBlockchains.Data.Find(b => b.ChainId != scoredChain && b.BlockchainSlug?.Equals(scoredChainDescriptor?.BlockchainSlug, StringComparison.InvariantCultureIgnoreCase) == true);
                    if (scoredChainDescriptor == null)
                    {
                        throw new NotSupportedException($"{mintChain} blockchain does not supported or disabled.");
                    }
                }

                return scoredChainDescriptor;
            }

            var mintBlockchain = supportedBlockchains.Data.Find(b => b.ChainId == (ulong)mintChain);
            if (mintBlockchain == null)
            {
                throw new NotSupportedException($"{mintChain} blockchain does not supported or disabled.");
            }

            return mintBlockchain;
        }
    }
}