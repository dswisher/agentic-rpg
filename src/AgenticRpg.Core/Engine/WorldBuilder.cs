// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using AgenticRpg.Core.Models.World;

namespace AgenticRpg.Core.Engine
{
    public class WorldBuilder : IWorldBuilder
    {
        public World Build()
        {
            // Create lists of rooms and items
            var rooms = new List<Room>();
            var items = new List<Item>();

            // Create the rooms, and add them to the world
            var briarGate = new Room
            {
                Id = "briar-gate",
                Name = "Briar Gate",
                Description = "A large stone gate stands before you, its surface covered in intricate carvings."
            };

            var cloister = new Room
            {
                Id = "silent-cloister",
                Name = "Silent Cloister",
                Description = "A serene cloister with an altar at its center, bathed in soft light filtering through stained glass windows."
            };

            var bellTower = new Room
            {
                Id = "old-bell-tower",
                Name = "Old Bell Tower",
                Description = "A tall, crumbling bell tower that overlooks the surrounding landscape, its bell long since fallen silent."
            };

            rooms.Add(briarGate);
            rooms.Add(cloister);
            rooms.Add(bellTower);

            // Link the rooms together
            briarGate.Exits.Add(new Exit
            {
                Name = "north",
                DestinationRoomId = cloister.Id
            });

            cloister.Exits.Add(new Exit
            {
                Name = "south",
                DestinationRoomId = briarGate.Id
            });

            cloister.Exits.Add(new Exit
            {
                Name = "up",
                DestinationRoomId = bellTower.Id
            });

            bellTower.Exits.Add(new Exit
            {
                Name = "down",
                DestinationRoomId = cloister.Id
            });

            // Add an item
            var scroll = new Item
            {
                Id = "ancient-scroll",
                Name = "Ancient Scroll",
                Description = "A fragile scroll covered in faded runes, emanating a faint magical aura.",
                Location = new ItemLocation.InRoom(bellTower.Id)
            };

            items.Add(scroll);

            // Return the world!
            return new World
            {
                Rooms = rooms,
                Items = items,
                StartingRoomId = briarGate.Id
            };
        }
    }
}
