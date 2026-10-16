// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;

namespace AgenticRpg.Core.Models.World
{
    public class World
    {
        /// <summary>
        /// The rooms in this world.
        /// </summary>
        public List<Room> Rooms { get; init; } = [];

        /// <summary>
        /// The items in this world.
        /// </summary>
        public List<Item> Items { get; init; } = [];

        /// <summary>
        /// The unique ID of the room where the player starts the game.
        /// </summary>
        public required string StartingRoomId { get; init; }
    }
}
