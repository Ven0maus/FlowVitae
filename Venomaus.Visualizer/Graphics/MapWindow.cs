using SadConsole;
using Venomaus.FlowVitae.Basics;
using Venomaus.Visualizer.Core;
using Venomaus.Visualizer.World;
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

        private void Grid_OnCellUpdate(object? sender, CellUpdateArgs<int, VisualCell<int>> args)
        {
            // Copy cell appearance onto the surface and sync visibility
            args.Cell.CopyAppearanceTo(Surface[args.ScreenX, args.ScreenY]);
            Surface[args.ScreenX, args.ScreenY].IsVisible = args.Cell.IsVisible;
        }
    }
}
