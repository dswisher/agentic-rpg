// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using AgenticRpg.Core.Commands;

namespace AgenticRpg.Core.Parsing
{
    public class Parser : IParser
    {
        public ParseResult Parse(string input)
        {
            // Split on whitespace
            var words = input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (words.Length == 0)
            {
                return new ParseResult.Empty();
            }

            var verb = words[0].ToLowerInvariant();

            if (words.Length == 1)
            {
                return verb switch
                {
                    "inventory" => new ParseResult.Local(new GameCommand.Inventory()),
                    "look" => new ParseResult.Local(new GameCommand.Look()),

                    "move" or "take" or "drop" => new ParseResult.Invalid("Missing argument"),

                    _ => new ParseResult.DelegateToLlm(input)
                };
            }

            if (words.Length == 2)
            {
                return verb switch
                {
                    "move" => new ParseResult.Local(new GameCommand.Move(words[1])),
                    "drop" => new ParseResult.Local(new GameCommand.Drop(words[1])),
                    "take" => new ParseResult.Local(new GameCommand.Take(words[1])),

                    _ => new ParseResult.DelegateToLlm(input)
                };
            }

            return new ParseResult.DelegateToLlm(input);
        }
    }
}
