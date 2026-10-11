// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;

namespace AgenticRpg.Core.Tools
{
    public static class DieRoller
    {
        public static int RollDice(int sides)
        {
            Console.WriteLine($"   [rolling a d{sides}]");

            if (sides < 2 || sides > 1000)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sides),
                    "The number of sides must be between 2 and 1000.");
            }

            return Random.Shared.Next(1, sides + 1);
        }
    }
}
