// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

namespace AgenticRpg.Core.Models.World
{
    public class Exit
    {
        /// <summary>
        /// The name for this exit, such as "north" or "down".
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The Id of the room where this exit leads.
        /// </summary>
        public required string DestinationRoomId { get; set; }
    }
}
