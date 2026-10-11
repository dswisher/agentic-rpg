// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;
using AgenticRpg.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Cli
{
    public static class Container
    {
        public static async Task<ServiceProvider> CreateContainerAsync(ILoggerFactory loggerFactory)
        {
            // Create the collection that we'll populate with goodies
            var services = new ServiceCollection();

            // Populate it
            await services.AddCoreServicesAsync(loggerFactory);

            services.AddLogging();
            services.Replace(ServiceDescriptor.Singleton(loggerFactory));

            services.AddSingleton<GameApp>();

            // Build the service provider from the collection
            return services.BuildServiceProvider(validateScopes: true);
        }
    }
}
