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

        trackRenderer.material.mainTexture = tex;
    }

    public static void MultPixel(Color color, float u, float v)
    {
        Texture2D tex = e.tex;

        int x = Mathf.RoundToInt(u * tex.width);
        int y = Mathf.RoundToInt(v * tex.height);

        Debug.Log(x + ", " + y);

        Color c = tex.GetPixel(x, y);
        tex.SetPixel(x, y, c * color);
        tex.Apply();
    }
}
