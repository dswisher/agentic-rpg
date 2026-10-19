// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using AgenticRpg.Core.Models.World;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Core.Tools
{
    public class DieRoller : IGameTool
    {
        private readonly ILogger<DieRoller> logger;

        public DieRoller(ILogger<DieRoller> logger)
        {
            this.logger = logger;
        }


        public int RollDice(int sides)
        {
            if (sides < 2 || sides > 1000)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sides),
                    "The number of sides must be between 2 and 1000.");
            }

            var result = Random.Shared.Next(1, sides + 1);

            logger.LogInformation("...rolled a {Die} and got {Pips}...", $"d{sides}", result);

            return result;
        }


        public AIFunction Create(Session session)
        {
            return AIFunctionFactory.Create(
                RollDice,
                name: "roll_dice",
                description: "Roll one fair die. The sides value must be between 2 and 1000.");
        }
    }
}
