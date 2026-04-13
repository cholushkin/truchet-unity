using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Truchet
{
    public class UIPointerProvider : MonoBehaviour, IPointerProvider
    {
        [SerializeField] private RectTransform _targetRect;
        [SerializeField] private Canvas _canvas;

        public bool TryGetUV(Vector2 screenPos, out Vector2 uv)
        {
            uv = default;

            if (_targetRect == null || _canvas == null)
                return false;

#if UNITY_EDITOR
            // ✅ FORCE SceneView camera in editor
            Camera cam = SceneView.lastActiveSceneView?.camera;
#else
            Camera cam = _canvas.worldCamera;
#endif

            if (!RectTransformUtility.RectangleContainsScreenPoint(
                    _targetRect,
                    screenPos,
                    cam))
                return false;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _targetRect,
                    screenPos,
                    cam,
                    out var localPoint))
                return false;

            Rect rect = _targetRect.rect;

            float u = (localPoint.x - rect.x) / rect.width;
            float v = (localPoint.y - rect.y) / rect.height;

            uv = new Vector2(u, v);

            Debug.Log($"[UIPointer] UV={uv}");

            return true;
        }
        
        public RectTransform GetTargetRect()
        {
            return _targetRect;
        }
    }
}