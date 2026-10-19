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
            // Register the LLM client machinery
            services.AddSingleton<IBedrockClientFactory, BedrockClientFactory>();
            services.AddSingleton<IChatClientEnhancer, ChatClientEnhancer>();
            services.AddSingleton<IChatSessionFactory, ChatSessionFactory>();
            services.AddSingleton<IClientFactory, ClientFactory>();
            services.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();

            // Register all the LLM tools
            services.AddSingleton<IGameTool, CurrentSceneTool>();
            services.AddSingleton<IGameTool, DieRoller>();

            // Set up the chat client wrapper factory
            services.AddSingleton<IChatClientWrapper>(sp => sp.GetRequiredService<IClientFactory>().Create(modelName));

            // Register the game mechanics
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
