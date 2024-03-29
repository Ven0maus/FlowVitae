﻿using SadConsole;
using Venomaus.FlowVitae.Grids;
using Venomaus.SadConsoleVisualizer.Core;
using Venomaus.SadConsoleVisualizer.World;
using Console = SadConsole.Console;

namespace Venomaus.SadConsoleVisualizer.Graphics
{
    internal class MapWindow : Console
    {
        public MapWindow(ColoredGlyph[] cells) : base(Constants.ScreenSettings.Width, Constants.ScreenSettings.Height, cells)
        {
            GameLoop.Instance.Grid.OnCellUpdate += Grid_OnCellUpdate;
        }

        private void Grid_OnCellUpdate(object? sender, CellUpdateArgs<int, VisualCell<int>> args)
        {
            // Copy cell appearance onto the surface and sync visibility
            if (args.Cell == null)
            {
                Surface[args.ScreenY * Width + args.ScreenX].IsVisible = false;
            }
            else
            {
                args.Cell.CopyAppearanceTo(Surface[args.ScreenY * Width + args.ScreenX]);
                Surface[args.ScreenY * Width + args.ScreenX].IsVisible = args.Cell.IsVisible;
            }
            Surface.IsDirty = true;
        }
    }
}
