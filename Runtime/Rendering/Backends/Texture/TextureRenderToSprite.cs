using UnityEngine;

namespace Truchet
{
    public class TextureRenderToSprite : TextureRenderBehaviourBase
    {
        public SpriteRenderer TargetRenderer;

        private Sprite _sprite;

        protected override void ApplyTexture(Texture2D texture)
        {
            if (TargetRenderer == null || texture == null)
                return;

            if (_sprite == null || _sprite.texture != texture)
            {
                _sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
            }

            TargetRenderer.sprite = _sprite;
        }
    }
}