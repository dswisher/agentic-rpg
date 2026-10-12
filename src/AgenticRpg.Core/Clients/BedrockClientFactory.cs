// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using AgenticRpg.Core.Models;
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


        public IChatClientWrapper Create(ConfigFileV1 config)
        {
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
            var nakedClient = runtime.AsIChatClient(config.ModelId);
            var client = enhancer.Enhance(nakedClient);

            // Return the wrapper
            return new BedrockClientWrapper(runtime, client);
        }
    }
}
