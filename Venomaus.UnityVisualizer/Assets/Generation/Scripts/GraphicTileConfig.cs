using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Generation.Scripts
{
    [Serializable]
    public class GraphicTileConfig
    {
        [Header("Use this to auto-create tile")]
        [Tooltip("A tile instance will be made using this sprite texture.")]
        public Sprite Sprite;

        [Header("Use this for custom tiles")]
        [Tooltip("If supplied, this tile will be used, and sprite field will be ignored.")]
        public TileBase Tile;
    }
}
