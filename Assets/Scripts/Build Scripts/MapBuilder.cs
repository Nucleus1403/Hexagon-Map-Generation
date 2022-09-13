using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class MapBuilder : MonoBehaviour
{
    public static MapBuilder Instance;
    [Header("Map Data")]
    public DataHolder DataHolder;
    public Vector3 Difference;

    [Space]
    [Header("Map Generation Settings")]
    public int MapWidth = 32;
    public int MapHeight = 32;
    public float NoiseScale = 0.2f;

    public List<MapHeightLevels> MapLevels;

    [Space]
    public Vector2 Offset;

    [Space]
    [Header("Extra Canvas Settings")]
    public RawImage Image;

    [HideInInspector]
    public List<GameObject> MapTilesList = new List<GameObject>();

    [Space] public WaterMeshCombiner WaterMeshCombiner;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        GenerateRealMap();
    }

    public void GenerateMap()
    {
        var noiseMap = Noise.GenerateNoiseMap(MapWidth, MapHeight, NoiseScale, Offset);

        Color[] pixel = new Color[MapHeight * MapWidth];
        int i = 0;
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                pixel[i] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
                i++;
            }
        }

        Texture2D tex = new Texture2D(MapWidth, MapHeight);
        tex.SetPixels(pixel);
        tex.filterMode = FilterMode.Point;
        tex.Apply();

        Image.texture = tex;
    }

    public void GenerateRealMap()
    {
        DestroyMap();

        var falloffMap = Noise.GenerateFalloffMap(MapWidth);

        var noiseMap = Noise.GenerateNoiseMap(MapWidth, MapHeight, NoiseScale, Offset);

        for (var x = 0; x < MapWidth; x++)
        {
            for (var y = 0; y < MapHeight; y++)
            {
                noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);

                GameObject go = null;

                foreach (var level in MapLevels)
                {
                    if (noiseMap[x, y] < level.Limit)
                    {

                        go = Instantiate(level.HexData.Prefab, new Vector3(0, 0, 0), Quaternion.identity);

                        if (level.HexData.Id == "water")
                        {
                            go.transform.position = new Vector3(go.transform.position.x, level.Limit, go.transform.position.z);
                            WaterMeshCombiner.AddMeshes(go.GetComponentInChildren<MeshFilter>());
                        }
                        else
                        {
                            int height = (int)(noiseMap[x, y] * 100);
                            go.transform.position = new Vector3(go.transform.position.x, (float)(height - (height % 5)) / 100, go.transform.position.z);
                        }

                        break;
                    }
                }

                if (y % 2 == 0)
                    go.transform.position = new Vector3((-MapWidth / 2f) * Difference.x + x * Difference.x, go.transform.position.y, (-MapHeight / 2f) * Difference.z + y * Difference.z);
                else
                    go.transform.position = new Vector3((-MapWidth / 2f) * Difference.x + x * Difference.x + 0.5f, go.transform.position.y, (-MapHeight / 2f) * Difference.z + y * Difference.z);

                go.transform.SetParent(this.transform);

                MapTilesList.Add(go);
            }
        }

        //WaterMeshCombiner.CombineMeshes();

    }

    private void DestroyMap()
    {
        foreach (var go in MapTilesList)
        {
            Destroy(go);
        }

        WaterMeshCombiner.RemoveAllMeshes();
    }
}
[Serializable]
public class MapHeightLevels
{
    public HexData HexData;
    public float Limit;
}

#if UNITY_EDITOR
[CustomEditor(typeof(MapBuilder))]
public class MapBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var myTarget = (MapBuilder)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Build Map"))
        {
            myTarget.GenerateRealMap();
        }
    }
}
#endif


public static class Noise
{
    public static float[,] GenerateNoiseMap(int width, int height, float scale, Vector2 offset)
    {
        var noiseMap = new float[width, height];
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var samplePosX = (float)i * scale + offset.x;
                var samplePosY = (float)j * scale + offset.y;

                noiseMap[i, j] = Mathf.PerlinNoise(samplePosX, samplePosY);
            }
        }

        return noiseMap;
    }

    public static float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;
                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    public static float Evaluate(float value)
    {
        float a = 3;
        float b = 5.7f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}