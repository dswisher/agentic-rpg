using System;
using Microsoft.Extensions.AI;

namespace AgenticRpg.Cli.Clients
{
    public interface IChatClientWrapper : IDisposable
    {
        IChatClient Chat { get; }
    }
}
