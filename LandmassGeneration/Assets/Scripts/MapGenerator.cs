using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, DrawMesh }
    public DrawMode drawMode;
    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int EeditorPreviewLOD;

    public bool autoUpdate;

    public int seed;
    public float heightMultiplier;
    public AnimationCurve meshHeightCurve;
    public float noiseScale;

    public int octaves;
    public float persistence;
    public float lacunarity;

    public Vector2 offset;

    public TerrainType[] regions;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector3.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        else if (drawMode == DrawMode.ColorMap)
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        else if (drawMode == DrawMode.DrawMesh)
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, heightMultiplier, meshHeightCurve, EeditorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
    }

    public void RequestMapData(Vector2 _center, Action<MapData> _callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(_center, _callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 _center, Action<MapData> _callback)
    {
        MapData mapData = GenerateMapData(_center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(_callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int _lod, Action<MeshData> _callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, _lod, _callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int _lod, Action<MeshData> _callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, heightMultiplier, meshHeightCurve, _lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(_callback, meshData));
        }
    }

    private void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    public MapData GenerateMapData(Vector2 _center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale, seed, octaves, persistence, lacunarity, _center + offset, normalizeMode);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        for(int y = 0; y < mapChunkSize; y++) {
            for(int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x, y];

                for(int i = 0; i < regions.Length; i++) {
                    if (currentHeight >= regions[i].height) {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
    }

    private void OnValidate()
    {
        if (lacunarity < 1) { lacunarity = 1; }
        if (octaves < 1) { octaves = 1; }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> _callback, T _parameter)
        {
            this.callback = _callback;
            this.parameter = _parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] _heightMap, Color[] _colorMap)
    {
        this.heightMap = _heightMap;
        this.colorMap = _colorMap;
    }
}
