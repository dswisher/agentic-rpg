// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using AgenticRpg.Core.Models.World;

namespace AgenticRpg.UnitTests.TestHelpers
{
    public static class TestWorldBuilder
    {
        public static Session CreateSession()
        {
            var world = CreateWorld();
            var player = new Player { CurrentRoomId = world.StartingRoomId };

            return new Session { Player = player, World = world };
        }


        public static World CreateWorld()
        {
            // Create lists of rooms and items
            var rooms = new List<Room>();
            var items = new List<Item>();

            // Create the rooms, and add them to the world
            var room1 = new Room
            {
                Id = "room-1",
                Name = "First Room",
                Description = "The first room in the world."
            };

            var room2 = new Room
            {
                Id = "room-2",
                Name = "Second Room",
                Description = "The second room in the world."
            };

            rooms.Add(room1);
            rooms.Add(room2);

            // Link the rooms together
            room1.Exits.Add(new Exit
            {
                Name = "north",
                DestinationRoomId = room2.Id
            });

            room2.Exits.Add(new Exit
            {
                Name = "south",
                DestinationRoomId = room1.Id
            });

            var item1 = new Item
            {
                Id = "item-1",
                Name = "First Item",
                Description = "The first item in the world.",
                Location = new ItemLocation.InRoom(room1.Id)
            };

            var item2 = new Item
            {
                Id = "item-2",
                Name = "Second Item",
                Description = "The second item in the world.",
                Location = new ItemLocation.InRoom(room2.Id)
            };

            var scroll = new Item
            {
                Id = "ancient-scroll",
                Name = "Ancient Scroll",
                Description = "An ancient scroll.",
                Location = new ItemLocation.InRoom(room1.Id)
            };

            items.Add(item1);
            items.Add(item2);
            items.Add(scroll);

            // Return the world!
            return new World
            {
                Rooms = rooms,
                Items = items,
                StartingRoomId = room1.Id
            };
        }
    }
}
