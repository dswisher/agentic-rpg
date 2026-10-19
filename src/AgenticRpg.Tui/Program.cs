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

            var debugOption = new Option<bool?>("--debug")
            {
                Description = "Enable debug logging.",
            };

            var verboseOption = new Option<bool?>("--verbose")
            {
                Description = "Enable verbose logging.",
            };

            var root = new RootCommand("Agentic RPG") { modelOption, debugOption, verboseOption };

            root.SetAction(parseResult =>
            {
                var model = parseResult.GetValue(modelOption);
                var verbose = parseResult.GetValue(verboseOption) ?? false;
                var debug = parseResult.GetValue(debugOption) ?? false;

                return RunGame(model, debug, verbose);
            });

            return root.Parse(args).Invoke();
        }


        private static int RunGame(string? modelName, bool debug, bool verbose)
        {
            try
            {
                using (var container = Container.CreateContainer(modelName, debug, verbose))
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
