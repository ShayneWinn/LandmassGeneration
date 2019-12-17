using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    const float scale = 3f;

    public LODInfo[] detailLevels;
    public static float maxViewDist;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisInViewDis;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistanceeThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisInViewDis = Mathf.RoundToInt(maxViewDist / chunkSize);
        mapGenerator = FindObjectOfType<MapGenerator>();

        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        for (int yOffset = -chunksVisInViewDis; yOffset <= chunksVisInViewDis; yOffset++)
        {
            for (int xOffset = -chunksVisInViewDis; xOffset <= chunksVisInViewDis; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDict.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }


    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        int previousLODIndex = -1;

        MapData mapData;
        bool mapDataRecieved;

        public TerrainChunk(Vector2 _coord, int _size, LODInfo[] _detailLevels, Transform _parent, Material _material)
        {
            detailLevels = _detailLevels;
            position = _coord * _size;
            bounds = new Bounds(position, Vector2.one * _size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = _material;

            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = _parent;
            meshObject.transform.localScale = Vector3.one * scale;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for(int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDataRecieved);
        }

        void OnMapDataRecieved(MapData _mapData)
        {
            mapDataRecieved = true;
            mapData = _mapData;

            Texture2D texture = TextureGenerator.TextureFromColorMap(_mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (mapDataRecieved)
            {
                float viewerFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerFromNearestEdge <= maxViewDist;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerFromNearestEdge > detailLevels[i].visibleDistanceeThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }
        }

        public void SetVisible(bool _visible)
        {
            meshObject.SetActive(_visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }


    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int _lod, System.Action _updateCallback)
        {
            this.lod = _lod;
            this.updateCallback = _updateCallback;
        }

        void OnMeshDataRecieved(MeshData _meshData)
        {
            hasMesh = true;
            this.mesh = _meshData.CreateMesh();
            updateCallback();
        }

        public void RequestMesh(MapData _mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(_mapData, lod, OnMeshDataRecieved);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistanceeThreshold;
    }
}
