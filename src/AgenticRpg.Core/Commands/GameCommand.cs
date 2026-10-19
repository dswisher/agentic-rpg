// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

namespace AgenticRpg.Core.Commands
{
    public abstract record GameCommand
    {
        public sealed record Inventory() : GameCommand;
        public sealed record Look() : GameCommand;
        public sealed record Move(string Direction) : GameCommand;
        public sealed record Take(string ItemName) : GameCommand;
    }
}
