// ------------------------------------------------------------------------------------------------------
// <copyright file="OpBnbExtensions.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Nomis.Api.Common.Extensions;
using Nomis.Api.OpBnb.Settings;
using Nomis.OpBnbBscscan.Interfaces;
using Nomis.ScoringService.Interfaces.Builder;

namespace Nomis.Api.OpBnb.Extensions
{
    /// <summary>
    /// opBNB extension methods.
    /// </summary>
    public static class OpBnbExtensions
    {
        /// <summary>
        /// Add opBNB blockchain.
        /// </summary>
        /// <typeparam name="TServiceRegistrar">The service registrar type.</typeparam>
        /// <param name="optionsBuilder"><see cref="IScoringOptionsBuilder"/>.</param>
        /// <returns>Returns <see cref="IScoringOptionsBuilder"/>.</returns>
        // ReSharper disable once InconsistentNaming
        public static IScoringOptionsBuilder WithOpBNBBlockchain<TServiceRegistrar>(
            this IScoringOptionsBuilder optionsBuilder)
            where TServiceRegistrar : IOpBnbServiceRegistrar, new()
        {
            return optionsBuilder
                .With<OpBnbAPISettings, TServiceRegistrar>();
        }
    }
}