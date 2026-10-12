// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticRpg.Core.Models;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Core.Clients
{
    public class ClientFactory : IClientFactory
    {
        private static readonly JsonSerializerOptions serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            RespectNullableAnnotations = true
        };

        private readonly IBedrockClientFactory bedrockFactory;
        private readonly IOllamaClientFactory ollamaFactory;
        private readonly ILogger<ClientFactory> logger;

        public ClientFactory(
            IBedrockClientFactory bedrockFactory,
            IOllamaClientFactory ollamaFactory,
            ILogger<ClientFactory> logger)
        {
            this.bedrockFactory = bedrockFactory;
            this.ollamaFactory = ollamaFactory;
            this.logger = logger;
        }


        public IChatClientWrapper Create(string? modelName)
        {
            // Read the config
            var config = LoadConfig<ConfigFile>("config.jsonc", "config");

            // Pick apart the config, and find the model config
            modelName ??= config.DefaultModel;

            var modelConfig = config.Models.SingleOrDefault(m => m.Name.Equals(modelName, StringComparison.Ordinal))
                              ?? throw new InvalidOperationException($"Model '{modelName}' not found in config file.");

            logger.LogInformation("Using model '{ModelName}' with ID '{ModelId}' from config file.", modelConfig.Name, modelConfig.ModelId);

            // If the model needs authentication, read the auth config and find the matching auth config for this model
            AuthEntry? authEntry = null;
            if (!string.IsNullOrEmpty(modelConfig.AuthProfile))
            {
                var authConfig = LoadConfig<AuthConfig>("auth.jsonc", "auth config");

                if (!authConfig.Profiles.TryGetValue(modelConfig.AuthProfile, out authEntry))
                {
                    throw new InvalidOperationException($"Auth profile '{modelConfig.AuthProfile}' not found in auth config file.");
                }
            }

            // Build the proper client
            return modelConfig.Provider switch
            {
                "bedrock" => bedrockFactory.Create(modelConfig, authEntry),
                "ollama" => ollamaFactory.Create(modelConfig),
                _ => throw new InvalidOperationException($"Unknown provider '{modelConfig.Provider}' for model '{modelConfig.Name}'."),
            };
        }


        private T LoadConfig<T>(string filename, string kind)
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var configFilePath = Path.Join(userProfile, ".config", "agentic-rpg", filename);

            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"{kind} file not found: {configFilePath}");
            }

            logger.LogInformation("...reading {Kind} file {ConfigFilePath}", kind, configFilePath);

            T? config;
            using (var stream = File.OpenRead(configFilePath))
            {
                config = JsonSerializer.Deserialize<T>(stream, serializerOptions);
            }

            if (config == null)
            {
                throw new InvalidOperationException($"Failed to deserialize {kind} file.");
            }

            return config;
        }
    }
}
