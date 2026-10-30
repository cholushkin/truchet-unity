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

    private const float LineHalfThickness = 1.0f / 6.0f;
    private const float WingRadius = 1.0f / 3.0f;

    [Button("Generate Pure Skeleton Array")]
    public void GenerateTextures()
    {
        if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

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

    private float CalculateBox(Vector2 uv)
    {
        Vector2 d = new Vector2(Mathf.Abs(uv.x), Mathf.Abs(uv.y)) - new Vector2(0.5f, 0.5f);
        return Mathf.Min(Mathf.Max(d.x, d.y), 0.0f) + Vector2.Max(d, Vector2.zero).magnitude;
    }

    private float CalculateCaps(Vector2 uv)
    {
        float capN = Vector2.Distance(uv, new Vector2(0.0f, 0.5f)) - LineHalfThickness;
        float capS = Vector2.Distance(uv, new Vector2(0.0f, -0.5f)) - LineHalfThickness;
        float capE = Vector2.Distance(uv, new Vector2(0.5f, 0.0f)) - LineHalfThickness;
        float capW = Vector2.Distance(uv, new Vector2(-0.5f, 0.0f)) - LineHalfThickness;
        
        return Mathf.Min(Mathf.Min(capN, capS), Mathf.Min(capE, capW));
    }

    private float CalculateOwnership(Vector2 uv)
    {
        float box = CalculateBox(uv);

        float w1 = Vector2.Distance(uv, new Vector2(-0.5f, -0.5f)) - WingRadius;
        float w2 = Vector2.Distance(uv, new Vector2(0.5f, -0.5f)) - WingRadius;
        float w3 = Vector2.Distance(uv, new Vector2(-0.5f, 0.5f)) - WingRadius;
        float w4 = Vector2.Distance(uv, new Vector2(0.5f, 0.5f)) - WingRadius;
        float wings = Mathf.Min(Mathf.Min(w1, w2), Mathf.Min(w3, w4));

        // Caps are strictly foreground geometry and do not belong in the ownership domain.
        return Mathf.Min(box, wings);
    }

    private float CalculateForegroundSlash(Vector2 uv)
    {
        float arc1 = Mathf.Abs(Vector2.Distance(uv, new Vector2(-0.5f, 0.5f)) - 0.5f) - LineHalfThickness;
        float arc2 = Mathf.Abs(Vector2.Distance(uv, new Vector2(0.5f, -0.5f)) - 0.5f) - LineHalfThickness;
        float unbounded = Mathf.Min(arc1, arc2);
        
        float boundedLine = Mathf.Max(unbounded, CalculateBox(uv));
        return Mathf.Min(boundedLine, CalculateCaps(uv));
    }

    private float CalculateForegroundPlus(Vector2 uv)
    {
        float dX = Mathf.Abs(uv.y) - LineHalfThickness;
        float dY = Mathf.Abs(uv.x) - LineHalfThickness;
        float unbounded = Mathf.Min(dX, dY);
        
        float boundedLine = Mathf.Max(unbounded, CalculateBox(uv));
        return Mathf.Min(boundedLine, CalculateCaps(uv));
    }
}