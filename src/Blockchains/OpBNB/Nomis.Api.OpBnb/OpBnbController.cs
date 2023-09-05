// ------------------------------------------------------------------------------------------------------
// <copyright file="OpBnbController.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nomis.Api.Common.Swagger.Examples;
using Nomis.OpBnbBscscan.Interfaces;
using Nomis.OpBnbBscscan.Interfaces.Models;
using Nomis.OpBnbBscscan.Interfaces.Requests;
using Nomis.Utils.Enums;
using Nomis.Utils.Wrapper;
using Swashbuckle.AspNetCore.Annotations;

namespace Nomis.Api.OpBnb
{
    /// <summary>
    /// A controller to aggregate all opBNB-related actions.
    /// </summary>
    [Route(BasePath)]
    [ApiVersion("1")]
    [SwaggerTag("opBNB blockchain.")]
    public sealed class OpBnbController :
        ControllerBase
    {
        /// <summary>
        /// Base path for routing.
        /// </summary>
        internal const string BasePath = "api/v{version:apiVersion}/opbnb";

        /// <summary>
        /// Common tag for opBNB actions.
        /// </summary>
        internal const string OpBnbTag = "OpBnb";

        private readonly ILogger<OpBnbController> _logger;
        private readonly IOpBnbScoringService _scoringService;

        /// <summary>
        /// Initialize <see cref="OpBnbController"/>.
        /// </summary>
        /// <param name="scoringService"><see cref="IOpBnbScoringService"/>.</param>
        /// <param name="logger"><see cref="ILogger{T}"/>.</param>
        public OpBnbController(
            IOpBnbScoringService scoringService,
            ILogger<OpBnbController> logger)
        {
            _scoringService = scoringService ?? throw new ArgumentNullException(nameof(scoringService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get Nomis Score for given wallet address.
        /// </summary>
        /// <param name="request">Request for getting the wallet stats.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
        /// <returns>An Nomis Score value and corresponding statistical data.</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/opbnb/wallet/0x58FF099b5624a8EBf9b6D27f9FCaD4fd88a6b9f1/score?scoreType=0&amp;nonce=0&amp;deadline=1790647549
        /// </remarks>
        /// <response code="200">Returns Nomis Score and stats.</response>
        /// <response code="400">Address not valid.</response>
        /// <response code="404">No data found.</response>
        /// <response code="500">Unknown internal error.</response>
        [HttpGet("wallet/{address}/score", Name = "GetOpBnbWalletScore")]
        [SwaggerOperation(
            OperationId = "GetOpBnbWalletScore",
            Tags = new[] { OpBnbTag })]
        [ProducesResponseType(typeof(Result<OpBnbWalletScore>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResult<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RateLimitResult), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResult<string>), StatusCodes.Status500InternalServerError)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetOpBnbWalletScoreAsync(
            [Required(ErrorMessage = "Request should be set")] OpBnbWalletStatsRequest request,
            CancellationToken cancellationToken = default)
        {
            switch (request.ScoreType)
            {
                case ScoreType.Finance:
                    return Ok(await _scoringService.GetWalletStatsAsync<OpBnbWalletStatsRequest, OpBnbWalletScore, OpBnbWalletStats, OpBnbTransactionIntervalData>(request, cancellationToken));
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Get Nomis Score for given wallets addresses.
        /// </summary>
        /// <param name="requests">The list of requests for getting the wallets stats.</param>
        /// <param name="concurrentRequestCount">Concurrent request count.</param>
        /// <param name="delayInMilliseconds">Delay in milliseconds between calls.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
        /// <returns>An Nomis Score values and corresponding statistical data.</returns>
        /// <response code="200">Returns Nomis Scores and stats.</response>
        /// <response code="400">Addresses not valid.</response>
        /// <response code="404">No data found.</response>
        /// <response code="500">Unknown internal error.</response>
        [HttpPost("wallets/score", Name = "GetOpBnbWalletsScore")]
        [SwaggerOperation(
            OperationId = "GetOpBnbWalletsScore",
            Tags = new[] { OpBnbTag })]
        [ProducesResponseType(typeof(Result<List<OpBnbWalletScore>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResult<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RateLimitResult), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResult<string>), StatusCodes.Status500InternalServerError)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetOpBnbWalletsScoreAsync(
            [Required(ErrorMessage = "Request should be set"), FromBody] IList<OpBnbWalletStatsRequest> requests,
            int concurrentRequestCount = 1,
            int delayInMilliseconds = 500,
            CancellationToken cancellationToken = default)
        {
            if (requests.Any(r => r.ScoreType == ScoreType.Token))
            {
                throw new NotImplementedException();
            }

            return Ok(await _scoringService.GetWalletsStatsAsync<OpBnbWalletStatsRequest, OpBnbWalletScore, OpBnbWalletStats, OpBnbTransactionIntervalData>(requests, concurrentRequestCount, delayInMilliseconds, cancellationToken));
        }

        /// <summary>
        /// Get Nomis Score for given wallets addresses by file.
        /// </summary>
        /// <param name="request">Requests for getting the wallets stats parameters.</param>
        /// <param name="file">File with wallets addresses separated line by line.</param>
        /// <param name="concurrentRequestCount">Concurrent request count.</param>
        /// <param name="delayInMilliseconds">Delay in milliseconds between calls.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
        /// <returns>An Nomis Score values and corresponding statistical data.</returns>
        /// <response code="200">Returns Nomis Scores and stats.</response>
        /// <response code="400">Addresses not valid.</response>
        /// <response code="404">No data found.</response>
        /// <response code="500">Unknown internal error.</response>
        [HttpPost("wallets/score-by-file", Name = "GetOpBnbWalletsScoreByFile")]
        [SwaggerOperation(
            OperationId = "GetOpBnbWalletsScoreByFile",
            Tags = new[] { OpBnbTag })]
        [ProducesResponseType(typeof(Result<List<OpBnbWalletScore>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResult<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResult<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RateLimitResult), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ErrorResult<string>), StatusCodes.Status500InternalServerError)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetOpBnbWalletsScoreByFileAsync(
            [Required(ErrorMessage = "Request should be set")] OpBnbWalletStatsRequest request,
            IFormFile file,
            int concurrentRequestCount = 1,
            int delayInMilliseconds = 500,
            CancellationToken cancellationToken = default)
        {
            switch (request.ScoreType)
            {
                case ScoreType.Finance:
                    var wallets = new List<string?>();
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        while (reader.Peek() >= 0)
                        {
                            wallets.Add(await reader.ReadLineAsync(cancellationToken));
                        }
                    }

                    return Ok(await _scoringService.GetWalletsStatsAsync<OpBnbWalletStatsRequest, OpBnbWalletScore, OpBnbWalletStats, OpBnbTransactionIntervalData>(
                        wallets.Where(x => x != null).Cast<string>().Select(wallet => new OpBnbWalletStatsRequest
                        {
                            Address = wallet,
                            UseTokenLists = request.UseTokenLists,
                            CalculationModel = request.CalculationModel,
                            TokenAddress = request.TokenAddress,
                            Deadline = request.Deadline,
                            GetChainanalysisData = request.GetChainanalysisData,
                            GetCyberConnectProtocolData = request.GetCyberConnectProtocolData,
                            GetGreysafeData = request.GetGreysafeData,
                            GetHoldTokensBalances = request.GetHoldTokensBalances,
                            GetSnapshotProtocolData = request.GetSnapshotProtocolData,
                            IncludeUniversalTokenLists = request.IncludeUniversalTokenLists,
                            MintBlockchainType = request.MintBlockchainType,
                            Nonce = request.Nonce,
                            ScoreType = request.ScoreType,
                            SearchWidthInHours = request.SearchWidthInHours
                        }).ToList(),
                        concurrentRequestCount,
                        delayInMilliseconds,
                        cancellationToken));
                default:
                    throw new NotImplementedException();
            }
        }
    }
}