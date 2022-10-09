using BenchmarkDotNet.Attributes;
using Venomaus.FlowVitae.Basics;
using Venomaus.FlowVitae.Basics.Procedural;
using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Grids;

namespace Venomaus.BenchmarkTests.Benchmarks
{
    public abstract class BaseGridBenchmarks<TCellType, TCell>
        where TCellType : struct
        where TCell : class, ICell<TCellType>, new()
    {
        protected Grid<TCellType, TCell> Grid { get; private set; }
        protected virtual IProceduralGen<TCellType, TCell>? ProcGen { get; }
        protected virtual Func<int, int, TCellType, TCell>? CustomConverter { get; }

        protected virtual int ViewPortWidth { get; }
        protected virtual int ViewPortHeight { get; }
        protected virtual int ChunkWidth { get; }
        protected virtual int ChunkHeight { get; }

        protected Cell<int>[] Cells { get; private set; }
        protected Cell<int>[] ProceduralCells { get; private set; }
        protected (int x, int y)[] Positions { get; private set; }
        protected (int x, int y)[] ProceduralPositions { get; private set; }
        protected (int x, int y)[] ProceduralPositionsInView { get; private set; }
        protected abstract int Seed { get; }

        [GlobalSetup]
        public void Setup()
        {
            Grid = new Grid<TCellType, TCell>(ViewPortWidth, ViewPortHeight, ChunkWidth, ChunkHeight, ProcGen);
            Grid.SetCustomConverter(CustomConverter);
            Grid.OnCellUpdate += OnCellUpdate;

            // Initialize benchmark data
            Cells = PopulateCellsArray(false);
            ProceduralCells = PopulateCellsArray(true);
            Positions = PopulatePositionsArray(false, false);
            ProceduralPositions = PopulatePositionsArray(true, false);
            ProceduralPositionsInView = PopulatePositionsArray(true, true);
        }

        protected virtual void OnCellUpdate(object? sender, CellUpdateArgs<TCellType, TCell> args)
        { }

        protected virtual (int x, int y)[] PopulatePositionsArray(bool procedural, bool inView)
        {
            var width10Procent = (ChunkWidth / 100 * 10);
            var height10Procent = (ChunkHeight / 100 * 10);
            var positions = new (int x, int y)[width10Procent * height10Procent];
            var rand = new Random(Seed);

            var center = (x: ViewPortWidth / 2, y: ViewPortHeight / 2);
            var minCoord = (x: center.x - center.x, y: center.y - center.y);
            var maxCoord = (x: center.x + center.x, y: center.y + center.y);

            for (int x = 0; x < width10Procent; x++)
            {
                for (int y = 0; y < height10Procent; y++)
                {
                    if (procedural) 
                    {
                        int randX, randY;
                        if (inView)
                        {
                            randX = rand.Next(minCoord.x, maxCoord.x);
                            randY = rand.Next(minCoord.y, maxCoord.y);
                        }
                        else
                        {
                            randX = rand.Next(-(ViewPortWidth * ChunkWidth * 5), ViewPortWidth * ChunkWidth * 5);
                            randY = rand.Next(-(ViewPortHeight * ChunkHeight * 5), ViewPortHeight * ChunkHeight * 5);
                        }
                        positions[y * width10Procent + x] = (randX, randY);
                    }
                    else 
                    {
                        positions[y * width10Procent + x] = (x, y);
                    }
                }
            }

            return positions;
        }

        protected virtual Cell<int>[] PopulateCellsArray(bool procedural)
        {
            var width10Procent = (ChunkWidth / 100 * 10);
            var height10Procent = (ChunkHeight / 100 * 10);
            var cells = new Cell<int>[width10Procent * height10Procent];
            var rand = new Random(Seed);

            for (int x = 0; x < width10Procent; x++)
            {
                for (int y = 0; y < height10Procent; y++)
                {
                    if (procedural)
                    {
                        var randX = rand.Next(-(ViewPortWidth * ChunkWidth * 5), ViewPortWidth * ChunkWidth * 5);
                        var randY = rand.Next(-(ViewPortHeight * ChunkHeight * 5), ViewPortHeight * ChunkHeight * 5);
                        cells[y * width10Procent + x] = new Cell<int>(randX, randY, rand.Next(-10, 11));
                    }
                    else
                    {
                        cells[y * width10Procent + x] = new Cell<int>(x, y, rand.Next(-10, 11));
                    }
                }
            }

            return cells;
        }
    }
}
