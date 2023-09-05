// ------------------------------------------------------------------------------------------------------
// <copyright file="OpBnbBscscan.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nomis.OpBnbBscscan.Extensions;
using Nomis.OpBnbBscscan.Interfaces;
using Nomis.Utils.Contracts.Services;

namespace Nomis.OpBnbBscscan
{
    /// <summary>
    /// OpBnb bscscan service registrar.
    /// </summary>
    public sealed class OpBnbBscscan :
        IOpBnbServiceRegistrar
    {
        /// <inheritdoc/>
        public IServiceCollection RegisterService(
            IServiceCollection services,
            IConfiguration configuration)
        {
            return services
                .AddOpBnbBscscanService(configuration);
        }

        /// <inheritdoc/>
        public IInfrastructureService GetService(
            IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IOpBnbScoringService>();
        }
    }
}