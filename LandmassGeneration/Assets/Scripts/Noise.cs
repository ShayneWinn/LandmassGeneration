using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    public enum NormalizeMode { Local, Global }

    public static float[,] GenerateNoiseMap(int _mapWidth, int _mapHeight, float _scale, int _seed, 
                                int _octaves, float _persistance, float _lacunarity, Vector2 _offset, NormalizeMode _normalizeMode)
    {
        // Generate offsets
        System.Random prng = new System.Random(_seed);
        Vector2[] octaveOffsets = new Vector2[_octaves];

        float maxPossibleHeight = 0f;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < _octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + _offset.x;
            float offsetY = prng.Next(-100000, 100000) - _offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= _persistance;
        }

        float[,] noiseMap = new float[_mapWidth, _mapHeight];

        // Avoide divide by zero error
        if (_scale <= 0) { _scale = 0.001f; }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = _mapWidth / 2f;
        float halfHeight = _mapHeight / 2f;

        for (int y = 0; y < _mapHeight; y++)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                // Go through each octave and add it's value
                for (int i = 0; i < _octaves; i++)
                {
                    // Get the sample values
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / _scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / _scale * frequency;

                    // Generate noise
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    // Change amplitude and frequency
                    amplitude *= _persistance;
                    frequency *= _lacunarity;
                }

                // Clamp noiseMap to -1 to 1
                if (noiseHeight > maxLocalNoiseHeight) { maxLocalNoiseHeight = noiseHeight; }
                else if (noiseHeight < minLocalNoiseHeight) { minLocalNoiseHeight = noiseHeight; }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < _mapHeight; y++)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                if (_normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 1.75f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }
}