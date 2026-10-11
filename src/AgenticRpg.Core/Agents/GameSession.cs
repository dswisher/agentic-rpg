// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgenticRpg.Core.Clients;
using AgenticRpg.Core.Tools;
using Microsoft.Extensions.AI;

namespace AgenticRpg.Core.Agents
{
    public sealed class GameSession : IGameSession
    {
        private readonly IChatClientWrapper wrapper;
        private readonly IDieRoller dieRoller;

        private readonly List<ChatMessage> messages = new();


        public GameSession(IChatClientWrapper wrapper, IDieRoller dieRoller)
        {
            this.wrapper = wrapper;
            this.dieRoller = dieRoller;
        }


        public async Task<ChatResponse> SendAsync(string input, CancellationToken cancellationToken = default)
        {
            var diceTool = AIFunctionFactory.Create(
                (int sides) => dieRoller.RollDice(sides),
                name: "roll_dice",
                description: "Roll one fair die. The sides value must be between 2 and 1000.");

            var chatOptions = new ChatOptions
            {
                Instructions = """
                               Only respond with plain text (no markdown), lines should not be longer than 80 characters.
                               """,
                Tools = [diceTool]
            };

            var userMessage = new ChatMessage(ChatRole.User, input);

            var updates = new List<ChatResponseUpdate>();

            await foreach (var update in
                           wrapper.Chat.GetStreamingResponseAsync(
                               [.. messages, userMessage],
                               chatOptions,
                               cancellationToken))
            {
                updates.Add(update);
            }

            var response = updates.ToChatResponse();

            messages.Add(userMessage);
            messages.AddMessages(response);

            return response;
        }
    }
}
