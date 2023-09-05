// ------------------------------------------------------------------------------------------------------
// <copyright file="BlockscoutApiService.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Nomis.Blockscout.Interfaces;
using Nomis.Blockscout.Interfaces.BlockscoutApiClient;
using Nomis.Blockscout.Interfaces.Enums;
using Nomis.Blockscout.Interfaces.Extensions;
using Nomis.Blockscout.Settings;
using Nomis.Utils.Contracts.Services;

namespace Nomis.Blockscout
{
    /// <inheritdoc cref="IBlockscoutApiService"/>
    internal sealed class BlockscoutService :
        IBlockscoutApiService,
        ITransientService
    {
        private readonly ILogger<BlockscoutService> _logger;
        private readonly BlockscoutSettings _blockscoutSettings;
        private readonly Dictionary<BlockscoutChain, IBlockscoutApiClient> _clients = new();

        /// <summary>
        /// Initialize <see cref="BlockscoutService"/>.
        /// </summary>
        /// <param name="blockscoutSettings"><see cref="BlockscoutSettings"/>.</param>
        /// <param name="logger"><see cref="ILogger{T}"/>.</param>
        public BlockscoutService(
            BlockscoutSettings blockscoutSettings,
            ILogger<BlockscoutService> logger)
        {
            _logger = logger;
            _blockscoutSettings = blockscoutSettings;
            foreach (var chain in blockscoutSettings.ApiBaseUrls.Keys)
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new(blockscoutSettings.ApiBaseUrls[chain])
                };

                var client = blockscoutSettings.ApiKeys.TryGetValue(chain, out var apiKeys)
                    ? new BlockscoutApiClientExtended(httpClient, apiKeys)
                    : new BlockscoutApiClientExtended(httpClient, new List<string>());
                _clients.Add(chain, client);
            }
        }

        /// <inheritdoc />
        public IBlockscoutApiClient? GetClientByChain(
            ulong chainId)
        {
            if (_clients.Keys.All(k => (ulong)k != chainId))
            {
                return null;
            }

            return _clients[_clients.Keys.FirstOrDefault(k => (ulong)k == chainId)];
        }
    }
}