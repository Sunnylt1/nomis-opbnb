// ------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nomis.BalanceChecker.Interfaces;
using Nomis.BalanceChecker.Settings;
using Nomis.Blockchain.Abstractions.Extensions;
using Nomis.CacheProviderService.Interfaces;
using Nomis.Utils.Extensions;

namespace Nomis.BalanceChecker.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Balance checker service.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/>.</param>
        /// <param name="configuration"><see cref="IConfiguration"/>.</param>
        /// <returns>Returns <see cref="IServiceCollection"/>.</returns>
        internal static IServiceCollection AddBalanceCheckerService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSettings<BalanceCheckerSettings>(configuration);
            var settings = configuration.GetSettings<BalanceCheckerSettings>();
            services
                .AddHttpClient<BalanceCheckerService>(client =>
                {
                    client.BaseAddress = new(settings.DeBankApiBaseUrl);
                    client.DefaultRequestHeaders.Add("AccessKey", settings.DeBankApiAccessKey);
                    client.Timeout = settings.HttpClientTimeout;
                })
                .AddRateLimitHandler(settings);
            return services
                .AddSingleton<IBalanceCheckerService, BalanceCheckerService>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<BalanceCheckerService>>();
                    var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(BalanceCheckerService));
                    var cacheProviderService = provider.GetRequiredService<ICacheProviderService>();
                    return new BalanceCheckerService(settings, client, cacheProviderService, logger);
                });
        }
    }
}