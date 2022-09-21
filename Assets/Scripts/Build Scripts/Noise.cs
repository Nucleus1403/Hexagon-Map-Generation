using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Noise
{
    public static List<Vector2Int> FindLocalMaxim(MapData[,] noiseMap, float minDistance)
    {

        var localMaxim = new List<Vector2Int>();

        for (var x = 0; x < noiseMap.GetLength(0); x++)
        {
            for (var y = 0; y < noiseMap.GetLength(1); y++)
            {
                var noiseVal = noiseMap[x, y].Value;

                foreach (var max in localMaxim.Where(max => Vector2Int.Distance(max, new Vector2Int(x, y)) < minDistance))
                {
                    goto end;
                }

                if (CheckNeighbours(x, y, noiseMap, (neighbourNoise) => neighbourNoise > noiseVal))
                {
                    if (noiseVal is 0 or 1)
                        continue;

                    localMaxim.Add(new Vector2Int(x, y));
                }
                end:
                continue;

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