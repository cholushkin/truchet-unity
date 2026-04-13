using UnityEngine;
using UnityEngine.UI;

namespace Core.Runtime.UI
{
    /// <summary>
    /// Utility for converting screen input into UV coordinates inside a RawImage.
    /// This is the ONLY supported path for UI interaction.
    /// </summary>
    public static class UIHitUtility
    {
        /// <summary>
        /// Try to convert screen position into UV (0–1) inside RawImage.
        /// </summary>
        public static bool TryGetUV(
            RawImage image,
            Vector2 screenPos,
            Camera cam,
            out Vector2 uv)
        {
            uv = default;

            if (image == null)
            {
                Debug.LogError("[UIHitUtility] RawImage is NULL.");
                return false;
            }

            var rect = image.rectTransform;

            // 🚫 Unsupported / warning states
            if (cam == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    "[UIHitUtility] Camera is NULL.\n" +
                    "- Likely Screen Space Overlay OR SceneView.\n" +
                    "- UV mapping may be incorrect."
                );
#else
                Debug.LogWarning(
                    "[UIHitUtility] Camera is NULL. Only Screen Space Camera / World Space is supported."
                );
#endif
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "[UIHitUtility] Called outside Play Mode (SceneView). Not a supported runtime path."
                );
            }
#endif

            // ✅ Convert screen → local
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect,
                    screenPos,
                    cam,
                    out var localPoint))
            {
                return false;
            }

            Rect r = rect.rect;

            if (Mathf.Approximately(r.width, 0) || Mathf.Approximately(r.height, 0))
            {
                Debug.LogError("[UIHitUtility] Rect has zero size.");
                return false;
            }

            // ✅ Local → UV (0–1)
            float u = (localPoint.x - r.x) / r.width;
            float v = (localPoint.y - r.y) / r.height;

            uv = new Vector2(u, v);

            return true;
        }

        /// <summary>
        /// Same as TryGetUV but clamps result into [0,1].
        /// Useful if you want guaranteed valid UV.
        /// </summary>
        public static bool TryGetClampedUV(
            RawImage image,
            Vector2 screenPos,
            Camera cam,
            out Vector2 uv)
        {
            if (!TryGetUV(image, screenPos, cam, out uv))
                return false;

            uv.x = Mathf.Clamp01(uv.x);
            uv.y = Mathf.Clamp01(uv.y);

            return true;
        }
    }
}