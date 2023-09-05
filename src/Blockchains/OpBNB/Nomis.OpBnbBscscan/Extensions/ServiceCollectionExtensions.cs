// ------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nomis.Blockchain.Abstractions.Extensions;
using Nomis.DexProviderService.Interfaces;
using Nomis.OpBnbBscscan.Interfaces;
using Nomis.OpBnbBscscan.Settings;
using Nomis.Utils.Contracts;
using Nomis.Utils.Extensions;

namespace Nomis.OpBnbBscscan.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add OpBnb bscscan service.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/>.</param>
        /// <param name="configuration"><see cref="IConfiguration"/>.</param>
        /// <returns>Returns <see cref="IServiceCollection"/>.</returns>
        internal static IServiceCollection AddOpBnbBscscanService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.CheckServiceDependencies(typeof(OpBnbBscscanService), typeof(IDexProviderService));
            services.AddSettings<OpBnbBscscanSettings>(configuration);
            var settings = configuration.GetSettings<OpBnbBscscanSettings>();
            services
                .AddSingleton<IValuePool<OpBnbBscscanService, string>>(_ => new ValuePool<OpBnbBscscanService, string>(settings.ApiKeys));
            services
                .AddHttpClient<OpBnbBscscanClient>(client =>
                {
                    client.BaseAddress = new(settings.ApiBaseUrl ?? "https://api-opbnb-testnet.bscscan.com/");
                })
                .AddRateLimitHandler();

            // .AddTraceLogHandler(_ => Task.FromResult(settings.UseHttpClientLogging));
            return services
                .AddTransient<IOpBnbBscscanClient, OpBnbBscscanClient>(provider =>
                {
                    var apiKeysPool = provider.GetRequiredService<IValuePool<OpBnbBscscanService, string>>();
                    var logger = provider.GetRequiredService<ILogger<OpBnbBscscanClient>>();
                    var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(OpBnbBscscanClient));
                    return new OpBnbBscscanClient(settings, apiKeysPool, client, logger);
                })
                .AddTransientInfrastructureService<IOpBnbScoringService, OpBnbBscscanService>();
        }
    }
}