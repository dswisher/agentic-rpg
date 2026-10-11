// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;
using AgenticRpg.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticRpg.Cli
{
    public static class Container
    {
        public static async Task<ServiceProvider> CreateContainerAsync()
        {
            // Create the collection that we'll populate with goodies
            var services = new ServiceCollection();

            // Populate it
            await services.AddCoreServicesAsync();

            services.AddSingleton<GameApp>();

            // Build the service provider from the collection
            return services.BuildServiceProvider(validateScopes: true);
        }
    }
}
