// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

namespace AgenticRpg.Core.Engine
{
    public abstract record CommandResult
    {
        public sealed record Completed(string Message) : CommandResult;
        public sealed record Rejected(string Message) : CommandResult;
    }
}
