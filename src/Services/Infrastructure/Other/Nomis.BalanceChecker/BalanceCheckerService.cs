// ------------------------------------------------------------------------------------------------------
// <copyright file="BalanceCheckerService.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Nethereum.JsonRpc.WebSocketClient;
using Nethereum.Util;
using Nethereum.Web3;
using Nomis.BalanceChecker.Contracts;
using Nomis.BalanceChecker.Interfaces;
using Nomis.BalanceChecker.Interfaces.Contracts;
using Nomis.BalanceChecker.Interfaces.Enums;
using Nomis.BalanceChecker.Interfaces.Requests;
using Nomis.BalanceChecker.Settings;
using Nomis.Blockchain.Abstractions.Contracts.Data;
using Nomis.CacheProviderService.Interfaces;
using Nomis.Utils.Contracts.Services;
using Nomis.Utils.Converters;
using Nomis.Utils.Exceptions;
using Nomis.Utils.Wrapper;

namespace Nomis.BalanceChecker
{
    /// <inheritdoc cref="IBalanceCheckerService"/>
    internal sealed class BalanceCheckerService :
        IBalanceCheckerService,
        ISingletonService
    {
        private readonly ILogger<BalanceCheckerService> _logger;
        private readonly BalanceCheckerSettings _settings;
        private readonly Dictionary<BalanceCheckerChain, Web3> _nethereumClients;
        private readonly HttpClient _debankClient;
        private readonly ICacheProviderService _cacheProviderService;

        /// <summary>
        /// Initialize <see cref="BalanceCheckerService"/>.
        /// </summary>
        /// <param name="settings"><see cref="BalanceCheckerSettings"/>.</param>
        /// <param name="debankClient"><see cref="HttpClient"/>.</param>
        /// <param name="cacheProviderService"><see cref="ICacheProviderService"/>.</param>
        /// <param name="logger"><see cref="ILogger{T}"/>.</param>
        public BalanceCheckerService(
            BalanceCheckerSettings settings,
            HttpClient debankClient,
            ICacheProviderService cacheProviderService,
            ILogger<BalanceCheckerService> logger)
        {
            _logger = logger;
            _settings = settings;
            _debankClient = debankClient;
            _cacheProviderService = cacheProviderService;
            _nethereumClients = new Dictionary<BalanceCheckerChain, Web3>();
            foreach (var chain in Enum.GetValues<BalanceCheckerChain>())
            {
                string url = _settings.DataFeeds?.Find(a => a.Blockchain == chain)?.RpcUrl ?? "http://localhost:8545";
                if (url.StartsWith("wss", StringComparison.OrdinalIgnoreCase))
                {
                    _nethereumClients.Add(chain, new Web3(new WebSocketClient(url))
                    {
                        TransactionManager =
                        {
                            DefaultGasPrice = new(0x4c4b40),
                            DefaultGas = new(0x4c4b40)
                        }
                    });
                }
                else
                {
                    _nethereumClients.Add(chain, new Web3(url)
                    {
                        TransactionManager =
                        {
                            DefaultGasPrice = new(0x4c4b40),
                            DefaultGas = new(0x4c4b40)
                        }
                    });
                }
            }
        }

        /// <inheritdoc />
        public async Task<Result<IEnumerable<BalanceCheckerTokenInfo>>> TokenBalancesAsync(
            TokenBalancesRequest request,
            Func<string, string, Task<TokenDataBalance?>>? tokenBalanceFunc = null)
        {
            if (!new AddressUtil().IsValidAddressLength(request.Owner))
            {
                throw new InvalidAddressException(request.Owner!);
            }

            bool tokensCalculated = false;
            var tokenInfos = new List<BalanceCheckerTokenInfo>();
            if (_settings.UseDeBankApi && request.UseDeBankApi)
            {
                var dataFeed = _settings.DataFeeds.Find(x => x.Blockchain == request.Blockchain);
                if (dataFeed?.DeBankId != null)
                {
                    try
                    {
                        var tokens = await GetDeBankTokenDataAsync(request.Owner!, dataFeed.DeBankId).ConfigureAwait(false);
                        foreach (var token in tokens.Where(x => x is { Amount: > 0 }))
                        {
                            tokenInfos.Add(new BalanceCheckerTokenInfo
                            {
                                Id = token.Id,
                                Balance = token.RawAmount,
                                Decimals = token.Decimals ?? 0,
                                Symbol = token.Symbol,
                                Name = token.Name,
                                LogoUri = token.LogoUrl,
                                Price = token.IsVerified ? token.Price : 0
                            });
                        }

                        tokensCalculated = true;
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "There is an error when calling DeBank API for {Blockchain} blockchain and {Owner} wallet", request.Blockchain, request.Owner);
                    }
                }
            }

            if (!tokensCalculated)
            {
                if (request.TokenAddresses.Any(x => !new AddressUtil().IsValidEthereumAddressHexFormat(x)))
                {
                    return await Result<IEnumerable<BalanceCheckerTokenInfo>>.FailAsync("There is an invalid token address in the request.").ConfigureAwait(false);
                }

                bool useSmartContract = true;
                var contractsData = _settings.DataFeeds.Find(a => a.Blockchain == request.Blockchain);
                if (contractsData == null || string.IsNullOrWhiteSpace(contractsData.ContractAbi) || string.IsNullOrWhiteSpace(contractsData.ContractAddress) || !new AddressUtil().IsValidAddressLength(contractsData.ContractAddress))
                {
                    useSmartContract = false;
                }

                if (!useSmartContract)
                {
                    foreach (string tokenAddress in request.TokenAddresses)
                    {
                        if (tokenBalanceFunc != null)
                        {
                            var tokenBalanceData =
                                await tokenBalanceFunc(request.Owner!, tokenAddress).ConfigureAwait(false);
                            if (tokenBalanceData?.Balance > 0)
                            {
                                tokenInfos.Add(new BalanceCheckerTokenInfo
                                {
                                    Id = tokenAddress,
                                    Balance = tokenBalanceData.Balance,
                                    Decimals = int.TryParse(tokenBalanceData.Decimals, out int decimals) ? decimals : 18,
                                    Symbol = tokenBalanceData.Symbol,
                                    Name = tokenBalanceData.Name
                                });
                            }
                        }
                    }
                }
                else if (contractsData != null)
                {
                    var nethereumClient = _nethereumClients[request.Blockchain];
                    var contract = nethereumClient.Eth.GetContract(contractsData.ContractAbi, contractsData.ContractAddress);
                    var function = contract.GetFunction(contractsData.MethodName);

                    int skip = 0;
                    int batchLimit = contractsData.BatchLimit;
                    string[] tokenAddresses = request.TokenAddresses.Take(contractsData.BatchLimit).Skip(skip).ToArray();
                    while (tokenAddresses.Any())
                    {
                        try
                        {
                            var result = await function.CallDeserializingToObjectAsync<BalanceCheckerTokensInfo>(request.Owner, tokenAddresses).ConfigureAwait(false);
                            tokenInfos.AddRange(result.TokenInfos);
                            skip += batchLimit;
                            tokenAddresses = request.TokenAddresses.Take(batchLimit + skip).Skip(skip).ToArray();
                        }
                        catch
                        {
                            // _logger.LogWarning(ex, "There is an error when calling smart-contract for {Blockchain}", request.Blockchain);
                            foreach (string tokenAddress in tokenAddresses)
                            {
                                if (tokenBalanceFunc != null)
                                {
                                    var tokenBalanceData = await tokenBalanceFunc(request.Owner!, tokenAddress).ConfigureAwait(false);
                                    if (tokenBalanceData?.Balance > 0)
                                    {
                                        tokenInfos.Add(new BalanceCheckerTokenInfo
                                        {
                                            Id = tokenAddress,
                                            Balance = tokenBalanceData.Balance,
                                            Decimals = int.TryParse(tokenBalanceData.Decimals, out int decimals) ? decimals : 18,
                                            Symbol = tokenBalanceData.Symbol,
                                            Name = tokenBalanceData.Name
                                        });
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        var result = await function.CallDeserializingToObjectAsync<BalanceCheckerTokensInfo>(request.Owner, new List<string> { tokenAddress }.ToArray()).ConfigureAwait(false);
                                        tokenInfos.AddRange(result.TokenInfos.Where(x => x.Balance > 0));
                                    }
                                    catch (Exception e)
                                    {
                                        _logger.LogWarning(e, "There is an error when calling smart-contract for {Blockchain} with address {Address} and wallet {Wallet}. Calculated {TokensCount}", request.Blockchain, tokenAddress, request.Owner, tokenInfos.Count);
                                    }
                                }
                            }

                            skip += batchLimit;
                            tokenAddresses = request.TokenAddresses.Take(batchLimit + skip).Skip(skip).ToArray();
                        }
                    }
                }
            }

            return await Result<IEnumerable<BalanceCheckerTokenInfo>>.SuccessAsync(tokenInfos, "Got token balances by given wallet address and blockchain.").ConfigureAwait(false);
        }

        private async Task<List<DeBankTokenData>> GetDeBankTokenDataAsync(
            string owner,
            string debankChainId)
        {
            var result = _settings.DeBankUseCaching
                ? await _cacheProviderService.GetFromCacheAsync<List<DeBankTokenData>>($"{debankChainId}_{owner}_debank_tokens").ConfigureAwait(false)
                : null;

            if (_settings.CheckDeBankUnits)
            {
                var deBankUnits = await GetDeBankUnitsDataAsync().ConfigureAwait(false);
                if (deBankUnits.Balance < 1000)
                {
                    _logger.LogWarning("DeBank units less than 1000!");
                }

                if (deBankUnits.Balance < 10)
                {
                    _logger.LogWarning("DeBank units less than 10!!!");
                    return result ?? new List<DeBankTokenData>();
                }
            }

            if (result == null)
            {
                var response = await _debankClient.GetAsync($"/v1/user/token_list?id={owner}&chain_id={debankChainId}&is_all=true").ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                result = await response.Content.ReadFromJsonAsync<List<DeBankTokenData>>(
                             new JsonSerializerOptions
                             {
                                 Converters = { new DecimalStringConverter() }
                             }).ConfigureAwait(false)
                         ?? throw new CustomException("Can't get account token balances from API.");

                if (_settings.DeBankUseCaching)
                {
                    await _cacheProviderService.SetCacheAsync($"{debankChainId}_{owner}_debank_tokens", result, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _settings.DeBankResponsesCacheDuration
                    }).ConfigureAwait(false);
                }
            }

            return result;
        }

        private async Task<DeBankUnitsData> GetDeBankUnitsDataAsync()
        {
            var response = await _debankClient.GetAsync("/v1/account/units").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DeBankUnitsData>().ConfigureAwait(false)
                   ?? throw new CustomException("Can't get DeBank units data.");
        }
    }
}