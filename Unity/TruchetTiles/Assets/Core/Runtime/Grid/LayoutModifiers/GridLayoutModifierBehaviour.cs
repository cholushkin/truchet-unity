// TODO ROADMAP:
// [x] Base class for stackable layout modifiers
// [ ] Add execution order override support
// [ ] Add enable/disable runtime toggle
// [ ] Add priority system
// [ ] Add multiscale compatibility

using UnityEngine;

namespace Truchet
{
    /// <summary>
    /// Base class for all layout modifiers.
    /// Multiple modifiers can be stacked on one GameObject
    /// and applied sequentially.
    /// </summary>
    public abstract class GridLayoutModifierBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Applies modification to the provided layout.
        /// Must NOT recreate layout.
        /// Must only mutate required cells.
        /// </summary>
        public abstract void Apply(RegularGridLayout layout);
    }
}