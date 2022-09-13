using UnityEngine;

public class HexDataBase : ScriptableObject
{
    public string Id;
}

public enum HexDirection
{
    N,   //X,
    NW, //XZ,
    NE,//X_Z,
    SW,//_XZ,
    SE,//_X_Z,
    S     //Z
};
