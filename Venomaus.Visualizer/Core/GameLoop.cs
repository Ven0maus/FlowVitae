using SadConsole;
using Venomaus.FlowVitae.Basics.Procedural;
using Venomaus.FlowVitae.Grids;
using Venomaus.Visualizer.Graphics;

namespace Venomaus.Visualizer.Core
{
    internal class GameLoop
    {
        private static GameLoop? _instance;
        public static GameLoop Instance { get { return _instance ??= new GameLoop(); } }

        private Grid<int, VisualCell<int>>? _grid;
        public Grid<int, VisualCell<int>> Grid
        {
            get { return _grid ?? throw new Exception("Grid is not initialized!"); }
        }

        public static void InitializeGameLoop()
        {
            Settings.WindowTitle = Constants.ScreenSettings.GameWindowTitle;
            Game.Instance.Screen = new MainScreen();
        }

        public void OnFrameUpdate(object? sender, GameHost e)
        {

        }

        public void InitStaticGrid()
        {
            _grid = new(Constants.ScreenSettings.Width, Constants.ScreenSettings.Height);
        }

        public void InitProceduralGrid()
        {
            const int screenWidth = Constants.ScreenSettings.Width;
            const int screenHeight = Constants.ScreenSettings.Height;
            const int chunkWidth = Constants.GridSettings.ChunkWidth;
            const int chunkHeight = Constants.GridSettings.ChunkHeight;

            var procedural = new ProceduralGenerator<int, VisualCell<int>>(1000, WorldGenerator.Generate);
            _grid = new(screenWidth, screenHeight, chunkWidth, chunkHeight, procedural);

            Game.Instance.Screen = new MapWindow(_grid.GetViewPortCells());
        }
    }
}
