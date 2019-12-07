using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, DrawMesh }
    public DrawMode drawMode;

    public bool autoUpdate;

    public int seed;
    public float height;
    public AnimationCurve meshHeightCurve;
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    public float persistence;
    public float lacunarity;

    public Vector2 offset;

    public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, seed,
                                                   octaves, persistence, lacunarity, offset);

        Color[] colorMap = new Color[mapHeight * mapWidth];

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for(int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawMode == DrawMode.ColorMap)
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        else if (drawMode == DrawMode.DrawMesh)
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, height, meshHeightCurve), TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
    }

    private void OnValidate()
    {
        if (mapWidth < 1) { mapWidth = 1; }
        if (mapHeight < 1) { mapHeight = 1; }
        if (lacunarity < 1) { lacunarity = 1; }
        if (octaves < 1) { octaves = 1; }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
