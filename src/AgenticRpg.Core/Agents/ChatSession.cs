// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgenticRpg.Core.Clients;
using AgenticRpg.Core.Models.World;
using AgenticRpg.Core.Tools;
using Microsoft.Extensions.AI;

namespace AgenticRpg.Core.Agents
{
    public sealed class ChatSession : IChatSession
    {
        private readonly IChatClientWrapper wrapper;
        private readonly IEnumerable<IGameTool> gameTools;

        private readonly List<ChatMessage> messages = new();


        public ChatSession(
            IChatClientWrapper wrapper,
            IEnumerable<IGameTool> gameTools)
        {
            this.wrapper = wrapper;
            this.gameTools = gameTools;
        }


        public async Task<ChatResponse> SendAsync(
            string input,
            Session gameSession,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(gameSession);
            var tools = gameTools
                .Select(tool => tool.Create(gameSession))
                .ToList<AITool>();

            var chatOptions = new ChatOptions
            {
                Instructions = """
                               Only respond with plain text (no markdown), lines should not be longer than 80 characters.
                               For any request involving a random outcome or dice roll, call roll_dice before answering. Never invent or claim a result without using the tool. Describe the result returned by the tool.
                               """,
                Tools = tools,

                // TODO: setting this seems to eliminate blank results on qwen 9b, but yields up strange results instead.
                //   (I don't know how to roll a die. You rolled a 3.)
                // Reasoning = new ReasoningOptions { Effort = ReasoningEffort.None }
            };

            var userMessage = new ChatMessage(ChatRole.User, input);

            var updates = new List<ChatResponseUpdate>();

            await foreach (var update in
                           wrapper.Chat.GetStreamingResponseAsync(
                               [.. messages, userMessage],
                               chatOptions,
                               cancellationToken).ConfigureAwait(false))
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
