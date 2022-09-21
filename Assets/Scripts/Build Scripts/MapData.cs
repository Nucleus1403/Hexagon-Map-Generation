using System;
using UnityEngine;

[Serializable]
public class MapData
{
    public GameObject Prefab;
    public Vector2Int Location;

    public float Value;

    public HexType Type;
    public HexLevel HeightLevel;
    
    public MapData(GameObject prefab, Vector2Int location, HexType type)
    {
        Prefab = prefab;
        Location = location;
        Type = type;
    }

    public MapData(Vector2Int location, float value)
    {
        Location = location;
        Type = HexType.Undefined;
        Value = value;
    }

    public MapData()
    {
        Type = HexType.Undefined;
    }
}