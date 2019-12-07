using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int _mapWidth, int _mapHeight, float _scale, int _seed, 
                                int _octaves, float _persistance, float _lacunarity, Vector2 _offset)
    {
        // Generate offsets
        System.Random prng = new System.Random(_seed);
        Vector2[] octaveOffsets = new Vector2[_octaves];
        for(int i = 0; i < _octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float[,] noiseMap = new float[_mapWidth, _mapHeight];

        // Avoide divide by zero error
        if (_scale <= 0) { _scale = 0.001f; }

        float maxNoise = float.MinValue;
        float minNoise = float.MaxValue;

        float halfWidth = _mapWidth / 2f;
        float halfHeight = _mapHeight / 2f;

        for (int y = 0; y < _mapHeight; y++)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                // Go through each octave and add it's value
                for (int i = 0; i < _octaves; i++)
                {
                    // Get the sample values
                    float sampleX = (x - halfWidth) / _scale * frequency + octaveOffsets[i].x + _offset.x;
                    float sampleY = (y - halfHeight) / _scale * frequency + octaveOffsets[i].y + _offset.y;

                    // Generate noise
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    // Change amplitude and frequency
                    amplitude *= _persistance;
                    frequency *= _lacunarity;
                }

                // Clamp noiseMap to -1 to 1
                if (noiseHeight > maxNoise) { maxNoise = noiseHeight; }
                else if (noiseHeight < minNoise) { minNoise = noiseHeight; }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < _mapHeight; y++)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoise, maxNoise, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}