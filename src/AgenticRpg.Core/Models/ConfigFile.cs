// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AgenticRpg.Core.Models
{
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global", Justification = "Deserialized from config file")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Deserialized from config file")]
    public class ConfigFile
    {
        public required string DefaultModel { get; init; }
        public required List<ModelConfig> Models { get; init; }
    }
}
