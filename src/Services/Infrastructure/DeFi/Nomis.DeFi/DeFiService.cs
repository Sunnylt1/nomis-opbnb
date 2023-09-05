// ------------------------------------------------------------------------------------------------------
// <copyright file="DeFiService.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Nodes;

using GraphQL;
using Microsoft.Extensions.Logging;
using Nomis.DeFi.Interfaces;
using Nomis.DeFi.Interfaces.Models;
using Nomis.DeFi.Interfaces.Requests;
using Nomis.Utils.Contracts.Services;
using Nomis.Utils.Wrapper;

namespace Nomis.DeFi
{
    /// <inheritdoc cref="IDeFiService"/>
    internal sealed class DeFiService :
        IDeFiService,
        ISingletonService
    {
        private readonly IDeFiGraphQLClient _client;
        private readonly ILogger<DeFiService> _logger;

        /// <summary>
        /// Initialize <see cref="DeFiService"/>.
        /// </summary>
        /// <param name="client"><see cref="IDeFiGraphQLClient"/>.</param>
        /// <param name="logger"><see cref="ILogger{T}"/>.</param>
        public DeFiService(
            IDeFiGraphQLClient client,
            ILogger<DeFiService> logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Result<IEnumerable<DeFiChainData>?>> ChainsAsync()
        {
            try
            {
                var query = new GraphQLRequest
                {
                    Query = """
                    query getChains {
                      chains{
                        id
                        absoluteChainId
                        abbr
                        name
                        type
                      }
                    }
                    """
                };

                var data = await GetDataAsync<IEnumerable<DeFiChainData>?>(query, "chains").ConfigureAwait(false);

                return await Result<IEnumerable<DeFiChainData>?>.SuccessAsync(data, "De.Fi chains data received.").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There is an error when getting chains.");
            }

            return await Result<IEnumerable<DeFiChainData>?>.FailAsync(null, "There is an error when getting chains.").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Result<DeFiShieldAdvancedData?>> ShieldsAsync(
            DeFiShieldsRequest request)
        {
            try
            {
                var query = new GraphQLRequest
                {
                    Query = """
                    query getShields($addresses: [String!]!, $chainId: Int!) {
                      shieldAdvanced( where: {
                          addresses: $addresses
                          chainId: $chainId
                        }){
                        contracts{
                          id
                          address
                          network
                          name
                          logo
                          inProgress
                          whitelisted
                          issues{
                            id
                            impact
                            title
                            description
                            category
                            data
                          }
                        }
                      }
                    }
                    """,
                    Variables = request
                };

                var data = await GetDataAsync<DeFiShieldAdvancedData?>(query, "shieldAdvanced").ConfigureAwait(false);

                return await Result<DeFiShieldAdvancedData?>.SuccessAsync(data, "De.Fi shields data received.").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "There is an error when getting shields.");
            }

            return await Result<DeFiShieldAdvancedData?>.FailAsync(null, "There is an error when getting shields.").ConfigureAwait(false);
        }

        private async Task<TResult?> GetDataAsync<TResult>(GraphQLRequest query, params string[] responseAliases)
        {
            var responseAliasList = responseAliases.ToList();
            var response = await _client.SendQueryAsync<JsonObject>(query).ConfigureAwait(false);
            var result = response.Data[responseAliasList[0]];
            responseAliasList.RemoveAt(0);
            foreach (string responseAlias in responseAliasList)
            {
                if (result == null)
                {
                    return default;
                }

                result = result[responseAlias];
            }

            var data = JsonSerializer.Deserialize<TResult?>(result!.ToJsonString()) !;

            return data;
        }
    }
}