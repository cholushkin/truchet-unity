using NaughtyAttributes;
using UnityEngine;

namespace Truchet
{
    public interface IPointerProvider
    {
        bool TryGetUV(Vector2 screenPos, out Vector2 uv);
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
        [SerializeField] private TruchetEngine _engine;
        [SerializeField] private InteractionMode _mode = InteractionMode.Random;

        [SerializeField] private bool _editingEnabled;

        private IPointerProvider _pointerProvider;
        private bool _lastEditingState;

        public TruchetEngine Engine => _engine;
        public IPointerProvider Pointer => _pointerProvider;
        public InteractionMode Mode => _mode;
        public bool EditingEnabled => _editingEnabled;

        private void Awake()
        {
            ResolvePointer();
            _lastEditingState = _editingEnabled;
        }

        private void OnValidate()
        {
            ResolvePointer();

            if (_lastEditingState != _editingEnabled)
            {
                Debug.Log($"[Interaction] Edit Mode = {(_editingEnabled ? "ENABLED" : "DISABLED")}");
                _lastEditingState = _editingEnabled;
            }
        }

        private void ResolvePointer()
        {
            _pointerProvider = _pointerProviderBehaviour as IPointerProvider;

            if (_pointerProviderBehaviour != null && _pointerProvider == null)
            {
                Debug.LogError($"{_pointerProviderBehaviour.name} does not implement IPointerProvider", this);
            }
        }

        public void ApplyAtUV(Vector2 uv)
        {
            _engine?.ModifyAtUV(uv, _mode);
        }

        public void SetMode(InteractionMode mode)
        {
            if (_mode == mode)
                return;

            _mode = mode;
            Debug.Log($"[Interaction] Mode = {mode.ToString().ToUpper()}");
        }
        
        public void SetEditingEnabled(bool enabled)
        {
            _editingEnabled = enabled;
            Debug.Log($"[Interaction] Edit Mode = {(enabled ? "ENABLED" : "DISABLED")}");
        }
        
        public void BakeState()
        {
            _engine?.StateController?.Capture(_engine);
        }

        public void BakeStructure()
        {
            _engine?.StateController?.CaptureStructure(_engine);
        }
    }
}