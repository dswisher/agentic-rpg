// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using AgenticRpg.Core.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AgenticRpg.UnitTests.Core.Tools
{
    public class DieRollerTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(1001)]
        public void RollDiceThrowsWhenSideCountIsOutOfRange(int sides)
        {
            // Arrange
            var dieRoller = new DieRoller(NullLogger<DieRoller>.Instance);

            // Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => dieRoller.RollDice(sides));

            // Assert
            Assert.Equal("sides", exception.ParamName);
        }
    }
}
