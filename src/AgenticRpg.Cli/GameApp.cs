// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;
using AgenticRpg.Core.Agents;
using AgenticRpg.Core.Engine;
using AgenticRpg.Core.Models.World;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Cli
{
    public class GameApp
    {
        private readonly IChatSessionFactory sessionFactory;
        private readonly ISessionBuilder sessionBuilder;
        private readonly IGameCommandProcessor gameProcessor;
        private readonly ILogger<GameApp> logger;

        public GameApp(
            IChatSessionFactory sessionFactory,
            ISessionBuilder sessionBuilder,
            IGameCommandProcessor gameProcessor,
            ILogger<GameApp> logger)
        {
            this.sessionFactory = sessionFactory;
            this.sessionBuilder = sessionBuilder;
            this.gameProcessor = gameProcessor;
            this.logger = logger;
        }


        public async Task RunAsync()
        {
            using var consoleCancellation = new ConsoleCancellation();
            using var chatSession = sessionFactory.CreateSession();

            // Create the game session
            var session = sessionBuilder.Build();

            // Show the player where they are before accepting input.
            await HandleGameResultAsync(gameProcessor.Process(session, "look"), consoleCancellation, chatSession, session, "look");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();

                if (input == null)
                {
                    break; // EOF
                }

                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                if (input.Equals("/quit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                var gameResult = gameProcessor.Process(session, input);
                await HandleGameResultAsync(gameResult, consoleCancellation, chatSession, session, input);
            }
        }


        private async Task HandleGameResultAsync(
            GameTurnResult gameResult,
            ConsoleCancellation consoleCancellation,
            IChatSession chatSession,
            Session session,
            string input)
        {
            if (gameResult is GameTurnResult.Handled handled)
            {
                if (handled.Result is CommandResult.Completed completed)
                {
                    Console.WriteLine(completed.Message);
                    chatSession.RecordLocalTurn(input, completed.Message);
                }
                else if (handled.Result is CommandResult.Rejected rejected)
                {
                    Console.WriteLine(rejected.Message);
                    chatSession.RecordLocalTurn(input, rejected.Message);
                }
                else
                {
                    Console.WriteLine($"Don't know how to process command result of type {handled.Result.GetType().Name}");
                }
            }
            else if (gameResult is GameTurnResult.DelegateToLlm delegated)
            {
                await RunAgentChat(consoleCancellation, chatSession, session, delegated.Input);
            }
            else if (gameResult is GameTurnResult.InvalidInput invalid)
            {
                Console.WriteLine(invalid.Reason);
                chatSession.RecordLocalTurn(input, invalid.Reason);
            }
            else if (gameResult is not GameTurnResult.NoOp)
            {
                Console.WriteLine($"Don't know how to handle game result of type {gameResult.GetType().Name}");
            }
        }


        private async Task RunAgentChat(
            ConsoleCancellation consoleCancellation,
            IChatSession chatSession,
            Session session,
            string input)
        {
            var cancellationToken = consoleCancellation.BeginRequest();

            try
            {
                var response = await chatSession.SendAsync(input, session, cancellationToken: cancellationToken);

                Console.WriteLine();
                Console.WriteLine(response.Text);
                Console.WriteLine();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine();
                Console.WriteLine("[cancelled]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting response from chat client: {ex.Message}");
                logger.LogError(ex, "Error getting response from chat client.");
            }
            finally
            {
                consoleCancellation.EndRequest();
            }
        }
    }
}
