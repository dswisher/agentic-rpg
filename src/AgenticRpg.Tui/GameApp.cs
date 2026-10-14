// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using AgenticRpg.Core.Agents;
using Microsoft.Extensions.Logging;
using Terminal.Gui.App;

namespace AgenticRpg.Tui
{
    public sealed class GameApp
    {
        private readonly IGameAgent agent;
        private readonly ILogger<GameApp> logger;

        private GameWindow? window; // per-run state
        private IGameSessionHandle? session;

        public GameApp(
            IGameAgent agent,
            ILogger<GameApp> logger)
        {
            this.agent = agent;
            this.logger = logger;
        }


        public void Run()
        {
            try
            {
                using var gameSession = agent.CreateSession();
                session = gameSession;

                using var app = Application.Create();
                app.Init();

                using var gameWindow = new GameWindow(app);
                window = gameWindow;
                gameWindow.MessageSubmitted += OnMessageSubmitted;

                logger.LogInformation("Starting game app...");

                try
                {
                    app.Run(gameWindow);
                }
                finally
                {
                    window = null;
                    session = null;
                }

                logger.LogInformation("Game app finished.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception in game app.");
                throw;
            }
        }


        private void OnMessageSubmitted(object? sender, string text)
        {
            if (window != null)
            {
                window.AddPlayerMessage(text);
                window.AddGmMessage("The **stone door** remains shut.\n\nWhat do you do next?");
            }
        }
    }
}
