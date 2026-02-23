// TODO ROADMAP:
// [x] Switch to ScriptableObject settings
// [ ] Add runtime regeneration button
// [ ] Add play-mode rebuild toggle

using NaughtyAttributes;
using UnityEngine;
using Truchet.Tiles;

public class TileTreeDebugVisualizer : MonoBehaviour
{
    public GenerationSettingsScriptableObject settingsAsset;

    public float baseTileSize = 1f;

    private TileNode[,] grid;

    private void OnValidate()
    {
        if (settingsAsset != null)
            Generate();
    }

    private void Generate()
    {
        var runtime = settingsAsset.ToRuntime();
    
        ITileSelectionStrategy strategy =
            runtime.UsePerlin
                ? new PerlinTileSelectionStrategy(
                    runtime.Seed,
                    (float)runtime.Frequency,
                    (float)runtime.Amplitude,
                    runtime.Octaves)
                : new RandomTileSelectionStrategy(runtime.Seed);

        var builder = new TileTreeBuilder(runtime, strategy);
        grid = builder.Build();
    }
    
    private void OnDrawGizmos()
    {
        if (grid == null)
            Generate();

        if (grid == null)
            return;

        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                DrawNode(grid[y, x], x * baseTileSize, y * baseTileSize, baseTileSize);
            }
        }
    }

    private void DrawNode(TileNode node, float worldX, float worldY, float size)
    {
        if (node.IsLeaf)
        {
            var leaf = (LeafTileNode)node;

            Gizmos.color = GetColorFromTileType(leaf.TileType);

            Vector3 center = new Vector3(
                worldX + size * 0.5f,
                0,
                worldY + size * 0.5f);

            Gizmos.DrawCube(center, new Vector3(size, 0.01f, size));
            return;
        }

        var container = (ContainerTileNode)node;

        float half = size * 0.5f;

        // NW
        DrawNode(container.Children[0], worldX, worldY + half, half);

        // NE
        DrawNode(container.Children[1], worldX + half, worldY + half, half);

        // SE
        DrawNode(container.Children[2], worldX + half, worldY, half);

        // SW
        DrawNode(container.Children[3], worldX, worldY, half);
    }

    private Color GetColorFromTileType(TileType type)
    {
        int id = type.GetTileId();

        // Simple deterministic color mapping
        float hue = (id * 0.1f) % 1f;
        return Color.HSVToRGB(hue, 0.6f, 0.9f);
    }

    [Button]
    void DbgPrintTree()
    {
        if (grid == null)
        {
            Debug.LogWarning("Grid not generated.");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder(4096);

        sb.AppendLine("==== TILE TREE DUMP ====");

        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                sb.AppendLine($"Root [{x},{y}]");
                PrintNode(grid[y, x], sb, 1);
            }
        }

        Debug.Log(sb.ToString());
    }

    private void PrintNode(TileNode node, System.Text.StringBuilder sb, int indent)
    {
        string pad = new string(' ', indent * 2);

        if (node.IsLeaf)
        {
            var leaf = (LeafTileNode)node;

            sb.AppendLine(
                $"{pad}- Leaf | Level: {leaf.Level} | Pos: ({leaf.X},{leaf.Y}) | Type: {leaf.TileType}");
            return;
        }

        var container = (ContainerTileNode)node;

        sb.AppendLine(
            $"{pad}- Container | Level: {container.Level} | Pos: ({container.X},{container.Y})");

        for (int i = 0; i < container.Children.Length; i++)
        {
            sb.AppendLine($"{pad}  Child[{i}]");
            PrintNode(container.Children[i], sb, indent + 2);
        }
    }
}