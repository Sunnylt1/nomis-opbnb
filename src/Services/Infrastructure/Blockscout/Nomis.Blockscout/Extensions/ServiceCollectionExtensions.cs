// ------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nomis.Blockscout.Interfaces;
using Nomis.Blockscout.Settings;
using Nomis.Utils.Extensions;

namespace Nomis.Blockscout.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Blockscout service.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/>.</param>
        /// <param name="configuration"><see cref="IConfiguration"/>.</param>
        /// <returns>Returns <see cref="IServiceCollection"/>.</returns>
        // ReSharper disable once InconsistentNaming
        public static IServiceCollection AddBlockscoutService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSettings<BlockscoutSettings>(configuration);
            var settings = configuration.GetSettings<BlockscoutSettings>();
            return services
                .AddTransient<IBlockscoutApiService, BlockscoutService>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<BlockscoutService>>();
                    return new BlockscoutService(settings, logger);
                });
        }
    }
}