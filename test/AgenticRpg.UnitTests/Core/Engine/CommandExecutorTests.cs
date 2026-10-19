// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
            completed.Message.Should().NotContain("Ancient Scroll");
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


        [Fact]
        public void CanTakeItemInCurrentRoomByNamePart()
        {
            // Arrange
            var session = CreateSession();
            var scroll = session.World.Items.Single(item => item.Id == "ancient-scroll");

            // Act
            var result = executor.Execute(session, new GameCommand.Take("SCROLL"));

            // Assert
            var completed = result.Should().BeOfType<CommandResult.Completed>().Which;

            completed.Message.Should().Contain("Ancient Scroll");
            scroll.Location.Should().BeOfType<ItemLocation.InPlayerInventory>();
        }


        [Fact]
        public void CannotTakeItemThatIsNotInCurrentRoom()
        {
            // Arrange
            var session = CreateSession();
            session.Player.CurrentRoomId = "room-2";
            var scroll = session.World.Items.Single(item => item.Id == "ancient-scroll");

            // Act
            var result = executor.Execute(session, new GameCommand.Take("scroll"));

            // Assert
            var rejected = result.Should().BeOfType<CommandResult.Rejected>().Which;

            rejected.Message.Should().Contain("scroll");
            scroll.Location.Should().BeOfType<ItemLocation.InRoom>().Which.RoomId.Should().Be("room-1");
        }


        [Fact]
        public void AmbiguousItemNameIsRejected()
        {
            // Arrange
            var session = CreateSession();
            var scroll = session.World.Items.Single(item => item.Id == "ancient-scroll");
            var silverScroll = new Item
            {
                Id = "silver-scroll",
                Name = "Silver Scroll",
                Description = "A silver scroll.",
                Location = new ItemLocation.InRoom("room-1")
            };
            session.World.Items.Add(silverScroll);

            // Act
            var result = executor.Execute(session, new GameCommand.Take("scroll"));

            // Assert
            var rejected = result.Should().BeOfType<CommandResult.Rejected>().Which;

            rejected.Message.Should().Contain("More than one item");
            scroll.Location.Should().BeOfType<ItemLocation.InRoom>().Which.RoomId.Should().Be("room-1");
            silverScroll.Location.Should().BeOfType<ItemLocation.InRoom>().Which.RoomId.Should().Be("room-1");
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
