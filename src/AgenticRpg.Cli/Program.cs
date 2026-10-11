// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Cli
{
    public static class Program
    {
        public static async Task<int> Main()
        {
            try
            {
                using var loggerFactory = LoggerFactory.Create(logging =>
                {
                    logging.AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss ";
                    });

                    logging.SetMinimumLevel(LogLevel.Information);
                });

                await using (var container = await Container.CreateContainerAsync(loggerFactory))
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
