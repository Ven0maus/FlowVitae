using Assets.Entities;
using Assets.Generation.Scripts;
using UnityEngine;

namespace Assets.Game
{
    public class GameLoop : MonoBehaviour
    {
        [SerializeField]
        private Player PlayerPrefab;
        [SerializeField]
        private CameraFollow CameraFollow;

        private void Start()
        {
            // First create the world
            GridSettings.Instance.TerrainGraphic.CreateGrid(WorldGenerator.GenerateTerrain);
            GridSettings.Instance.ObjectsGraphic.CreateGrid(WorldGenerator.GenerateObjects);

            // Spawn player in center of the map
            var player = Instantiate(PlayerPrefab);
            player.Position = new Vector2Int(GridSettings.Instance.Width / 2, GridSettings.Instance.Height / 2);
            player.transform.position = new (player.Position.x + 0.5f, player.Position.y + 0.5f, 0f);
            CameraFollow.Target = player.gameObject;
        }
    }
}
