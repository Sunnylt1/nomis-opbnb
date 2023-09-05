// ------------------------------------------------------------------------------------------------------
// <copyright file="RapydApi.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nomis.Rapyd.Extensions;
using Nomis.Rapyd.Interfaces;
using Nomis.Utils.Contracts.Services;

namespace Nomis.Rapyd
{
    /// <summary>
    /// Rapyd payment API service registrar.
    /// </summary>
    public sealed class RapydApi :
        IRapydServiceRegistrar
    {
        /// <inheritdoc/>
        public IServiceCollection RegisterService(
            IServiceCollection services,
            IConfiguration configuration)
        {
            return services
                .AddRapydService(configuration);
        }

        /// <inheritdoc/>
        public IInfrastructureService GetService(
            IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IRapydService>();
        }
    }
}