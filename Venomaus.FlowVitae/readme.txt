Thank you for using FlowVitae!

You can find an overview of information on how to use this library on GitHub:
https://github.com/Ven0maus/FlowVitae/blob/main/README.md

Here is an example to get you started with a procedural grid.

using System;
using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Chunking.Generators;
using Venomaus.FlowVitae.Grids;

namespace FlowVitae.Examples
{
    internal class Program
    {
        // The width / height of the screen you are rendering too
        private const int _viewPortWidth = 100, _viewPortHeight = 100;
        // The size of the chunks (smaller can be less computationally expensive)
        private const int _chunkWidth = 32, _chunkHeight = 32;
        // The minimum radius of chunks the grid should generate on the outside of the viewport
        // Eg. when centering for example into a new chunk, it will load the extra chunks
        // outside the viewport on another thread to prevent pop-in and freezing effects
        // Recommended 1-2, default is 1
        private const int _extraChunkRadius = 1;
        // The seed used by the grid to generate the unique random for each chunk
        // This means that the random provided by the chunk generation method is unique for the given chunk,
        // but it will always be the same random if you generate the chunk multiple times.
        // (the chunk random seed is a combination of the chunk coordinate and the grid seed)
        private const int _gridSeed = 500;

        static void Main()
        {
            // Create chunk generator
            var procGen = new ProceduralGenerator<int, Cell<int>>(_gridSeed, GenerateChunk);

            // Create grid with procedural chunk generator
            var proceduralGrid = new Grid<int, Cell<int>>(_viewPortWidth, _viewPortHeight, _chunkWidth, _chunkHeight, procGen, _extraChunkRadius);

            // Verify our chunk data is what our generator is supplying
            Console.WriteLine($"Value of (0, 0): {proceduralGrid.GetCell(0, 0).CellType}");
            int indexX = _chunkWidth + 5;
            int indexY = _chunkHeight + 5;
            Console.WriteLine($"Value of ({indexX}, {indexY}): {proceduralGrid.GetCell(indexX, indexY).CellType}");

            // Value of (0, 0): 1
            // Value of(37, 37): 2

            Console.ReadKey();

            // Move viewport center to start of chunk to the right of (0, 0)
            // If you render this on a screen, you should see the 2 values in this chunk
            proceduralGrid.Center(x: _chunkWidth, y: _chunkHeight);
        }

        static void GenerateChunk(Random random, int[] chunk, int width, int height, (int x, int y) chunkCoordinate)
        {
            // Fill chunk with some data
            for (int x=0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // For the chunk to the right of chunk 0, 0 we set some other data
                    if (chunkCoordinate.x == _chunkWidth && chunkCoordinate.y == _chunkHeight)
                        chunk[y * width + x] = 2;
                    else
                        chunk[y * width + x] = 1;
                }
            }
        }
    }
}