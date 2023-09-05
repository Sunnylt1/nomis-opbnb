﻿// ------------------------------------------------------------------------------------------------------
// <copyright file="BaseEvmClient.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using System.Net.Http.Json;

using Microsoft.Extensions.Logging;
using Nomis.Blockchain.Abstractions.Contracts.Models;
using Nomis.Blockchain.Abstractions.Contracts.Settings;
using Nomis.Blockchain.Abstractions.Enums;
using Nomis.Utils.Attributes.Logging;
using Nomis.Utils.Exceptions;

namespace Nomis.Blockchain.Abstractions.Clients
{
    /// <inheritdoc cref="IBaseEvmClient"/>
    public class BaseEvmClient<TSettings> :
        IBaseEvmClient
        where TSettings : IBlockchainSettings
    {
        private readonly TSettings _settings;
        private readonly HttpClient _client;
        private readonly string? _apiKey;

        /// <summary>
        /// Initialize <see cref="BaseEvmClient{TSettings}"/>.
        /// </summary>
        /// <param name="settings"><see cref="IBlockchainSettings"/>.</param>
        /// <param name="client"><see cref="HttpClient"/>.</param>
        /// <param name="logger"><see cref="ILogger{TCategoryName}"/>.</param>
        /// <param name="apiKey">API key.</param>
        public BaseEvmClient(
            TSettings settings,
            HttpClient client,
            ILogger logger,
            string? apiKey = null)
        {
            _settings = settings;

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _apiKey = apiKey;
                var maskedAttribute = new LogMaskedAttribute(showFirst: 3, preserveLength: true);
                object? maskedApiKey = maskedAttribute.MaskValue(_apiKey);
                ulong chainId = _settings.BlockchainDescriptors.TryGetValue(BlockchainKind.Mainnet, out var mainnetValue)
                    ? mainnetValue.ChainId : _settings.BlockchainDescriptors[BlockchainKind.Testnet].ChainId;
                logger.LogDebug("Used {ApiKey} API key for {ChainId} chain ID.", maskedApiKey, chainId);
            }

            _client = client;
        }

        /// <inheritdoc/>
        public virtual async Task<BaseEvmAccount> GetBalanceAsync(string address)
        {
            string request =
                $"/api?module=account&action=balance&address={address}&tag=latest";
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                request += $"&apiKey={_apiKey}";
            }

            var response = await _client.GetAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BaseEvmAccount>().ConfigureAwait(false) ?? throw new CustomException("Can't get account balance.");
        }

        /// <inheritdoc/>
        public virtual async Task<BaseEvmAccount> GetTokenBalanceAsync(string address, string contractAddress)
        {
            string request =
                $"/api?module=account&action=tokenbalance&address={address}&contractaddress={contractAddress}&tag=latest";
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                request += $"&apiKey={_apiKey}";
            }

            var response = await _client.GetAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BaseEvmAccount>().ConfigureAwait(false) ?? throw new CustomException("Can't get account token balance.");
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TResultItem>> GetTransactionsAsync<TResult, TResultItem>(string address)
            where TResult : IBaseEvmTransferList<TResultItem>
            where TResultItem : IBaseEvmTransfer
        {
            var result = new List<TResultItem>();
            var transactionsData = await GetTransactionList<TResult>(address).ConfigureAwait(false);
            result.AddRange(transactionsData.Data ?? new List<TResultItem>());
            while (transactionsData.Data?.Count >= _settings.ItemsFetchLimitPerRequest)
            {
                transactionsData = await GetTransactionList<TResult>(address, transactionsData.Data.LastOrDefault()?.BlockNumber).ConfigureAwait(false);
                result.AddRange(transactionsData.Data ?? new List<TResultItem>());
            }

            return result;
        }

        private async Task<TResult> GetTransactionList<TResult>(
            string address,
            string? startBlock = null)
        {
            string request =
                $"/api?module=account&address={address}&sort=asc";
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                request += $"&apiKey={_apiKey}";
            }

            if (typeof(TResult) == typeof(BaseEvmNormalTransactions))
            {
                request = $"{request}&action=txlist";
            }
            else if (typeof(TResult) == typeof(BaseEvmInternalTransactions))
            {
                request = $"{request}&action=txlistinternal";
            }
            else if (typeof(TResult) == typeof(BaseEvmERC20TokenTransfers))
            {
                request = $"{request}&action=tokentx";
            }
            else if (typeof(TResult) == typeof(BaseEvmERC721TokenTransfers))
            {
                request = $"{request}&action=tokennfttx";
            }
            else if (typeof(TResult) == typeof(BaseEvmERC1155TokenTransfers))
            {
                request = $"{request}&action=token1155tx";
            }
            else
            {
                return default!;
            }

            request = !string.IsNullOrWhiteSpace(startBlock)
                ? $"{request}&startblock={startBlock}"
                : $"{request}&startblock=0";

            request = $"{request}&endblock=999999999";

            var response = await _client.GetAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false) ?? throw new CustomException("Can't get account transactions.");
        }
    }
}