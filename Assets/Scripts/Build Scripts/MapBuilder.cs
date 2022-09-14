using System;
using System.Collections.Generic;
using System.Linq;
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
    public int MapLength = 32;
    public float NoiseScale = 0.2f;

    public List<MapHeightLevels> MapLevels;

    public float RiverLevel = 0.6f;

    public int RiverPoints = 10;

    [Space]
    public Vector2 Offset;

    [Space]
    [Header("Extra Canvas Settings")]
    public RawImage Image;

    [Space] public WaterMeshCombiner WaterMeshCombiner;

    [HideInInspector]
    public List<GameObject> MapTilesList = new List<GameObject>();

    private MapData[,] _map;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        GenerateRealMap();
    }

    public void GenerateRealMap()
    {
        DestroyMap();

        _map = Noise.GenerateNoiseMapWithFalloff(MapLength, NoiseScale, Offset);

        SetRiverStartLocations();

        CreateMap();



        //WaterMeshCombiner.CombineMeshes();

    }


    private void SetRiverStartLocations()
    {
        var result = Noise.FindLocalMaxim(_map);

        result = result.Where(pos => _map[pos.x, pos.y].Value > RiverLevel).OrderBy(pos => _map[pos.x, pos.y].Value).Take(RiverPoints).ToList();

        foreach (var riverStartLocation in result)
        {
            _map[riverStartLocation.x, riverStartLocation.y].Type = HexType.RiverStart;
        }
    }

    private void CreateMap()
    {
        for (var x = 0; x < MapLength; x++)
        {
            for (var y = 0; y < MapLength; y++)
            {
                GameObject go = null;

                foreach (var level in MapLevels)
                {
                    if (_map[x, y].Value < level.Limit)
                    {
                        if (_map[x, y].Type != HexType.RiverStart)
                            go = Instantiate(level.HexData.Prefab, new Vector3(0, 0, 0), Quaternion.identity);
                        else
                            go = Instantiate(DataHolder.GetHexOfType(HexType.River).Prefab, new Vector3(0, 0, 0), Quaternion.identity);

                        if (level.HexData.Type == HexType.Water)
                        {
                            go.transform.position = new Vector3(go.transform.position.x, level.Limit, go.transform.position.z);
                            WaterMeshCombiner.AddMeshes(go.GetComponentInChildren<MeshFilter>());
                            _map[x, y].Type = HexType.Water;
                            break;
                        }

                        var height = (int)(_map[x, y].Value * 100);
                        go.transform.position = new Vector3(go.transform.position.x, (float)(height - (height % 5)) / 100, go.transform.position.z);

                        if (_map[x, y].Type == HexType.Undefined)
                            _map[x, y].Type = level.HexData.Type;

                        break;
                    }
                }

                if (y % 2 == 0)
                    go.transform.position = new Vector3((-MapLength / 2f) * Difference.x + x * Difference.x, go.transform.position.y, (-MapLength / 2f) * Difference.z + y * Difference.z);
                else
                    go.transform.position = new Vector3((-MapLength / 2f) * Difference.x + x * Difference.x + 0.5f, go.transform.position.y, (-MapLength / 2f) * Difference.z + y * Difference.z);

                go.transform.SetParent(this.transform);

                MapTilesList.Add(go);
            }
        }

    }

    private void DestroyMap()
    {
        foreach (var go in MapTilesList)
        {
            Destroy(go);
        }

        WaterMeshCombiner.RemoveAllMeshes();
    }

    public void GenerateMapExample()
    {
        var noiseMap = Noise.GenerateNoiseMap(MapLength, MapLength, NoiseScale, Offset);

        Color[] pixel = new Color[MapLength * MapLength];
        int i = 0;
        for (int x = 0; x < MapLength; x++)
        {
            for (int y = 0; y < MapLength; y++)
            {
                pixel[i] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
                i++;
            }
        }

        Texture2D tex = new Texture2D(MapLength, MapLength);
        tex.SetPixels(pixel);
        tex.filterMode = FilterMode.Point;
        tex.Apply();

        Image.texture = tex;
    }
}
[Serializable]
public class MapHeightLevels
{
    public HexData HexData;
    public float Limit;
}

[Serializable]
public class MapData
{
    public GameObject Prefab;
    public Vector2Int Location;

    public float Value;

    public HexType Type;

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

public enum HexType
{
    Undefined, River, RiverStart, Grass, Forest, Water, Mountain, ForestHill, Cabin, Hill, Castle
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
    public static List<Vector2Int> FindLocalMaxim(MapData[,] noiseMap)
    {
        var localMaxim = new List<Vector2Int>();

        for (var x = 0; x < noiseMap.GetLength(0); x++)
        {
            for (var y = 0; y < noiseMap.GetLength(1); y++)
            {
                var noiseVal = noiseMap[x, y].Value;

                if (CheckNeighbours(x, y, noiseMap, (neighbourNoise) => neighbourNoise > noiseVal))
                {
                    if (noiseVal == 0)
                        continue;

                    localMaxim.Add(new Vector2Int(x, y));
                }

            }
        }
        return localMaxim;
    }

    private static bool CheckNeighbours(int x, int y, MapData[,] noiseMap, Func<float, bool> failCondition)
    {
        var directions = new List<Vector2Int>
        {
            new Vector2Int( 0, 1), //N
            new Vector2Int( 1, 1), //NE
            new Vector2Int( 1, 0), //E
            new Vector2Int(-1, 1), //SE
            new Vector2Int(-1, 0), //S
            new Vector2Int(-1,-1), //SW
            new Vector2Int( 0,-1), //W
            new Vector2Int( 1,-1)  //NW
        };

        foreach (var dir in directions)
        {
            var newPost = new Vector2Int(x + dir.x, y + dir.y);

            if (newPost.x < 0 || newPost.x >= noiseMap.GetLength(0) || newPost.y < 0 || newPost.y >= noiseMap.GetLength(1))
            {
                continue;
            }

            if (failCondition(noiseMap[x + dir.x, y + dir.y].Value))
            {
                return false;
            }
        }
        return true;
    }

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
                float f = i / (float)size * 2 - 1;
                float t = j / (float)size * 2 - 1;
                float value = Mathf.Max(Mathf.Abs(f), Mathf.Abs(t));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    public static MapData[,] GenerateNoiseMapWithFalloff(int size, float scale, Vector2 offset)
    {
        var noiseMap = new MapData[size, size];
        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                var samplePosX = (float)i * scale + offset.x;
                var samplePosY = (float)j * scale + offset.y;

                var f = i / (float)size * 2 - 1;
                var t = j / (float)size * 2 - 1;

                var valueFalloff = Evaluate(Mathf.Max(Mathf.Abs(f), Mathf.Abs(t)));
                var valueMap = Mathf.PerlinNoise(samplePosX, samplePosY);
                var value = Mathf.Clamp01(valueMap - valueFalloff);

                noiseMap[i, j] = new MapData(new Vector2Int(i, j), value)
                {
                    Location = new Vector2Int(i, j)
                };
            }
        }

        return noiseMap;
    }

    public static float Evaluate(float value)
    {
        float a = 3;
        float b = 5.7f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}