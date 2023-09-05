// ------------------------------------------------------------------------------------------------------
// <copyright file="DeFiSettings.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Nomis.Utils.Contracts.Common;

namespace Nomis.DeFi.Settings
{
    /// <summary>
    /// De.Fi settings.
    /// </summary>
    internal class DeFiSettings :
        ISettings
    {
        /// <summary>
        /// API key.
        /// </summary>
        public string? ApiKey { get; init; }

        /// <summary>
        /// API base URL.
        /// </summary>
        /// <remarks>
        /// <see href="https://docs.de.fi/api/api"/>
        /// </remarks>
        public string? ApiBaseUrl { get; init; }
    }
}