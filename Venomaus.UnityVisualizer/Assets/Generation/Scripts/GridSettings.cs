using System;
using UnityEngine;

public class GridSettings : MonoBehaviour
{
    public static GridSettings Instance;

    public enum FlowGridType
    {
        Static,
        Procedural
    }

    public FlowGridType GridType;

    private void Awake()
    {
        if (Instance != null)
            throw new Exception("Cannot have multiple instances of GridSettings.");
        Instance = this;
    }
}
