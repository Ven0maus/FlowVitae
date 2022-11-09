using System.Diagnostics.CodeAnalysis;
using Venomaus.FlowVitae.Cells;
using Venomaus.FlowVitae.Helpers;

namespace Venomaus.UnitTests.Tools
{
    internal class CellFullComparer<TCellType> : IEqualityComparer<Cell<TCellType>?>
        where TCellType : struct
    {
        public bool Equals(Cell<TCellType>? cell, Cell<TCellType>? other)
        {
            if (cell == null && other == null) return true;
            if (cell == null || other == null) return false;
            return cell.X == other.X && cell.Y == other.Y 
                && cell.CellType.Equals(other.CellType)
                && cell.Walkable == other.Walkable;
        }

        public int GetHashCode([DisallowNull] Cell<TCellType> obj)
        {
            return Fnv1a.Hash32(obj.X, obj.Y, obj.Walkable, obj.CellType);
        }
    }

    internal class CellWalkableComparer<TCellType> : IEqualityComparer<Cell<TCellType>?>
    where TCellType : struct
    {
        public bool Equals(Cell<TCellType>? cell, Cell<TCellType>? other)
        {
            if (cell == null && other == null) return true;
            if (cell == null || other == null) return false;
            return cell.X == other.X && cell.Y == other.Y
                && cell.Walkable == other.Walkable;
        }

        public int GetHashCode([DisallowNull] Cell<TCellType> obj)
        {
            return Fnv1a.Hash32(obj.X, obj.Y, obj.Walkable);
        }
    }
}
