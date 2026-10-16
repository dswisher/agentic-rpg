// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

namespace AgenticRpg.Core.Models.World
{
    public class Item
    {
        /// <summary>
        /// A unique ID for this item that never changes.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// The human-readable name for this item, such as "Steel Shield".
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// A description of this item.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Where this item exists in the world.
        /// </summary>
        public required ItemLocation Location { get; set; }
    }
}
