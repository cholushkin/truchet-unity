using UnityEngine;
using UnityEngine.InputSystem;

// TODO: Support rotate current tile

namespace Truchet
{
    public interface IPointerProvider
    {
        bool TryGetUV(out Vector2 uv);
    }

    public class TileInteractionController : MonoBehaviour
    {
        public enum InteractionMode
        {
            Random,
            Turn,
            Split,
            Merge,
            Erase
        }

        [SerializeField] private MonoBehaviour _pointerProviderBehaviour;
        [SerializeField] private TruchetRuntime _runtime;

        [Header("Input Actions")]
        [SerializeField] private InputAction _clickAction;
        [SerializeField] private InputAction _modeRandomAction;
        [SerializeField] private InputAction _modeSplitAction;
        [SerializeField] private InputAction _modeMergeAction;
        [SerializeField] private InputAction _modeEraseAction;
        [SerializeField] private InputAction _modeTurnAction;

        private IPointerProvider _pointerProvider;
        private InteractionMode _mode = InteractionMode.Random;

        private void Awake()
        {
            _pointerProvider = _pointerProviderBehaviour as IPointerProvider;
        }

        private void OnEnable()
        {
            _clickAction.Enable();
            _modeRandomAction.Enable();
            _modeSplitAction.Enable();
            _modeMergeAction.Enable();
            _modeEraseAction.Enable();
            _modeTurnAction.Enable();
            
            _clickAction.performed += OnClick;

            _modeRandomAction.performed += OnRandomMode;
            _modeSplitAction.performed += OnSplitMode;
            _modeMergeAction.performed += OnMergeMode;
            _modeEraseAction.performed += OnEraseMode;
            _modeTurnAction.performed += OnTurnMode;
        }

        private void OnDisable()
        {
            _clickAction.performed -= OnClick;

            _modeRandomAction.performed -= OnRandomMode;
            _modeSplitAction.performed -= OnSplitMode;
            _modeMergeAction.performed -= OnMergeMode;
            _modeEraseAction.performed -= OnEraseMode;
            _modeTurnAction.performed -= OnTurnMode;
            

            _clickAction.Disable();
            _modeRandomAction.Disable();
            _modeSplitAction.Disable();
            _modeMergeAction.Disable();
            _modeEraseAction.Disable();
            _modeTurnAction.Disable();
        }

        private void OnRandomMode(InputAction.CallbackContext ctx)
        {
            _mode = InteractionMode.Random;
            Debug.Log("[Interaction] Mode = RANDOM");
        }

        private void OnSplitMode(InputAction.CallbackContext ctx)
        {
            _mode = InteractionMode.Split;
            Debug.Log("[Interaction] Mode = SPLIT");
        }

        private void OnMergeMode(InputAction.CallbackContext ctx)
        {
            _mode = InteractionMode.Merge;
            Debug.Log("[Interaction] Mode = MERGE");
        }

        private void OnEraseMode(InputAction.CallbackContext ctx)
        {
            _mode = InteractionMode.Erase;
            Debug.Log("[Interaction] Mode = ERASE");
        }
        
        private void OnTurnMode(InputAction.CallbackContext ctx)
        {
            _mode = InteractionMode.Turn;
            Debug.Log("[Interaction] Mode = TURN");
        }

        private void OnClick(InputAction.CallbackContext ctx)
        {
            if (_pointerProvider == null)
                return;

            if (!_pointerProvider.TryGetUV(out var uv))
                return;

            //Debug.Log($"[Interaction] Click UV = {uv} | Mode = {_mode}");

            _runtime?.ModifyAtUV(uv, _mode);
        }
    }
}