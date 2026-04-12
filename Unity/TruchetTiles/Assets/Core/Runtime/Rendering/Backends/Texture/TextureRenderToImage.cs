using UnityEngine;
using UnityEngine.UI;

namespace Truchet
{
    public class TextureRenderToImage : TextureRenderBehaviourBase
    {
        public Image TargetImage;

        private Sprite _sprite;

        protected override void ApplyTexture(Texture2D texture)
        {
            if (TargetImage == null || texture == null)
                return;

            if (_sprite == null || _sprite.texture != texture)
            {
                _sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
            }

            TargetImage.sprite = _sprite;
        }
    }
}