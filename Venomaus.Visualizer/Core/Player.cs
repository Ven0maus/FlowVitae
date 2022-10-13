using SadConsole;
using SadConsole.Entities;
using SadConsole.Input;
using SadRogue.Primitives;

namespace Venomaus.Visualizer.Core
{
    internal class Player : Entity
    {
        public Point WorldPosition { get; private set; }
        private readonly bool _isStaticGrid;

        public Player(Point position, ColoredGlyph appearance, int zIndex, bool isStaticGrid) : base(appearance, zIndex)
        {
            _isStaticGrid = isStaticGrid;

            var (x, y) = isStaticGrid ? (GameLoop.Instance.Grid.Width / 2, GameLoop.Instance.Grid.Height / 2) :
                (Constants.GridSettings.ChunkWidth / 2, Constants.GridSettings.ChunkHeight / 2);
            WorldPosition = new Point(x, y);

            // If we are on a static grid we don't need to center, but move the actual player coord on screen
            if (_isStaticGrid)
                Position = WorldPosition;
            else
                GameLoop.Instance.Grid.Center(WorldPosition.X, WorldPosition.Y);

            // Sadconsole related things
            Position = position;
            IsFocused = true;
            GameLoop.Instance.EntityRenderer.Add(this);
        }

        public void MoveTowards(Direction dir, bool checkCanMove = true)
        {
            var point = WorldPosition;
            point += dir;
            MoveTowards(point.X, point.Y, checkCanMove);
        }

        public void MoveTowards(int x, int y, bool checkCanMove = true)
        {
            var cell = GameLoop.Instance.Grid.GetCell(x, y);
            if (cell == null || (checkCanMove && !cell.Walkable)) return;

            WorldPosition = new Point(x, y);

            // If we are on a static grid we don't need to center, but move the actual player coord on screen
            if (_isStaticGrid)
                Position = WorldPosition;
            else
                GameLoop.Instance.Grid.Center(WorldPosition.X, WorldPosition.Y);
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            foreach (var key in _playerMovements.Keys)
            {
                if (keyboard.IsKeyPressed(key))
                {
                    var moveDirection = _playerMovements[key];
                    MoveTowards(moveDirection);
                    return true;
                }
            }

            return base.ProcessKeyboard(keyboard);
        }

        private readonly Dictionary<Keys, Direction> _playerMovements =
            new Dictionary<Keys, Direction>
        {
            {Keys.Z, Direction.Up},
            {Keys.S, Direction.Down},
            {Keys.Q, Direction.Left},
            {Keys.D, Direction.Right}
        };
    }
}
