using System.Collections.Generic;
using UnityEngine;

namespace Truchet
{
    /// <summary>
    /// Base class for all rendering backends.
    /// Lives as a MonoBehaviour and owns its config.
    /// </summary>
    public abstract class TruchetRenderBehaviour : MonoBehaviour
    {
        public abstract void Render(
            List<TileInstance> instances,
            TileSet[] tileSets,
            int gridWidth);
    }
}