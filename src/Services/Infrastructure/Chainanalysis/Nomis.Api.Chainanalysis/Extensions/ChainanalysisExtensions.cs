﻿// ------------------------------------------------------------------------------------------------------
// <copyright file="ChainanalysisExtensions.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Nomis.Api.Chainanalysis.Settings;
using Nomis.Api.Common.Extensions;
using Nomis.Chainanalysis.Interfaces;
using Nomis.ScoringService.Interfaces.Builder;

namespace Nomis.Api.Chainanalysis.Extensions
{
    /// <summary>
    /// Chainanalysis extension methods.
    /// </summary>
    public static class ChainanalysisExtensions
    {
        /// <summary>
        /// Add Chainanalysis sanctions reporting service.
        /// </summary>
        /// <typeparam name="TServiceRegistrar">The service registrar type.</typeparam>
        /// <param name="optionsBuilder"><see cref="IScoringOptionsBuilder"/>.</param>
        /// <returns>Returns <see cref="IScoringOptionsBuilder"/>.</returns>
        // ReSharper disable once InconsistentNaming
        public static IScoringOptionsBuilder WithChainanalysisService<TServiceRegistrar>(
            this IScoringOptionsBuilder optionsBuilder)
            where TServiceRegistrar : IChainanalysisServiceRegistrar, new()
        {
            return optionsBuilder
                .With<ChainanalysisAPISettings, TServiceRegistrar>();
        }
    }
}