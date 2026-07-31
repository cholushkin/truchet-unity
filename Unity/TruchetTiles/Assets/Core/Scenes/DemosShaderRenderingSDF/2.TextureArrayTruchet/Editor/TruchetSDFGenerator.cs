using UnityEngine;
using UnityEditor;
using NaughtyAttributes;
using System.IO;

[CreateAssetMenu(fileName = "TruchetSDFGenerator", menuName = "Tools/Truchet SDF Generator")]
public class TruchetSDFGenerator : ScriptableObject
{
    [Header("Texture Settings")]
    [Tooltip("Resolution of the baked SDF texture (e.g., 512, 1024).")]
    public int resolution = 512;
    
    [Tooltip("Directory to save the generated .exr files. Relative to project root.")]
    public string saveDirectory = "Assets/Textures/TruchetSDF";

    [Header("Shape Prefix")]
    public string filePrefix = "SDF_Tile_";

    [Button("Generate SDF Textures")]
    public void GenerateTextures()
    {
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        // Bake the two fundamental shapes
        BakeTexture("Arcs", CalculateArcsSDF);
        BakeTexture("Cross", CalculateCrossSDF);

        AssetDatabase.Refresh();
        Debug.Log($"<color=green><b>SDF Textures generated successfully at {saveDirectory}</b></color>");
    }

    private void BakeTexture(string shapeName, System.Func<Vector2, float> sdfMath)
    {
        // Use RFloat or RGBAHalf to ensure high precision for the distance field
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RFloat, false);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // Normalize pixel coordinates to [-0.5, 0.5] UV space
                float u = (x / (float)(resolution - 1)) - 0.5f;
                float v = (y / (float)(resolution - 1)) - 0.5f;
                Vector2 localUV = new Vector2(u, v);

                // Calculate the raw distance to the centerline
                float distance = sdfMath(localUV);

                // Store raw distance directly in the float texture. 
                // No need to map to 0-1 because we are using EXR/Float format!
                tex.SetPixel(x, y, new Color(distance, distance, distance, 1.0f));
            }
        }

        tex.Apply();

        // Encode to EXR (16-bit float) for flawless precision
        byte[] bytes = tex.EncodeToEXR(Texture2D.EXRFlags.None);
        string path = Path.Combine(saveDirectory, $"{filePrefix}{shapeName}.exr");
        
        File.WriteAllBytes(path, bytes);
        DestroyImmediate(tex);
    }

    // --- SDF MATHEMATICS ---

    /// <summary>
    /// Calculates the unsigned distance to the centerline of the classic Truchet arcs.
    /// </summary>
    private float CalculateArcsSDF(Vector2 uv)
    {
        // Distance from bottom-left corner (-0.5, -0.5) and top-right corner (0.5, 0.5)
        float dist1 = Vector2.Distance(uv, new Vector2(-0.5f, -0.5f)) - 0.5f;
        float dist2 = Vector2.Distance(uv, new Vector2(0.5f, 0.5f)) - 0.5f;
        
        // Return the distance to the *closest* centerline
        return Mathf.Min(Mathf.Abs(dist1), Mathf.Abs(dist2));
    }

    /// <summary>
    /// Calculates the unsigned distance to the centerline of an intersecting cross.
    /// </summary>
    private float CalculateCrossSDF(Vector2 uv)
    {
        float dist1 = Mathf.Abs(uv.x);
        float dist2 = Mathf.Abs(uv.y);
        
        return Mathf.Min(dist1, dist2);
    }
}