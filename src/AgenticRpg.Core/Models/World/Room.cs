// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;

namespace AgenticRpg.Core.Models.World
{
    public class Room
    {
        /// <summary>
        /// A unique ID for this room that never changes.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// The human-readable name for this room, such as "Throne Room".
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// A description of this room.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// The list of exits from this room.
        /// </summary>
        public List<Exit> Exits { get; init; } = [];
    }
}
