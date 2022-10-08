using SadConsole;
using Venomaus.Visualizer.Core;
using Console = SadConsole.Console;

namespace Venomaus.Visualizer.Graphics
{
    internal class MapWindow : Console
    {
        public MapWindow(ColoredGlyph[] cells) : base(Constants.ScreenSettings.Width, Constants.ScreenSettings.Height, cells)
        {
            GameLoop.Instance.Grid.OnCellUpdate += Grid_OnCellUpdate;
            Game.Instance.Screen = this;
        }

        private void Grid_OnCellUpdate(object? sender, VisualCell<int> e)
        {
            Surface.SetGlyph(e.X, e.Y, e);
        }
    }
}
