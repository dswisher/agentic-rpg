// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;
using AgenticRpg.Core.Agents;

namespace AgenticRpg.Cli
{
    public class GameApp
    {
        private readonly IGameAgent agent;

        public GameApp(IGameAgent agent)
        {
            this.agent = agent;
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
                        Console.WriteLine($"[tokens: input={usage.InputTokenCount}, output={usage.OutputTokenCount}, total={usage.TotalTokenCount}]");
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine();
                    Console.WriteLine("[cancelled]");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error getting response from chat client:");
                    Console.WriteLine(ex);
                }
                finally
                {
                    consoleCancellation.EndRequest();
                }
            }
        }
    }
}
