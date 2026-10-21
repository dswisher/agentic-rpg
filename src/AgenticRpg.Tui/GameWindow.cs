// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace AgenticRpg.Tui
{
    public sealed class GameWindow : Window
    {
        private readonly IApplication app;
        private readonly TextField inputField;
        private readonly FrameView chatPane;
        private readonly FrameView inputPane;
        private readonly StatusPane statusPane;

        private View? lastEntry;
        private bool busy;


        public GameWindow(IApplication app)
        {
            this.app = app;

            Title = "Agentic RPG";
            BorderStyle = LineStyle.None;
            X = 0;
            Y = 0;
            Width = Dim.Fill();
            Height = Dim.Fill();

            (inputPane, inputField) = BuildInputPane();
            statusPane = new StatusPane(inputPane);
            chatPane = BuildChatPane(statusPane, inputPane);

            Add(chatPane, statusPane, inputPane);

            app.Mouse.MouseEvent += OnAppMouseEvent;

            inputField.SetFocus();
        }


        public StatusPane StatusPane => statusPane;

        public event EventHandler<string>? MessageSubmitted;
        public event EventHandler? CancelRequested;


        private (FrameView, TextField) BuildInputPane()
        {
            // The frame where the user can enter text, at the bottom of the window
            FrameView inPane = new()
            {
                Title = "Input",
                X = 0,
                Y = Pos.AnchorEnd(),
                Width = Dim.Fill(),
                Height = 6
            };

            // Add the input field, where the user can enter text
            TextField inField = new()
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(),
                Height = 1
            };

            inField.Accepted += OnInputAccepted;
            inField.KeyDown += OnInputKeyDown;

            inPane.Add(inField);

            return (inPane, inField);
        }


        private static FrameView BuildChatPane(View statusPane, View inputPane)
        {
            // Add the chat pane, where messages are displayed
            FrameView chatPane = new()
            {
                Title = "Chat",
                BorderStyle = LineStyle.None,
                X = 0,
                Y = 0,
                Width = Dim.Fill(to: statusPane),
                Height = Dim.Fill(to: inputPane),
                ContentSizeTracksViewport = false
            };

            chatPane.ViewportSettings |= ViewportSettingsFlags.HasVerticalScrollBar;

            return chatPane;
        }


        public void AddPlayerMessage(string text)
        {
            FrameView entry = new()
            {
                Title = "You",
                BorderStyle = LineStyle.Single,
                X = 1,
                Width = Dim.Fill(),
                Height = Dim.Auto()
            };

            // Only show a border on the left
            // entry.Border.Thickness = new Thickness(left: 1, top: 0, right: 0, bottom: 0);

            entry.Add(new Label
            {
                Text = text,
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Auto()
            });

            AddEntry(entry);
        }


        public void AddGmMessage(string text)
        {
            FrameView entry = new()
            {
                Title = "GM",
                X = 1,
                Width = Dim.Fill(),
                Height = Dim.Auto()
            };

            entry.Add(new Markdown
            {
                Text = text,
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Auto()
            });

            AddEntry(entry);
        }


        public void AddSystemMessage(string text)
        {
            FrameView entry = new()
            {
                Title = "System",
                X = 1,
                Width = Dim.Fill(),
                Height = Dim.Auto()
            };

            entry.Add(new Label
            {
                Text = text,
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Auto()
            });

            AddEntry(entry);
        }


        private void AddEntry(FrameView entry)
        {
            var y = lastEntry is null
                ? Pos.Absolute(1)
                : Pos.Bottom(lastEntry) + 1;

            entry.Y = y;

            chatPane.Add(entry);

            lastEntry = entry;

            chatPane.Layout();
            chatPane.SetContentHeight(chatPane.GetHeightRequiredForSubViews());
            chatPane.ScrollVertical(chatPane.GetContentHeight());
        }


        public void SetBusy(bool newVal)
        {
            // Set the flag
            busy = newVal;

            // Show something to the user
            inputPane.Title = busy ? "Input (busy)" : "Input";
        }


        private void OnInputAccepted(object? sender, EventArgs e)
        {
            if (busy)
            {
                return;
            }

            var message = inputField.Text.Trim();
            inputField.Text = string.Empty;

            if (!string.IsNullOrEmpty(message))
            {
                MessageSubmitted?.Invoke(this, message);
            }
        }


        private void OnAppMouseEvent(object? sender, Mouse mouse)
        {
            if (!chatPane.FrameToScreen().Contains(mouse.ScreenPosition))
            {
                return;
            }

            if (mouse.Flags.HasFlag(MouseFlags.WheeledUp))
            {
                chatPane.ScrollVertical(-3);
                mouse.Handled = true;
            }
            else if (mouse.Flags.HasFlag(MouseFlags.WheeledDown))
            {
                chatPane.ScrollVertical(3);
                mouse.Handled = true;
            }
        }


        private void OnInputKeyDown(object? sender, Key key)
        {
            if (key == Key.PageUp)
            {
                chatPane.ScrollVertical(-chatPane.Viewport.Height);
                key.Handled = true;
            }
            else if (key == Key.PageDown)
            {
                chatPane.ScrollVertical(chatPane.Viewport.Height);
                key.Handled = true;
            }
            else if (key == Key.Esc)
            {
                if (busy)
                {
                    CancelRequested?.Invoke(this, EventArgs.Empty);
                    key.Handled = true;
                }
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                app.Mouse.MouseEvent -= OnAppMouseEvent;
            }

            base.Dispose(disposing);
        }
    }
}
