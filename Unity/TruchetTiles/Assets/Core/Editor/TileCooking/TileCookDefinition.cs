using UnityEngine;

namespace Truchet
{
    [CreateAssetMenu(
        fileName = "TileCookDefinition",
        menuName = "Truchet/Tile Cooking/Tile Cook Definition",
        order = 0)]
    public class TileCookDefinition : ScriptableObject
    {
        [Header("Resolution")]
        public int Width = 256;
        public int Height = 256;

        [Header("Winged Mode")]
        public bool IsWinged;

        [Header("Output Folder (relative to Assets/)")]
        public string OutputFolder = "Core/Textures/Tiles";

        [Header("Topology")]
        public TileTopology Topology;

        [Header("Command Script")]
        [TextArea(10, 40)]
        public string CommandScript;
    }
}