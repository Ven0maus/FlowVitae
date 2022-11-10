using UnityEngine;

namespace Assets.Entities
{
    public class Player : MonoBehaviour
    {
        public Vector2Int Position { get; set; }

        public void MoveTowards(int x, int y, bool checkCanMove = true)
        {
            if (checkCanMove && !CanMove(x, y)) return;

            Position = new Vector2Int(x, y);

            // If we are on a static grid we don't need to center, but move the actual player coord on screen
            if (GridSettings.Instance.GridType == GridSettings.FlowGridType.Static)
            {
                transform.position = new Vector3(x + .5f, y + .5f, 0);
            }
            else
            {
                GridSettings.Instance.TerrainGraphic.Grid.Center(x, y);
                GridSettings.Instance.ObjectsGraphic.Grid.Center(x, y);
            }
        }

        private bool CanMove(int x, int y)
        {
            // Can't move if there is no terrain, or not walkable terrain
            var terrainCell = GridSettings.Instance.TerrainGraphic.Grid.GetCell(x, y);
            if (terrainCell == null || (!terrainCell.Walkable)) return false;

            // Can't move if object is not walkable
            var objectsCell = GridSettings.Instance.ObjectsGraphic.Grid.GetCell(x, y);
            if (objectsCell != null && !objectsCell.Walkable) return false;

            return true;
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
