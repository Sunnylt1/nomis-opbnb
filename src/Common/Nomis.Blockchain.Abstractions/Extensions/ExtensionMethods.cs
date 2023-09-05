// ------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using System.Numerics;
using System.Text;
using System.Text.Json;

using Ipfs.CoreApi;
using Microsoft.Extensions.Caching.Distributed;
using Nomis.Blockchain.Abstractions.Clients;
using Nomis.Blockchain.Abstractions.Contracts.Data;
using Nomis.Blockchain.Abstractions.Contracts.Models;
using Nomis.Blockchain.Abstractions.Contracts.Settings;
using Nomis.Blockchain.Abstractions.Requests;
using Nomis.Blockchain.Abstractions.Stats;
using Nomis.CacheProviderService.Interfaces;
using Nomis.DefiLlama.Interfaces;
using Nomis.IPFS.Interfaces;
using Nomis.IPFS.Interfaces.Requests;
using Nomis.ScoringService.Interfaces;
using Nomis.SoulboundTokenService.Interfaces.Contracts;
using Nomis.SoulboundTokenService.Interfaces.Requests;
using Nomis.Utils.Contracts.NFT;
using Nomis.Utils.Contracts.Requests;
using Nomis.Utils.Enums;
using Nomis.Utils.Extensions;
using Nomis.Utils.Wrapper;

// ReSharper disable InconsistentNaming
namespace Nomis.Blockchain.Abstractions.Extensions
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Get the token data balance.
        /// </summary>
        /// <param name="client"><see cref="IBaseEvmClient"/>.</param>
        /// <param name="owner">Owner wallet address.</param>
        /// <param name="tokenAddress">The token smart-contract address.</param>
        /// <param name="chainId">Blockchain id.</param>
        /// <returns>Returns the token data balance or null.</returns>
        public static async Task<TokenDataBalance?> GetTokenDataBalanceAsync(
            this IBaseEvmClient client,
            string owner,
            string tokenAddress,
            ulong chainId)
        {
            var tokenBalance = (await client.GetTokenBalanceAsync(owner, tokenAddress).ConfigureAwait(false)).Balance?.ToBigInteger();
            if (tokenBalance > 0)
            {
                return new TokenDataBalance
                {
                    ChainId = chainId,
                    Balance = (BigInteger)tokenBalance,
                    Id = tokenAddress
                };
            }

            return null;
        }

        /// <summary>
        /// Get the ONFT token metadata IPFS URL.
        /// </summary>
        /// <param name="soulboundTokenService"><see cref="IScoreSoulboundTokenService"/>.</param>
        /// <param name="ipfsService"><see cref="IIPFSService"/>.</param>
        /// <param name="cacheProviderService"><see cref="ICacheProviderService"/>.</param>
        /// <param name="prepareToMint">Is prepare to mint.</param>
        /// <param name="chainId">Blockchain id.</param>
        /// <param name="chainName">Blockchain name.</param>
        /// <param name="onftImageIpfsUrl">The ONFT image IPFS URL.</param>
        /// <param name="additionalTraits">Additional traits.</param>
        /// <returns>Returns the token metadata IPFS URL.</returns>
        public static async Task<Result<string?>> ONFTTokenMetadataAsync(
            this IScoreSoulboundTokenService soulboundTokenService,
            IIPFSService ipfsService,
            ICacheProviderService cacheProviderService,
            bool prepareToMint,
            ulong chainId,
            string? chainName,
            string onftImageIpfsUrl,
            IList<NFTTrait>? additionalTraits = default)
        {
            if (!prepareToMint)
            {
                return await Result<string?>.FailAsync($"Can't get ONFT token metadata: {nameof(prepareToMint)} parameter is false.").ConfigureAwait(false);
            }

            try
            {
                var metadataResult = await soulboundTokenService.GetONFTSoulboundTokenMetadataAsync(new NFTMetadataRequest
                {
                    Image = onftImageIpfsUrl,
                    Attributes = new List<NFTTrait>
                    {
                        new()
                        {
                            TraitType = "Initial Blockchain",
                            Value = chainName
                        },
                        new()
                        {
                            DisplayType = "number",
                            TraitType = "Initial Chain id",
                            Value = chainId
                        },
                        new()
                        {
                            DisplayType = "date",
                            TraitType = "Timestamp",
                            Value = DateTime.UtcNow.ConvertToTimestamp()
                        }
                    }.Union(additionalTraits ?? new List<NFTTrait>()).ToList()
                }).ConfigureAwait(false);

                if (metadataResult.Succeeded)
                {
                    string? tokenMetadata = await cacheProviderService.GetStringFromCacheAsync($"onft_token_metadata_{chainId}_{onftImageIpfsUrl}").ConfigureAwait(false);
                    if (tokenMetadata == null)
                    {
                        using var tokenMetadataStream = new MemoryStream(JsonSerializer.Serialize(metadataResult.Data).ToByteArray(Encoding.UTF8));
                        var uploadMetadataResult = await ipfsService.UploadFileAsync(new IPFSUploadFileRequest
                        {
                            FileContent = tokenMetadataStream,
                            FileName = $"{chainId}_Nomis_ONFT.json",
                            Options = new AddFileOptions
                            {
                                Pin = true
                            }
                        }).ConfigureAwait(false);

                        if (uploadMetadataResult.Succeeded)
                        {
                            tokenMetadata = uploadMetadataResult.Data;
                            await cacheProviderService.SetCacheAsync($"onft_token_metadata_{chainId}_{onftImageIpfsUrl}", tokenMetadata!, new DistributedCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = new TimeSpan(1, 0, 0, 0)
                            }).ConfigureAwait(false);
                        }
                    }

                    if (tokenMetadata != null)
                    {
                        return await Result<string?>.SuccessAsync(string.Format(ipfsService.Settings.IpfsGatewayUrlTemplate!, tokenMetadata), "Successfully got ONFT token metadata URL.").ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                return await Result<string?>.FailAsync(e.Message).ConfigureAwait(false);
            }

            return await Result<string?>.FailAsync("Cant get ONFT token metadata.").ConfigureAwait(false);
        }

        /// <summary>
        /// Get the token metadata IPFS URL.
        /// </summary>
        /// <typeparam name="TWalletRequest">The wallet request type.</typeparam>
        /// <param name="soulboundTokenService"><see cref="IScoreSoulboundTokenService"/>.</param>
        /// <param name="ipfsService"><see cref="IIPFSService"/>.</param>
        /// <param name="cacheProviderService"><see cref="ICacheProviderService"/>.</param>
        /// <param name="request"><see cref="WalletStatsRequest"/>.</param>
        /// <param name="chainId">Blockchain id.</param>
        /// <param name="chainName">Blockchain name.</param>
        /// <param name="score">The wallet score.</param>
        /// <param name="additionalTraits">Additional traits.</param>
        /// <returns>Returns the token metadata IPFS URL.</returns>
        public static async Task<Result<string?>> TokenMetadataAsync<TWalletRequest>(
            this IScoreSoulboundTokenService soulboundTokenService,
            IIPFSService ipfsService,
            ICacheProviderService cacheProviderService,
            TWalletRequest request,
            ulong chainId,
            string? chainName,
            double score,
            IList<NFTTrait>? additionalTraits = default)
            where TWalletRequest : WalletStatsRequest
        {
            if (!request.PrepareToMint)
            {
                return await Result<string?>.FailAsync($"Can't get token metadata: {nameof(request.PrepareToMint)} parameter is false.").ConfigureAwait(false);
            }

            try
            {
                var tokenImageResult = await soulboundTokenService.GetSoulboundTokenImageAsync(new ScoreSoulboundTokenImageRequest
                {
                    Address = request.Address,
                    Score = (byte)(score * 100),
                    Type = request.CalculationModel.ToString(),
                    Size = 512,
                    ChainId = chainId == 11101011 ? null : chainId
                }).ConfigureAwait(false);

                if (tokenImageResult.Succeeded)
                {
                    string? uploadedImageData = await cacheProviderService.GetStringFromCacheAsync($"image_data_{request.Address}_{chainId}_{(int)request.CalculationModel}_{request.ScoreType.ToString()}_{score}").ConfigureAwait(false);
                    if (uploadedImageData == null)
                    {
                        using var tokenImageStream = new MemoryStream(tokenImageResult.Data.Image!);
                        var uploadImageResult = await ipfsService.UploadFileAsync(new IPFSUploadFileRequest
                        {
                            FileContent = tokenImageStream,
                            FileName = $"{request.Address}_{chainId}_{request.CalculationModel.ToString()}_{request.ScoreType.ToString()}.png",
                            Options = new AddFileOptions
                            {
                                Pin = true
                            }
                        }).ConfigureAwait(false);
                        if (uploadImageResult.Succeeded)
                        {
                            uploadedImageData = uploadImageResult.Data;
                            await cacheProviderService.SetCacheAsync($"image_data_{request.Address}_{chainId}_{(int)request.CalculationModel}_{request.ScoreType.ToString()}_{score}", uploadedImageData!, new DistributedCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = new TimeSpan(1, 0, 0, 0)
                            }).ConfigureAwait(false);
                        }
                    }

                    if (uploadedImageData != null)
                    {
                        var metadataResult = await soulboundTokenService.GetSoulboundTokenMetadataAsync(new NFTMetadataRequest
                        {
                            Image = string.Format(ipfsService.Settings.IpfsGatewayUrlTemplate!, uploadedImageData),
                            Attributes = new List<NFTTrait>
                            {
                                new()
                                {
                                    TraitType = "Blockchain",
                                    Value = chainName
                                },
                                new()
                                {
                                    DisplayType = "number",
                                    TraitType = "Chain id",
                                    Value = chainId
                                },
                                new()
                                {
                                    DisplayType = "boost_percentage",
                                    TraitType = "Score",
                                    Value = score * 100
                                },
                                new()
                                {
                                    DisplayType = "number",
                                    TraitType = "Calculation model",
                                    Value = request.CalculationModel.ToString()
                                },
                                new()
                                {
                                    DisplayType = "number",
                                    TraitType = "Score type",
                                    Value = request.ScoreType.ToString()
                                },
                                new()
                                {
                                    DisplayType = "date",
                                    TraitType = "Timestamp",
                                    Value = DateTime.UtcNow.ConvertToTimestamp()
                                }
                            }.Union(additionalTraits ?? new List<NFTTrait>()).ToList()
                        }).ConfigureAwait(false);

                        if (metadataResult.Succeeded)
                        {
                            string? tokenMetadata = await cacheProviderService.GetStringFromCacheAsync($"token_metadata_{request.Address}_{chainId}_{(int)request.CalculationModel}_{request.ScoreType.ToString()}_{score}").ConfigureAwait(false);
                            if (tokenMetadata == null)
                            {
                                using var tokenMetadataStream = new MemoryStream(JsonSerializer.Serialize(metadataResult.Data).ToByteArray(Encoding.UTF8));
                                var uploadMetadataResult = await ipfsService.UploadFileAsync(new IPFSUploadFileRequest
                                {
                                    FileContent = tokenMetadataStream,
                                    FileName = $"{request.Address}_{chainId}_{request.CalculationModel.ToString()}_{request.ScoreType.ToString()}.json",
                                    Options = new AddFileOptions
                                    {
                                        Pin = true
                                    }
                                }).ConfigureAwait(false);

                                if (uploadMetadataResult.Succeeded)
                                {
                                    tokenMetadata = uploadMetadataResult.Data;
                                    await cacheProviderService.SetCacheAsync($"token_metadata_{request.Address}_{chainId}_{(int)request.CalculationModel}_{request.ScoreType.ToString()}_{score}", tokenMetadata!, new DistributedCacheEntryOptions
                                    {
                                        AbsoluteExpirationRelativeToNow = new TimeSpan(1, 0, 0, 0)
                                    }).ConfigureAwait(false);
                                }
                            }

                            if (tokenMetadata != null)
                            {
                                return await Result<string?>.SuccessAsync(string.Format(ipfsService.Settings.IpfsGatewayUrlTemplate!, tokenMetadata), "Successfully got token metadata URL.").ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return await Result<string?>.FailAsync(e.Message).ConfigureAwait(false);
            }

            return await Result<string?>.FailAsync("Cant get token metadata.").ConfigureAwait(false);
        }

        /// <summary>
        /// Get historical median balance in USD.
        /// </summary>
        /// <typeparam name="TWalletStats">The wallet stats type.</typeparam>
        /// <param name="scoringService"><see cref="IScoringService"/>.</param>
        /// <param name="wallet">Wallet address.</param>
        /// <param name="chainId">Blockchain id.</param>
        /// <param name="calculationModel">Scoring calculation model.</param>
        /// <param name="settings"><see cref="IUseHistoricalMedianBalanceUSDSettings"/>.</param>
        /// <param name="currentUsdBalance">Current balance in USD.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
        /// <returns>Returns historical median balance in USD.</returns>
        public static async Task<decimal> MedianBalanceUsdAsync<TWalletStats>(
            this IScoringService scoringService,
            string wallet,
            ulong chainId,
            ScoringCalculationModel calculationModel,
            IUseHistoricalMedianBalanceUSDSettings settings,
            decimal? currentUsdBalance = null,
            CancellationToken cancellationToken = default)
            where TWalletStats : class, IWalletBalanceStats
        {
            if (!settings.UseHistoricalMedianBalanceUSD.TryGetValue(calculationModel, out bool use) || !use)
            {
                return 0;
            }

            var historicalScoringData = await scoringService.GetScoringStatsDataFromDatabaseAsync(wallet, chainId, calculationModel, settings.MedianBalanceStartFrom, cancellationToken).ConfigureAwait(false);

            // ReSharper disable once InconsistentNaming
            var historicalUSDBalances = historicalScoringData?
                .OrderByDescending(x => x.CreatedOn)
                .Take(settings.MedianBalanceLastCount ?? historicalScoringData.Count)
                .Select(x => JsonSerializer.Deserialize<TWalletStats>(x.StatData))
                .Where(x => x != null)
                .Select(x => x as IWalletBalanceStats)
                .Where(x => x != null)
                .Select(x => (x?.NativeBalanceUSD ?? 0) + (x?.HoldTokensBalanceUSD ?? 0))
                .ToList();

            if (currentUsdBalance != null)
            {
                historicalUSDBalances?.Add((decimal)currentUsdBalance);
            }

            var sortedValues = historicalUSDBalances?.Distinct().OrderBy(val => val).ToList();
            var sortedValuesWithPrecision = sortedValues?
                .Where((val, index) => index == 0 || Math.Abs(val - sortedValues[index - 1]) >= settings.MedianBalancePrecision)
                .ToList();
            if (sortedValuesWithPrecision?.Count == 1)
            {
                return sortedValues?.Median() ?? 0;
            }

            return sortedValuesWithPrecision?.Median() ?? 0;
        }

        /// <summary>
        /// Get extended counterparties data.
        /// </summary>
        /// <typeparam name="TWalletRequest">The wallet request type.</typeparam>
        /// <typeparam name="TNormalTransaction">The normal transaction type.</typeparam>
        /// <typeparam name="TInternalTransaction">The internal transaction type.</typeparam>
        /// <typeparam name="TERC20TokenTransfer">The ERC20 token transfer type.</typeparam>
        /// <param name="request"><see cref="WalletStatsRequest"/>.</param>
        /// <param name="defiLlamaService"><see cref="IDefiLlamaService"/>.</param>
        /// <param name="settings"><see cref="IFilterCounterpartiesByCalculationModelSettings"/>.</param>
        /// <param name="transactions">Transactions.</param>
        /// <param name="internalTransactions">Internal transactions.</param>
        /// <param name="erc20Tokens">ERC20 token transfers,</param>
        /// <param name="tokenTransfers">NFT token transfers.</param>
        /// <param name="tokenDataBalances">List of <see cref="TokenDataBalance"/>.</param>
        /// <param name="chainId">Blockchain id.</param>
        /// <param name="defillamaPlatformId">Defillama platform id.</param>
        /// <param name="nativeTokenDataBalance"><see cref="TokenDataBalance"/>.</param>
#pragma warning disable MA0016
        public static async Task<ExtendedCounterpartiesDataLists<TNormalTransaction, TInternalTransaction, TERC20TokenTransfer>?> ExtendedCounterpartiesDataAsync<TWalletRequest, TNormalTransaction, TInternalTransaction, TERC20TokenTransfer>(
            this TWalletRequest request,
            IDefiLlamaService defiLlamaService,
            IFilterCounterpartiesByCalculationModelSettings settings,
#pragma warning disable CA1045
            List<TNormalTransaction> transactions,
            List<TInternalTransaction> internalTransactions,
            List<TERC20TokenTransfer> erc20Tokens,
            List<NFTTokenTransfer> tokenTransfers,
            List<TokenDataBalance> tokenDataBalances,
            ulong chainId,
            string? defillamaPlatformId,
            TokenDataBalance? nativeTokenDataBalance = null)
#pragma warning restore CA1045
#pragma warning restore MA0016
            where TWalletRequest : WalletStatsRequest
            where TNormalTransaction : INormalTransaction
            where TInternalTransaction : IInternalTransaction
            where TERC20TokenTransfer : IERC20TokenTransfer
        {
            if (settings.CounterpartiesFilterData.TryGetValue(request.CalculationModel, out var counterpartiesData))
            {
                var extendedCounterpartiesData = new List<ExtendedCounterpartyData>();
                var filteredTransactions = new List<TNormalTransaction>();
                var filteredInternalTransactions = new List<TInternalTransaction>();
                var filteredErc20Transfers = new List<TERC20TokenTransfer>();
                var filteredTokenTransfers = new List<NFTTokenTransfer>();

                foreach (var counterpartyData in counterpartiesData.Where(x => x.UseCounterparty))
                {
                    if (string.IsNullOrWhiteSpace(counterpartyData.ContractAddress))
                    {
                        continue;
                    }

                    var counterpartyTransactions = transactions
                        .Where(x =>
                            x.IsError?.Equals("1") != true &&
                            x.FunctionName?.StartsWith("approve", StringComparison.OrdinalIgnoreCase) != true &&
                            (x.ContractAddress?.Equals(counterpartyData.ContractAddress, StringComparison.InvariantCultureIgnoreCase) == true ||
                            x.To?.Equals(counterpartyData.ContractAddress, StringComparison.InvariantCultureIgnoreCase) == true))
                        .ToList();
                    var counterpartyTransactionHashes = counterpartyTransactions.Select(x => x.Hash?.ToLowerInvariant()).Where(x => x != null).Cast<string>().ToList();
                    filteredTransactions.AddRange(counterpartyTransactions);

                    var counterpartyInternalTransactions = internalTransactions
                        .Where(x =>
                            x.To?.Equals(counterpartyData.ContractAddress, StringComparison.InvariantCultureIgnoreCase) == true)
                        .ToList();
                    filteredInternalTransactions.AddRange(counterpartyInternalTransactions);

                    var counterpartyErc20Transfers = erc20Tokens
                        .Where(x =>
                            x.ContractAddress?.Equals(counterpartyData.ContractAddress, StringComparison.InvariantCultureIgnoreCase) == true ||
                            x.To?.Equals(counterpartyData.ContractAddress, StringComparison.InvariantCultureIgnoreCase) == true ||
                            counterpartyTransactionHashes.Contains(x.Hash?.ToLowerInvariant() ?? string.Empty))
                        .ToList();
                    filteredErc20Transfers.AddRange(counterpartyErc20Transfers);

                    counterpartyTransactionHashes.RemoveAll(x =>
                        erc20Tokens.Select(t => t.Hash).Contains(x) &&
                        !filteredErc20Transfers.Select(t => t.Hash).Contains(x));

                    var counterpartyTokenTransfers = tokenTransfers
                        .Where(x =>
                            x.ContractAddress?.Equals(counterpartyData.ContractAddress, StringComparison.InvariantCultureIgnoreCase) == true ||
                            counterpartyTransactionHashes.Contains(x.Hash?.ToLowerInvariant() ?? string.Empty))
                        .ToList();
                    filteredTokenTransfers.AddRange(counterpartyTokenTransfers);

                    if (counterpartyTransactions.Count > 0 || counterpartyErc20Transfers.Count > 0)
                    {
                        List<TokenDataBalance>? counterpartyTransferBalances = null;
                        foreach (var counterpartyErc20Transfer in counterpartyErc20Transfers)
                        {
                            var erc20TokenData = tokenDataBalances.Find(x => x.Id?.Equals(counterpartyErc20Transfer.ContractAddress, StringComparison.InvariantCultureIgnoreCase) == true);
                            if (erc20TokenData != null && counterpartyErc20Transfer.From?.Equals(request.Address, StringComparison.InvariantCultureIgnoreCase) == true)
                            {
                                if (request.UseDefillamaForUnknownTokens && !string.IsNullOrWhiteSpace(defillamaPlatformId) && erc20TokenData.Price == 0)
                                {
                                    var tokenPrices = await defiLlamaService.TokensPriceAsync(
                                        new List<string> { $"{defillamaPlatformId}:{counterpartyErc20Transfer.ContractAddress}" }).ConfigureAwait(false);

                                    if (tokenPrices?.TokensPrices.ContainsKey($"{defillamaPlatformId}:{counterpartyErc20Transfer.ContractAddress}") == true)
                                    {
                                        erc20TokenData.Price = tokenPrices.TokensPrices[$"{defillamaPlatformId}:{counterpartyErc20Transfer.ContractAddress}"].Price;
                                    }
                                }

                                counterpartyTransferBalances ??= new List<TokenDataBalance>();
                                var counterpartyErc20TokenData = new TokenDataBalance(erc20TokenData, counterpartyErc20Transfer.Value.ToBigInteger());
                                counterpartyTransferBalances.Add(counterpartyErc20TokenData);
                            }
                            else if (request.UseDefillamaForUnknownTokens && !string.IsNullOrWhiteSpace(defillamaPlatformId) && counterpartyErc20Transfer.From?.Equals(request.Address, StringComparison.InvariantCultureIgnoreCase) == true)
                            {
                                var tokenPrices = await defiLlamaService.TokensPriceAsync(
                                    new List<string> { $"{defillamaPlatformId}:{counterpartyErc20Transfer.ContractAddress}" }).ConfigureAwait(false);

                                if (tokenPrices?.TokensPrices.ContainsKey($"{defillamaPlatformId}:{counterpartyErc20Transfer.ContractAddress}") == true)
                                {
                                    var tokenPriceData = tokenPrices.TokensPrices[$"{defillamaPlatformId}:{counterpartyErc20Transfer.ContractAddress}"];
                                    var tokenDataBalance = new TokenDataBalance(
                                        new TokenDataBalance
                                        {
                                            Confidence = tokenPriceData.Confidence,
                                            Decimals = tokenPriceData.Decimals?.ToString(),
                                            Id = counterpartyErc20Transfer.ContractAddress,
                                            LastPriceDateTime = tokenPriceData.LastPriceDateTime,
                                            Price = tokenPriceData.Price,
                                            Symbol = tokenPriceData.Symbol,
                                            ChainId = chainId,
                                            LogoUri = tokenPriceData.LogoUri,
                                        },
                                        counterpartyErc20Transfer.Value.ToBigInteger());

                                    counterpartyTransferBalances ??= new List<TokenDataBalance>();
                                    var counterpartyErc20TokenData = new TokenDataBalance(tokenDataBalance, counterpartyErc20Transfer.Value.ToBigInteger());
                                    counterpartyTransferBalances.Add(counterpartyErc20TokenData);
                                }
                            }
                        }

                        if (counterpartyTransactions.Count > 0)
                        {
                            var transactionsValue = new BigInteger(0);
                            foreach (var counterpartyTransaction in counterpartyTransactions)
                            {
                                transactionsValue += counterpartyTransaction.Value.ToBigInteger();
                            }

                            if (!string.IsNullOrWhiteSpace(defillamaPlatformId) && nativeTokenDataBalance?.Price == 0)
                            {
                                var tokenPrices = await defiLlamaService.TokensPriceAsync(
                                    new List<string> { $"{defillamaPlatformId}:{nativeTokenDataBalance.Id}" }).ConfigureAwait(false);

                                if (tokenPrices?.TokensPrices.ContainsKey($"{defillamaPlatformId}:{nativeTokenDataBalance.Id}") == true)
                                {
                                    nativeTokenDataBalance.Price = tokenPrices.TokensPrices[$"{defillamaPlatformId}:{nativeTokenDataBalance.Id}"].Price;
                                }
                            }

                            counterpartyTransferBalances ??= new List<TokenDataBalance>();
                            var counterpartyNativeTokenData = new TokenDataBalance(nativeTokenDataBalance ?? new TokenDataBalance(), transactionsValue);
                            counterpartyTransferBalances.Add(counterpartyNativeTokenData);
                        }

                        extendedCounterpartiesData.Add(new ExtendedCounterpartyData(counterpartyData)
                        {
                            CounterpartyTurnoverUSD = counterpartyTransferBalances?.Sum(x => x.TotalAmountPrice),
                            CounterpartyTransactions = counterpartyTransactions.Count > 0 ? counterpartyTransactions.Count : null,
                            CounterpartyTransfers = counterpartyErc20Transfers.Count > 0 ? counterpartyErc20Transfers.Count : null,
                            CounterpartyTransactionHashes = counterpartyTransactionHashes.Count > 0 ? counterpartyTransactionHashes : null,
                            CounterpartyNFTTransfers = counterpartyTokenTransfers.Count > 0 ? counterpartyTokenTransfers : null,
                            CounterpartyTransferBalances = counterpartyTransferBalances,
                            ChainId = chainId
                        });
                    }
                }

                bool onlyCounterparties = request is IWalletCounterpartiesRequest { CalculateOnlyCounterparties: true };
                return new ExtendedCounterpartiesDataLists<TNormalTransaction, TInternalTransaction, TERC20TokenTransfer>
                {
                    ExtendedCounterpartiesDataList = extendedCounterpartiesData,
                    Transactions = onlyCounterparties ? filteredTransactions : transactions,
                    InternalTransactions = onlyCounterparties ? filteredInternalTransactions : internalTransactions,
                    Erc20Tokens = onlyCounterparties ? filteredErc20Transfers : erc20Tokens,
                    TokenTransfers = onlyCounterparties ? filteredTokenTransfers : tokenTransfers
                };
            }

            return null;
        }
    }
}