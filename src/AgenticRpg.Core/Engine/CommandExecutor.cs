// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Text;
using AgenticRpg.Core.Commands;
using AgenticRpg.Core.Models.World;

namespace AgenticRpg.Core.Engine
{
    public class CommandExecutor : ICommandExecutor
    {
        public CommandResult Execute(Session session, GameCommand command)
        {
            return command switch
            {
                GameCommand.Inventory =>
                    ExecuteInventory(session),

                GameCommand.Look =>
                    ExecuteLook(session),

                GameCommand.Move move =>
                    ExecuteMove(session, move.Direction),

                GameCommand.Drop drop =>
                    ExecuteDrop(session, drop.ItemName),

                GameCommand.Take take =>
                    ExecuteTake(session, take.ItemName),

                _ => throw new InvalidOperationException(
                    $"Don't know how to handle GameCommand {command.GetType().Name}")
            };
        }


        private static CommandResult ExecuteDrop(Session session, string itemName)
        {
            var matchingItems = session.World.Items
                .Where(item =>
                    item.Location is ItemLocation.InPlayerInventory &&
                    ItemNameMatches(item.Name, itemName))
                .ToList();

            if (matchingItems.Count == 0)
            {
                return new CommandResult.Rejected($"There is no item named '{itemName}' in your inventory.");
            }

            if (matchingItems.Count > 1)
            {
                return new CommandResult.Rejected($"More than one item in your inventory matches '{itemName}'.");
            }

            var item = matchingItems[0];
            item.Location = new ItemLocation.InRoom(session.Player.CurrentRoomId);

            return new CommandResult.Completed($"You drop the {item.Name}.");
        }


        private static CommandResult.Completed ExecuteInventory(Session session)
        {
            var items = session.World.Items
                .Where(item => item.Location is ItemLocation.InPlayerInventory)
                .OrderBy(item => item.Name)
                .ToList();

            if (items.Count == 0)
            {
                return new CommandResult.Completed("Your inventory is empty.");
            }

            var builder = new StringBuilder();
            builder.AppendLine("You are carrying:");

            foreach (var item in items)
            {
                builder.AppendLine($"- {item.Name}");
            }

            return new CommandResult.Completed(builder.ToString());
        }


        private static CommandResult ExecuteTake(Session session, string itemName)
        {
            var matchingItems = session.World.Items
                .Where(item =>
                    item.Location is ItemLocation.InRoom inRoom &&
                    inRoom.RoomId == session.Player.CurrentRoomId &&
                    ItemNameMatches(item.Name, itemName))
                .ToList();

            if (matchingItems.Count == 0)
            {
                return new CommandResult.Rejected($"There is no item named '{itemName}' here.");
            }

            if (matchingItems.Count > 1)
            {
                return new CommandResult.Rejected($"More than one item here matches '{itemName}'.");
            }

            var item = matchingItems[0];
            item.Location = new ItemLocation.InPlayerInventory();

            return new CommandResult.Completed($"You take the {item.Name}.");
        }


        private static bool ItemNameMatches(string itemName, string requestedName)
        {
            if (string.Equals(itemName, requestedName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var nameParts = itemName.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return nameParts.Any(part => string.Equals(part, requestedName, StringComparison.OrdinalIgnoreCase));
        }


        private static CommandResult ExecuteMove(Session session, string direction)
        {
            // Find the current room
            var room = session.World.Rooms.Single(x => x.Id == session.Player.CurrentRoomId);

            // Find the exit
            var exit = room.Exits.SingleOrDefault(x => x.Name == direction);

            if (exit == null)
            {
                return new CommandResult.Rejected($"There is no exit with name {direction}.");
            }

            // Move to the new room
            session.Player.CurrentRoomId = exit.DestinationRoomId;

            // Show the new room description
            return ExecuteLook(session);
        }


        private static CommandResult.Completed ExecuteLook(Session session)
        {
            // Find the current room
            var room = session.World.Rooms.Single(x => x.Id == session.Player.CurrentRoomId);

            // Build the description, starting with the room itself.
            var builder = new StringBuilder();

            builder.AppendLine($"**{room.Name}**");
            builder.AppendLine();
            builder.AppendLine(room.Description);

            // Add the exits.
            builder.AppendLine();
            if (room.Exits.Count > 0)
            {
                var exits = string.Join(", ", room.Exits.OrderBy(x => x.Name).Select(x => x.Name));
                builder.AppendLine($"There are the following exits: {exits}");
            }
            else
            {
                builder.AppendLine("There are no exits!");
            }

            // Add the items
            var items = session.World.Items
                .Where(item =>
                    item.Location is ItemLocation.InRoom inRoom &&
                    inRoom.RoomId == room.Id)
                .ToList();

            if (items.Count > 0)
            {
                builder.AppendLine();
            }

            foreach (var item in items.OrderBy(x => x.Name))
            {
                builder.AppendLine($"There is a {item.Name} here.");
            }

            // Return what we've got
            return new CommandResult.Completed(builder.ToString());
        }
    }
}
