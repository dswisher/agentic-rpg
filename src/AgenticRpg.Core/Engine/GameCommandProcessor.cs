// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using AgenticRpg.Core.Models.World;
using AgenticRpg.Core.Parsing;

namespace AgenticRpg.Core.Engine
{
    public class GameCommandProcessor : IGameCommandProcessor
    {
        private readonly IParser parser;
        private readonly ICommandExecutor executor;

        public GameCommandProcessor(
            IParser parser,
            ICommandExecutor executor)
        {
            this.parser = parser;
            this.executor = executor;
        }


        public GameTurnResult Process(Session session, string input)
        {
            var parseResult = parser.Parse(input);

            return parseResult switch
            {
                ParseResult.DelegateToLlm delegated =>
                    new GameTurnResult.DelegateToLlm(delegated.Input),

                ParseResult.Empty =>
                    new GameTurnResult.NoOp(),

                ParseResult.Invalid invalid =>
                    new GameTurnResult.InvalidInput(invalid.Reason),

                ParseResult.Local local =>
                    new GameTurnResult.Handled(
                        executor.Execute(session, local.Command)),

                _ => throw new InvalidOperationException(
                    $"Don't know how to handle ParseResult {parseResult.GetType().Name}")
            };
        }
    }
}
