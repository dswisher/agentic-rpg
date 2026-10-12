// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace AgenticRpg.Core.Models
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Deserialized from config file")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Deserialized from config file")]
    public class ModelConfig
    {
        public required string Name { get; init; }
        public required string Provider { get; init; }
        public required string ModelId { get; init; }

        public ModelOptions? Options { get; init; }
        public string? AuthProfile { get; init; }
    }
}
