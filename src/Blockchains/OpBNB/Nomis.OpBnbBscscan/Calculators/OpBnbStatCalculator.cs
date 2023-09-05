// ------------------------------------------------------------------------------------------------------
// <copyright file="OpBnbStatCalculator.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using System.Numerics;

using Nomis.Blockchain.Abstractions.Calculators;
using Nomis.Blockchain.Abstractions.Contracts.Data;
using Nomis.Blockchain.Abstractions.Contracts.Models;
using Nomis.Blockchain.Abstractions.Stats;
using Nomis.Chainanalysis.Interfaces.Models;
using Nomis.CyberConnect.Interfaces.Responses;
using Nomis.Greysafe.Interfaces.Models;
using Nomis.OpBnbBscscan.Interfaces.Extensions;
using Nomis.OpBnbBscscan.Interfaces.Models;
using Nomis.Snapshot.Interfaces.Responses;
using Nomis.Tally.Interfaces.Models;
using Nomis.Utils.Contracts.Calculators;

namespace Nomis.OpBnbBscscan.Calculators
{
    /// <summary>
    /// opBNB wallet stats calculator.
    /// </summary>
    internal sealed class OpBnbStatCalculator :
        BaseEvmStatCalculator<OpBnbWalletStats, OpBnbTransactionIntervalData>,
        IWalletNftStatsCalculator<OpBnbWalletStats, OpBnbTransactionIntervalData>
    {
        private readonly string _address;
        private readonly IEnumerable<BaseEvmInternalTransaction> _internalTransactions;
        private readonly IEnumerable<INFTTokenTransfer> _tokenTransfers;

        public OpBnbStatCalculator(
            string address,
            decimal balance,
            decimal usdBalance,
            decimal medianUsdBalance,
            IEnumerable<BaseEvmNormalTransaction> transactions,
            IEnumerable<BaseEvmInternalTransaction> internalTransactions,
            IEnumerable<INFTTokenTransfer> tokenTransfers,
            IEnumerable<BaseEvmERC20TokenTransfer> erc20TokenTransfers,
            SnapshotData? snapshotData,
            TallyAccount? tallyAccount,
            IEnumerable<TokenDataBalance>? tokenDataBalances,
            IEnumerable<GreysafeReport>? greysafeReports,
            IEnumerable<ChainanalysisReport>? chainanalysisReports,
            CyberConnectData? cyberConnectData)
            : base(address, balance, usdBalance, medianUsdBalance, transactions, erc20TokenTransfers, snapshotData, tallyAccount, tokenDataBalances, greysafeReports, chainanalysisReports, cyberConnectData, value => value.ToBnb())
        {
            _address = address;
            _internalTransactions = internalTransactions;
            _tokenTransfers = tokenTransfers;
        }

        /// <inheritdoc />
        IWalletNftStats IWalletNftStatsCalculator<OpBnbWalletStats, OpBnbTransactionIntervalData>.Stats()
        {
            var soldTokens = _tokenTransfers.Where(x => x.From?.Equals(_address, StringComparison.InvariantCultureIgnoreCase) == true).ToList();
            var soldSum = IWalletStatsCalculator
                .TokensSum(soldTokens.Select(x => x.Hash!), _internalTransactions.Select(x => (x.Hash!, BigInteger.TryParse(x.Value, out var amount) ? amount : 0)));

            var soldTokensIds = soldTokens.Select(x => x.GetTokenUid());
            var buyTokens = _tokenTransfers.Where(x => x.To?.Equals(_address, StringComparison.InvariantCultureIgnoreCase) == true && soldTokensIds.Contains(x.GetTokenUid()));
            var buySum = IWalletStatsCalculator
                .TokensSum(buyTokens.Select(x => x.Hash!), _internalTransactions.Select(x => (x.Hash!, BigInteger.TryParse(x.Value, out var amount) ? amount : 0)));

            var buyNotSoldTokens = _tokenTransfers.Where(x => x.To?.Equals(_address, StringComparison.InvariantCultureIgnoreCase) == true && !soldTokensIds.Contains(x.GetTokenUid()));
            var buyNotSoldSum = IWalletStatsCalculator
                .TokensSum(buyNotSoldTokens.Select(x => x.Hash!), _internalTransactions.Select(x => (x.Hash!, BigInteger.TryParse(x.Value, out var amount) ? amount : 0)));

            int holdingTokens = _tokenTransfers.Count() - soldTokens.Count;
            decimal nftWorth = buySum == 0 ? 0 : soldSum.ToNative() / buySum.ToNative() * buyNotSoldSum.ToNative();

            return new OpBnbWalletStats
            {
                NftHolding = holdingTokens,
                NftTrading = (soldSum - buySum).ToNative(),
                NftWorth = nftWorth
            };
        }
    }
}