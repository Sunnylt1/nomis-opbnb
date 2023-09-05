// ------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nomis.Rapyd.Interfaces;
using Nomis.Rapyd.Settings;
using Nomis.Utils.Extensions;

namespace Nomis.Rapyd.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Rapyd payment API service.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/>.</param>
        /// <param name="configuration"><see cref="IConfiguration"/>.</param>
        /// <returns>Returns <see cref="IServiceCollection"/>.</returns>
        // ReSharper disable once InconsistentNaming
        internal static IServiceCollection AddRapydService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSettings<RapydSettings>(configuration);
            return services
                .AddSingletonInfrastructureService<IRapydService, RapydService>();
        }
    }
}