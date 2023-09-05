﻿// ------------------------------------------------------------------------------------------------------
// <copyright file="BlockscoutApi.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nomis.Blockscout.Extensions;
using Nomis.Blockscout.Interfaces;
using Nomis.Utils.Contracts.Services;

namespace Nomis.Blockscout
{
    /// <summary>
    /// Blockscout API service registrar.
    /// </summary>
    public sealed class BlockscoutApi :
        IBlockscoutApiServiceRegistrar
    {
        /// <inheritdoc/>
        public IServiceCollection RegisterService(
            IServiceCollection services,
            IConfiguration configuration)
        {
            return services
                .AddBlockscoutService(configuration);
        }

        /// <inheritdoc/>
        public IInfrastructureService GetService(
            IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IBlockscoutApiService>();
        }
    }
}