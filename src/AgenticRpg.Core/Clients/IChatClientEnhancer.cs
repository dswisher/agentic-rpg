// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using Microsoft.Extensions.AI;

namespace AgenticRpg.Core.Clients
{
    public interface IChatClientEnhancer
    {
        IChatClient Enhance(IChatClient original);
    }
}
