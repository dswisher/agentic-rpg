// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace AgenticRpg.Core.Agents
{
    public interface IChatSession
    {
        Task<ChatResponse> SendAsync(string input, CancellationToken cancellationToken = default);
    }
}
