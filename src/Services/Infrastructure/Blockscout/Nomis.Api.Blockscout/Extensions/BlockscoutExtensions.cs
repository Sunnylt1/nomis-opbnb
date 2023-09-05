// ------------------------------------------------------------------------------------------------------
// <copyright file="BlockscoutExtensions.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Nomis.Api.Blockscout.Settings;
using Nomis.Api.Common.Extensions;
using Nomis.Blockscout.Interfaces;
using Nomis.ScoringService.Interfaces.Builder;

namespace Nomis.Api.Blockscout.Extensions
{
    /// <summary>
    /// Blockscout extension methods.
    /// </summary>
    public static class BlockscoutExtensions
    {
        /// <summary>
        /// Add Blockscout API.
        /// </summary>
        /// <typeparam name="TServiceRegistrar">The service registrar type.</typeparam>
        /// <param name="optionsBuilder"><see cref="IScoringOptionsBuilder"/>.</param>
        /// <returns>Returns <see cref="IScoringOptionsBuilder"/>.</returns>
        // ReSharper disable once InconsistentNaming
        public static IScoringOptionsBuilder WithBlockscoutAPI<TServiceRegistrar>(
            this IScoringOptionsBuilder optionsBuilder)
            where TServiceRegistrar : IBlockscoutApiServiceRegistrar, new()
        {
            return optionsBuilder
                .With<BlockscoutAPISettings, TServiceRegistrar>();
        }
    }
}