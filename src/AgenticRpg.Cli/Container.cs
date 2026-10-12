// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using AgenticRpg.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AgenticRpg.Cli
{
    public static class Container
    {
        public static ServiceProvider CreateContainer(string? modelName)
        {
            // Create the collection that we'll populate with goodies
            var services = new ServiceCollection();

            // Set up logging
            var serilog = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.With<ShortSourceContextEnricher>()
                .WriteTo.Console(outputTemplate:
                    "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContextShort,-18}: {Message:lj}{NewLine}{Exception}")
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
