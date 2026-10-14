// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using AgenticRpg.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AgenticRpg.Tui
{
    public static class Container
    {
        public static ServiceProvider CreateContainer(string? modelName)
        {
            // Create the collection that we'll populate with goodies
            var services = new ServiceCollection();

            // Set up logging
            // ~/.local/state/agentic-rpg/tui.log
            var userRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var logPath = Path.Combine(userRoot, ".local", "state", "agentic-rpg", "tui.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.Delete(logPath);

            var serilog = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.File(logPath, outputTemplate:
                    "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {SourceContext:l}: {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog(serilog, dispose: true);
            });

            // Populate it
            services.AddCoreServices(modelName);

            services.AddSingleton<GameApp>();

            // Build the service provider from the collection
            return services.BuildServiceProvider(validateScopes: true);
        }
    }
}
