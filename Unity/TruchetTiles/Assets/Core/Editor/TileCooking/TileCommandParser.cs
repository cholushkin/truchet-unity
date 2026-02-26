// TODO ROADMAP:
// [x] Parse text script into render instructions
// [x] Support RCT, ELP, PIE, BZR
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
                string opcode = tokens[0].ToUpperInvariant();

                try
                {
                    switch (opcode)
                    {
                        case "RCT":
                            instructions.Add(ParseRectangle(tokens));
                            break;

                        case "ELP":
                            instructions.Add(ParseEllipse(tokens));
                            break;

                        case "PIE":
                            instructions.Add(ParsePie(tokens));
                            break;

                        case "BZR":
                            instructions.Add(ParseBezier(tokens));
                            break;

                        default:
                            Debug.LogWarning($"Unknown command '{opcode}' at line {i + 1}");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed parsing line {i + 1}: {line}\n{e.Message}");
                }
            }

            return instructions;
        }

        private static RectangleInstruction ParseRectangle(string[] t)
        {
            return new RectangleInstruction
            {
                Center = new Vector2(ParseFloat(t[1]), ParseFloat(t[2])),
                Size = new Vector2(ParseFloat(t[3]), ParseFloat(t[4]))
            };
        }

        private static EllipseInstruction ParseEllipse(string[] t)
        {
            return new EllipseInstruction
            {
                Center = new Vector2(ParseFloat(t[1]), ParseFloat(t[2])),
                Size = new Vector2(ParseFloat(t[3]), ParseFloat(t[4]))
            };
        }

        private static PieInstruction ParsePie(string[] t)
        {
            return new PieInstruction
            {
                Center = new Vector2(ParseFloat(t[1]), ParseFloat(t[2])),
                Size = new Vector2(ParseFloat(t[3]), ParseFloat(t[4])),
                StartAngle = ParseFloat(t[5]),
                SweepAngle = ParseFloat(t[6])
            };
        }

        private static BezierInstruction ParseBezier(string[] t)
        {
            return new BezierInstruction
            {
                P0 = new Vector2(ParseFloat(t[1]), ParseFloat(t[2])),
                P1 = new Vector2(ParseFloat(t[3]), ParseFloat(t[4])),
                P2 = new Vector2(ParseFloat(t[5]), ParseFloat(t[6])),
                P3 = new Vector2(ParseFloat(t[7]), ParseFloat(t[8])),
                Thickness = ParseFloat(t[9])
            };
        }

        private static float ParseFloat(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}