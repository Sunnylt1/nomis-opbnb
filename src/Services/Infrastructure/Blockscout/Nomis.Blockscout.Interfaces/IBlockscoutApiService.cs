// ------------------------------------------------------------------------------------------------------
// <copyright file="IBlockscoutApiService.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Nomis.Blockscout.Interfaces.BlockscoutApiClient;
using Nomis.Utils.Contracts.Services;

namespace Nomis.Blockscout.Interfaces
{
    /// <summary>
    /// Blockscout API service.
    /// </summary>
    public interface IBlockscoutApiService :
        IInfrastructureService
    {
        /// <summary>
        /// Get blockscout API client be blockchain id.
        /// </summary>
        /// <param name="chainId">Blockchain id.</param>
        /// <returns>Returns blockscout API client be blockchain id.</returns>
        public IBlockscoutApiClient? GetClientByChain(
            ulong chainId);
    }
}