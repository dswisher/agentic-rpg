// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

namespace AgenticRpg.Core.Agents
{
    public abstract record ChatProgress
    {
        public sealed record ToolCallRequested(string Name) : ChatProgress;
    }
}
