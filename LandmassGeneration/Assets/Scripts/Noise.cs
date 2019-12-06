using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    public static float[,] GenerateNoiseMap(int _mapWidth, int _mapHeight, float _scale)
    {
        float[,] noiseMap = new float[_mapWidth, _mapHeight];

        if (_scale <= 0)
        {
            _scale = 0.001f;
        }

        for (int y = 0; y < _mapHeight; y++)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                float sampleX = x / _scale;
                float sampleY = y / _scale;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x, y] = perlinValue;
            }
        }

        return noiseMap;
    }
}