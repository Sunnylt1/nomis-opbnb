// ------------------------------------------------------------------------------------------------------
// <copyright file="BlockscoutController.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nomis.Blockscout.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Nomis.Api.Blockscout
{
    /// <summary>
    /// A controller to aggregate all Blockscout-related actions.
    /// </summary>
    [Route(BasePath)]
    [ApiVersion("1")]
    [SwaggerTag("Blockscout API.")]
    public sealed class BlockscoutController :
        ControllerBase
    {
        /// <summary>
        /// Base path for routing.
        /// </summary>
        internal const string BasePath = "api/v{version:apiVersion}/blockscout";

        /// <summary>
        /// Common tag for Blockscout actions.
        /// </summary>
        internal const string BlockscoutTag = "Blockscout";

        private readonly ILogger<BlockscoutController> _logger;
        private readonly IBlockscoutApiService _blockscoutApiService;

        /// <summary>
        /// Initialize <see cref="BlockscoutController"/>.
        /// </summary>
        /// <param name="blockscoutApiExplorerService"><see cref="IBlockscoutApiService"/>.</param>
        /// <param name="logger"><see cref="ILogger{T}"/>.</param>
        public BlockscoutController(
            IBlockscoutApiService blockscoutApiExplorerService,
            ILogger<BlockscoutController> logger)
        {
            _blockscoutApiService = blockscoutApiExplorerService ?? throw new ArgumentNullException(nameof(blockscoutApiExplorerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}