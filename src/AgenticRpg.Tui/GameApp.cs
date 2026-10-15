// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AgenticRpg.Core.Agents;
using Microsoft.Extensions.Logging;
using Terminal.Gui.App;

namespace AgenticRpg.Tui
{
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The disposable fields are disposed in Run()")]
    public sealed class GameApp
    {
        private readonly IGameAgent agent;
        private readonly ILogger<GameApp> logger;

        // Per-run state
        private GameWindow? window;
        private IGameSessionHandle? session;
        private IApplication? app;
        private CancellationTokenSource? turnCts;
        private bool turnActive;
        private bool quitPending;


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

                using var theApp = Application.Create();
                app = theApp;
                theApp.Init();

                using var gameWindow = new GameWindow(theApp);
                window = gameWindow;
                gameWindow.MessageSubmitted += OnMessageSubmitted;
                gameWindow.CancelRequested += OnCancelRequested;
                gameWindow.IsRunningChanging += OnWindowRunningChanging;

                logger.LogInformation("Starting game app...");

                try
                {
                    theApp.Run(gameWindow);

                    if (turnActive)
                    {
                        logger.LogWarning("Game app stopped while turn was active...");
                    }
                }
                finally
                {
                    window = null;
                    session = null;
                    app = null;

                    turnActive = false;
                    quitPending = false;

                    turnCts?.Dispose();
                    turnCts = null;
                }

                logger.LogInformation("Game app finished.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception in game app.");
                throw;
            }
        }


        private void OnCancelRequested(object? sender, EventArgs e)
        {
            turnCts?.Cancel();
        }


        private void OnMessageSubmitted(object? sender, string text)
        {
            if (window != null && !turnActive)
            {
                window.AddPlayerMessage(text);
                window.SetBusy(true);
                turnActive = true;

                turnCts?.Dispose();
                turnCts = new CancellationTokenSource();

                // Not awaited: runs synchronously until its first await, then resumes on the UI
                // thread via MainLoopSyncContext. It never faults (catches everything), and
                // turnActive tracks completion.
                _ = RunTurnAsync(text, turnCts.Token);
            }
        }


        private async Task RunTurnAsync(string input, CancellationToken ct)
        {
            try
            {
                var response = await session!.SendAsync(input, ct);

                logger.LogInformation("*** Turn completed ***\n-> Input:\n{Input}\n-> Output:\n{Output}\n-> Tokens: input={InputTokens}, output={OutputTokens}, total={TotalTokens}.",
                    input, response.Text,
                    response.Usage?.InputTokenCount, response.Usage?.OutputTokenCount, response.Usage?.TotalTokenCount);

                window!.AddGmMessage(response.Text);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                window!.AddSystemMessage("[cancelled]");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Turn failed.");
                window!.AddSystemMessage($"[error: {ex.Message}]");
            }
            finally
            {
                window?.SetBusy(false);
                turnActive = false;
                if (quitPending)
                {
                    app?.RequestStop(window);
                }
            }
        }


        private void OnWindowRunningChanging(object? sender, CancelEventArgs<bool> e)
        {
            if (e.NewValue || !turnActive)
            {
                return;  // starting, or idle: let it stop
            }

            e.Cancel = true;                        // not yet...
            quitPending = true;
            window!.AddSystemMessage("[quitting…]");

            // Not Cancel(): it would run the turn's continuation inline, and its RequestStop
            // would be lost when this vetoed stop resets StopRequested.
            _ = turnCts!.CancelAsync();             // ...the turn's finally will RequestStop
        }
    }
}
