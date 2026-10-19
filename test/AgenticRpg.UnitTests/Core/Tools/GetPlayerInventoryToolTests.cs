// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AgenticRpg.Core.Commands;
using AgenticRpg.Core.Engine;
using AgenticRpg.Core.Models.World;
using AgenticRpg.Core.Tools;
using AgenticRpg.UnitTests.TestHelpers;
using AwesomeAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AgenticRpg.UnitTests.Core.Tools
{
    public class GetPlayerInventoryToolTests
    {
        private readonly GetPlayerInventoryTool inventoryTool;

        public GetPlayerInventoryToolTests()
        {
            inventoryTool = new GetPlayerInventoryTool(NullLogger<GetPlayerInventoryTool>.Instance);
        }


        [Fact]
        public async Task ReturnsEmptyArrayWhenInventoryIsEmpty()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            var function = inventoryTool.Create(session);

            // Act
            var items = await InvokeInventoryAsync(function);

            // Assert
            items.GetArrayLength().Should().Be(0);
        }


        [Fact]
        public async Task ReturnsOnlyInventoryItemsWithTheirDescriptions()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            var firstItem = session.World.Items[0];
            var scroll = session.World.Items[2];
            firstItem.Location = new ItemLocation.InPlayerInventory();
            scroll.Location = new ItemLocation.InPlayerInventory();
            scroll.Description = "A fragile scroll emanating a faint magical aura.";
            var function = inventoryTool.Create(session);

            // Act
            var items = await InvokeInventoryAsync(function);

            // Assert
            items.GetArrayLength().Should().Be(2);
            items[0].GetProperty("name").GetString().Should().Be("Ancient Scroll");
            items[0].GetProperty("description").GetString()
                .Should().Be("A fragile scroll emanating a faint magical aura.");
            items[1].GetProperty("name").GetString().Should().Be("First Item");

            var result = items.GetRawText();
            result.Should().NotContain("Second Item");
        }


        [Fact]
        public async Task ReflectsItemsTakenAndDroppedAfterToolCreation()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            var function = inventoryTool.Create(session);
            var executor = new CommandExecutor();

            // Act & Assert: taking the room item adds it to the tool result.
            (await InvokeInventoryAsync(function)).GetArrayLength().Should().Be(0);
            executor.Execute(session, new GameCommand.Take("scroll"))
                .Should().BeOfType<CommandResult.Completed>();

            var carriedItems = await InvokeInventoryAsync(function);
            carriedItems.GetArrayLength().Should().Be(1);
            carriedItems[0].GetProperty("name").GetString().Should().Be("Ancient Scroll");

            // Dropping it removes it from the tool result.
            executor.Execute(session, new GameCommand.Drop("scroll"))
                .Should().BeOfType<CommandResult.Completed>();
            (await InvokeInventoryAsync(function)).GetArrayLength().Should().Be(0);
        }


        private static async Task<JsonElement> InvokeInventoryAsync(AIFunction function)
        {
            var result = await function.InvokeAsync(new AIFunctionArguments(), CancellationToken.None);
            return result.Should().BeOfType<JsonElement>().Which;
        }
    }
}
