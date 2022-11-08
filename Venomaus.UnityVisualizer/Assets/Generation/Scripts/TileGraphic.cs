using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Venomaus.FlowVitae.Grids;
using Venomaus.FlowVitae.Procedural;

namespace Assets.Generation.Scripts
{
    public class TileGraphic : MonoBehaviour
    {
        public static TileGraphic Instance { get; private set; }

        [SerializeField]
        private int _width, _height, _chunkWidth, _chunkHeight, seed;
        public FlowGrid Overworld { get; private set; }

        private Tilemap _graphic;

        [SerializeField]
        private GraphicTileConfig[] _cells;

        private void Start()
        {
            // Singleton for easy access to the FlowGrid, eg. TileGraphic.Instance.WorldGrid
            if (Instance != null)
                throw new Exception("Cannot have more than one TileGraphic in the scene.");
            Instance = this;

            // Unity tilemap
            _graphic = GetComponent<Tilemap>();
        }

        public void CreateOverworld()
        {
            // FlowVitae grid with update event
            switch (GridSettings.Instance.GridType)
            {
                case GridSettings.FlowGridType.Static:
                    // Same generation as procedural, but only one array
                    int[] chunk = new int[_width * _height];
                    WorldGenerator.GenerateOverworld(new System.Random(seed), chunk, _width, _height, (0, 0));
                    Overworld = CreateStaticGrid(chunk);
                    break;
                case GridSettings.FlowGridType.Procedural:
                    Overworld = CreateProceduralGrid(WorldGenerator.GenerateOverworld);
                    break;
            }

            // Hook-up cell update event
            Overworld.OnCellUpdate += UpdateTileGraphic;

            // To load tiles that weren't changed in the grid (eg, default 0 values)
            CopyViewportToGraphic();
        }

        private FlowGrid CreateStaticGrid(int[] chunk)
        {
            var grid = new FlowGrid(_width, _height);
            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                    grid.SetCell(x, y, chunk[y * _width + x]);
            return grid;
        }

        private FlowGrid CreateProceduralGrid(Action<System.Random, int[], int, int, (int x, int y)> generator)
        {
            var procedural = new ProceduralGenerator<int, FlowCell>(seed, generator);
            var grid = new FlowGrid(_width, _height, _chunkWidth, _chunkHeight, procedural);
            return grid;
        }

        private void CopyViewportToGraphic()
        {
            var positionsArr = Overworld.GetViewPortWorldCoordinates();
            var positionsArrUnity = positionsArr.Select(a => new Vector3Int(a.x, a.y, 0)).ToArray();

            _graphic.SetTiles(positionsArrUnity, Overworld.GetCells(positionsArr)
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
