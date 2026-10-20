// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AgenticRpg.Core.Models.World;
using AgenticRpg.Core.Tools;
using AgenticRpg.UnitTests.TestHelpers;
using AwesomeAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AgenticRpg.UnitTests.Core.Tools
{
    public class InspectVisibleItemToolTests
    {
        private readonly InspectVisibleItemTool inspectTool;

        public InspectVisibleItemToolTests()
        {
            inspectTool = new InspectVisibleItemTool(NullLogger<InspectVisibleItemTool>.Instance);
        }


        [Fact]
        public async Task ReturnsDetailsForVisibleItem()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            var function = inspectTool.Create(session);

            // Act
            var result = await InvokeInspectAsync(function, "scroll");

            // Assert
            result.GetProperty("status").GetString().Should().Be("found");
            result.GetProperty("name").GetString().Should().Be("Ancient Scroll");
            result.GetProperty("description").GetString().Should().Be("An ancient scroll.");
            result.GetProperty("matches").GetArrayLength().Should().Be(0);
        }


        [Fact]
        public async Task ReturnsDetailsForCarriedItem()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            var scroll = session.World.Items[2];
            scroll.Location = new ItemLocation.InPlayerInventory();
            var function = inspectTool.Create(session);

            // Act
            var result = await InvokeInspectAsync(function, "Ancient Scroll");

            // Assert
            result.GetProperty("status").GetString().Should().Be("found");
            result.GetProperty("name").GetString().Should().Be("Ancient Scroll");
            result.GetProperty("description").GetString().Should().Be("An ancient scroll.");
        }


        [Fact]
        public async Task DoesNotDistinguishMissingItemFromItemOutsideAccessibleLocations()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            var hiddenDescription = session.World.Items[1].Description;
            var function = inspectTool.Create(session);

            // Act
            var elsewhere = await InvokeInspectAsync(function, "Second Item");
            var missing = await InvokeInspectAsync(function, "Unknown Item");

            // Assert
            elsewhere.GetProperty("status").GetString().Should().Be("no_accessible_match");
            missing.GetProperty("status").GetString().Should().Be("no_accessible_match");
            elsewhere.TryGetProperty("description", out _).Should().BeFalse();
            missing.TryGetProperty("description", out _).Should().BeFalse();
            elsewhere.GetRawText().Should().NotContain(hiddenDescription);
            elsewhere.GetRawText().Should().NotContain("Second Item");
        }


        [Fact]
        public async Task ReportsAccessibleAmbiguousMatchesWithoutReturningDescriptions()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            session.World.Items.Add(new Item
            {
                Id = "iron-key",
                Name = "Iron Key",
                Description = "A cold iron key.",
                Location = new ItemLocation.InRoom("room-1")
            });
            session.World.Items.Add(new Item
            {
                Id = "brass-key",
                Name = "Brass Key",
                Description = "A polished brass key.",
                Location = new ItemLocation.InRoom("room-1")
            });
            var function = inspectTool.Create(session);

            // Act
            var result = await InvokeInspectAsync(function, "key");

            // Assert
            result.GetProperty("status").GetString().Should().Be("ambiguous");
            result.TryGetProperty("name", out _).Should().BeFalse();
            result.TryGetProperty("description", out _).Should().BeFalse();
            var matches = result.GetProperty("matches");
            matches.GetArrayLength().Should().Be(2);
            matches[0].GetString().Should().Be("Brass Key");
            matches[1].GetString().Should().Be("Iron Key");
            var json = result.GetRawText();
            json.Should().NotContain("A cold iron key.");
            json.Should().NotContain("A polished brass key.");
        }


        private static async Task<JsonElement> InvokeInspectAsync(AIFunction function, string itemName)
        {
            var result = await function.InvokeAsync(
                new AIFunctionArguments { ["itemName"] = itemName },
                CancellationToken.None);
            return result.Should().BeOfType<JsonElement>().Which;
        }
    }
}
