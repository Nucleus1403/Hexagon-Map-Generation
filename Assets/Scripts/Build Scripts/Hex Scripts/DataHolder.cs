using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "HexData/Hex Data Holder")]
public class DataHolder : ScriptableObject
{
    public List<HexData> HexDataList = new List<HexData>();

    public HexData GetHexOfType(HexType type)
    {
        foreach (var hex in HexDataList)
        {
            if (hex.Type == type)
            {
                return hex;
            }

        }

        return null;
    }
}