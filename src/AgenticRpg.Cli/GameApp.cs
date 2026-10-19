// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;
using AgenticRpg.Core.Agents;
using AgenticRpg.Core.Engine;
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

                if (gameResult is GameTurnResult.Handled handled)
                {
                    if (handled.Result is CommandResult.Completed completed)
                    {
                        Console.WriteLine(completed.Message);
                    }
                    else if (handled.Result is CommandResult.Rejected rejected)
                    {
                        Console.WriteLine(rejected.Message);
                    }
                    else
                    {
                        Console.WriteLine($"Don't know how to process handled result of type {handled.GetType().Name}");
                    }
                }
                else if (gameResult is GameTurnResult.DelegateToLlm delegated)
                {
                    await RunAgentChat(consoleCancellation, chatSession, delegated.Input);
                }
                else if (gameResult is GameTurnResult.InvalidInput invalid)
                {
                    Console.WriteLine(invalid.Reason);
                }
                else if (gameResult is not GameTurnResult.NoOp)
                {
                    Console.WriteLine($"Don't know how to handle game result of type {gameResult.GetType().Name}");
                }
            }
        }


        private async Task RunAgentChat(ConsoleCancellation consoleCancellation, IChatSession chatSession, string input)
        {
            var cancellationToken = consoleCancellation.BeginRequest();

            try
            {
                var response = await chatSession.SendAsync(input, cancellationToken);

                Console.WriteLine();
                Console.WriteLine(response.Text);
                Console.WriteLine();

                if (response.Usage is { } usage)
                {
                    logger.LogInformation("...tokens: input={InputTokens}, output={OutputTokens}, total={TotalTokens}.", usage.InputTokenCount, usage.OutputTokenCount, usage.TotalTokenCount);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine();
                Console.WriteLine("[cancelled]");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting response from chat client.");
            }
            finally
            {
                consoleCancellation.EndRequest();
            }
        }
    }
}
