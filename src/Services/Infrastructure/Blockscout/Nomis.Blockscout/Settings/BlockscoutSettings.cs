// ------------------------------------------------------------------------------------------------------
// <copyright file="BlockscoutSettings.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Nomis.Blockscout.Interfaces.Enums;
using Nomis.Utils.Contracts.Common;

namespace Nomis.Blockscout.Settings
{
    /// <summary>
    /// Blockscout settings.
    /// </summary>
    internal class BlockscoutSettings :
        ISettings
    {
        /// <summary>
        /// API base URLs for supported blockchains.
        /// </summary>
        public Dictionary<BlockscoutChain, string> ApiBaseUrls { get; init; } = new();

        /// <summary>
        /// API keys for supported blockchains.
        /// </summary>
        public Dictionary<BlockscoutChain, List<string>> ApiKeys { get; init; } = new();
    }
}