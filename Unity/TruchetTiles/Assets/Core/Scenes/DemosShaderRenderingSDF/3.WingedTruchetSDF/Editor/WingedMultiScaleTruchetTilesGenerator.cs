using UnityEngine;
using UnityEditor;
using System.IO;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "WingedMultiScaleTruchetTilesGenerator", menuName = "Tools/Winged MultiScale Truchet Tiles Generator")]
public class WingedMultiScaleTruchetTilesGenerator : ScriptableObject
{
    [Header("Texture Settings")]
    public int resolution = 512;
    public string saveDirectory = "Assets/Textures/WingedMultiScaleSDF";
    public string arrayFileName = "SDF_WingedArray.asset";

    // Carlson Constants
    private const float LineHalfThickness = 1.0f / 6.0f;
    private const float WingRadius = 1.0f / 3.0f;

    [Button("Generate Pure Skeleton Array")]
    public void GenerateTextures()
    {
        if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

        // Bake boolean-resolved distances into RGFloat textures
        Texture2D texSlash = BakeTexture(CalculateForegroundSlash);
        Texture2D texPlus = BakeTexture(CalculateForegroundPlus);

        Texture2DArray array = new Texture2DArray(resolution, resolution, 2, TextureFormat.RGFloat, false, true)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Graphics.CopyTexture(texSlash, 0, array, 0);
        Graphics.CopyTexture(texPlus, 0, array, 1);

        string arrayPath = Path.Combine(saveDirectory, arrayFileName);
        AssetDatabase.CreateAsset(array, arrayPath);
        AssetDatabase.SaveAssets();

        DestroyImmediate(texSlash);
        DestroyImmediate(texPlus);

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = array;
        Debug.Log($"<color=green><b>Baked Multi-Scale SDF Array generated at {arrayPath}</b></color>");
    }

    private Texture2D BakeTexture(System.Func<Vector2, float> foregroundMath)
    {
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGFloat, false);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // Expanded domain: Texture UV 0..1 maps to -1..1
                float u = (x / (float)(resolution - 1)) * 2.0f - 1.0f;
                float v = (y / (float)(resolution - 1)) * 2.0f - 1.0f;
                Vector2 uv = new Vector2(u, v);
                
                float ownershipSDF = CalculateOwnership(uv);
                float foregroundSDF = foregroundMath(uv);

                tex.SetPixel(x, y, new Color(foregroundSDF, ownershipSDF, 0, 1.0f));
            }
        }
        tex.Apply();
        return tex;
    }

    // --- BAKED SDF BOOLEAN OPERATIONS ---

    private float CalculateForegroundSlash(Vector2 uv)
    {
        // Full circles bound to ownership mathematically equate to perfect corner arcs
        float arc1 = Mathf.Abs(Vector2.Distance(uv, new Vector2(-0.5f, 0.5f)) - 0.5f) - LineHalfThickness;
        float arc2 = Mathf.Abs(Vector2.Distance(uv, new Vector2(0.5f, -0.5f)) - 0.5f) - LineHalfThickness;
        float unbounded = Mathf.Min(arc1, arc2);
        
        // Intersection (Max) with ownership prevents dangling lines
        return Mathf.Max(unbounded, CalculateOwnership(uv));
    }

    private float CalculateForegroundPlus(Vector2 uv)
    {
        float dX = Mathf.Abs(uv.y) - LineHalfThickness;
        float dY = Mathf.Abs(uv.x) - LineHalfThickness;
        float unbounded = Mathf.Min(dX, dY);
        
        // Intersection (Max) with ownership
        return Mathf.Max(unbounded, CalculateOwnership(uv));
    }

    private float CalculateOwnership(Vector2 uv)
    {
        // 1. Core Square [-0.5, 0.5]
        Vector2 d = new Vector2(Mathf.Abs(uv.x), Mathf.Abs(uv.y)) - new Vector2(0.5f, 0.5f);
        float box = Mathf.Min(Mathf.Max(d.x, d.y), 0.0f) + Vector2.Max(d, Vector2.zero).magnitude;

        // 2. Protruding Corner Wings (Radius = 1/3)
        float w1 = Vector2.Distance(uv, new Vector2(-0.5f, -0.5f)) - WingRadius;
        float w2 = Vector2.Distance(uv, new Vector2(0.5f, -0.5f)) - WingRadius;
        float w3 = Vector2.Distance(uv, new Vector2(-0.5f, 0.5f)) - WingRadius;
        float w4 = Vector2.Distance(uv, new Vector2(0.5f, 0.5f)) - WingRadius;
        float wings = Mathf.Min(Mathf.Min(w1, w2), Mathf.Min(w3, w4));

        // 3. Union (Min) of Square and Protruding Wings
        return Mathf.Min(box, wings);
    }
}