﻿// ------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nomis.DeFi.Interfaces;
using Nomis.DeFi.Settings;
using Nomis.Utils.Extensions;

namespace Nomis.DeFi.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add De.Fi service.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/>.</param>
        /// <param name="configuration"><see cref="IConfiguration"/>.</param>
        /// <returns>Returns <see cref="IServiceCollection"/>.</returns>
        // ReSharper disable once InconsistentNaming
        public static IServiceCollection AddDeFiService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSettings<DeFiSettings>(configuration);
            var settings = configuration.GetSettings<DeFiSettings>();
            services.AddSingleton<IDeFiGraphQLClient>(_ =>
            {
                var graphQlOptions = new GraphQLHttpClientOptions
                {
                    EndPoint = new(settings.ApiBaseUrl ?? "https://public-api.de.fi/graphql")
                };

                var client = new DeFiGraphQLClient(graphQlOptions, new SystemTextJsonSerializer());
                client.HttpClient.DefaultRequestHeaders.Add("X-Api-Key", settings.ApiKey);
                return client;
            });
            return services
                .AddSingletonInfrastructureService<IDeFiService, DeFiService>();
        }
    }
}