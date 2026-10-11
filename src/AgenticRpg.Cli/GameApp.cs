// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;
using AgenticRpg.Core.Agents;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Cli
{
    public class GameApp
    {
        private readonly IGameAgent agent;
        private readonly ILogger<GameApp> logger;

        public GameApp(IGameAgent agent, ILogger<GameApp> logger)
        {
            this.agent = agent;
            this.logger = logger;
        }


        public async Task RunAsync()
        {
            using var consoleCancellation = new ConsoleCancellation();
            using var gameSession = agent.CreateSession();

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

                var cancellationToken = consoleCancellation.BeginRequest();

                try
                {
                    var response = await gameSession.SendAsync(input, cancellationToken);

                    Console.WriteLine(response.Text);

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
}
