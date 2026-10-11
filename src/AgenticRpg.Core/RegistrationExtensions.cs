// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;
using AgenticRpg.Core.Agents;
using AgenticRpg.Core.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticRpg.Core
{
    public static class RegistrationExtensions
    {
        public static async Task AddCoreServicesAsync(this IServiceCollection services)
        {
            var wrapper = await ClientFactory.Create();
            services.AddSingleton<IChatClientWrapper>(_ => wrapper);

            services.AddSingleton<IGameAgent, GameAgent>();
        }
    }
}
