using System;

namespace Assets.Generation.Scripts
{
    public class WorldGenerator
    {
        public const TileTypes.WorldTile DefaultTile = TileTypes.WorldTile.Grass;
        public const int DirtChance = 10;

        public static void GenerateOverworld(Random random, int[] chunk, int width, int height, (int x, int y) chunkCoordinate)
        {
            // Every position contains default value of int (0) which should represent grass
            // Add some trees and border tiles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        // Chunk coordinate
                        chunk[y * width + x] = (int)TileTypes.WorldTile.ChunkCoordinateTile;
                    }
                    else if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                        chunk[y * width + x] = (int)TileTypes.WorldTile.Border; // border
                    else if (random.Next(0, 100) < DirtChance)
                        chunk[y * width + x] = (int)TileTypes.WorldTile.Dirt;
                    else
                        chunk[y * width + x] = (int)TileTypes.WorldTile.Grass;
                }
            }
        }
    }
}
