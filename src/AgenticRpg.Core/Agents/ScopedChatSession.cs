// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
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
            string input, CancellationToken cancellationToken = default) =>
            inner.SendAsync(input, cancellationToken);


        public void Dispose() => scope.Dispose();
    }
}
