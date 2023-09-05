// ------------------------------------------------------------------------------------------------------
// <copyright file="OpBnbBscscanClient.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Nomis.Blockchain.Abstractions.Clients;
using Nomis.OpBnbBscscan.Interfaces;
using Nomis.OpBnbBscscan.Settings;
using Nomis.Utils.Contracts;

namespace Nomis.OpBnbBscscan
{
    /// <inheritdoc cref="IOpBnbBscscanClient"/>
    internal sealed class OpBnbBscscanClient :
        BaseEvmClient<OpBnbBscscanSettings>,
        IOpBnbBscscanClient
    {
        /// <summary>
        /// Initialize <see cref="OpBnbBscscanClient"/>.
        /// </summary>
        /// <param name="settings"><see cref="OpBnbBscscanSettings"/>.</param>
        /// <param name="apiKeysPool"><see cref="IValuePool{TService,TValue}"/>.</param>
        /// <param name="client"><see cref="HttpClient"/>.</param>
        /// <param name="logger"><see cref="ILogger{TCategoryName}"/>.</param>
        public OpBnbBscscanClient(
            OpBnbBscscanSettings settings,
            IValuePool<OpBnbBscscanService, string> apiKeysPool,
            HttpClient client,
            ILogger<OpBnbBscscanClient> logger)
            : base(settings, client, logger, apiKeysPool.GetNextValue())
        {
        }
    }
}