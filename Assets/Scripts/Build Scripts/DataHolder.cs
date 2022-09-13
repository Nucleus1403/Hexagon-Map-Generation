using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "HexData/Hex Data Holder")]
public class DataHolder : ScriptableObject
{
    public List<HexData> HexDataList = new List<HexData>();
}