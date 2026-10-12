// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticRpg.Cli
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var modelOption = new Option<string?>("--model", "-m")
            {
                Description = "Name of the chat model to use.",
            };

            var root = new RootCommand("Agentic RPG") { modelOption };

            root.SetAction(async (parseResult, _) =>
            {
                var model = parseResult.GetValue(modelOption);

                return await RunGame(model);
            });

            // We handle cancellation ourselves, so we don't want the default behavior of terminating the process on Ctrl+C.
            return await root.Parse(args).InvokeAsync(
                new InvocationConfiguration { ProcessTerminationTimeout = null });
        }


        private static async Task<int> RunGame(string? modelName)
        {
            try
            {
                await using (var container = Container.CreateContainer(modelName))
                {
                    var gameApp = container.GetRequiredService<GameApp>();

                    await gameApp.RunAsync();
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception");
                Console.WriteLine(ex);

                return 1;
            }
        }
    }
}
