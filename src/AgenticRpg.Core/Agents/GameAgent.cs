// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using AgenticRpg.Core.Clients;

namespace AgenticRpg.Core.Agents
{
    public class GameAgent : IGameAgent
    {
        private readonly IChatClientWrapper wrapper;

        public GameAgent(IChatClientWrapper wrapper)
        {
            this.wrapper = wrapper;
        }


        public IGameSession CreateSession()
        {
            return new GameSession(wrapper);
        }
    }
}
