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
            _grid.SetCustomConverter(WorldGenerator.CellConverter);

            // Basic grid with some borders
            for (int x=0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    // Using set cell won't hurt when we're in the same chunk
                    if (x == 0 || y == 0 || x == _grid.Width -1 || y == _grid.Height -1)
                        _grid.SetCell(x, y, (int)WorldGenerator.Tiles.Border);
                    else
                        _grid.SetCell(x, y, (int)WorldGenerator.Tiles.Grass);
                }
            }

            Game.Instance.Screen = new MapWindow(_grid.GetViewPortCells());

            // Set a happy little tree
            _grid.SetCell(5, 2, 2);
        }

        public void InitProceduralGrid()
        {
            const int screenWidth = Constants.ScreenSettings.Width;
            const int screenHeight = Constants.ScreenSettings.Height;
            const int chunkWidth = Constants.GridSettings.ChunkWidth;
            const int chunkHeight = Constants.GridSettings.ChunkHeight;

            var procedural = new ProceduralGenerator<int, VisualCell<int>>(1000, WorldGenerator.Generate);
            _grid = new(screenWidth, screenHeight, chunkWidth, chunkHeight, procedural);
            _grid.SetCustomConverter(WorldGenerator.CellConverter);

            Game.Instance.Screen = new MapWindow(_grid.GetViewPortCells());

            // Set a happy little tree
            _grid.SetCell(5, 2, 2);

            System.Diagnostics.Debug.WriteLine("Chunks loaded after map generation: " + _grid.ChunksLoaded);
        }
    }
}
