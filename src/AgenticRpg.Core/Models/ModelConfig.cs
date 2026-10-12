// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

namespace AgenticRpg.Core.Models
{
    public class ModelConfig
    {
        public required string Name { get; init; }
        public required string Provider { get; init; }
        public required string ModelId { get; init; }

        public ModelOptions? Options { get; init; }
        public string? AuthProfile { get; init; }
    }
}
