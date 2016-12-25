using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager e;
    void Awake() { e = this; }

    public Renderer trackRenderer;

    public Texture2D tex;

    private void Start()
    {
        Texture2D origTex = e.trackRenderer.material.mainTexture as Texture2D;
        tex = Instantiate(origTex) as Texture2D;

        //colors = new Color32[tex.width * tex.height];
        colors = tex.GetPixels32();

        trackRenderer.material.mainTexture = tex;
    }

    public Color32[] colors;

    private void Update()
    {
        // Apply main texture
        tex.SetPixels32(colors);
        tex.Apply();
    }

    public static void MultPixel(Color color, float u, float v, int size = 1)
    {
        Texture2D tex = e.tex;

        int x = Mathf.RoundToInt(u * tex.width);
        int y = Mathf.RoundToInt(v * tex.height);

        int w = tex.width;

        e.colors[y * w + x] *= color;

        if (size > 1)
        {
            e.colors[y * w + x - 1] *= color;
            e.colors[y * w + x + 1] *= color;
            e.colors[(y - 1) * w + x] *= color;
            e.colors[(y + 1) * w + x] *= color;
        }
    }
}
