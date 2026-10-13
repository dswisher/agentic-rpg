// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using Terminal.Gui.App;

namespace AgenticRpg.Tui
{
    public static class Program
    {
        public static void Main()
        {
            using (var app = Application.Create())
            {
                app.Init();

                using (var window = new GameWindow(app))
                {
                    window.MessageSubmitted += (_, text) =>
                    {
                        // ReSharper disable twice AccessToDisposedClosure -- only raised during app.Run
                        window.AddPlayerMessage(text);
                        window.AddGmMessage("The **stone door** remains shut.\n\nWhat do you do next?");
                    };

                    app.Run(window);
                }
            }
        }
    }
}
