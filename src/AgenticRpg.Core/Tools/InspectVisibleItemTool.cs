// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using AgenticRpg.Core.Models.World;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Core.Tools
{
    public sealed record InspectVisibleItemResult(
        string Status,
        string Message,
        string? Name,
        string? Description,
        string[] Matches);


    public class InspectVisibleItemTool : IGameTool
    {
        private readonly ILogger<InspectVisibleItemTool> logger;

        public InspectVisibleItemTool(ILogger<InspectVisibleItemTool> logger)
        {
            this.logger = logger;
        }


        public AIFunction Create(Session session)
        {
            return AIFunctionFactory.Create(
                (string itemName) => InspectItem(session, itemName),
                name: "inspect_visible_item",
                description: "Inspect an item by name or name fragment, but only if it is in the player's current room or inventory. Returns its authoritative description, or reports no accessible match or ambiguity without revealing item details.");
        }


        private InspectVisibleItemResult InspectItem(Session session, string itemName)
        {
            var requestedName = itemName.Trim();
            var currentRoomId = session.Player.CurrentRoomId;
            var matchingItems = session.World.Items
                .Where(item => IsAccessible(item, currentRoomId))
                .Where(item => ItemNameMatches(item.Name, requestedName))
                .ToList();

            InspectVisibleItemResult result;
            if (matchingItems.Count == 0)
            {
                result = new InspectVisibleItemResult(
                    "no_accessible_match",
                    "No item matching that request is accessible here or in the player's inventory. This result does not indicate whether it exists elsewhere in the world.",
                    null,
                    null,
                    []);
            }
            else if (matchingItems.Count > 1)
            {
                var matches = matchingItems
                    .Select(item => item.Name)
                    .OrderBy(name => name)
                    .ToArray();
                result = new InspectVisibleItemResult(
                    "ambiguous",
                    $"More than one accessible item matches '{requestedName}'. Ask the player which item they mean.",
                    null,
                    null,
                    matches);
            }
            else
            {
                var item = matchingItems[0];
                result = new InspectVisibleItemResult(
                    "found",
                    "Item details are authoritative; do not add properties not stated here.",
                    item.Name,
                    item.Description,
                    []);
            }

            logger.LogInformation(
                "...inspect_visible_item called with status {Status} and {MatchCount} accessible matches...",
                result.Status,
                matchingItems.Count);

            return result;
        }


        private static bool IsAccessible(Item item, string currentRoomId)
        {
            return item.Location is ItemLocation.InPlayerInventory ||
                   item.Location is ItemLocation.InRoom inRoom && inRoom.RoomId == currentRoomId;
        }


        private static bool ItemNameMatches(string itemName, string requestedName)
        {
            if (requestedName.Length == 0)
            {
                return false;
            }

            if (string.Equals(itemName, requestedName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var nameParts = itemName.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return nameParts.Any(part => string.Equals(part, requestedName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
