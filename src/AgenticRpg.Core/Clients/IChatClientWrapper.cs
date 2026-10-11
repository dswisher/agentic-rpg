// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using Microsoft.Extensions.AI;

namespace AgenticRpg.Core.Clients
{
    public interface IChatClientWrapper : IDisposable
    {
        IChatClient Chat { get; }
    }
}
