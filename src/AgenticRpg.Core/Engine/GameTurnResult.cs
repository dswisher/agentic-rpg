// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

namespace AgenticRpg.Core.Engine
{
    public abstract record GameTurnResult
    {
        public sealed record Handled(CommandResult Result) : GameTurnResult;
        public sealed record DelegateToLlm(string Input) : GameTurnResult;
        public sealed record InvalidInput(string Reason) : GameTurnResult;
        public sealed record NoOp : GameTurnResult;
    }
}
