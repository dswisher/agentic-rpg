// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using AgenticRpg.Core.Models;
using AgenticRpg.Core.Models.Config;

namespace AgenticRpg.Core.Clients
{
    public interface IBedrockClientFactory
    {
        IChatClientWrapper Create(ModelConfig modelConfig, AuthEntry? authEntry);
    }
}
