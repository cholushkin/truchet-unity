using UnityEngine;

namespace Truchet
{
    public class TextureRenderToMesh : TextureRenderBehaviourBase
    {
        public Renderer TargetRenderer;

        protected override void ApplyTexture(Texture2D texture)
        {
            if (TargetRenderer == null || texture == null)
                return;

            TargetRenderer.sharedMaterial.mainTexture = texture;
        }
    }
}