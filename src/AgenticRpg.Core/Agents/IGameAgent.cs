// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

namespace AgenticRpg.Core.Agents
{
    public interface IGameAgent
    {
        IGameSessionHandle CreateSession();
    }
}
