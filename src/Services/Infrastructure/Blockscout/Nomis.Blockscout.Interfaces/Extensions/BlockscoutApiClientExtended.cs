// ------------------------------------------------------------------------------------------------------
// <copyright file="BlockscoutApiClientExtended.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using System.Text;

using Nomis.Utils.Contracts;

namespace Nomis.Blockscout.Interfaces.Extensions
{
    /// <summary>
    /// Blockscout API client.
    /// </summary>
    public class BlockscoutApiClientExtended :
        BlockscoutApiClient.BlockscoutApiClient
    {
        private readonly IValuePool<BlockscoutApiClientExtended, string> _apiKeysPool;

        /// <summary>
        /// Initialize <see cref="BlockscoutApiClientExtended"/>.
        /// </summary>
        /// <param name="client"><see cref="HttpClient"/>.</param>
        /// <param name="apiKeys">List of API key.</param>
        public BlockscoutApiClientExtended(
            HttpClient client,
            IList<string> apiKeys)
            : base(client)
        {
            _apiKeysPool = new ValuePool<BlockscoutApiClientExtended, string>(apiKeys);
        }

        /// <summary>
        /// Prepare request.
        /// </summary>
        /// <param name="client"><see cref="HttpClient"/>.</param>
        /// <param name="request"><see cref="HttpRequestMessage"/>.</param>
        /// <param name="urlBuilder">URL builder.</param>
        protected override void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder)
        {
            string apiKey = _apiKeysPool.GetNextValue();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                if (urlBuilder.ToString().Contains('?'))
                {
                    urlBuilder.Append($"&apiKey={apiKey}");
                }
                else
                {
                    urlBuilder.Append($"?apiKey={apiKey}");
                }
            }
        }
    }
}