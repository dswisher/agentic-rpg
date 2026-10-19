// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using AgenticRpg.Core.Commands;
using AgenticRpg.Core.Engine;
using AgenticRpg.Core.Models.World;
using AgenticRpg.Core.Parsing;
using AgenticRpg.UnitTests.TestHelpers;
using AwesomeAssertions;
using FakeItEasy;
using Xunit;

namespace AgenticRpg.UnitTests.Core.Engine
{
    public class GameCommandProcessorTests
    {
        private readonly IParser parser = A.Fake<IParser>();
        private readonly ICommandExecutor executor = A.Fake<ICommandExecutor>();
        private readonly GameCommandProcessor processor;


        public GameCommandProcessorTests()
        {
            processor = new GameCommandProcessor(parser, executor);
        }


        [Fact]
        public void DelegatesUnrecognizedInputToLlm()
        {
            // Arrange
            const string input = "tell me a story";
            const string delegatedInput = "tell me a story";
            A.CallTo(() => parser.Parse(input)).Returns(new ParseResult.DelegateToLlm(delegatedInput));

            // Act
            var result = processor.Process(TestWorldBuilder.CreateSession(), input);

            // Assert
            result.Should().BeOfType<GameTurnResult.DelegateToLlm>().Which.Input.Should().Be(delegatedInput);
            A.CallTo(() => executor.Execute(A<Session>._, A<GameCommand>._)).MustNotHaveHappened();
        }


        [Fact]
        public void ReturnsNoOpForEmptyInput()
        {
            // Arrange
            const string input = "  ";
            A.CallTo(() => parser.Parse(input)).Returns(new ParseResult.Empty());

            // Act
            var result = processor.Process(TestWorldBuilder.CreateSession(), input);

            // Assert
            result.Should().BeOfType<GameTurnResult.NoOp>();
            A.CallTo(() => executor.Execute(A<Session>._, A<GameCommand>._)).MustNotHaveHappened();
        }


        [Fact]
        public void ReturnsInvalidInputWithParserReason()
        {
            // Arrange
            const string input = "move";
            const string reason = "Missing argument";
            A.CallTo(() => parser.Parse(input)).Returns(new ParseResult.Invalid(reason));

            // Act
            var result = processor.Process(TestWorldBuilder.CreateSession(), input);

            // Assert
            result.Should().BeOfType<GameTurnResult.InvalidInput>().Which.Reason.Should().Be(reason);
            A.CallTo(() => executor.Execute(A<Session>._, A<GameCommand>._)).MustNotHaveHappened();
        }


        [Fact]
        public void ExecutesLocalCommandAndReturnsItsResult()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            const string input = "look";
            var command = new GameCommand.Look();
            var commandResult = new CommandResult.Completed("The room is quiet.");
            A.CallTo(() => parser.Parse(input)).Returns(new ParseResult.Local(command));
            A.CallTo(() => executor.Execute(session, command)).Returns(commandResult);

            // Act
            var result = processor.Process(session, input);

            // Assert
            result.Should().BeOfType<GameTurnResult.Handled>().Which.Result.Should().BeSameAs(commandResult);
            A.CallTo(() => executor.Execute(session, command)).MustHaveHappenedOnceExactly();
        }


        [Fact]
        public void ThrowsForUnknownParseResultType()
        {
            // Arrange
            const string input = "anything";
            A.CallTo(() => parser.Parse(input)).Returns(new UnknownParseResult());

            // Act
            Action act = () => processor.Process(TestWorldBuilder.CreateSession(), input);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Don't know how to handle ParseResult UnknownParseResult");
            A.CallTo(() => executor.Execute(A<Session>._, A<GameCommand>._)).MustNotHaveHappened();
        }


        private sealed record UnknownParseResult : ParseResult;
    }
}
