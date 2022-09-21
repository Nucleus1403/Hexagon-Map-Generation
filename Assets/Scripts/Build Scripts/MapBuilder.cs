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

        _map = Noise.GenerateNoiseMapWithFalloff(MapLength, NoiseScale, Offset);

        SetRiverStartLocations();

        CreateMap();

        SetOcean();

        Invoke(nameof(StartRiverGeneration), 0.5f);

        //WaterMeshCombiner.CombineMeshes();

    }

    private void SetOcean()
    {
        SetToOcean(0, 0);

    }

    private void SetToOcean(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < MapLength && y < MapLength)
        {
            if (_map[x, y].Prefab.transform.GetChild(0).tag != "water") return;

            _map[x, y].Prefab.transform.GetChild(0).tag = "water_ocean";

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
            _map[riverStartLocation.x, riverStartLocation.y].Prefab.GetComponent<RiverGenerator>().StartSearching();
        }
    }


    private void SetRiverStartLocations()
    {
        var result = Noise.FindLocalMaxim(_map, 3);

        result = result.Where(pos => _map[pos.x, pos.y].Value > RiverLevel).OrderByDescending(pos => _map[pos.x, pos.y].Value).Take(RiverPoints).ToList();

        foreach (var riverStartLocation in result)
        {
            _map[riverStartLocation.x, riverStartLocation.y].Type = HexType.RiverStart;
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
                    if (_map[x, y].Value <= level.Limit)
                    {
                        if (_map[x, y].Type != HexType.RiverStart)
                            go = Instantiate(level.HexData.Prefab, new Vector3(0, 0, 0), Quaternion.identity);
                        else
                            go = Instantiate(DataHolder.GetHexOfType(HexType.RiverStart).Prefab, new Vector3(0, 0, 0), Quaternion.identity);

                        if (level.HexData.Type == HexType.Water)
                        {
                            go.transform.position = new Vector3(go.transform.position.x, level.Limit, go.transform.position.z);
                            WaterMeshCombiner.AddMeshes(go.GetComponentInChildren<MeshFilter>());

                        }
                        else
                        {
                            var height = (int)(_map[x, y].Value * 100);
                            go.transform.position = new Vector3(go.transform.position.x, (float)(height - (height % 5)) / 100, go.transform.position.z);

                        }

                        if (_map[x, y].Type == HexType.Undefined)
                            _map[x, y].Type = level.HexData.Type;

                        _map[x, y].Prefab = go;
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
        _map[location.x, location.y].Type = HexType.River;
        _map[location.x, location.y].Prefab = prefab;
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
        Destroy(_map[location.x, location.y].Prefab);
    }

    private void DestroyMap()
    {
        if (!_map.IsUnityNull())
            foreach (var go in _map)
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