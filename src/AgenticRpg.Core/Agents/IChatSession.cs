// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using AgenticRpg.Core.Models.World;
using Microsoft.Extensions.AI;

namespace AgenticRpg.Core.Agents
{
    public interface IChatSession
    {
        Task<ChatResponse> SendAsync(
            string input,
            Session gameSession,
            CancellationToken cancellationToken = default);

        void RecordLocalTurn(string input, string output);
    }
}
