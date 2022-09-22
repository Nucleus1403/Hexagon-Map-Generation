using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "HexData/Forest Hex")]
public class ForestHexData : HexData
{
    public List<ForestData> ForestData;

    public GameObject GetPrefabByLevel(ForestLevel level)
    {
        foreach (var value in ForestData)
        {
            if (level == value.Level)
                return value.Prefab;
        }

        return null;
    }
}

[Serializable]
public class ForestData
{
    public ForestLevel Level;
    public GameObject Prefab;
}


public enum ForestLevel
{
    None,Low,Medium,High
}
