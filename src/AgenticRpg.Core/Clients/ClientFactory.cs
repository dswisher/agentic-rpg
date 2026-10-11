// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticRpg.Core.Models;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Microsoft.Extensions.AI;
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

        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<ClientFactory> logger;

        public ClientFactory(ILoggerFactory loggerFactory, ILogger<ClientFactory> logger)
        {
            this.loggerFactory = loggerFactory;
            this.logger = logger;
        }


        public IChatClientWrapper Create()
        {
            // Read the config and pick it apart
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var configFilePath = Path.Join(userProfile, ".config", "agentic-rpg", "config.jsonc");

            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"Config file not found: {configFilePath}");
            }

            logger.LogInformation("...reading config file {ConfigFilePath}", configFilePath);

            ConfigFile? config;
            using (var stream = File.OpenRead(configFilePath))
            {
                config = JsonSerializer.Deserialize<ConfigFile>(stream, serializerOptions);
            }

            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize configuration file.");
            }

            var region = RegionEndpoint.GetBySystemName(config.RegionName);

            // Set up the AWS config
            var awsConfig = new AmazonBedrockRuntimeConfig
            {
                RegionEndpoint = region,
                AWSTokenProvider = new StaticTokenProvider(config.BearerToken),
                AuthSchemePreference = ["httpBearerAuth"], // otherwise SigV4 wins if AWS creds exist
            };

            logger.LogInformation("...creating bedrock IChatClient, model {ModelId}...", config.ModelId);
            var runtime = new AmazonBedrockRuntimeClient(awsConfig);
            var client = runtime.AsIChatClient(config.ModelId)
                .AsBuilder()
                .UseLogging(loggerFactory)
                .UseFunctionInvocation()
                .Build();

            // Return the wrapper
            return new BedrockClientWrapper(runtime, client);
        }
    }
}
