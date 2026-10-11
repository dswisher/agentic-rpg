// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticRpg.Core.Agents
{
    public class GameAgent : IGameAgent
    {
        private readonly IServiceScopeFactory scopeFactory;

        public GameAgent(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }


        public IGameSessionHandle CreateSession()
        {
            var scope = scopeFactory.CreateScope();

            try
            {
                var session = scope.ServiceProvider.GetRequiredService<IGameSession>();

                return new ScopedGameSession(scope, session);
            }
            catch (Exception)
            {
                scope.Dispose();
                throw;
            }
        }
    }
}
