// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using AgenticRpg.Core.Models.World;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticRpg.Core.Agents
{
    public sealed class ScopedChatSession : IChatSessionHandle
    {
        private readonly IServiceScope scope;
        private readonly IChatSession inner;

        public ScopedChatSession(IServiceScope scope, IChatSession inner)
        {
            this.scope = scope;
            this.inner = inner;
        }


        public Task<ChatResponse> SendAsync(
            string input,
            Session gameSession,
            CancellationToken cancellationToken = default) =>
            inner.SendAsync(input, gameSession, cancellationToken);


        public void RecordLocalTurn(string input, string output)
        {
            inner.RecordLocalTurn(input, output);
        }


        public void Dispose() => scope.Dispose();
    }
}
