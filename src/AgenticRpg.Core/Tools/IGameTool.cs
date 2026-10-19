// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using AgenticRpg.Core.Models.World;
using Microsoft.Extensions.AI;

namespace AgenticRpg.Core.Tools
{
    /// <summary>
    /// Creates a game tool bound to the active game session for one LLM request.
    /// </summary>
    public interface IGameTool
    {
        AIFunction Create(Session session);
    }
}
