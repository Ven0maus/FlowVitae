using Venomaus.FlowVitae.Cells;

namespace Assets.Generation.Scripts
{
    public class FlowCell : ICell<int>
    {
        // Custom impl, to be able to expand
        public int X { get; set; }
        public int Y { get; set; }
        public int CellType { get; set; }

        public bool Equals(ICell<int> other)
        {
            return other != null && other.X == X && other.Y == Y;
        }

        public bool Equals((int x, int y) other)
        {
            return other.x == X && other.y == Y;
        }
    }
}
