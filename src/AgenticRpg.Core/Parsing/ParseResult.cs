// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using AgenticRpg.Core.Commands;

namespace AgenticRpg.Core.Parsing
{
    public abstract record ParseResult
    {
        public sealed record Local(GameCommand Command) : ParseResult;
        public sealed record DelegateToLlm(string Input) : ParseResult;
        public sealed record Empty : ParseResult;
        public sealed record Invalid(string Reason) : ParseResult;
    }
}
