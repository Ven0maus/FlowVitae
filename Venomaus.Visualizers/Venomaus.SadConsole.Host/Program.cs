using SadConsole;
using Venomaus.SadConsoleVisualizer.Core;

namespace Venomaus.SadConsoleVisualizer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Some settings
            Settings.ResizeMode = Settings.WindowResizeOptions.Stretch;
            Settings.AllowWindowResize = true;
            Settings.WindowTitle = Constants.ScreenSettings.GameWindowTitle;

            // Setup the engine and create the main window.
            Game.Create(Constants.ScreenSettings.Width, Constants.ScreenSettings.Height);

            // Hook the start event so we can add consoles to the system.
            Game.Instance.OnStart = GameLoop.InitializeGameLoop;

            // Start the game.
            Game.Instance.Run();
            Game.Instance.Dispose();
        }
    }
}