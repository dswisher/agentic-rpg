// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Threading.Tasks;
using AgenticRpg.Core.Agents;
using AgenticRpg.Core.Clients;
using AgenticRpg.Core.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Core
{
    public static class RegistrationExtensions
    {
        public static async Task AddCoreServicesAsync(this IServiceCollection services, ILoggerFactory loggerFactory)
        {
            var wrapper = await ClientFactory.Create(loggerFactory);

            services.AddSingleton<IChatClientWrapper>(_ => wrapper);

            services.AddSingleton<IDieRoller, DieRoller>();
            services.AddSingleton<IGameAgent, GameAgent>();

            // Session-specific bits are scoped
            services.AddScoped<IGameSession, GameSession>();
        }
    }
}
