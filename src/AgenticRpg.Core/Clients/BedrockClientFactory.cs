// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using AgenticRpg.Core.Models;
using AgenticRpg.Core.Models.Config;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Core.Clients
{
    public class BedrockClientFactory : IBedrockClientFactory
    {
        private readonly IChatClientEnhancer enhancer;
        private readonly ILogger<BedrockClientFactory> logger;

        public BedrockClientFactory(
            IChatClientEnhancer enhancer,
            ILogger<BedrockClientFactory> logger)
        {
            this.enhancer = enhancer;
            this.logger = logger;
        }


        public IChatClientWrapper Create(ModelConfig modelConfig, AuthEntry? authEntry)
        {
            if (authEntry == null || string.IsNullOrWhiteSpace(authEntry.BearerToken))
            {
                throw new InvalidOperationException($"Auth entry with a bearerToken is required for model '{modelConfig.Name}'.");
            }

            var region = RegionEndpoint.GetBySystemName(modelConfig.Options?.Region ?? "us-east-1");

            // Set up the AWS config
            var awsConfig = new AmazonBedrockRuntimeConfig
            {
                RegionEndpoint = region,
                AWSTokenProvider = new StaticTokenProvider(authEntry.BearerToken),
                AuthSchemePreference = ["httpBearerAuth"], // otherwise SigV4 wins if AWS creds exist
            };

            logger.LogInformation("...creating bedrock IChatClient, model {ModelId}...", modelConfig.ModelId);
            var runtime = new AmazonBedrockRuntimeClient(awsConfig);
            var nakedClient = runtime.AsIChatClient(modelConfig.ModelId);
            var client = enhancer.Enhance(nakedClient);

            // Return the wrapper
            return new BedrockClientWrapper(runtime, client);
        }
    }
}
