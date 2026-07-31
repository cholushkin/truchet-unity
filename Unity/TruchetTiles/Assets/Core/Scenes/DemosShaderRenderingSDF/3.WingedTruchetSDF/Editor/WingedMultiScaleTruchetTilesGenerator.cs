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

    [Button("Generate Pure Skeleton Array")]
    public void GenerateTextures()
    {
        if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

        // Bake pure skeletons into RGFloat textures
        Texture2D texSlash = BakeTexture(CalculateSlashSkeleton, CalculateCornerWingsSkeleton);
        Texture2D texPlus = BakeTexture(CalculatePlusSkeleton, CalculateCornerWingsSkeleton);

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
        Debug.Log($"<color=green><b>Pure Skeleton Array generated at {arrayPath}</b></color>");
    }

    private Texture2D BakeTexture(System.Func<Vector2, float> sdfMathMain, System.Func<Vector2, float> sdfMathCorners)
    {
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGFloat, false);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = (x / (float)(resolution - 1)) * 2.0f - 1.0f;
                float v = (y / (float)(resolution - 1)) * 2.0f - 1.0f;
                Vector2 uv = new Vector2(u, v);
                
                // Pack Main Skeleton (R) and Wing Skeleton (G)
                tex.SetPixel(x, y, new Color(sdfMathMain(uv), sdfMathCorners(uv), 0, 1.0f));
            }
        }
        tex.Apply();
        return tex;
    }

    // --- PURE SKELETON MATH (No thickness subtracted here!) ---

    private float CalculateSlashSkeleton(Vector2 uv)
    {
        float arc1 = SdQuarterArc(uv, new Vector2(-0.5f, 0.5f), 0.5f, 1f, -1f);
        float arc2 = SdQuarterArc(uv, new Vector2(0.5f, -0.5f), 0.5f, -1f, 1f);
        return Mathf.Min(arc1, arc2);
    }

    private float CalculatePlusSkeleton(Vector2 uv)
    {
        float hLine = SdSegment(uv, new Vector2(-0.5f, 0), new Vector2(0.5f, 0));
        float vLine = SdSegment(uv, new Vector2(0, -0.5f), new Vector2(0, 0.5f));
        return Mathf.Min(hLine, vLine);
    }

    private float CalculateCornerWingsSkeleton(Vector2 uv)
    {
        // Pure distance to the corners. 
        float d1 = Vector2.Distance(uv, new Vector2(-0.5f, -0.5f));
        float d2 = Vector2.Distance(uv, new Vector2(0.5f, -0.5f));
        float d3 = Vector2.Distance(uv, new Vector2(-0.5f, 0.5f));
        float d4 = Vector2.Distance(uv, new Vector2(0.5f, 0.5f));
        return Mathf.Min(Mathf.Min(d1, d2), Mathf.Min(d3, d4));
    }

    // --- UTILITY SDF MATH ---

    private float SdQuarterArc(Vector2 p, Vector2 center, float radius, float signX, float signY)
    {
        Vector2 rel = p - center;
        rel.x *= signX; 
        rel.y *= signY;
        if (rel.x >= 0 && rel.y >= 0) return Mathf.Abs(rel.magnitude - radius);
        float d1 = Vector2.Distance(rel, new Vector2(radius, 0));
        float d2 = Vector2.Distance(rel, new Vector2(0, radius));
        return Mathf.Min(d1, d2);
    }

    private float SdSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 pa = p - a;
        Vector2 ba = b - a;
        float h = Mathf.Clamp(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba), 0.0f, 1.0f);
        return Vector2.Distance(pa, ba * h);
    }
}