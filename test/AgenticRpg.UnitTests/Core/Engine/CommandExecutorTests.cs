// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Linq;
using AgenticRpg.Core.Commands;
using AgenticRpg.Core.Engine;
using AgenticRpg.Core.Models.World;
using AgenticRpg.UnitTests.TestHelpers;
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
            var session = TestWorldBuilder.CreateSession();
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
            var session = TestWorldBuilder.CreateSession();

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
            var session = TestWorldBuilder.CreateSession();

            // Act
            var result = executor.Execute(session, new GameCommand.Move("east"));

            // Assert
            var rejected = result.Should().BeOfType<CommandResult.Rejected>().Which;

            session.Player.CurrentRoomId.Should().Be("room-1");

            rejected.Message.Should().Contain("no exit");
            rejected.Message.Should().Contain("east");
        }


        [Fact]
        public void InventoryShowsOnlyItemsInPlayerInventory()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            var firstItem = session.World.Items.Single(item => item.Id == "item-1");
            var scroll = session.World.Items.Single(item => item.Id == "ancient-scroll");
            firstItem.Location = new ItemLocation.InPlayerInventory();
            scroll.Location = new ItemLocation.InPlayerInventory();

            // Act
            var result = executor.Execute(session, new GameCommand.Inventory());

            // Assert
            var completed = result.Should().BeOfType<CommandResult.Completed>().Which;

            completed.Message.Should().Contain("First Item");
            completed.Message.Should().Contain("Ancient Scroll");
            completed.Message.Should().NotContain("Second Item");
        }


        [Fact]
        public void EmptyInventoryIsReported()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();

            // Act
            var result = executor.Execute(session, new GameCommand.Inventory());

            // Assert
            var completed = result.Should().BeOfType<CommandResult.Completed>().Which;

            completed.Message.Should().Be("Your inventory is empty.");
        }


        [Fact]
        public void CanDropInventoryItemIntoCurrentRoom()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            session.Player.CurrentRoomId = "room-2";
            var scroll = session.World.Items.Single(item => item.Id == "ancient-scroll");
            scroll.Location = new ItemLocation.InPlayerInventory();

            // Act
            var result = executor.Execute(session, new GameCommand.Drop("SCROLL"));

            // Assert
            var completed = result.Should().BeOfType<CommandResult.Completed>().Which;

            completed.Message.Should().Contain("Ancient Scroll");
            scroll.Location.Should().BeOfType<ItemLocation.InRoom>().Which.RoomId.Should().Be("room-2");
        }


        [Fact]
        public void CannotDropItemThatIsNotInInventory()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            var scroll = session.World.Items.Single(item => item.Id == "ancient-scroll");

            // Act
            var result = executor.Execute(session, new GameCommand.Drop("scroll"));

            // Assert
            var rejected = result.Should().BeOfType<CommandResult.Rejected>().Which;

            rejected.Message.Should().Contain("inventory");
            scroll.Location.Should().BeOfType<ItemLocation.InRoom>().Which.RoomId.Should().Be("room-1");
        }


        [Fact]
        public void AmbiguousInventoryItemNameIsRejectedWhenDropping()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            var scroll = session.World.Items.Single(item => item.Id == "ancient-scroll");
            scroll.Location = new ItemLocation.InPlayerInventory();
            var silverScroll = new Item
            {
                Id = "silver-scroll",
                Name = "Silver Scroll",
                Description = "A silver scroll.",
                Location = new ItemLocation.InPlayerInventory()
            };
            session.World.Items.Add(silverScroll);

            // Act
            var result = executor.Execute(session, new GameCommand.Drop("scroll"));

            // Assert
            var rejected = result.Should().BeOfType<CommandResult.Rejected>().Which;

            rejected.Message.Should().Contain("More than one item");
            scroll.Location.Should().BeOfType<ItemLocation.InPlayerInventory>();
            silverScroll.Location.Should().BeOfType<ItemLocation.InPlayerInventory>();
        }


        [Fact]
        public void CanTakeItemInCurrentRoomByNamePart()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
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
            var session = TestWorldBuilder.CreateSession();
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
            var session = TestWorldBuilder.CreateSession();
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
    }
}
