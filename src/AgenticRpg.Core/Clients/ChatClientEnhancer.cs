// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Core.Clients
{
    public class ChatClientEnhancer : IChatClientEnhancer
    {
        private readonly ILoggerFactory loggerFactory;

        public ChatClientEnhancer(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }


        public IChatClient Enhance(IChatClient original)
        {
            return original
                .AsBuilder()
                .UseLogging(loggerFactory)
                .UseFunctionInvocation()
                .Build();
        }
    }
}
