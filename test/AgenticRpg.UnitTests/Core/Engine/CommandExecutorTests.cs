// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using AgenticRpg.Core.Commands;
using AgenticRpg.Core.Engine;
using AgenticRpg.Core.Models.World;
using AwesomeAssertions;
using Xunit;

namespace AgenticRpg.UnitTests.Core.Engine
{
    public class CommandExecutorTests
    {
        private readonly CommandExecutor executor = new();


        [Fact]
        public void LookShowsCurrentRoomDescriptionExitsAndItems()
        {
            // Arrange
            var session = CreateSession();
            session.Player.CurrentRoomId = "room-2";

            // Act
            var result = executor.Execute(session, new GameCommand.Look());

            // Assert
            var completed = result.Should().BeOfType<CommandResult.Completed>().Which;

            completed.Message.Should().Contain("Second Room");
            completed.Message.Should().Contain("The second room in the world.");
            completed.Message.Should().Contain("exits: south");
            completed.Message.Should().Contain("Second Item");

            completed.Message.Should().NotContain("First Item");
        }


        [Fact]
        public void CanMoveToNewRoom()
        {
            // Arrange
            var session = CreateSession();

            // Act
            var result = executor.Execute(session, new GameCommand.Move("north"));

            // Assert
            var completed = result.Should().BeOfType<CommandResult.Completed>().Which;

            session.Player.CurrentRoomId.Should().Be("room-2");

            completed.Message.Should().Contain("Second Room");
        }


        [Fact]
        public void AttemptingToMoveThroughNonExistentExitIsRejected()
        {
            // Arrange
            var session = CreateSession();

            // Act
            var result = executor.Execute(session, new GameCommand.Move("east"));

            // Assert
            var rejected = result.Should().BeOfType<CommandResult.Rejected>().Which;

            session.Player.CurrentRoomId.Should().Be("room-1");

            rejected.Message.Should().Contain("no exit");
            rejected.Message.Should().Contain("east");
        }


        private static Session CreateSession()
        {
            var world = CreateWorld();
            var player = new Player { CurrentRoomId = world.StartingRoomId };

            return new Session { Player = player, World = world };
        }


        private static World CreateWorld()
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

            items.Add(item1);
            items.Add(item2);

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
