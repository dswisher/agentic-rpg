// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using AgenticRpg.Core.Models.World;

namespace AgenticRpg.Core.Engine
{
    public class SessionBuilder : ISessionBuilder
    {
        private readonly IWorldBuilder worldBuilder;

        public SessionBuilder(IWorldBuilder worldBuilder)
        {
            this.worldBuilder = worldBuilder;
        }


        public Session Build()
        {
            var world = worldBuilder.Build();
            var player = new Player
            {
                CurrentRoomId = world.StartingRoomId
            };

            return new Session
            {
                Player = player,
                World = world
            };
        }
    }
}
