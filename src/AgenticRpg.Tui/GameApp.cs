// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AgenticRpg.Core.Agents;
using AgenticRpg.Core.Engine;
using AgenticRpg.Core.Models.World;
using Microsoft.Extensions.Logging;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;

namespace AgenticRpg.Tui
{
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The disposable fields are disposed in Run()")]
    public sealed class GameApp
    {
        private readonly IChatSessionFactory sessionFactory;
        private readonly ISessionBuilder sessionBuilder;
        private readonly IGameCommandProcessor gameProcessor;
        private readonly ILogger<GameApp> logger;

        // Per-run state
        private GameWindow? window;
        private IChatSessionHandle? chat;
        private Session? session;
        private IApplication? app;
        private CancellationTokenSource? turnCts;
        private bool turnActive;
        private bool quitPending;


        public GameApp(
            IChatSessionFactory sessionFactory,
            ISessionBuilder sessionBuilder,
            IGameCommandProcessor gameProcessor,
            ILogger<GameApp> logger)
        {
            this.sessionFactory = sessionFactory;
            this.sessionBuilder = sessionBuilder;
            this.gameProcessor = gameProcessor;
            this.logger = logger;
        }


        public void Run()
        {
            try
            {
                // Create the chat session with the LLM
                using var chatSession = sessionFactory.CreateSession();
                chat = chatSession;

                // Create the TUI app and game window
                using var theApp = Application.Create();
                app = theApp;
                theApp.Init();

                using var gameWindow = new GameWindow(theApp);
                window = gameWindow;
                gameWindow.MessageSubmitted += OnMessageSubmitted;
                gameWindow.CancelRequested += OnCancelRequested;
                gameWindow.IsRunningChanging += OnWindowRunningChanging;

                logger.LogInformation("Starting game app...");

                // Create the game session
                session = sessionBuilder.Build();

                // Wait until the window has its real layout before adding the first chat entry.
                // Otherwise the chat pane can calculate its initial scroll position using an
                // uninitialized viewport and clip the top of the room description.
                var initialLook = gameProcessor.Process(session, "look");
                void OnInitialLayout(object? sender, LayoutEventArgs e)
                {
                    gameWindow.SubViewsLaidOut -= OnInitialLayout;
                    DisplayGameResult("look", initialLook);
                }

                gameWindow.SubViewsLaidOut += OnInitialLayout;

                // Run the TUI app
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
                    chat = null;
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

                GameTurnResult gameResult;
                try
                {
                    gameResult = gameProcessor.Process(session!, text);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Command processing failed.");
                    window.AddSystemMessage($"[error: {ex.Message}]");
                    return;
                }

                if (gameResult is not GameTurnResult.DelegateToLlm delegated)
                {
                    DisplayGameResult(text, gameResult);
                    return;
                }

                // Only LLM turns need the busy state and cancellation token source. Local
                // commands complete synchronously and must not enter the async cancellation path.
                window.SetBusy(true);
                turnActive = true;

                turnCts?.Dispose();
                turnCts = new CancellationTokenSource();

                // Not awaited: runs synchronously until its first await, then resumes on the UI
                // thread via MainLoopSyncContext. It never faults (catches everything), and
                // turnActive tracks completion.
                _ = RunTurnAsync(delegated.Input, session!, turnCts.Token);
            }
        }


        private void DisplayGameResult(string input, GameTurnResult gameResult)
        {
            switch (gameResult)
            {
                case GameTurnResult.Handled { Result: CommandResult.Completed completed }:
                    window!.AddGmMessage(completed.Message);
                    chat!.RecordLocalTurn(input, completed.Message);
                    break;

                case GameTurnResult.Handled { Result: CommandResult.Rejected rejected }:
                    window!.AddGmMessage(rejected.Message);
                    chat!.RecordLocalTurn(input, rejected.Message);
                    break;

                case GameTurnResult.InvalidInput invalid:
                    window!.AddGmMessage(invalid.Reason);
                    chat!.RecordLocalTurn(input, invalid.Reason);
                    break;

                case GameTurnResult.NoOp:
                    break;

                case GameTurnResult.DelegateToLlm:
                    throw new InvalidOperationException("LLM-delegated input must be handled by the chat path.");

                default:
                    throw new InvalidOperationException(
                        $"Don't know how to handle game result of type {gameResult.GetType().Name}");
            }
        }


        private async Task RunTurnAsync(string input, Session gameSession, CancellationToken ct)
        {
            try
            {
                var response = await chat!.SendAsync(input, gameSession, ct);

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
