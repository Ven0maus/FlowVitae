using SadConsole;
using SadConsole.Configuration;
using Venomaus.SadConsoleVisualizer.Core;

namespace Venomaus.SadConsoleVisualizer
{
    internal class Program
    {
        private static void Main()
        {
            // Some settings
            Settings.ResizeMode = Settings.WindowResizeOptions.Stretch;
            Settings.AllowWindowResize = true;
            Settings.WindowTitle = Constants.ScreenSettings.GameWindowTitle;

            Builder gameStartup = new Builder()
                            .SetScreenSize(Constants.ScreenSettings.Width, Constants.ScreenSettings.Height)
                            .UseDefaultConsole()
                            .OnStart((sender, args) => GameLoop.InitializeGameLoop());

            Game.Create(gameStartup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }
    }
}