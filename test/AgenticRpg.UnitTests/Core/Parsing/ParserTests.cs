// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using AgenticRpg.Core.Commands;
using AgenticRpg.Core.Parsing;
using AwesomeAssertions;
using Xunit;

namespace AgenticRpg.UnitTests.Core.Parsing
{
    public class ParserTests
    {
        private readonly Parser parser = new();


        public static TheoryData<string, GameCommand> OneArgumentCommands => new()
        {
            { "move north", new GameCommand.Move("north") },
            { "move n", new GameCommand.Move("north") },
            { "MOVE U", new GameCommand.Move("up") },
            { " move north ", new GameCommand.Move("north") },
            { "move\tnorth", new GameCommand.Move("north") },
            { "drop loot", new GameCommand.Drop("loot") },
            { "take loot", new GameCommand.Take("loot") },
        };


        public static TheoryData<string, GameCommand.Move> DirectionAliases => new()
        {
            { "up", new GameCommand.Move("up") },
            { "u", new GameCommand.Move("up") },
            { "down", new GameCommand.Move("down") },
            { "d", new GameCommand.Move("down") },
            { "north", new GameCommand.Move("north") },
            { "n", new GameCommand.Move("north") },
            { "south", new GameCommand.Move("south") },
            { "s", new GameCommand.Move("south") },
            { "east", new GameCommand.Move("east") },
            { "e", new GameCommand.Move("east") },
            { "west", new GameCommand.Move("west") },
            { "w", new GameCommand.Move("west") },
        };


        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(" \t ")]
        public void EmptyInputIsHandled(string input)
        {
            // Act
            var parsed = parser.Parse(input);

            // Assert
            parsed.Should().BeOfType<ParseResult.Empty>();
        }


        [Theory]
        [InlineData("look", typeof(GameCommand.Look))]
        [InlineData("inventory", typeof(GameCommand.Inventory))]
        public void CanParseZeroArgumentLocalCommand(string input, Type expected)
        {
            // Act
            var parsed = parser.Parse(input);

            // Assert
            var local = parsed.Should().BeOfType<ParseResult.Local>().Which;

            local.Command.Should().BeOfType(expected);
        }


        [Theory]
        [InlineData("move", "Missing argument")]
        [InlineData("drop", "Missing argument")]
        [InlineData("take", "Missing argument")]
        public void InvalidOneArgumentCommandsAreHandled(string input, string reason)
        {
            // Act
            var parsed = parser.Parse(input);

            // Assert
            var invalid = parsed.Should().BeOfType<ParseResult.Invalid>().Which;

            invalid.Reason.Should().Be(reason);
        }


        [Theory]
        [MemberData(nameof(OneArgumentCommands))]
        public void CanParseOneArgumentLocalCommand(string input, GameCommand expected)
        {
            // Act
            var parsed = parser.Parse(input);

            // Assert
            var local = parsed.Should().BeOfType<ParseResult.Local>().Which;

            local.Command.Should().Be(expected);
        }


        [Theory]
        [MemberData(nameof(DirectionAliases))]
        public void CanParseDirectionAliasAsMove(string input, GameCommand.Move expected)
        {
            // Act
            var parsed = parser.Parse(input);

            // Assert
            var local = parsed.Should().BeOfType<ParseResult.Local>().Which;

            local.Command.Should().Be(expected);
        }


        [Theory]
        [InlineData("how are you today?")]
        [InlineData("monkey")]
        public void UnhandledInputIsDelegatedToTheAgent(string input)
        {
            // Act
            var parsed = parser.Parse(input);

            // Assert
            var delegated = parsed.Should().BeOfType<ParseResult.DelegateToLlm>().Which;

            delegated.Input.Should().Be(input);
        }
    }
}
