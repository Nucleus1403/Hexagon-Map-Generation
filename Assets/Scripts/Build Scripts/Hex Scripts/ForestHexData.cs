using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "HexData/Forest Hex")]
public class ForestHexData : HexData
{
    public List<ForestData> ForestData;
}

[Serializable]
public class ForestData
{
    public ForestLevel Level;
    public GameObject Prefab;
}

public enum ForestLevel
{
    Low,Medium,High
}
