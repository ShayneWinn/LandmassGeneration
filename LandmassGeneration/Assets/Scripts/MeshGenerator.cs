using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] _heightMap, float _height, AnimationCurve _heightCurve, int _lod)
    {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        int width = _heightMap.GetLength(0);
        int height = _heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (_lod == 0) ? 1 : _lod * 2;
        int verticiesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        for(int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {
                meshData.verticies[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(_heightMap[x, y]) * _height, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                if(x < width-1 && y < height-1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticiesPerLine + 1, vertexIndex + verticiesPerLine);
                    meshData.AddTriangle(vertexIndex + verticiesPerLine + 1, vertexIndex, vertexIndex + 1);
                }
                vertexIndex += 1;
            }
        }

        return meshData;
    }
}


public class MeshData
{
    public Vector3[] verticies;
    public int[] triangles;
    public Vector2[] uvs;

    int triIndex;

    public MeshData(int _meshWidth, int _meshHeight)
    {
        verticies = new Vector3[_meshWidth * _meshHeight];
        uvs = new Vector2[_meshWidth * _meshHeight];
        triangles = new int[(_meshWidth - 1) * (_meshHeight - 1) * 6];
    }

    public void AddTriangle(int _v1, int _v2, int _v3)
    {
        triangles[triIndex] = _v1;
        triangles[triIndex + 1] = _v2;
        triangles[triIndex + 2] = _v3;
        triIndex += 3;
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[verticies.Length];
        int triangleCount = triangles.Length / 3;
        for(int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 trianglesNormal = SurfaceNormalFromIndicies(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += trianglesNormal;
            vertexNormals[vertexIndexB] += trianglesNormal;
            vertexNormals[vertexIndexC] += trianglesNormal;
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndicies(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = verticies[indexA];
        Vector3 pointB = verticies[indexB];
        Vector3 pointC = verticies[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = CalculateNormals();
        return mesh;
    }
}