// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.IO;
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


        public IChatClientWrapper Create()
        {
            // Read the old config and pick it apart
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var oldConfigFilePath = Path.Join(userProfile, ".config", "agentic-rpg", "old-config.jsonc");

            if (!File.Exists(oldConfigFilePath))
            {
                throw new FileNotFoundException($"Old Config file not found: {oldConfigFilePath}");
            }

            logger.LogInformation("...reading old config file {ConfigFilePath}", oldConfigFilePath);

            ConfigFileV1? oldConfig;
            using (var stream = File.OpenRead(oldConfigFilePath))
            {
                oldConfig = JsonSerializer.Deserialize<ConfigFileV1>(stream, serializerOptions);
            }

            if (oldConfig == null)
            {
                throw new InvalidOperationException("Failed to deserialize old configuration file.");
            }

            // Read the new config and pick it apart
            var configFilePath = Path.Join(userProfile, ".config", "agentic-rpg", "config.jsonc");

            if (!File.Exists(oldConfigFilePath))
            {
                throw new FileNotFoundException($"Config file not found: {oldConfigFilePath}");
            }

            logger.LogInformation("...reading config file {ConfigFilePath}", oldConfigFilePath);

            ConfigFileV2? config;
            using (var stream = File.OpenRead(configFilePath))
            {
                config = JsonSerializer.Deserialize<ConfigFileV2>(stream, serializerOptions);
            }

            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize configuration file.");
            }

            // Read the auth file (which is optional)
            // TODO

            // TODO - an awful hack to test out ollama - use the new V2 config to figure this out!
            if (oldConfig.ModelId.StartsWith("qwen", StringComparison.OrdinalIgnoreCase))
            {
                return ollamaFactory.Create(oldConfig);
            }

            return bedrockFactory.Create(oldConfig);
        }
    }
}
