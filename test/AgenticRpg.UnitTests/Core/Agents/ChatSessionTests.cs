// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AgenticRpg.Core.Agents;
using AgenticRpg.Core.Clients;
using AgenticRpg.Core.Models.World;
using AgenticRpg.Core.Tools;
using AwesomeAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AgenticRpg.UnitTests.Core.Agents
{
    public class ChatSessionTests
    {
        [Fact]
        public async Task GameToolIsBoundToTheSessionForThatRequest()
        {
            // Arrange
            var chatClient = new FakeChatClient();
            using var wrapper = new SimpleClientWrapper(chatClient);
            var gameTool = new CurrentRoomIdTool();
            var chatSession = new ChatSession(
                wrapper,
                [gameTool, new DieRoller(NullLogger<DieRoller>.Instance)]);
            var gameSession = CreateSession("first-room");

            // Act
            await chatSession.SendAsync("What room am I in?", gameSession);

            // Assert
            gameTool.Sessions.Should().ContainSingle().Which.Should().BeSameAs(gameSession);

            var function = chatClient.Options[0].Tools!
                .OfType<AIFunction>()
                .Should().ContainSingle(tool => tool.Name == "get_current_room_id")
                .Which;
            var result = await function.InvokeAsync(new AIFunctionArguments(), CancellationToken.None);

            result?.ToString().Should().Be("first-room");
            var diceFunction = chatClient.Options[0].Tools!
                .OfType<AIFunction>()
                .Should().ContainSingle(tool => tool.Name == "roll_dice")
                .Which;
            var diceResult = await diceFunction.InvokeAsync(
                new AIFunctionArguments { ["sides"] = 6 },
                CancellationToken.None);
            diceResult.Should().BeOfType<JsonElement>().Which.GetInt32().Should().BeInRange(1, 6);
        }


        [Fact]
        public async Task GameToolsAreBoundSeparatelyForEachRequest()
        {
            // Arrange
            var chatClient = new FakeChatClient();
            using var wrapper = new SimpleClientWrapper(chatClient);
            var gameTool = new CurrentRoomIdTool();
            var chatSession = new ChatSession(
                wrapper,
                [gameTool, new DieRoller(NullLogger<DieRoller>.Instance)]);
            var firstSession = CreateSession("first-room");
            var secondSession = CreateSession("second-room");

            // Act
            await chatSession.SendAsync("Where am I?", firstSession);
            await chatSession.SendAsync("Where am I now?", secondSession);

            // Assert
            gameTool.Sessions.Should().HaveCount(2);
            gameTool.Sessions[0].Should().BeSameAs(firstSession);
            gameTool.Sessions[1].Should().BeSameAs(secondSession);

            var firstFunction = chatClient.Options[0].Tools!
                .OfType<AIFunction>()
                .Should().ContainSingle(tool => tool.Name == "get_current_room_id")
                .Which;
            var secondFunction = chatClient.Options[1].Tools!
                .OfType<AIFunction>()
                .Should().ContainSingle(tool => tool.Name == "get_current_room_id")
                .Which;

            (await firstFunction.InvokeAsync(new AIFunctionArguments(), CancellationToken.None))
                ?.ToString().Should().Be("first-room");
            (await secondFunction.InvokeAsync(new AIFunctionArguments(), CancellationToken.None))
                ?.ToString().Should().Be("second-room");
        }


        private static Session CreateSession(string roomId) => new()
        {
            World = new World { StartingRoomId = roomId },
            Player = new Player { CurrentRoomId = roomId }
        };


        private sealed class CurrentRoomIdTool : IGameTool
        {
            public List<Session> Sessions { get; } = [];

            public AIFunction Create(Session session)
            {
                Sessions.Add(session);

                return AIFunctionFactory.Create(
                    () => session.Player.CurrentRoomId,
                    name: "get_current_room_id",
                    description: "Return the current room ID for a test.");
            }
        }


        private sealed class FakeChatClient : IChatClient
        {
            public List<ChatOptions> Options { get; } = [];

            public Task<ChatResponse> GetResponseAsync(
                IEnumerable<ChatMessage> messages,
                ChatOptions? options = null,
                CancellationToken cancellationToken = default) =>
                throw new NotSupportedException("The test uses streaming chat responses.");

            public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
                IEnumerable<ChatMessage> messages,
                ChatOptions? options = null,
                CancellationToken cancellationToken = default)
            {
                Options.Add(options!);
                return GetStubResponseAsync();
            }

            public object? GetService(Type serviceType, object? serviceKey = null) => null;

            public void Dispose()
            {
            }

            private static async IAsyncEnumerable<ChatResponseUpdate> GetStubResponseAsync()
            {
                await Task.Yield();
                yield return new ChatResponseUpdate(ChatRole.Assistant, "stub response");
            }
        }
    }
}
