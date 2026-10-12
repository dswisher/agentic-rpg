// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace AgenticRpg.Core.Models
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Deserialized from config file")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Deserialized from config file")]
    public class AuthEntry
    {
        public string? BearerToken { get; init; }
    }
}
