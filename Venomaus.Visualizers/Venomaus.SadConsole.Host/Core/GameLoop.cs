using SadConsole;
using SadConsole.Entities;
using SadRogue.Primitives;
using Venomaus.FlowVitae.Grids;
using Venomaus.FlowVitae.Chunking.Generators;
using Venomaus.SadConsoleVisualizer.Graphics;
using Venomaus.SadConsoleVisualizer.World;

namespace Venomaus.SadConsoleVisualizer.Core
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

        public Renderer? _entityRenderer;
        public Renderer EntityRenderer { get { return _entityRenderer ??= new Renderer(); } }

        private Player? _player;
        public Player Player
        {
            get { return _player ?? throw new Exception("Cannot access player before it is initialized."); }
            private set { _player = value; }
        }

        public static void InitializeGameLoop()
        {
            Game.Instance.Screen = new MainScreen();
        }

        public void InitStaticGrid()
        {
            const int screenWidth = Constants.ScreenSettings.Width;
            const int screenHeight = Constants.ScreenSettings.Height;

            _grid = new(screenWidth, screenHeight);
            _grid.SetCustomConverter(WorldGenerator.CellConverter);

            // Same grid as procedural gen, but with no chunks
            int[] chunk = new int[_grid.Width * _grid.Height];
            WorldGenerator.Generate(new Random(1000), chunk, _grid.Width, _grid.Height, (0,0));
            for (int x = 0; x < _grid.Width; x++)
                for (int y = 0; y < _grid.Height; y++)
                {
                    _grid.SetCell(x, y, chunk[y * _grid.Width + x], false);
                }

            InitializeMapWindow();
            AfterGridInitialization(true, false);
        }

        public void InitStaticChunkedGrid()
        {
            const int screenWidth = Constants.ScreenSettings.Width;
            const int screenHeight = Constants.ScreenSettings.Height;
            const int chunkWidth = Constants.GridSettings.ChunkWidthStatic;
            const int chunkHeight = Constants.GridSettings.ChunkHeightStatic;

            // Generate base map
            var baseMap = new int[screenWidth * screenHeight];
            WorldGenerator.Generate(new Random(0), baseMap, screenWidth, screenHeight, (0, 0));

            var chunkGenerator = new StaticGenerator<int, VisualCell<int>>(baseMap, screenWidth, screenHeight, 0);
            _grid = new(screenWidth, screenHeight, chunkWidth, chunkHeight, chunkGenerator);
            _grid.SetCustomConverter(WorldGenerator.CellConverter);

            InitializeMapWindow();
            AfterGridInitialization(true, true);
        }

        public void InitProceduralGrid()
        {
            const int screenWidth = Constants.ScreenSettings.Width;
            const int screenHeight = Constants.ScreenSettings.Height;
            const int chunkWidth = Constants.GridSettings.ChunkWidthProcedural;
            const int chunkHeight = Constants.GridSettings.ChunkHeightProcedural;

            var procedural = new ProceduralGenerator<int, VisualCell<int>>(1000, WorldGenerator.Generate);
            _grid = new(screenWidth, screenHeight, chunkWidth, chunkHeight, procedural);
            _grid.SetCustomConverter(WorldGenerator.CellConverter);

            InitializeMapWindow();
            AfterGridInitialization(false, true);
        }

        private void AfterGridInitialization(bool isStaticGrid, bool isChunked)
        {
            Player = new Player(new(Grid.Width / 2, Grid.Height / 2), new ColoredGlyph(Color.White, Color.Transparent, '@'), 1, isStaticGrid, isChunked);
        }

        private void InitializeMapWindow()
        {
            Game.Instance.Screen = new MapWindow(Grid.GetCells(Grid.GetViewPortWorldCoordinates()).Cast<VisualCell<int>>().ToArray());
            Game.Instance.Screen.SadComponents.Add(EntityRenderer);
        }
    }
}
