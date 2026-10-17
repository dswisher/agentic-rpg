// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticRpg.Core.Agents
{
    public class ChatSessionFactory : IChatSessionFactory
    {
        private readonly IServiceScopeFactory scopeFactory;

        public ChatSessionFactory(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }


        public IChatSessionHandle CreateSession()
        {
            var scope = scopeFactory.CreateScope();

            try
            {
                var session = scope.ServiceProvider.GetRequiredService<IChatSession>();

                return new ScopedChatSession(scope, session);
            }
            catch (Exception)
            {
                scope.Dispose();
                throw;
            }
        }
    }
}
