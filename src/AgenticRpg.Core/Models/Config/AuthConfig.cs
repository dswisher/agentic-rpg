// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AgenticRpg.Core.Models.Config
{
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global", Justification = "Deserialized from config file")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Deserialized from config file")]
    public class AuthConfig
    {
        public required Dictionary<string, AuthEntry> Profiles { get; init; }
    }
}
