using System;
using UnityEditor;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "HexData/Generic Hex")]
public class HexData : HexDataBase
{
    public Sprite Sprite;
    public GameObject Prefab;
}
#if UNITY_EDITOR
[CustomEditor(typeof(HexData))]
public class HexDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var myTarget = (HexData)target;
        var texture = AssetPreview.GetAssetPreview(myTarget.Sprite);
        GUILayout.Label(texture);
    }
}
#endif

