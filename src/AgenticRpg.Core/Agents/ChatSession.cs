// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AgenticRpg.Core.Clients;
using AgenticRpg.Core.Models.World;
using AgenticRpg.Core.Tools;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Core.Agents
{
    public sealed class ChatSession : IChatSession
    {
        private readonly IChatClientWrapper wrapper;
        private readonly IEnumerable<IGameTool> gameTools;
        private readonly ILogger<ChatSession> logger;

        private readonly List<ChatMessage> messages = new();


        public ChatSession(
            IChatClientWrapper wrapper,
            IEnumerable<IGameTool> gameTools,
            ILogger<ChatSession> logger)
        {
            this.wrapper = wrapper;
            this.gameTools = gameTools;
            this.logger = logger;
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
                               You have the role of game master for a world in which the player can explore.

                               Use roll_dice only when the player explicitly asks for a roll or an implemented game rule requires one.
                               Don’t invent checks or offer rolls for examining, investigating, or answering factual questions.
                               A die result has no in-game meaning unless a rule defines it.
                               If the available scene information doesn't provide more detail, say so rather than inventing a discovery.
                               Never claim a roll result without calling roll_dice; report the result it returns.

                               The world state may change between turns.
                               For questions about the current room, surroundings, visible items, exits, or what can be investigated, call get_current_scene during this turn—even if earlier messages describe the scene.
                               Treat its latest result as authoritative, not prior conversation.
                               """,
                Tools = tools,

                // TODO: setting this seems to eliminate blank results on qwen 9b, but yields up strange results instead.
                //   (I don't know how to roll a die. You rolled a 3.)
                // Reasoning = new ReasoningOptions { Effort = ReasoningEffort.None }
            };

            var userMessage = new ChatMessage(ChatRole.User, input);
            logger.LogInformation("Agent input: {Input}", input);

            var updates = new List<ChatResponseUpdate>();

            await foreach (var update in
                           wrapper.Chat.GetStreamingResponseAsync(
                               [.. messages, userMessage],
                               chatOptions,
                               cancellationToken).ConfigureAwait(false))
            {
                updates.Add(update);

                LogAgentUpdate(update);
            }

            var response = updates.ToChatResponse();

            logger.LogInformation("Agent output: {Output}", response.ToString());
            logger.LogInformation("Agent tokens: input={InputTokens}, output={OutputTokens}, total={TotalTokens}.",
                response.Usage?.InputTokenCount, response.Usage?.OutputTokenCount, response.Usage?.TotalTokenCount);

            messages.Add(userMessage);
            messages.AddMessages(response);

            return response;
        }


        public void RecordLocalTurn(string input, string output)
        {
            logger.LogInformation("Local input: {Input}", input);
            logger.LogInformation("Local output: {Output}", output);

            var userMessage = new ChatMessage(ChatRole.User, input);
            var gmMessage = new ChatMessage(ChatRole.Assistant, output);

            messages.Add(userMessage);
            messages.Add(gmMessage);
        }


        private void LogAgentUpdate(ChatResponseUpdate update)
        {
            if (update.Contents.Count == 1 && update.Contents[0] is TextContent && update.Text.Length == 0 && update.FinishReason is null)
            {
                return;
            }

            var contentTypes = string.Join(", ", update.Contents.Select(content => content.GetType().Name));

            logger.LogDebug(
                "   ...agent update: role={Role}, contentTypes={ContentTypes}, textLength={TextLength}, finishReason={FinishReason}, messageId={MessageId}",
                update.Role, contentTypes, update.Text.Length, update.FinishReason, update.MessageId);

            foreach (var call in update.Contents.OfType<FunctionCallContent>())
            {
                logger.LogInformation("   ...agent requested tool {ToolName} with arguments {Arguments}",
                    call.Name, JsonSerializer.Serialize(call.Arguments));
            }

            foreach (var result in update.Contents.OfType<FunctionResultContent>())
            {
                logger.LogDebug("   ...tool result content type: {ResultType}", result.Result?.GetType().Name ?? "(null)");
            }

            foreach (var reasoning in update.Contents.OfType<TextReasoningContent>())
            {
                logger.LogDebug("   ...reasoning update: {Reasoning}", reasoning.Text);
            }

            if (!string.IsNullOrWhiteSpace(update.Text))
            {
                logger.LogTrace("Agent text chunk: {Text}", update.Text);
            }
        }
    }
}
