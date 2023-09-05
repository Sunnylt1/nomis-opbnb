// ------------------------------------------------------------------------------------------------------
// <copyright file="NonEvmScoreSoulboundTokenService.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Options;
using Nomis.SoulboundTokenService.Interfaces;
using Nomis.SoulboundTokenService.Interfaces.Models;
using Nomis.SoulboundTokenService.Interfaces.Requests;
using Nomis.SoulboundTokenService.Settings;
using Nomis.Utils.Contracts.NFT;
using Nomis.Utils.Contracts.Requests;
using Nomis.Utils.Contracts.Services;
using Nomis.Utils.Enums;
using Nomis.Utils.Wrapper;

namespace Nomis.SoulboundTokenService
{
    /// <inheritdoc cref="INonEvmScoreSoulboundTokenService"/>
    internal sealed class NonEvmScoreSoulboundTokenService :
        INonEvmScoreSoulboundTokenService,
        ITransientService
    {
        private readonly ScoreSoulboundTokenSettings _settings;

        /// <summary>
        /// Initialize <see cref="NonEvmScoreSoulboundTokenService"/>.
        /// </summary>
        /// <param name="settings"><see cref="ScoreSoulboundTokenSettings"/>.</param>
        public NonEvmScoreSoulboundTokenService(
            IOptions<ScoreSoulboundTokenSettings> settings)
        {
            _settings = settings.Value;
        }

        /// <inheritdoc />
        public Result<NFTSignature> GetMigrationSignature(
            ScoreMigrationRequest request)
        {
            // TODO - add implementation for all non EVM-compatible blockchains
            return Result<NFTSignature>.Fail(new() { Signature = null }, "Get migration signature: Verifying the contract signature for non EVM-compatible blockchains is not implemented yet.");
        }

        /// <inheritdoc />
        public Result<NFTSignature> GetSoulboundTokenSignature(
            ScoreSoulboundTokenRequest request)
        {
            // TODO - add implementation for all non EVM-compatible blockchains
            return Result<NFTSignature>.Fail(new() { Signature = null }, "Get token signature: Verifying the contract signature for non EVM-compatible blockchains is not implemented yet.");
        }

        /// <inheritdoc />
        public async Task<Result<NFTImage>> GetSoulboundTokenImageAsync(
            ScoreSoulboundTokenImageRequest request)
        {
            // TODO - add implementation for all non EVM-compatible blockchains
            return await Result<NFTImage>.FailAsync(new NFTImage(), "Get token image: for non EVM-compatible blockchains is not implemented yet.").ConfigureAwait(false);
        }

        public async Task<Result<NFTMetadata>> GetSoulboundTokenMetadataAsync(NFTMetadataRequest request)
        {
            // TODO - add implementation for all non EVM-compatible blockchains
            return await Result<NFTMetadata>.FailAsync(new NFTMetadata(), "Get token metadata: for non EVM-compatible blockchains is not implemented yet.").ConfigureAwait(false);
        }

        public async Task<Result<NFTMetadata>> GetONFTSoulboundTokenMetadataAsync(NFTMetadataRequest request)
        {
            // TODO - add implementation for all non EVM-compatible blockchains
            return await Result<NFTMetadata>.FailAsync(new NFTMetadata(), "Get token metadata: for non EVM-compatible blockchains is not implemented yet.").ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Result<IList<ScoringCalculationModelData>>> GetScoringCalculationModelsAsync()
        {
            var result = new List<ScoringCalculationModelData>();
            result.AddRange(Enum.GetValues<ScoringCalculationModel>().Select(x => new ScoringCalculationModelData
            {
                Model = x
            }));

            return await Result<IList<ScoringCalculationModelData>>.SuccessAsync(result, "Successfully got calculation models.").ConfigureAwait(false);
        }
    }
}