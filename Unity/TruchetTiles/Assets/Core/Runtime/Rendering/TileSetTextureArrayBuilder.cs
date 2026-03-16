using Truchet;
using UnityEngine;

/// <summary>
/// TEMPORARY GPU RESOURCE BUILDER
/// --------------------------------
/// 
/// Purpose:
/// --------
/// Converts a TileSet (collection of Tile ScriptableObjects with Texture2D)
/// into a Texture2DArray for GPU sampling in instanced rendering.
/// 
/// Why this exists:
/// ----------------
/// - Our current rendering backend is GPU instanced.
/// - Each tile instance carries a motifIndex.
/// - The shader needs a single GPU resource containing all tile textures.
/// - Texture2DArray allows sampling by slice index (motifIndex).
/// 
/// This is NOT the final SDF system.
/// It is only a fast bridge to:
/// 
///     Layout → Instances → TextureArray → Shader sampling
/// 
/// so we can verify the GPU pipeline end-to-end.
///
/// Future Direction:
/// -----------------
/// - Replace PNG textures with SDF textures.
/// - Potentially generate Texture2DArray at import time.
/// - Possibly support multiple TileSets.
/// - Possibly support TextureArray caching.
/// - Eventually abstract into a proper GPU resource layer.
///
/// This class exists purely to validate rendering correctness.
/// It may be deleted or refactored once SDF pipeline is finalized.
/// </summary>
public static class TileSetTextureArrayBuilder
{
    public static Texture2DArray Build(TileSet tileSet)
    {
        if (tileSet == null || tileSet.tiles.Length == 0)
            return null;

        // Canonical size (choose first tile size)
        int width = tileSet.tiles[0].texture.width;
        int height = tileSet.tiles[0].texture.height;

        int count = tileSet.tiles.Length;

        // Force RGBA32 uncompressed
        Texture2DArray array = new Texture2DArray(
            width,
            height,
            count,
            TextureFormat.RGBA32,
            false);

        for (int i = 0; i < count; i++)
        {
            Texture2D source = tileSet.tiles[i].texture;

            if (source.width != width || source.height != height)
            {
                Debug.LogError(
                    $"Tile resolution mismatch in TileSet: " +
                    $"{source.width}x{source.height} vs expected {width}x{height}");

                continue;
            }

            // Force CPU read (this decompresses if needed)
            Color[] pixels = source.GetPixels();

            array.SetPixels(pixels, i);
        }

        array.Apply();
        return array;
    }
}