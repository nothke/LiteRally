using UnityEngine;

[System.Serializable]
public class VehiclePaint
{
    public Color primaryColor;
    public Color secondaryColor;

    public void Paint(Renderer renderer)
    {
        if (renderer.sharedMaterial.shader.name != "LiteRally/Splat")
        {
            Debug.LogError("The vehicle doesn't have a LiteRally/Splat shader. Can't paint.");
            return;
        }

        renderer.material.SetColor("_PaintPrimary", primaryColor);
        renderer.material.SetColor("_PaintSecondary", secondaryColor);
    }
}
