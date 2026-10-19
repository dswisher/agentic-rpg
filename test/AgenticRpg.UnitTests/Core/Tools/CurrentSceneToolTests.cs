// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AgenticRpg.Core.Tools;
using AgenticRpg.UnitTests.TestHelpers;
using AwesomeAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AgenticRpg.UnitTests.Core.Tools
{
    public class CurrentSceneToolTests
    {
        private readonly CurrentSceneTool sceneTool;

        public CurrentSceneToolTests()
        {
            sceneTool = new CurrentSceneTool(NullLogger<CurrentSceneTool>.Instance);
        }


        [Fact]
        public async Task ReturnsOnlyTheFirstRoomScene()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            var function = sceneTool.Create(session);

            // Act
            var scene = await InvokeSceneAsync(function);

            // Assert
            scene.GetProperty("name").GetString().Should().Be("First Room");
            scene.GetProperty("description").GetString()
                .Should().Be("The first room in the world.");
            var firstItems = scene.GetProperty("visibleItems");
            firstItems.GetArrayLength().Should().Be(2);
            firstItems[0].GetString().Should().Be("Ancient Scroll");
            firstItems[1].GetString().Should().Be("First Item");
            var firstExits = scene.GetProperty("exitDirections");
            firstExits.GetArrayLength().Should().Be(1);
            firstExits[0].GetString().Should().Be("north");

            var result = scene.GetRawText();
            result.Should().NotContain("Second Room");
            result.Should().NotContain("room-2");
            result.Should().NotContain("Second Item");
        }


        [Fact]
        public async Task ReturnsUpdatedSceneAfterPlayerChangesRooms()
        {
            // Arrange
            var session = TestWorldBuilder.CreateSession();
            var function = sceneTool.Create(session);

            // Act
            session.Player.CurrentRoomId = "room-2";
            var scene = await InvokeSceneAsync(function);

            // Assert
            scene.GetProperty("name").GetString().Should().Be("Second Room");
            scene.GetProperty("description").GetString()
                .Should().Be("The second room in the world.");
            var visibleItems = scene.GetProperty("visibleItems");
            visibleItems.GetArrayLength().Should().Be(1);
            visibleItems[0].GetString().Should().Be("Second Item");
            var exitDirections = scene.GetProperty("exitDirections");
            exitDirections.GetArrayLength().Should().Be(1);
            exitDirections[0].GetString().Should().Be("south");

            var result = scene.GetRawText();
            result.Should().NotContain("First Room");
            result.Should().NotContain("room-1");
            result.Should().NotContain("First Item");
            result.Should().NotContain("Ancient Scroll");
        }


        private static async Task<JsonElement> InvokeSceneAsync(AIFunction function)
        {
            var result = await function.InvokeAsync(new AIFunctionArguments(), CancellationToken.None);
            return result.Should().BeOfType<JsonElement>().Which;
        }
    }
}
