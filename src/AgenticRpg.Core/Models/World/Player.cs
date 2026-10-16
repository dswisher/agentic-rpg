// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

namespace AgenticRpg.Core.Models.World
{
    public class Player
    {
        /// <summary>
        /// The unique ID of the room where the player is currently located.
        /// </summary>
        public required string CurrentRoomId { get; set; }
    }
}
