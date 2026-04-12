using UnityEngine;

namespace Truchet
{
    public class TileInteractionController : MonoBehaviour
    {
        public enum InteractionMode
        {
            Random,
            Split,
            Merge
        }

        [Header("Input")]
        [SerializeField] private Camera _camera;

        [Header("Target")]
        [SerializeField] private Collider _targetCollider;

        private InteractionMode _mode = InteractionMode.Random;

        private void Update()
        {
            HandleModeSwitch();
            HandleClick();
        }

        // --------------------------------------------------
        // Mode Switching
        // --------------------------------------------------

        private void HandleModeSwitch()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                _mode = InteractionMode.Merge;
                Debug.Log("[Interaction] Mode = MERGE");
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                _mode = InteractionMode.Split;
                Debug.Log("[Interaction] Mode = SPLIT");
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                _mode = InteractionMode.Random;
                Debug.Log("[Interaction] Mode = RANDOM");
            }
        }

        // --------------------------------------------------
        // Click Handling
        // --------------------------------------------------

        private void HandleClick()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            if (_camera == null || _targetCollider == null)
                return;

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != _targetCollider)
                    return;

                Vector2 uv = hit.textureCoord;

                Debug.Log($"[Interaction] Click UV = {uv} | Mode = {_mode}");
            }
        }
    }
}