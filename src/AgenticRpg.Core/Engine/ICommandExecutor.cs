// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using AgenticRpg.Core.Commands;
using AgenticRpg.Core.Models.World;

namespace AgenticRpg.Core.Engine
{
    public interface ICommandExecutor
    {
        CommandResult Execute(Session session, GameCommand command);
    }
}
