using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
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

    [Space]
    [Header("Forest Settings")]
    public List<HexLevel> AcceptedLevel;
    public ForestHexData ForestHexData;
    public List<MapForestLevels> MapForestLevels;

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

    public MapData[,] Map { get; private set; }

    private float[,] _forestMap;

    private List<Vector2Int> _riverStartLocations;

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

        GenerateNoiseMaps();

        //SetOcean();

        SetRiverStartLocations();

        ShowMap();

        //CreateMap();
        SetOcean();

        Invoke(nameof(StartRiverGeneration), 0.5f);

        //WaterMeshCombiner.CombineMeshes();

    }

    private void ShowMap()
    {
        for (var x = 0; x < MapLength; x++)
        {
            for (var y = 0; y < MapLength; y++)
            {
                GameObject go = null;

                go = GenerateHex(go, Map[x, y], _forestMap[x, y]);

                if (y % 2 == 0)
                    go.transform.position = new Vector3((-MapLength / 2f) * Difference.x + x * Difference.x, go.transform.position.y, (-MapLength / 2f) * Difference.z + y * Difference.z);
                else
                    go.transform.position = new Vector3((-MapLength / 2f) * Difference.x + x * Difference.x + 0.5f, go.transform.position.y, (-MapLength / 2f) * Difference.z + y * Difference.z);

                go.transform.SetParent(this.transform);

                MapTilesList.Add(go);
            }
        }
    }

    private MapHeightLevels GetMapHeightLevels(float value)
    {
        foreach (var level in MapLevels.Where(level => value <= level.Limit))
        {
            return level;
        }

        return MapLevels[^0];
    }

    private MapForestLevels GetMapForestLevels(float value)
    {
        foreach (var level in MapForestLevels.Where(level => value <= level.Limit))
        {
            return level;
        }

        return MapForestLevels[0];
    }

    private GameObject GenerateHex(GameObject go, MapData data, float forestValue)
    {
        var level = GetMapHeightLevels(data.Value);

        data.HeightLevel = level.Level;
        var height = (int)(data.Value * 100);

        if (level.Level == HexLevel.Water)
        {
            go = Instantiate(level.HexData.Prefab, new Vector3(0, 0, 0), Quaternion.identity);
            go.transform.position = new Vector3(go.transform.position.x, level.Limit, go.transform.position.z);
            WaterMeshCombiner.AddMeshes(go.GetComponentInChildren<MeshFilter>());

            data.Type = HexType.Water;
            data.Prefab = go;
            return go;
        }

        if (data.Type == HexType.RiverStart)
        {
            go = Instantiate(DataHolder.GetHexOfType(HexType.RiverStart).Prefab, new Vector3(0, 0, 0), Quaternion.identity);
            go.transform.position = new Vector3(go.transform.position.x, (float)(height - (height % 5)) / 100, go.transform.position.z);

            data.Type = HexType.RiverStart;
            data.Prefab = go;
            return go;
        }

        switch (level.Level)
        {
            case HexLevel.Shoreline:
            case HexLevel.Plain:
            case HexLevel.HighPlain:
                var forestLevel = GetMapForestLevels(forestValue);

                switch (forestLevel.Level)
                {
                    case ForestLevel.None:
                        go = Instantiate(level.HexData.Prefab, new Vector3(0, 0, 0), Quaternion.identity);
                        break;
                    case ForestLevel.Low:
                    case ForestLevel.Medium:
                    case ForestLevel.High:
                        go = Instantiate(ForestHexData.GetPrefabByLevel(forestLevel.Level), new Vector3(0, 0, 0), Quaternion.identity);
                        if (level.Level == HexLevel.Shoreline || level.Level == HexLevel.Plain)
                        {
                            go.transform.GetChild(1).gameObject.SetActive(false);
                        }
                        else
                        { 
                            go.transform.GetChild(0).gameObject.SetActive(false);
                        }
                        break;
                }
                break;
            case HexLevel.Hill:
            case HexLevel.HighHill:
            case HexLevel.Mountain:
                go = Instantiate(level.HexData.Prefab, new Vector3(0, 0, 0), Quaternion.identity);

                break;
        }

        go.transform.position = new Vector3(go.transform.position.x, (float)(height - (height % 5)) / 100, go.transform.position.z);

        data.Prefab = go;
        return go;
    }

    private void GenerateNoiseMaps()
    {
        Map = Noise.GenerateNoiseMapWithFalloff(MapLength, NoiseScale, Offset);
        _forestMap = Noise.GenerateNoiseMap(MapLength, MapLength, 0.16f, Offset * 3);

    }

    private void SetOcean()
    {
        SetToOcean(0, 0);

    }

    private void SetToOcean(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < MapLength && y < MapLength)
        {
            if (Map[x, y].Prefab.transform.GetChild(0).tag != "water") return;

            Map[x, y].Prefab.transform.GetChild(0).tag = "water_ocean";

            SetToOcean(x + 1, y);
            SetToOcean(x - 1, y);
            SetToOcean(x, y + 1);
            SetToOcean(x, y - 1);
        }
    }

    private void StartRiverGeneration()
    {
        foreach (var riverStartLocation in _riverStartLocations)
        {
            Map[riverStartLocation.x, riverStartLocation.y].Prefab.GetComponent<RiverGenerator>().StartSearching();
        }
    }


    private void SetRiverStartLocations()
    {
        var result = Noise.FindLocalMaxim(Map, 3);

        result = result.Where(pos => Map[pos.x, pos.y].Value > RiverLevel).OrderByDescending(pos => Map[pos.x, pos.y].Value).Take(RiverPoints).ToList();

        foreach (var riverStartLocation in result)
        {
            Map[riverStartLocation.x, riverStartLocation.y].Type = HexType.RiverStart;
        }

        _riverStartLocations = result;
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
                    if (Map[x, y].Value <= level.Limit)
                    {
                        if (Map[x, y].Type != HexType.RiverStart)
                        {
                            go = Instantiate(level.HexData.Prefab, new Vector3(0, 0, 0), Quaternion.identity);
                        }
                        else
                            go = Instantiate(DataHolder.GetHexOfType(HexType.RiverStart).Prefab, new Vector3(0, 0, 0), Quaternion.identity);

                        if (level.HexData.Type == HexType.Water)
                        {
                            go.transform.position = new Vector3(go.transform.position.x, level.Limit, go.transform.position.z);
                            WaterMeshCombiner.AddMeshes(go.GetComponentInChildren<MeshFilter>());

                        }
                        else
                        {
                            var height = (int)(Map[x, y].Value * 100);
                            go.transform.position = new Vector3(go.transform.position.x, (float)(height - (height % 5)) / 100, go.transform.position.z);

                        }

                        if (Map[x, y].Type == HexType.Undefined)
                            Map[x, y].Type = level.HexData.Type;

                        Map[x, y].Prefab = go;
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

    public void SetRiverLocation(GameObject prefab, Vector2Int location)
    {
        Map[location.x, location.y].Type = HexType.River;
        Map[location.x, location.y].Prefab = prefab;
    }

    public Vector2Int GetTileLocationByPosition(Vector2 location)
    {
        float x = (int)Math.Floor(location.x);
        x = (x - (-MapLength / 2f) * Difference.x) / Difference.x;

        float y = (location.y - (-MapLength / 2f) * Difference.z) / Difference.z;

        return new Vector2Int((int)x, (int)y);
    }

    public void DestroyHex(Vector2Int location)
    {
        Destroy(Map[location.x, location.y].Prefab);
    }

    private void DestroyMap()
    {
        if (!Map.IsUnityNull())
            foreach (var go in Map)
            {
                Destroy(go.Prefab);
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
    public HexLevel Level;
    public HexData HexData;
    public float Limit;
}

[Serializable]
public class MapForestLevels
{
    public float Limit;
    public ForestLevel Level;
}

public enum HexLevel
{
    Water, Shoreline, Plain, HighPlain, Hill, HighHill, Mountain
}
public enum HexType
{
    Undefined, River, RiverStart, Grass, Forest, Water, Mountain, ForestHill, Cabin, Hill, Castle, Sand, SandRock, WaterRock
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