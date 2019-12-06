using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;

    public void DrawNoiseMap(float[,] _noiseMap)
    {
        int width = _noiseMap.GetLength(0);
        int height = _noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < 0; x++)
            {
                colorMap[y * width + x] = _noiseMap[x, y];
            }
        }
    }
}
