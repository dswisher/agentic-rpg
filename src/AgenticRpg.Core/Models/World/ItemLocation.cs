// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

namespace AgenticRpg.Core.Models.World
{
    public abstract record ItemLocation
    {
        public sealed record InRoom(string RoomId) : ItemLocation;
        public sealed record InPlayerInventory : ItemLocation;
    }
}
