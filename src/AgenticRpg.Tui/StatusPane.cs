// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace AgenticRpg.Tui
{
    public sealed class StatusPane : FrameView
    {
        private const int NumberWidth = 9;

        private readonly Label locationLabel;
        private readonly Label cumulativeLabel;
        private readonly Label mostRecentLabel;
        private readonly Label toolCallLabel;

        private readonly List<string> toolCalls = [];

        private long cumulativeInputTokens;
        private long cumulativeOutputTokens;
        private long? recentInputTokens;
        private long? recentOutputTokens;
        private TimeSpan? elapsedTime;


        public StatusPane(View inputPane)
        {
            Title = "Status";
            X = Pos.AnchorEnd();
            Y = 0;
            Width = 45;
            Height = Dim.Fill(to: inputPane);

            locationLabel = new Label
            {
                Text = string.Empty,
                X = 1,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Auto()
            };

            cumulativeLabel = new Label
            {
                X = 1,
                Y = Pos.Bottom(locationLabel) + 1,
                Width = Dim.Fill(),
                Height = Dim.Auto()
            };

            mostRecentLabel = new Label
            {
                X = 1,
                Y = Pos.Bottom(cumulativeLabel) + 1,
                Width = Dim.Fill(),
                Height = Dim.Auto(),
                Visible = false
            };

            toolCallLabel = new Label
            {
                X = 1,
                Y = Pos.Bottom(mostRecentLabel) + 1,
                Width = Dim.Fill(),
                Height = Dim.Auto(),
                Visible = false,
                HotKeySpecifier = (Rune)0xFFFF
            };

            IncrementCumulative(0, 0);

            Add(
                locationLabel,
                cumulativeLabel,
                mostRecentLabel,
                toolCallLabel
            );
        }


        public void SetLocation(string roomName, IReadOnlyList<string> inventory)
        {
            var builder = new StringBuilder();

            builder.Append($"Current Room: {roomName}\n");

            if (inventory.Count > 0)
            {
                builder.Append("Inventory:\n");
                foreach (var item in inventory)
                {
                    builder.Append($"  {item}\n");
                }
            }

            locationLabel.Text = builder.ToString();
        }


        public void HideExchange()
        {
            mostRecentLabel.Visible = false;
            toolCallLabel.Visible = false;
        }


        public void BeginExchange()
        {
            mostRecentLabel.Visible = true;
            toolCallLabel.Visible = true;

            recentInputTokens = null;
            recentOutputTokens = null;
            elapsedTime = null;

            toolCalls.Clear();

            UpdateMostRecentLabel();
            UpdateToolCallLabel();
        }


        public void CompleteExchange(long? inputTokens, long? outputTokens, TimeSpan elapsed)
        {
            IncrementCumulative(inputTokens ?? 0, outputTokens ?? 0);

            recentInputTokens = inputTokens;
            recentOutputTokens = outputTokens;
            elapsedTime = elapsed;

            UpdateMostRecentLabel();
        }


        public void SetElapsed(TimeSpan elapsed)
        {
            elapsedTime = elapsed;

            UpdateMostRecentLabel();
        }


        public void AddToolCall(string toolName)
        {
            toolCalls.Add(toolName);

            UpdateToolCallLabel();
        }


        private void UpdateMostRecentLabel()
        {
            var builder = new StringBuilder();

            builder.Append("Most Recent Exchange:\n");
            builder.Append($"  Input tokens:  {recentInputTokens,NumberWidth:N0}\n");
            builder.Append($"  Output tokens: {recentOutputTokens,NumberWidth:N0}\n");
            builder.Append($"  Elapsed time:  {elapsedTime,NumberWidth:mm\\:ss}\n");

            mostRecentLabel.Text = builder.ToString();
        }


        private void UpdateToolCallLabel()
        {
            var builder = new StringBuilder();

            builder.Append("Tool Calls:\n");

            if (toolCalls.Count > 0)
            {
                foreach (var toolCall in toolCalls.TakeLast(5))
                {
                    builder.Append($"  {toolCall}\n");
                }
            }
            else
            {
                builder.Append("  (none)\n");
            }

            toolCallLabel.Text = builder.ToString();
        }


        private void IncrementCumulative(long inputTokens, long outputTokens)
        {
            cumulativeInputTokens += inputTokens;
            cumulativeOutputTokens += outputTokens;

            var builder = new StringBuilder();
            builder.Append("Cumulative Stats:\n");
            builder.Append($"  Input tokens:  {cumulativeInputTokens,NumberWidth:N0}\n");
            builder.Append($"  Output tokens: {cumulativeOutputTokens,NumberWidth:N0}\n");

            cumulativeLabel.Text = builder.ToString();
        }
    }
}
