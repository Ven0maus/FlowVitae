using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Venomaus.FlowVitae.Grids;
using Random = UnityEngine.Random;

namespace Assets.Generation.Scripts
{
    public class TileGraphic : MonoBehaviour
    {
        public static TileGraphic Instance { get; private set; }

        [SerializeField]
        private int _width, _height;
        public FlowGrid WorldGrid { get; private set; }

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

            // FlowVitae grid with update event
            WorldGrid = new FlowGrid(_width, _height);
            WorldGrid.OnCellUpdate += UpdateTileGraphic;

            // Some basic initialization on startup
            InitializeTilemap();
        }

        private void InitializeTilemap()
        {
            // Do some initial map generation
            // Grid.GenerateSomeStartupMapData();

            // To load tiles that weren't changed in the grid (eg, default 0 values)
            CopyViewportToGraphic();

            // Example of setting a tile after, graphic update is handled automatically by event
            // Set one random cell to dirt
            int randomX = Random.Range(0, WorldGrid.Width);
            int randomY = Random.Range(0, WorldGrid.Height);
            WorldGrid.SetCell(randomX, randomY, (int)TileTypes.WorldTile.Dirt);
        }

        private void CopyViewportToGraphic()
        {
            var positionsArr = WorldGrid.GetViewPortWorldCoordinates();
            var positionsArrUnity = positionsArr.Select(a => new Vector3Int(a.x, a.y, 0)).ToArray();

            _graphic.SetTiles(positionsArrUnity, WorldGrid.GetCells(positionsArr)
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
