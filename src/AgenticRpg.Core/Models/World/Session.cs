// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

namespace AgenticRpg.Core.Models.World
{
    public class Session
    {
        public required World World { get; init; }
        public required Player Player { get; init; }
    }
}
