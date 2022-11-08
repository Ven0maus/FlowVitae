using Assets.Generation.Scripts;
using UnityEngine;

namespace Assets.Entities
{
    public class Player : MonoBehaviour
    {
        public Vector2Int Position { get; set; }

        public void MoveTowards(int x, int y, bool checkCanMove = true)
        {
            var cell = TileGraphic.Instance.Overworld.GetCell(x, y);
            if (cell == null || (checkCanMove && !cell.Walkable)) return;

            Position = new Vector2Int(x, y);

            // If we are on a static grid we don't need to center, but move the actual player coord on screen
            if (GridSettings.Instance.GridType == GridSettings.FlowGridType.Static)
                transform.position = new Vector3(x + .5f, y + .5f, 0);
            else
                TileGraphic.Instance.Overworld.Center(x, y);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z))
                MoveTowards(Position.x, Position.y + 1);
            else if (Input.GetKeyDown(KeyCode.S))
                MoveTowards(Position.x, Position.y - 1);
            else if (Input.GetKeyDown(KeyCode.Q))
                MoveTowards(Position.x - 1, Position.y);
            else if (Input.GetKeyDown(KeyCode.D))
                MoveTowards(Position.x + 1, Position.y);
        }
    }
}
