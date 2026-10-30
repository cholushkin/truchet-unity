using UnityEngine;
using UnityEngine.InputSystem;

namespace Truchet
{
    // public class WorldPointerProvider : MonoBehaviour, IPointerProvider
    // {
    //     [SerializeField] private Camera _camera;
    //     [SerializeField] private Collider _targetCollider;
    //
    //     public bool TryGetUV(out Vector2 uv)
    //     {
    //         uv = default;
    //
    //         if (_camera == null || _targetCollider == null)
    //             return false;
    //
    //         var mouse = Mouse.current;
    //         if (mouse == null)
    //             return false;
    //
    //         Vector2 screenPos = mouse.position.ReadValue();
    //         Ray ray = _camera.ScreenPointToRay(screenPos);
    //
    //         if (!Physics.Raycast(ray, out RaycastHit hit))
    //             return false;
    //
    //         if (hit.collider != _targetCollider)
    //             return false;
    //
    //         uv = hit.textureCoord;
    //         return true;
    //     }
    // }
}