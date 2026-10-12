// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;

namespace AgenticRpg.Core.Models
{
    public class ConfigFileV2
    {
        public required string DefaultModel { get; init; }
        public required List<ModelConfig> Models { get; init; }
    }
}
