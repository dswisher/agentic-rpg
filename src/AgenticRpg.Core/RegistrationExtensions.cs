// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using AgenticRpg.Core.Agents;
using AgenticRpg.Core.Clients;
using AgenticRpg.Core.Engine;
using AgenticRpg.Core.Parsing;
using AgenticRpg.Core.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticRpg.Core
{
    public static class RegistrationExtensions
    {
        public static void AddCoreServices(this IServiceCollection services, string? modelName)
        {
            services.AddSingleton<IBedrockClientFactory, BedrockClientFactory>();
            services.AddSingleton<IChatClientEnhancer, ChatClientEnhancer>();
            services.AddSingleton<IClientFactory, ClientFactory>();
            services.AddSingleton<IDieRoller, DieRoller>();
            services.AddSingleton<IChatSessionFactory, ChatSessionFactory>();
            services.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();

            services.AddSingleton<IChatClientWrapper>(sp => sp.GetRequiredService<IClientFactory>().Create(modelName));

            services.AddSingleton<ICommandExecutor, CommandExecutor>();
            services.AddSingleton<IGameCommandProcessor, GameCommandProcessor>();
            services.AddSingleton<IParser, Parser>();
            services.AddSingleton<ISessionBuilder, SessionBuilder>();
            services.AddSingleton<IWorldBuilder, WorldBuilder>();

            // Chat-session-specific bits are scoped
            services.AddScoped<IChatSession, ChatSession>();
        }
    }
}
