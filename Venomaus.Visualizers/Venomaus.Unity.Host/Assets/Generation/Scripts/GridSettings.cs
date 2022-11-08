using Assets.Generation.Scripts;
using System;
using UnityEngine;

public class GridSettings : MonoBehaviour
{
    public static GridSettings Instance;

    public int Width, Height, ChunkWidth, ChunkHeight, WorldSeed;

    public TileGraphic TerrainGraphic;
    public TileGraphic ObjectsGraphic;

    public enum FlowGridType
    {
        Static,
        Procedural
    }

    public enum Grid
    {
        Terrain,
        Objects
    }

    public FlowGridType GridType;

    private void Awake()
    {
        if (Instance != null)
            throw new Exception("Cannot have multiple instances of GridSettings.");
        Instance = this;
    }
}
