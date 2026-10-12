// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

namespace AgenticRpg.Core.Models
{
    public class ConfigFileV1
    {
        public required string BearerToken { get; init; }
        public required string ModelId { get; init; }
        public required string RegionName { get; init; }
    }
}
