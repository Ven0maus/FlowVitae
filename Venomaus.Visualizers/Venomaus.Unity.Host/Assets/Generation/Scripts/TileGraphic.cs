using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Venomaus.FlowVitae.Grids;
using Venomaus.FlowVitae.Chunking.Generators;

namespace Assets.Generation.Scripts
{
    public class TileGraphic : MonoBehaviour
    {
        private int Width { get { return GridSettings.Instance.Width; } }
        private int Height { get { return GridSettings.Instance.Height; } }
        private int ChunkWidth { get { return GridSettings.Instance.ChunkWidth; } }
        private int ChunkHeight { get { return GridSettings.Instance.ChunkHeight; } }
        private int WorldSeed { get { return GridSettings.Instance.WorldSeed; } }

        public FlowGrid Grid { get; private set; }

        private Tilemap _graphic;

        [SerializeField]
        private TilemapLayer _layer;

        public enum TilemapLayer
        {
            Terrain,
            Objects
        }

        [SerializeField]
        private GraphicTileConfig[] _cells;

        private void Start()
        {
            // Unity tilemap
            _graphic = GetComponent<Tilemap>();
        }

        public void CreateGrid(Action<System.Random, int[], int, int, (int x, int y)> generator)
        {
            // FlowVitae grid with update event
            switch (GridSettings.Instance.GridType)
            {
                case GridSettings.FlowGridType.Static:
                    // Same generation as procedural, but no chunking
                    Grid = CreateStaticGrid(generator);
                    break;
                case GridSettings.FlowGridType.StaticChunked:
                    Grid = CreateStaticChunkedGrid(generator);
                    break;
                case GridSettings.FlowGridType.Procedural:
                    Grid = CreateProceduralGrid(generator);
                    break;
            }

            // Hook-up cell update event
            Grid.OnCellUpdate += UpdateTileGraphic;
            Grid.SetCustomConverter(CustomCellConverter);

            // To load tiles that weren't changed in the grid (eg, default 0 values)
            CopyViewportToGraphic();
        }

        private FlowCell CustomCellConverter(int x, int y, int cellType)
        {
            var config = cellType < 0 ? null : _cells[cellType];
            var cell = new FlowCell { X = x, Y = y, CellType = cellType };
            if (config != null)
                cell.Walkable = config.Walkable;
            else
                cell.Walkable = _layer == TilemapLayer.Objects; // Object layer can walk over non configurated values
            return cell;
        }

        private FlowGrid CreateStaticGrid(Action<System.Random, int[], int, int, (int x, int y)> generator)
        {
            // Create a static chunk and generate onto it
            int[] chunk = new int[Width * Height];
            generator(new System.Random(WorldSeed), chunk, Width, Height, (0, 0));

            // Add data from static chunk into the grid
            var grid = new FlowGrid(Width, Height);
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    grid.SetCell(x, y, chunk[y * Width + x]);
            return grid;
        }

        private FlowGrid CreateStaticChunkedGrid(Action<System.Random, int[], int, int, (int x, int y)> generator)
        {
            // Create a base map and generate onto it
            int[] baseMap = new int[Width * Height];
            generator(new System.Random(WorldSeed), baseMap, Width, Height, (0, 0));

            var staticGen = new StaticGenerator<int, FlowCell>(baseMap, Width, Height, 
                _layer == TilemapLayer.Terrain ? (int)WorldGenerator.DefaultTerrainTile : 
                (int)WorldGenerator.DefaultObjectTile);
            var grid = new FlowGrid(Width, Height, ChunkWidth, ChunkHeight, staticGen);
            return grid;
        }

        private FlowGrid CreateProceduralGrid(Action<System.Random, int[], int, int, (int x, int y)> generator)
        {
            var procedural = new ProceduralGenerator<int, FlowCell>(WorldSeed, generator);
            var grid = new FlowGrid(Width, Height, ChunkWidth, ChunkHeight, procedural);
            return grid;
        }

        private void CopyViewportToGraphic()
        {
            var positionsArr = Grid.GetViewPortWorldCoordinates();
            var positionsArrUnity = positionsArr.Select(a => new Vector3Int(a.x, a.y, 0)).ToArray();

            _graphic.SetTiles(positionsArrUnity, Grid.GetCells(positionsArr)
                .Select(cell => Converter(cell.CellType))
                .ToArray());
        }

        private void UpdateTileGraphic(object sender, CellUpdateArgs<int, FlowCell> e)
        {
            var graphicCell = Converter(e.Cell.CellType);
            _graphic.SetTile(new Vector3Int(e.ScreenX, e.ScreenY, 0), graphicCell);
        }

        private readonly Dictionary<int, TileBase> _graphicCellCache = new();
        private TileBase Converter(int cellType)
        {
            if (cellType < 0) return null;

            // Cached impl for retrieving Tile graphic
            if (!_graphicCellCache.TryGetValue(cellType, out var tile))
            {
                var config = _cells[cellType];
                if (config.Tile != null)
                {
                    tile = config.Tile;
                }
                else
                {
                    // Create an instance of type Tile
                    tile = ScriptableObject.CreateInstance<Tile>();
                    ((Tile)tile).sprite = config.Sprite;
                }

                _graphicCellCache.Add(cellType, tile);
            }
            return tile;
        }
    }
}
