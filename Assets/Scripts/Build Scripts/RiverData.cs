using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "HexData/River Data")]
public class RiverData : ScriptableObject
{
    public List<RiverDataStruct> Models = new List<RiverDataStruct>();
}

[Serializable]
public class RiverDataStruct
{
    public string Type;
    public int EdgeCount;
    public GameObject Prefab;
}
