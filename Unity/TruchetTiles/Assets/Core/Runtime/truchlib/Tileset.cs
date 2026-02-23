// TODO ROADMAP:
// [x] Remove all System.Drawing usage
// [x] Remove Image/Bitmap return types
// [x] Replace with geometry command emission
// [ ] Plug into Unity Texture2D renderer
// [ ] Plug into Mesh renderer
// [ ] Plug into Shader renderer

using System;
using System.Collections.Generic;

namespace Truchet.Tiles
{
    class Tileset
    {
        private readonly int levels;
        private readonly int baseTileSize;

        public readonly int tileCount;

        // Instead of Image[,]
        // Each tile now holds draw commands
        private readonly List<TileDrawCommand>[,] tileCommands;

        public Tileset(int tileSize, int levels)
        {
            this.baseTileSize = tileSize;
            this.levels = levels;
            tileCount = Enum.GetNames(typeof(TileType)).Length;

            tileCommands = InitializeTileset();
        }

        public IReadOnlyList<TileDrawCommand> GetTile(int level, int index)
        {
            return tileCommands[level, index];
        }

        private List<TileDrawCommand>[,] InitializeTileset()
        {
            var array = new List<TileDrawCommand>[levels, tileCount];

            int currentTileSize = baseTileSize;
            var tiles = Enum.GetValues(typeof(TileType));

            for (int currentLevel = 0; currentLevel < levels; currentLevel++)
            {
                int i = 0;
                foreach (TileType tile in tiles)
                {
                    array[currentLevel, i++] =
                        GenerateTileCommands(tile, currentTileSize);
                }

                currentTileSize /= 2;
            }

            return array;
        }

        private List<TileDrawCommand> GenerateTileCommands(
            TileType type,
            int tileSize)
        {
            var commands = new List<TileDrawCommand>();

            FillWhiteCube(commands, tileSize);

            switch (type)
            {
                case TileType.Empty:
                    break;

                case TileType.Vertical:
                    FillVerticalBlackQuad(commands, tileSize);
                    break;

                case TileType.Horizontal:
                    FillHorizontalBlackQuad(commands, tileSize);
                    break;

                case TileType.Cross:
                    FillCornerBlackPies(commands, tileSize, new[] { true, true, true, true });
                    FillVerticalBlackQuad(commands, tileSize);
                    break;

                case TileType.ForwardSlash:
                    FillCornerBlackPies(commands, tileSize, new[] { true, false, true, false });
                    break;

                case TileType.BackSlash:
                    FillCornerBlackPies(commands, tileSize, new[] { false, true, false, true });
                    break;

                case TileType.Frown_NW:
                    FillCornerBlackPies(commands, tileSize, new[] { true, false, false, false });
                    break;

                case TileType.Frown_NE:
                    FillCornerBlackPies(commands, tileSize, new[] { false, true, false, false });
                    break;

                case TileType.Frown_SW:
                    FillCornerBlackPies(commands, tileSize, new[] { false, false, true, false });
                    break;

                case TileType.Frown_SE:
                    FillCornerBlackPies(commands, tileSize, new[] { false, false, false, true });
                    break;

                case TileType.T_N:
                    FillCornerBlackPies(commands, tileSize, new[] { true, true, false, false });
                    FillHorizontalBlackQuad(commands, tileSize);
                    break;

                case TileType.T_E:
                    FillCornerBlackPies(commands, tileSize, new[] { false, true, true, false });
                    FillVerticalBlackQuad(commands, tileSize);
                    break;

                case TileType.T_S:
                    FillCornerBlackPies(commands, tileSize, new[] { false, false, true, true });
                    FillHorizontalBlackQuad(commands, tileSize);
                    break;

                case TileType.T_W:
                    FillCornerBlackPies(commands, tileSize, new[] { true, false, false, true });
                    FillVerticalBlackQuad(commands, tileSize);
                    break;

                default:
                    throw new Exception("Invalid TileType");
            }

            FillCornerWhiteCircles(commands, tileSize);
            FillMiddleBlackCircles(commands, tileSize);

            return commands;
        }

        // =========================
        // GEOMETRY COMMAND HELPERS
        // =========================

        private static void FillWhiteCube(List<TileDrawCommand> cmds, int tileSize)
        {
            cmds.Add(new TileDrawCommand
            {
                Type = DrawCommandType.Rectangle,
                X = tileSize / 2f,
                Y = tileSize / 2f,
                Width = tileSize,
                Height = tileSize,
                IsPrimary = true
            });
        }

        private static void FillVerticalBlackQuad(List<TileDrawCommand> cmds, int tileSize)
        {
            int a = tileSize / 2;
            int b = tileSize / 3;

            cmds.Add(new TileDrawCommand
            {
                Type = DrawCommandType.Rectangle,
                X = a + b,
                Y = a,
                Width = b,
                Height = tileSize,
                IsPrimary = false
            });
        }

        private static void FillHorizontalBlackQuad(List<TileDrawCommand> cmds, int tileSize)
        {
            int a = tileSize / 2;
            int b = tileSize / 3;

            cmds.Add(new TileDrawCommand
            {
                Type = DrawCommandType.Rectangle,
                X = a,
                Y = a + b,
                Width = tileSize,
                Height = b,
                IsPrimary = false
            });
        }

        private static void FillCornerBlackPies(
            List<TileDrawCommand> cmds,
            int tileSize,
            bool[] corners)
        {
            int corner = -tileSize / 6;
            int diameter = (tileSize / 3) * 4;

            void Add(int x, int y, float start)
            {
                cmds.Add(new TileDrawCommand
                {
                    Type = DrawCommandType.Pie,
                    X = x,
                    Y = y,
                    Width = diameter,
                    Height = diameter,
                    StartAngle = start,
                    SweepAngle = 90,
                    IsPrimary = false
                });
            }

            if (corners[0]) Add(corner, corner, 0);
            if (corners[1]) Add(corner + tileSize, corner, 90);
            if (corners[2]) Add(corner + tileSize, corner + tileSize, 180);
            if (corners[3]) Add(corner, corner + tileSize, 270);
        }

        private static void FillCornerWhiteCircles(List<TileDrawCommand> cmds, int tileSize)
        {
            int corner = tileSize / 6;
            int diameter = (tileSize / 3) * 2;

            void Add(int x, int y)
            {
                cmds.Add(new TileDrawCommand
                {
                    Type = DrawCommandType.Ellipse,
                    X = x,
                    Y = y,
                    Width = diameter,
                    Height = diameter,
                    IsPrimary = true
                });
            }

            Add(corner, corner);
            Add(corner, corner + tileSize);
            Add(corner + tileSize, corner);
            Add(corner + tileSize, corner + tileSize);
        }

        private static void FillMiddleBlackCircles(List<TileDrawCommand> cmds, int tileSize)
        {
            int diameter = (tileSize / 3);
            int a = (tileSize / 2) + (tileSize / 3);
            int b = (tileSize / 3);
            int c = (tileSize * 4) / 3;

            void Add(int x, int y)
            {
                cmds.Add(new TileDrawCommand
                {
                    Type = DrawCommandType.Ellipse,
                    X = x,
                    Y = y,
                    Width = diameter,
                    Height = diameter,
                    IsPrimary = false
                });
            }

            Add(a, b);
            Add(a, c);
            Add(b, a);
            Add(c, a);
        }
    }
}