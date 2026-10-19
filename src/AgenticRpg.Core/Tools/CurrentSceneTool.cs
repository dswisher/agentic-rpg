// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Linq;
using AgenticRpg.Core.Models.World;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Core.Tools
{
    public sealed record CurrentScene(
        string Name,
        string Description,
        string[] VisibleItems,
        string[] ExitDirections);


    public class CurrentSceneTool : IGameTool
    {
        private readonly ILogger<CurrentSceneTool> logger;

        public CurrentSceneTool(ILogger<CurrentSceneTool> logger)
        {
            this.logger = logger;
        }


        public AIFunction Create(Session session)
        {
            return AIFunctionFactory.Create(
                () => GetCurrentScene(session),
                name: "get_current_scene",
                description: "Return the current room, visible items, and available exit directions. Does not reveal destination rooms or unseen items.");
        }


        private CurrentScene GetCurrentScene(Session session)
        {
            var room = session.World.Rooms.Single(x => x.Id == session.Player.CurrentRoomId);

            var visibleItems = session.World.Items
                .Where(item =>
                    item.Location is ItemLocation.InRoom inRoom &&
                    inRoom.RoomId == room.Id)
                .OrderBy(item => item.Name)
                .Select(item => item.Name)
                .ToArray();

            var exitDirections = room.Exits
                .OrderBy(exit => exit.Name)
                .Select(exit => exit.Name)
                .ToArray();

            logger.LogInformation("...get_current_scene called, room {RoomId}, with {NumExits} exits and {NumItems} items...",
                room.Id, exitDirections.Length, visibleItems.Length);

            return new CurrentScene(room.Name, room.Description, visibleItems, exitDirections);
        }
    }
}
