using System;
using Microsoft.Extensions.AI;

namespace MeaiPoc.Cli.Clients
{
    public interface IChatClientWrapper : IDisposable
    {
        IChatClient Chat { get; }
    }
}
