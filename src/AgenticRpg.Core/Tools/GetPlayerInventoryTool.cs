// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Linq;
using AgenticRpg.Core.Models.World;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Core.Tools
{
    public sealed record PlayerInventoryItem(string Name, string Description);


    public class GetPlayerInventoryTool : IGameTool
    {
        private readonly ILogger<GetPlayerInventoryTool> logger;

        public GetPlayerInventoryTool(ILogger<GetPlayerInventoryTool> logger)
        {
            this.logger = logger;
        }


        public AIFunction Create(Session session)
        {
            return AIFunctionFactory.Create(
                () => GetPlayerInventory(session),
                name: "get_player_inventory",
                description: "Return the items the player is currently carrying, including their descriptions.");
        }


        private PlayerInventoryItem[] GetPlayerInventory(Session session)
        {
            var items = session.World.Items
                .Where(item => item.Location is ItemLocation.InPlayerInventory)
                .OrderBy(item => item.Name)
                .Select(item => new PlayerInventoryItem(item.Name, item.Description))
                .ToArray();

            logger.LogInformation("...get_player_inventory called, with {NumItems} items...", items.Length);

            return items;
        }
    }
}
