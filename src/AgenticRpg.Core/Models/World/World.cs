// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;

namespace AgenticRpg.Core.Models.World
{
    public class World
    {
        public List<Room> Rooms { get; init; } = [];
        public List<Item> Items { get; init; } = [];
    }
}
