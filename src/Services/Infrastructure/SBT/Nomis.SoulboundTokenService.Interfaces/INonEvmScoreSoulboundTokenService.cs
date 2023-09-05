﻿// ------------------------------------------------------------------------------------------------------
// <copyright file="INonEvmScoreSoulboundTokenService.cs" company="Nomis">
// Copyright (c) Nomis, 2023. All rights reserved.
// The Application under the MIT license. See LICENSE file in the solution root for full license information.
// </copyright>
// ------------------------------------------------------------------------------------------------------

using Nomis.SoulboundTokenService.Interfaces.Contracts;
using Nomis.Utils.Contracts.Services;

namespace Nomis.SoulboundTokenService.Interfaces
{
    /// <summary>
    /// Non EVM score soulbound token service.
    /// </summary>
    public interface INonEvmScoreSoulboundTokenService :
        IScoreSoulboundTokenService,
        IInfrastructureService
    {
    }
}