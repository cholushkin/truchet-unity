using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Truchet
{
    public class UIPointerProvider : MonoBehaviour, IPointerProvider
    {
        [SerializeField] private RectTransform _targetRect;
        [SerializeField] private Camera _uiCamera;

        public bool TryGetUV(out Vector2 uv)
        {
            uv = default;

            if (_targetRect == null)
                return false;

            var mouse = Mouse.current;
            if (mouse == null)
                return false;

            Vector2 screenPos = mouse.position.ReadValue();

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _targetRect,
                    screenPos,
                    _uiCamera,
                    out Vector2 localPoint))
                return false;

            Rect rect = _targetRect.rect;

            float u = (localPoint.x - rect.x) / rect.width;
            float v = (localPoint.y - rect.y) / rect.height;

            if (u < 0f || u > 1f || v < 0f || v > 1f)
                return false;

            uv = new Vector2(u, v);
            return true;
        }
    }
}