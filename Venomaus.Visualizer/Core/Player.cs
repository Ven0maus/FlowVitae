using SadConsole;
using SadConsole.Entities;
using SadConsole.Input;
using SadRogue.Primitives;

namespace Venomaus.Visualizer.Core
{
    internal class Player : Entity
    {
        public Player(Point position, ColoredGlyph appearance, int zIndex) : base(appearance, zIndex)
        {
            IsFocused = true;
            Position = position;
            GameLoop.Instance.EntityRenderer.Add(this);
        }

        protected override void OnPositionChanged(Point oldPosition, Point newPosition)
        {
            GameLoop.Instance.Grid.Center(newPosition.X, newPosition.Y);
        }

        public void MoveTowards(Direction dir, bool checkCanMove = true)
        {
            var point = Position;
            point += dir;
            MoveTowards(point.X, point.Y, checkCanMove);
        }

        public void MoveTowards(int x, int y, bool checkCanMove = true)
        {
            var cell = GameLoop.Instance.Grid.GetCell(x, y);
            if (cell == null || (checkCanMove && !cell.Walkable)) return;

            Position = cell.Position;
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
