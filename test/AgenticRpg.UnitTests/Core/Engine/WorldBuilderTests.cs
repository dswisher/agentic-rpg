// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using AgenticRpg.Core.Engine;
using AwesomeAssertions;
using Xunit;

namespace AgenticRpg.UnitTests.Core.Engine
{
    public class WorldBuilderTests
    {
        [Fact]
        public void CanBuildWorld()
        {
            // Arrange
            var builder = new WorldBuilder();

            // Act
            var world = builder.Build();

            // Assert
            world.Should().NotBeNull();
            world.Rooms.Should().HaveCountGreaterThan(0);
            world.Items.Should().HaveCountGreaterThan(0);
        }
    }
}
