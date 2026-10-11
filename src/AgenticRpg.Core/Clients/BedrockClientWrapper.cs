// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using Amazon.BedrockRuntime;
using Microsoft.Extensions.AI;

namespace AgenticRpg.Core.Clients
{
    public sealed class BedrockClientWrapper : IChatClientWrapper
    {
        private readonly AmazonBedrockRuntimeClient runtime;


        public BedrockClientWrapper(AmazonBedrockRuntimeClient runtime, IChatClient chat)
        {
            this.runtime = runtime;
            Chat = chat;
        }


        public IChatClient Chat { get; }


        public void Dispose()
        {
            Chat.Dispose();
            runtime.Dispose();
        }
    }
}
