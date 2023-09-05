// ------------------------------------------------------------------------------------------------------
// <copyright file="OpBnbBscscanService.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using System.Numerics;
using System.Text.Json;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Util;
using Nomis.Blockchain.Abstractions;
using Nomis.Blockchain.Abstractions.Contracts;
using Nomis.Blockchain.Abstractions.Contracts.Data;
using Nomis.Blockchain.Abstractions.Contracts.Models;
using Nomis.Blockchain.Abstractions.Enums;
using Nomis.Blockchain.Abstractions.Extensions;
using Nomis.Blockchain.Abstractions.Requests;
using Nomis.CacheProviderService.Interfaces;
using Nomis.Chainanalysis.Interfaces;
using Nomis.Chainanalysis.Interfaces.Contracts;
using Nomis.Chainanalysis.Interfaces.Extensions;
using Nomis.CyberConnect.Interfaces;
using Nomis.CyberConnect.Interfaces.Contracts;
using Nomis.CyberConnect.Interfaces.Extensions;
using Nomis.DefiLlama.Interfaces;
using Nomis.DefiLlama.Interfaces.Models;
using Nomis.Dex.Abstractions.Contracts;
using Nomis.Dex.Abstractions.Enums;
using Nomis.DexProviderService.Interfaces;
using Nomis.DexProviderService.Interfaces.Contracts;
using Nomis.DexProviderService.Interfaces.Extensions;
using Nomis.DexProviderService.Interfaces.Requests;
using Nomis.Domain.Scoring.Entities;
using Nomis.Greysafe.Interfaces;
using Nomis.Greysafe.Interfaces.Contracts;
using Nomis.Greysafe.Interfaces.Extensions;
using Nomis.IPFS.Interfaces;
using Nomis.OpBnbBscscan.Calculators;
using Nomis.OpBnbBscscan.Interfaces;
using Nomis.OpBnbBscscan.Interfaces.Extensions;
using Nomis.OpBnbBscscan.Interfaces.Models;
using Nomis.OpBnbBscscan.Interfaces.Requests;
using Nomis.OpBnbBscscan.Settings;
using Nomis.PolygonId.Interfaces;
using Nomis.ReferralService.Interfaces;
using Nomis.ReferralService.Interfaces.Extensions;
using Nomis.ScoringService.Interfaces;
using Nomis.Snapshot.Interfaces;
using Nomis.Snapshot.Interfaces.Contracts;
using Nomis.Snapshot.Interfaces.Extensions;
using Nomis.SoulboundTokenService.Interfaces;
using Nomis.SoulboundTokenService.Interfaces.Extensions;
using Nomis.Tally.Interfaces;
using Nomis.Tally.Interfaces.Contracts;
using Nomis.Tally.Interfaces.Extensions;
using Nomis.Utils.Contracts;
using Nomis.Utils.Contracts.Requests;
using Nomis.Utils.Contracts.Services;
using Nomis.Utils.Contracts.Stats;
using Nomis.Utils.Exceptions;
using Nomis.Utils.Extensions;
using Nomis.Utils.Wrapper;

namespace Nomis.OpBnbBscscan
{
    /// <inheritdoc cref="IOpBnbScoringService"/>
    internal sealed class OpBnbBscscanService :
        BlockchainDescriptor,
        IOpBnbScoringService,
        ITransientService
    {
        private readonly OpBnbBscscanSettings _settings;
        private readonly IOpBnbBscscanClient _client;
        private readonly IScoringService _scoringService;
        private readonly IReferralService _referralService;
        private readonly IEvmScoreSoulboundTokenService _soulboundTokenService;
        private readonly ISnapshotService _snapshotService;
        private readonly ITallyService _tallyService;
        private readonly IDexProviderService _dexProviderService;
        private readonly IDefiLlamaService _defiLlamaService;
        private readonly IGreysafeService _greysafeService;
        private readonly IChainanalysisService _chainanalysisService;
        private readonly ICyberConnectService _cyberConnectService;
        private readonly IIPFSService _ipfsService;
        private readonly IPolygonIdService _polygonIdService;
        private readonly ICacheProviderService _cacheProviderService;

        /// <summary>
        /// Initialize <see cref="OpBnbBscscanService"/>.
        /// </summary>
        /// <param name="settings"><see cref="OpBnbBscscanSettings"/>.</param>
        /// <param name="client"><see cref="IOpBnbBscscanClient"/>.</param>
        /// <param name="scoringService"><see cref="IScoringService"/>.</param>
        /// <param name="referralService"><see cref="IReferralService"/>.</param>
        /// <param name="soulboundTokenService"><see cref="IEvmScoreSoulboundTokenService"/>.</param>
        /// <param name="snapshotService"><see cref="ISnapshotService"/>.</param>
        /// <param name="tallyService"><see cref="ITallyService"/>.</param>
        /// <param name="dexProviderService"><see cref="IDexProviderService"/>.</param>
        /// <param name="defiLlamaService"><see cref="IDefiLlamaService"/>.</param>
        /// <param name="greysafeService"><see cref="IGreysafeService"/>.</param>
        /// <param name="chainanalysisService"><see cref="IChainanalysisService"/>.</param>
        /// <param name="cyberConnectService"><see cref="ICyberConnectService"/>.</param>
        /// <param name="ipfsService"><see cref="IIPFSService"/>.</param>
        /// <param name="polygonIdService"><see cref="IPolygonIdService"/>.</param>
        /// <param name="cacheProviderService"><see cref="ICacheProviderService"/>.</param>
        /// <param name="logger"><see cref="ILogger{T}"/>.</param>
        public OpBnbBscscanService(
            IOptions<OpBnbBscscanSettings> settings,
            IOpBnbBscscanClient client,
            IScoringService scoringService,
            IReferralService referralService,
            IEvmScoreSoulboundTokenService soulboundTokenService,
            ISnapshotService snapshotService,
            ITallyService tallyService,
            IDexProviderService dexProviderService,
            IDefiLlamaService defiLlamaService,
            IGreysafeService greysafeService,
            IChainanalysisService chainanalysisService,
            ICyberConnectService cyberConnectService,
            IIPFSService ipfsService,
            IPolygonIdService polygonIdService,
            ICacheProviderService cacheProviderService,
            ILogger<OpBnbBscscanService> logger)
#pragma warning disable S3358
            : base(settings.Value.BlockchainDescriptors.TryGetValue(BlockchainKind.Mainnet, out var value) ? value : settings.Value.BlockchainDescriptors.TryGetValue(BlockchainKind.Testnet, out var testnetValue) ? testnetValue : null)
#pragma warning restore S3358
        {
            _settings = settings.Value;
            _client = client;
            _scoringService = scoringService;
            _referralService = referralService;
            _soulboundTokenService = soulboundTokenService;
            _snapshotService = snapshotService;
            _tallyService = tallyService;
            _dexProviderService = dexProviderService;
            _defiLlamaService = defiLlamaService;
            _greysafeService = greysafeService;
            _chainanalysisService = chainanalysisService;
            _cyberConnectService = cyberConnectService;
            _ipfsService = ipfsService;
            _polygonIdService = polygonIdService;
            _cacheProviderService = cacheProviderService;
            Logger = logger;
        }

        /// <inheritdoc/>
        public ILogger Logger { get; }

        /// <inheritdoc/>
        public async Task<Result<TWalletScore>> GetWalletStatsAsync<TWalletStatsRequest, TWalletScore, TWalletStats, TTransactionIntervalData>(
            TWalletStatsRequest request,
            CancellationToken cancellationToken = default)
            where TWalletStatsRequest : WalletStatsRequest
            where TWalletScore : IWalletScore<TWalletStats, TTransactionIntervalData>, new()
            where TWalletStats : class, IWalletCommonStats<TTransactionIntervalData>, new()
            where TTransactionIntervalData : class, ITransactionIntervalData
        {
            if (!new AddressUtil().IsValidAddressLength(request.Address) || !new AddressUtil().IsValidEthereumAddressHexFormat(request.Address))
            {
                throw new InvalidAddressException(request.Address);
            }

            var messages = new List<string>();

            #region Referral

            var ownReferralCodeResult = await _referralService.GetOwnReferralCodeAsync(request, _cacheProviderService, Logger, (request as BaseEvmWalletStatsRequest)?.ShouldGetReferrerCode ?? true, cancellationToken).ConfigureAwait(false);
            messages.AddRange(ownReferralCodeResult.Messages);
            string? ownReferralCode = ownReferralCodeResult.Data;

            #endregion Referral

            var mintBlockchain = _dexProviderService.MintChain(request, ChainId);

            TWalletStats? walletStats = null;
            bool calculateNewScore = false;
            if (_settings.GetFromCacheStatsIsEnabled)
            {
                walletStats = await _cacheProviderService.GetFromCacheAsync<OpBnbWalletStats>($"{request.Address}_{ChainId}_{(int)request.CalculationModel}_{(int)request.ScoreType}").ConfigureAwait(false) as TWalletStats;
            }

            if (walletStats == null)
            {
                calculateNewScore = true;
                string? balanceWei = (await _client.GetBalanceAsync(request.Address).ConfigureAwait(false)).Balance;
                TokenPriceData? priceData = null;
                (await _defiLlamaService.TokensPriceAsync(new List<string> { $"coingecko:{PlatformIds?[BlockchainPlatform.Coingecko]}" }).ConfigureAwait(false))?.TokensPrices.TryGetValue($"coingecko:{PlatformIds?[BlockchainPlatform.Coingecko]}", out priceData);
                decimal usdBalance = (priceData?.Price ?? 0M) * balanceWei!.ToBnb();
                var tokenTransfers = new List<INFTTokenTransfer>();
                var transactions = (await _client.GetTransactionsAsync<BaseEvmNormalTransactions, BaseEvmNormalTransaction>(request.Address).ConfigureAwait(false)).ToList();
                if (!transactions.Any())
                {
                    return await Result<TWalletScore>.FailAsync(
                        new()
                        {
                            Address = request.Address,
                            Stats = new TWalletStats
                            {
                                NoData = true
                            },
                            Score = 0
                        }, new List<string> { "There is no transactions for this wallet." }).ConfigureAwait(false);
                }

                if (!transactions.Any())
                {
                    return await Result<TWalletScore>.FailAsync(
                        new()
                        {
                            Address = request.Address,
                            Stats = new TWalletStats
                            {
                                NoData = true
                            },
                            Score = 0
                        }, new List<string> { "There is no transactions for this wallet." }).ConfigureAwait(false);
                }

                var erc20Tokens = (await _client.GetTransactionsAsync<BaseEvmERC20TokenTransfers, BaseEvmERC20TokenTransfer>(request.Address).ConfigureAwait(false)).ToList();
                var internalTransactions = (await _client.GetTransactionsAsync<BaseEvmInternalTransactions, BaseEvmInternalTransaction>(request.Address).ConfigureAwait(false)).ToList();
                var erc721Tokens = (await _client.GetTransactionsAsync<BaseEvmERC721TokenTransfers, BaseEvmERC721TokenTransfer>(request.Address).ConfigureAwait(false)).ToList();

                // var erc1155Tokens = (await _client.GetTransactionsAsync<BaseEvmERC1155TokenTransfers, BaseEvmERC1155TokenTransfer>(request.Address).ConfigureAwait(false)).ToList();
                // tokenTransfers.AddRange(erc1155Tokens);
                tokenTransfers.AddRange(erc721Tokens);

                #region Greysafe scam reports

                var greysafeReportsResponse = await _greysafeService.ReportsAsync(request as IWalletGreysafeRequest).ConfigureAwait(false);

                #endregion Greysafe scam reports

                #region Chainanalysis sanctions reports

                var chainanalysisReportsResponse = await _chainanalysisService.ReportsAsync(request as IWalletChainanalysisRequest).ConfigureAwait(false);

                #endregion Chainanalysis sanctions reports

                #region Snapshot protocol

                var snapshotData = await _snapshotService.DataAsync(request as IWalletSnapshotProtocolRequest, ChainId).ConfigureAwait(false);

                #endregion Snapshot protocol

                #region Tally protocol

                var tallyAccountData = await _tallyService.AccountDataAsync(request as IWalletTallyProtocolRequest, ChainId).ConfigureAwait(false);

                #endregion Tally protocol

                #region CyberConnect protocol

                var cyberConnectData = await _cyberConnectService.DataAsync(request as IWalletCyberConnectProtocolRequest, ChainId).ConfigureAwait(false);

                #endregion CyberConnect protocol

                #region Tokens data

                var tokenDataBalances = new List<TokenDataBalance>();
                if ((request as IWalletTokensSwapPairsRequest)?.GetTokensSwapPairs == true
                    || (request as IWalletTokensBalancesRequest)?.GetHoldTokensBalances == true)
                {
                    foreach (string? erc20TokenContractId in erc20Tokens.Select(x => x.ContractAddress).Distinct())
                    {
                        var tokenDataBalance = await _client.GetTokenDataBalanceAsync(request.Address, erc20TokenContractId!, ChainId).ConfigureAwait(false);
                        if (tokenDataBalance != null)
                        {
                            tokenDataBalances.Add(tokenDataBalance);
                        }
                    }
                }

                #endregion Tokens data

                #region Tokens balances (DefiLlama)

                var dexTokensData = new List<DexTokenData>();
                if (request is IWalletTokensBalancesRequest balancesRequest)
                {
                    var tokenPrices = await _defiLlamaService.TokensPriceAsync(
                        tokenDataBalances.Select(t => $"{PlatformIds?[BlockchainPlatform.DefiLLama]}:{t.Id}").ToList(), balancesRequest.SearchWidthInHours).ConfigureAwait(false);
                    foreach (var tokenDataBalance in tokenDataBalances)
                    {
                        if (tokenPrices?.TokensPrices.ContainsKey($"{PlatformIds?[BlockchainPlatform.DefiLLama]}:{tokenDataBalance.Id}") == true)
                        {
                            var tokenPrice = tokenPrices.TokensPrices[$"{PlatformIds?[BlockchainPlatform.DefiLLama]}:{tokenDataBalance.Id}"];
                            tokenDataBalance.Confidence = tokenPrice.Confidence;
                            tokenDataBalance.LastPriceDateTime = tokenPrice.LastPriceDateTime;
                            tokenDataBalance.Price = tokenPrice.Price;
                            tokenDataBalance.Decimals ??= tokenPrice.Decimals?.ToString();
                            tokenDataBalance.Symbol ??= tokenPrice.Symbol;
                        }
                    }

                    if (balancesRequest.UseTokenLists)
                    {
                        var dexTokensDataResult = await _dexProviderService.TokensDataAsync(new TokensDataRequest
                        {
                            Blockchain = (Chain)ChainId,
                            IncludeUniversalTokenLists = balancesRequest.IncludeUniversalTokenLists,
                            FromCache = true
                        }).ConfigureAwait(false);
                        dexTokensData = dexTokensDataResult.Data;

                        foreach (var tokenDataBalance in tokenDataBalances)
                        {
                            var dexTokenData = dexTokensDataResult.Data.Find(t => t.Id?.Equals(tokenDataBalance.Id, StringComparison.OrdinalIgnoreCase) == true);
                            tokenDataBalance.LogoUri ??= dexTokenData?.LogoUri;
                            tokenDataBalance.Decimals ??= dexTokenData?.Decimals;
                            tokenDataBalance.Symbol ??= dexTokenData?.Symbol;
                            tokenDataBalance.Name ??= dexTokenData?.Name;
                        }
                    }

                    tokenDataBalances = tokenDataBalances
                        .Where(b => b.TotalAmountPrice > 0)
                        .OrderByDescending(b => b.TotalAmountPrice)
                        .ThenByDescending(b => b.Balance)
                        .ThenBy(b => b.Symbol)
                        .ToList();
                }

                #endregion Tokens balances

                #region Swap pairs from DEXes

                var dexTokenSwapPairs = new List<DexTokenSwapPairsData>();
                if ((request as IWalletTokensSwapPairsRequest)?.GetTokensSwapPairs == true && tokenDataBalances.Any())
                {
                    var swapPairsResult = await _dexProviderService.BlockchainSwapPairsAsync(new()
                    {
                        Blockchain = (Chain)ChainId,
                        First = (request as IWalletTokensSwapPairsRequest)?.FirstSwapPairs ?? 100,
                        Skip = (request as IWalletTokensSwapPairsRequest)?.Skip ?? 0,
                        FromCache = false
                    }).ConfigureAwait(false);
                    if (swapPairsResult.Succeeded)
                    {
                        dexTokenSwapPairs.AddRange(tokenDataBalances.Select(t =>
                            DexTokenSwapPairsData.ForSwapPairs(t.Id!, t.Balance, swapPairsResult.Data, dexTokensData)));
                        dexTokenSwapPairs.RemoveAll(p => !p.TokenSwapPairs.Any());
                    }
                }

                #endregion Swap pairs from DEXes

                #region Median USD balance

                decimal medianUsdBalance = await _scoringService.MedianBalanceUsdAsync<OpBnbWalletStats>(request.Address, ChainId, request.CalculationModel, _settings, usdBalance + (tokenDataBalances.Any() ? tokenDataBalances : null)?.Sum(b => b.TotalAmountPrice) ?? 0, cancellationToken).ConfigureAwait(false);

                #endregion Median USD balance

                walletStats = new OpBnbStatCalculator(
                        request.Address,
                        decimal.TryParse(balanceWei, out decimal weiBalance) ? weiBalance : 0,
                        usdBalance,
                        medianUsdBalance,
                        transactions,
                        internalTransactions,
                        tokenTransfers,
                        erc20Tokens,
                        snapshotData,
                        tallyAccountData,
                        tokenDataBalances,
                        greysafeReportsResponse?.Reports,
                        chainanalysisReportsResponse?.Identifications,
                        cyberConnectData)
                .Stats() as TWalletStats;

                await _cacheProviderService.SetCacheAsync($"{request.Address}_{ChainId}_{(int)request.CalculationModel}_{(int)request.ScoreType}", walletStats!, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _settings.GetFromCacheStatsTimeLimit
                }).ConfigureAwait(false);
            }

            double score = walletStats!.CalculateScore<TWalletStats, TTransactionIntervalData>(ChainId, request.CalculationModel);

            if (calculateNewScore)
            {
                var scoringData = new ScoringData(request.Address, request.Address, request.CalculationModel, JsonSerializer.Serialize(request), ChainId, score, JsonSerializer.Serialize(walletStats));
                await _scoringService.SaveScoringDataToDatabaseAsync(scoringData, cancellationToken).ConfigureAwait(false);
            }

            var metadataResult = await _soulboundTokenService.TokenMetadataAsync(_ipfsService, _cacheProviderService, request, ChainId, ChainName, score).ConfigureAwait(false);

            // getting signature
            ushort mintedScore = (ushort)(score * 10000);
            var signatureResult = await _soulboundTokenService
                .SignatureAsync(request, mintedScore, mintBlockchain?.ChainId ?? request.GetChainId(_settings), mintBlockchain?.SBTData ?? request.GetSBTData(_settings), metadataResult.Data, ChainId, ownReferralCode ?? "anon", request.ReferrerCode ?? "nomis", (request as IWalletGreysafeRequest)?.GetGreysafeData, (request as IWalletChainanalysisRequest)?.GetChainanalysisData).ConfigureAwait(false);
            messages.AddRange(signatureResult.Messages);
            messages.Add($"Got {ChainName} wallet {request.ScoreType.ToString()} score.");

            #region DID

            var didDataResult = await _polygonIdService.CreateClaimAndGetQrAsync<OpBnbWalletStatsRequest, OpBnbWalletStats, OpBnbTransactionIntervalData>((request as OpBnbWalletStatsRequest) !, mintedScore, (walletStats as OpBnbWalletStats) !, DateTime.UtcNow.AddYears(5).ConvertToTimestamp(), ChainId, cancellationToken).ConfigureAwait(false);
            messages.Add(didDataResult.Messages.FirstOrDefault() ?? string.Empty);

            #endregion DID

            return await Result<TWalletScore>.SuccessAsync(
                new()
                {
                    Address = request.Address,
                    Stats = walletStats,
                    Score = score,
                    MintData = request.PrepareToMint ? new MintData(signatureResult.Data.Signature, mintedScore, request.CalculationModel, request.Deadline, metadataResult.Data, ChainId, mintBlockchain ?? this, ownReferralCode ?? "anon", request.ReferrerCode ?? "nomis") : null,
                    DIDData = didDataResult.Data,
                    ReferralCode = ownReferralCode,
                    ReferrerCode = request.ReferrerCode
                }, messages).ConfigureAwait(false);
        }
    }
}