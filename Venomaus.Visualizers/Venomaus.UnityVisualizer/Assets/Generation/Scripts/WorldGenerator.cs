using System;

namespace Assets.Generation.Scripts
{
    public class WorldGenerator
    {
        public const TileTypes.TerrainTile DefaultTile = TileTypes.TerrainTile.Grass;
        public const int DirtChance = 10;
        public const int TreeChance = 10;

        public static void GenerateTerrain(Random random, int[] chunk, int width, int height, (int x, int y) chunkCoordinate)
        {
            // Every position contains default value of int (0) which should represent grass
            // Add some terrain and border tiles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        // Chunk coordinate
                        chunk[y * width + x] = (int)TileTypes.TerrainTile.ChunkCoordinateTile;
                    }
                    else if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                        chunk[y * width + x] = (int)TileTypes.TerrainTile.Border; // border
                    else if (random.Next(0, 100) < DirtChance)
                        chunk[y * width + x] = (int)TileTypes.TerrainTile.Dirt;
                    else
                        chunk[y * width + x] = (int)TileTypes.TerrainTile.Grass;
                }
            }
        }

        public static void GenerateObjects(Random random, int[] chunk, int width, int height, (int x, int y) chunkCoordinate)
        {
            // Generate terrain for this chunk so we can check where to spawn objects
            var terrain = new int[width * height];
            GenerateTerrain(random, terrain, width, height, chunkCoordinate);

            // Every position contains default value of int (0) which should represent grass
            // Add some trees on grass tiles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (random.Next(0, 100) < TreeChance && terrain[y * width + x] == (int)TileTypes.TerrainTile.Grass)
                        chunk[y * width + x] = (int)TileTypes.ObjectTile.Tree;
                    else
                        chunk[y * width + x] = (int)TileTypes.ObjectTile.None;
                }
            }
        }
    }
}
