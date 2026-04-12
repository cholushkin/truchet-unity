// TODO ROADMAP:
// [x] Parse text script into render instructions
// [x] Support RCT, ELP, PIE, BZR
// [x] Color prefix support (B/W)
// [x] Safe token length validation
// [ ] Add syntax validation UI feedback
// [ ] Add comment support with #
// [ ] Add error highlighting in inspector

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Truchet
{
    internal static class TileCommandParser
    {
        public static List<TileRenderInstruction> Parse(string script)
        {
            var instructions = new List<TileRenderInstruction>();

            if (string.IsNullOrWhiteSpace(script))
                return instructions;

            string[] lines = script.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string rawLine = lines[i];
                string line = rawLine.Trim();

                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("//"))
                    continue;

                string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0)
                    continue;

                int offset = 0;
                DrawColor color = DrawColor.Black;

                // Optional color prefix
                if (IsColorToken(tokens[0], out DrawColor parsedColor))
                {
                    color = parsedColor;
                    offset = 1;
                }

                if (tokens.Length <= offset)
                {
                    Debug.LogWarning($"Invalid command at line {i + 1}: {line}");
                    continue;
                }

                string opcode = tokens[offset].ToUpperInvariant();

                try
                {
                    TileRenderInstruction instruction = opcode switch
                    {
                        "RCT" => ParseRectangle(tokens, offset),
                        "ELP" => ParseEllipse(tokens, offset),
                        "PIE" => ParsePie(tokens, offset),
                        "BZR" => ParseBezier(tokens, offset),
                        _ => null
                    };

                    if (instruction != null)
                    {
                        instruction.Color = color;
                        instructions.Add(instruction);
                    }
                    else
                    {
                        Debug.LogWarning($"Unknown opcode '{opcode}' at line {i + 1}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed parsing line {i + 1}: {line}\n{e.Message}");
                }
            }

            return instructions;
        }

        // --------------------------------------------------
        // Color Handling
        // --------------------------------------------------

        private static bool IsColorToken(string token, out DrawColor color)
        {
            if (token.Equals("W", StringComparison.OrdinalIgnoreCase))
            {
                color = DrawColor.White;
                return true;
            }

            if (token.Equals("B", StringComparison.OrdinalIgnoreCase))
            {
                color = DrawColor.Black;
                return true;
            }

            color = DrawColor.Black;
            return false;
        }

        // --------------------------------------------------
        // Parse Helpers (Offset Aware)
        // --------------------------------------------------

        private static RectangleInstruction ParseRectangle(string[] t, int o)
        {
            RequireLength(t, o, 5);

            return new RectangleInstruction
            {
                Center = new Vector2(ParseFloat(t[o + 1]), ParseFloat(t[o + 2])),
                Size   = new Vector2(ParseFloat(t[o + 3]), ParseFloat(t[o + 4]))
            };
        }

        private static EllipseInstruction ParseEllipse(string[] t, int o)
        {
            RequireLength(t, o, 5);

            return new EllipseInstruction
            {
                Center = new Vector2(ParseFloat(t[o + 1]), ParseFloat(t[o + 2])),
                Size   = new Vector2(ParseFloat(t[o + 3]), ParseFloat(t[o + 4]))
            };
        }

        private static PieInstruction ParsePie(string[] t, int o)
        {
            RequireLength(t, o, 7);

            return new PieInstruction
            {
                Center     = new Vector2(ParseFloat(t[o + 1]), ParseFloat(t[o + 2])),
                Size       = new Vector2(ParseFloat(t[o + 3]), ParseFloat(t[o + 4])),
                StartAngle = ParseFloat(t[o + 5]),
                SweepAngle = ParseFloat(t[o + 6])
            };
        }

        private static BezierInstruction ParseBezier(string[] t, int o)
        {
            RequireLength(t, o, 10);

            return new BezierInstruction
            {
                P0        = new Vector2(ParseFloat(t[o + 1]), ParseFloat(t[o + 2])),
                P1        = new Vector2(ParseFloat(t[o + 3]), ParseFloat(t[o + 4])),
                P2        = new Vector2(ParseFloat(t[o + 5]), ParseFloat(t[o + 6])),
                P3        = new Vector2(ParseFloat(t[o + 7]), ParseFloat(t[o + 8])),
                Thickness = ParseFloat(t[o + 9])
            };
        }

        // --------------------------------------------------
        // Utilities
        // --------------------------------------------------

        private static float ParseFloat(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        private static void RequireLength(string[] tokens, int offset, int requiredCount)
        {
            if (tokens.Length < offset + requiredCount)
                throw new Exception("Not enough parameters for command.");
        }
    }
}