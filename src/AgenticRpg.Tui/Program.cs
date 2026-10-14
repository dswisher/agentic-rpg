// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticRpg.Tui
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var modelOption = new Option<string?>("--model", "-m")
            {
                Description = "Name of the chat model to use.",
            };

            var root = new RootCommand("Agentic RPG") { modelOption };

            root.SetAction(parseResult =>
            {
                var model = parseResult.GetValue(modelOption);
                return RunGame(model);
            });

            return root.Parse(args).Invoke();
        }


        private static int RunGame(string? modelName)
        {
            try
            {
                using (var container = Container.CreateContainer(modelName))
                {
                    var gameApp = container.GetRequiredService<GameApp>();

                    gameApp.Run();

                    return 0;
                }
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
