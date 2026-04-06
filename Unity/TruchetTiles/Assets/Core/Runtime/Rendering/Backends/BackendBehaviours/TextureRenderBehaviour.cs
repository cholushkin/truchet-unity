using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    public class TextureRenderBehaviour : TruchetRenderBehaviour
    {
        [Header("Output")]
        [SerializeField] private int _resolution = 512;

        [SerializeField] private Renderer _previewRenderer;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private TextureRenderBackend _backend = new TextureRenderBackend();

        public override void Render(
            List<TileInstance> instances,
            TileSet[] tileSets,
            int gridWidth)
        {
            Texture2D texture = _backend.Render(
                instances,
                tileSets,
                gridWidth,
                _resolution);

            ApplyTexture(texture);
        }

        private void ApplyTexture(Texture2D texture)
        {
            if (_previewRenderer != null)
                _previewRenderer.material.mainTexture = texture;

            if (_spriteRenderer != null)
            {
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    texture.width);

                _spriteRenderer.sprite = sprite;
            }
        }
    }
}