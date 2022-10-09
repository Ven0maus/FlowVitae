using SadRogue.Primitives;

namespace Venomaus.Visualizer.World
{
    internal class WorldGenerator
    {
        public enum Tiles
        {
            None = 0,
            Border = -1,
            ChunkCoordinate = -2,
            Grass = 1,
            Tree = 2,
        }

        public const Tiles DefaultTile = Tiles.Grass;
        public const int TreeChance = 10;

        public static void Generate(Random random, int[] chunk, int width, int height)
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
                        chunk[y * width + x] = (int)Tiles.ChunkCoordinate;
                    }
                    else if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                        chunk[y * width + x] = (int)Tiles.Border; // border
                    else if (random.Next(0, 100) < TreeChance)
                        chunk[y * width + x] = (int)Tiles.Tree;
                    else
                        chunk[y * width + x] = (int)Tiles.Grass;
                }
            }
        }

        public static VisualCell<int> CellConverter(int x, int y, int cellType)
        {
            var tile = (Tiles)cellType;
            switch (tile)
            {
                case Tiles.Border:
                    return new VisualCell<int>(x, y, cellType, '.', Color.Green, Color.Lerp(Color.White, Color.Transparent, 0.6f));
                case Tiles.ChunkCoordinate:
                    return new VisualCell<int>(x, y, cellType, '%', Color.Cyan);
                case Tiles.Grass:
                    return new VisualCell<int>(x, y, cellType, '.', Color.Green);
                case Tiles.Tree:
                    return new VisualCell<int>(x, y, cellType, '&', Color.Brown) { Walkable = false };
                default:
                    return new VisualCell<int>(x, y, cellType, 0) { Walkable = false};
            }
        }
    }
}
