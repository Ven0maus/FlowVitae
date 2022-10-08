﻿using SadConsole;
using Venomaus.Visualizer.Core;

namespace Venomaus.Visualizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Setup the engine and create the main window.
            Game.Create(Constants.ScreenSettings.Width, Constants.ScreenSettings.Height);

            // Hook the start event so we can add consoles to the system.
            Game.Instance.OnStart = GameLoop.InitializeGameLoop;
            Game.Instance.FrameUpdate += GameLoop.Instance.OnFrameUpdate;

            // Start the game.
            Game.Instance.Run();
            Game.Instance.Dispose();
        }
    }
}