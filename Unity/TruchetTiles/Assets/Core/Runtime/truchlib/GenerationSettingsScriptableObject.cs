// TODO ROADMAP:
// [x] Introduce ScriptableObject settings
// [x] Runtime conversion method
// [ ] Add validation warnings
// [ ] Add custom editor
// [ ] Add preset variants (Random / Perlin)

using UnityEngine;
using Truchet.Tiles;

[CreateAssetMenu(
    fileName = "GenerationSettings",
    menuName = "Truchet/Generation Settings",
    order = 0)]
public class GenerationSettingsScriptableObject : ScriptableObject
{
    [Header("Grid")]
    public int seed = 1234;
    public int rows = 4;
    public int columns = 4;
    public int levels = 3;

    [Header("Noise")]
    public bool usePerlin = true;
    public float frequency = 0.1f;
    public float amplitude = 1f;
    public int octaves = 3;

    public GenerationSettings ToRuntime()
    {
        return new GenerationSettings(
            seed,
            rows,
            columns,
            levels,
            usePerlin,
            frequency,
            amplitude,
            octaves);
    }
}