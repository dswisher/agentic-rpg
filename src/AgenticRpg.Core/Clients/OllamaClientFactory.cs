// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.ClientModel;
using AgenticRpg.Core.Models;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

namespace AgenticRpg.Core.Clients
{
    public class OllamaClientFactory : IOllamaClientFactory
    {
        private readonly IChatClientEnhancer enhancer;

        public OllamaClientFactory(IChatClientEnhancer enhancer)
        {
            this.enhancer = enhancer;
        }


        public IChatClientWrapper Create(ConfigFileV1 config)
        {
            var openAiClient = new ChatClient(
                model: config.ModelId,
                credential: new ApiKeyCredential("ollama"),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri("http://localhost:11434/v1/")
                });

            var nakedClient = openAiClient.AsIChatClient();
            var client = enhancer.Enhance(nakedClient);

            return new SimpleClientWrapper(client);
        }
    }
}
