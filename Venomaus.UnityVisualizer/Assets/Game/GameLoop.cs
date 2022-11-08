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
            TileGraphic.Instance.CreateOverworld();

            // Spawn player in center of the map
            var player = Instantiate(PlayerPrefab);
            player.Position = new Vector2Int(TileGraphic.Instance.Overworld.Width / 2, TileGraphic.Instance.Overworld.Height / 2);
            player.transform.position = new (player.Position.x + 0.5f, player.Position.y + 0.5f, 0f);
            CameraFollow.Target = player.gameObject;
        }
    }
}
