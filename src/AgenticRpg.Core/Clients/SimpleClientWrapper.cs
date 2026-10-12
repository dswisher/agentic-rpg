// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using Microsoft.Extensions.AI;

namespace AgenticRpg.Core.Clients
{
    public sealed class SimpleClientWrapper : IChatClientWrapper
    {
        public SimpleClientWrapper(IChatClient chat)
        {
            Chat = chat;
        }


        public IChatClient Chat { get; }


        public void Dispose()
        {
            Chat.Dispose();
        }
    }
}
