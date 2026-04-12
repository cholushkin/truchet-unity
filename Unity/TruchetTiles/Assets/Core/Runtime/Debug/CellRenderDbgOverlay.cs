using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Truchet
{
    // TODO ROADMAP:
    // [x] Separate debug overlay renderer
    // [x] Pixel-perfect cell drawing
    // [x] Editor auto-refresh (OnValidate)
    // [x] Runtime redraw support
    // [x] Click → cell mapping
    // [x] Multi-target output (Mesh, Sprite, UI)
    // [ ] Hover highlight
    // [ ] Cell info UI
    // [ ] Quadtree debug (non-uniform)

    [ExecuteAlways]
    public class CellRenderDbgOverlay : MonoBehaviour
    {
        [Header("Targets")]
        public Renderer TargetRenderer;
        public SpriteRenderer TargetSpriteRenderer;
        public Image TargetImage;

        [Header("Overlay")]
        public int Resolution = 512;
        public bool DrawCells = true;

        [Tooltip("Colors per level (includes alpha)")]
        public Color[] LevelColors = new Color[]
        {
            new Color(0,1,0,0.8f),
            new Color(1,0,0,0.8f),
            new Color(0,0,1,0.8f)
        };

        private Texture2D _texture;
        private Sprite _sprite;

        private List<TileInstance> _instances;

        // --------------------------------------------------
        // PUBLIC API
        // --------------------------------------------------

        public void SetData(List<TileInstance> instances)
        {
            _instances = instances;
            Redraw();
        }

        public void Redraw()
        {
            if (_instances == null)
                return;

            EnsureTexture();
            Clear();

            if (DrawCells)
            {
                DrawCellsInternal();
            }

            _texture.Apply();

            ApplyToTargets();
        }

        // --------------------------------------------------
        // TARGET APPLICATION
        // --------------------------------------------------

        private void ApplyToTargets()
        {
            if (_texture == null)
                return;

            // Mesh Renderer
            if (TargetRenderer != null)
            {
                TargetRenderer.sharedMaterial.mainTexture = _texture;
            }

            // Sprite Renderer
            if (TargetSpriteRenderer != null)
            {
                EnsureSprite();
                TargetSpriteRenderer.sprite = _sprite;
            }

            // UI Image
            if (TargetImage != null)
            {
                EnsureSprite();
                TargetImage.sprite = _sprite;
            }
        }

        private void EnsureSprite()
        {
            if (_sprite != null && _sprite.texture == _texture)
                return;

            _sprite = Sprite.Create(
                _texture,
                new Rect(0, 0, _texture.width, _texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        // --------------------------------------------------
        // DRAWING
        // --------------------------------------------------

        private void DrawCellsInternal()
        {
            int res = _texture.width;

            foreach (var inst in _instances)
            {
                Vector2 minF = (inst.Position - Vector2.one * inst.Size * 0.5f) * res;
                Vector2 maxF = (inst.Position + Vector2.one * inst.Size * 0.5f) * res;

                int minX = Mathf.FloorToInt(minF.x);
                int minY = Mathf.FloorToInt(minF.y);

                int maxX = Mathf.FloorToInt(maxF.x) - 1;
                int maxY = Mathf.FloorToInt(maxF.y) - 1;

                Color32 color = GetLevelColor(inst.Level);

                DrawRect(minX, minY, maxX, maxY, color);
            }
        }

        private void DrawRect(int minX, int minY, int maxX, int maxY, Color32 color)
        {
            int res = _texture.width;

            minX = Mathf.Clamp(minX, 0, res - 1);
            maxX = Mathf.Clamp(maxX, 0, res - 1);
            minY = Mathf.Clamp(minY, 0, res - 1);
            maxY = Mathf.Clamp(maxY, 0, res - 1);

            if (maxX < minX || maxY < minY)
                return;

            for (int x = minX; x <= maxX; x++)
            {
                SetPixel(x, minY, color);
                SetPixel(x, maxY, color);
            }

            for (int y = minY; y <= maxY; y++)
            {
                SetPixel(minX, y, color);
                SetPixel(maxX, y, color);
            }
        }

        private void SetPixel(int x, int y, Color32 c)
        {
            _texture.SetPixel(x, y, c);
        }

        private void Clear()
        {
            Color clear = new Color(0, 0, 0, 0);
            var pixels = _texture.GetPixels32();

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = clear;

            _texture.SetPixels32(pixels);
        }

        private Color32 GetLevelColor(int level)
        {
            if (LevelColors == null || LevelColors.Length == 0)
                return new Color32(0, 255, 0, 255);

            Color c = LevelColors[level % LevelColors.Length];

            return new Color32(
                (byte)(c.r * 255),
                (byte)(c.g * 255),
                (byte)(c.b * 255),
                (byte)(c.a * 255)
            );
        }

        private void EnsureTexture()
        {
            if (_texture == null || _texture.width != Resolution)
            {
                _texture = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, false);
                _texture.filterMode = FilterMode.Point;
            }
        }

        // --------------------------------------------------
        // EDITOR AUTO UPDATE
        // --------------------------------------------------

        private void OnValidate()
        {
            Redraw();
        }
    }
}