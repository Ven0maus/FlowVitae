using SadConsole;
using SadRogue.Primitives;
using Venomaus.FlowVitae.Basics;

namespace Venomaus.Visualizer.Core
{
    internal class VisualCell<TCellType> : ColoredGlyph, ICell<TCellType>
        where TCellType : struct
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Point Position { get { return new Point(X, Y); } }
        public TCellType CellType { get; set; }

        public VisualCell() { }

        public VisualCell(int x, int y, TCellType cellType, int glyph)
        {
            X = x;
            Y = y;
            CellType = cellType;
            Glyph = glyph;
        }

        public VisualCell(int x, int y, TCellType cellType, int glyph, Color foreground)
        {
            X = x;
            Y = y;
            CellType = cellType;
            Glyph = glyph;
            Foreground = foreground;
        }

        public VisualCell(int x, int y, TCellType cellType, int glyph, Color foreground, Color background) 
        {
            X = x;
            Y = y;
            CellType = cellType;
            Glyph = glyph;
            Foreground = foreground;
            Background = background;
        }

        public bool Equals(ICell<TCellType>? other)
        {
            return other != null && other.X == X && other.Y == Y;
        }

        public bool Equals((int x, int y) other)
        {
            return other.x == X && other.y == Y;
        }
    }
}
