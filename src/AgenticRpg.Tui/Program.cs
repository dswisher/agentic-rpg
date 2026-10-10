// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace AgenticRpg.Tui
{
    public static class Program
    {
        public static void Main()
        {
            using (var app = Application.Create())
            {
                app.Init();

                using (Window window = new())
                {
                    window.Title = "Agentic RPG";
                    window.BorderStyle = LineStyle.None;
                    window.X = 0;
                    window.Y = 0;
                    window.Width = Dim.Fill();
                    window.Height = Dim.Fill();

                    FrameView inputPane = new()
                    {
                        Title = "Input",
                        X = 0,
                        Y = Pos.AnchorEnd(),
                        Width = Dim.Fill(),
                        Height = 6
                    };

                    // Add the status pane, with a couple of placeholder labels
                    FrameView statusPane = new()
                    {
                        Title = "Status",
                        X = Pos.AnchorEnd(),
                        Y = 0,
                        Width = 45,
                        Height = Dim.Fill(to: inputPane)
                    };

                    statusPane.Add(
                        new Label { Text = "Location: Starting area", X = 1, Y = 1 },
                        new Label { Text = "Health: 100", X = 1, Y = 2 }
                    );

                    // Set up the chat pane, where the conversation will appear
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

                    // Add the input field, where the user can enter text
                    TextField inputField = new()
                    {
                        X = 1,
                        Y = 1,
                        Width = Dim.Fill(),
                        Height = 1
                    };

                    // Allow PageUp and PageDown to scroll the chatPane, even when InputField still has the focus.
                    inputField.KeyDown += (_, key) =>
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
                    };

                    // Allow mouse-wheel scrolling of the chat pane, regardless of where the mouse is pointing
                    app.Mouse.MouseEvent += (_, mouse) =>
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
                    };

                    // Do the needful when the user hits enter in the input box
                    View? lastMessage = null;

                    inputField.Accepted += (_, _) =>
                    {
                        var message = inputField.Text.Trim();
                        if (message.Length == 0)
                        {
                            return;
                        }

                        var y = lastMessage is null
                            ? Pos.Absolute(1)
                            : Pos.Bottom(lastMessage) + 1;

                        FrameView entry = new()
                        {
                            Title = "You",
                            BorderStyle = LineStyle.Single,
                            X = 1,
                            Y = y,
                            Width = Dim.Fill(),
                            Height = 3
                        };

                        // Only show a border on the left
                        // entry.Border.Thickness = new Thickness(left: 1, top: 0, right: 0, bottom: 0);

                        entry.Add(new Label
                        {
                            Text = message,
                            X = 0,
                            Y = 0,
                            Width = Dim.Fill(),
                            Height = Dim.Fill()
                        });

                        chatPane.Add(entry);

                        FrameView gmEntry = new()
                        {
                            Title = "GM",
                            X = 1,
                            Y = Pos.Bottom(entry) + 1,
                            Width = Dim.Fill(),
                            Height = 5
                        };

                        gmEntry.Add(new Markdown
                        {
                            Text = "The **stone door** remains shut.\n\nWhat do you do next?",
                            X = 0,
                            Y = 0,
                            Width = Dim.Fill(),
                            Height = Dim.Fill()
                        });

                        chatPane.Add(gmEntry);
                        lastMessage = gmEntry;

                        chatPane.Layout();
                        chatPane.SetContentHeight(chatPane.GetHeightRequiredForSubViews());
                        chatPane.ScrollVertical(chatPane.GetContentHeight());

                        inputField.Text = "";
                    };

                    inputPane.Add(inputField);

                    window.Add(chatPane, statusPane, inputPane);

                    inputField.SetFocus();

                    app.Run(window);
                }
            }
        }
    }
}
