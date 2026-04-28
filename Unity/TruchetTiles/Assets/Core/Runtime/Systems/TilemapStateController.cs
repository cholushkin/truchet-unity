using Truchet;
using UnityEngine;

public class TilemapStateController : MonoBehaviour
{
    [SerializeField] private byte[] _snapshot;

    public bool HasState() => _snapshot != null && _snapshot.Length > 0;

    public void Apply(TruchetEngine runtime)
    {
        var snapshot = SnapshotsController.Deserialize(_snapshot);

        var quad = new QuadTree(snapshot.Structure.RootSize);
        quad.ApplyStructureSnapshot(snapshot.Structure);
        quad.ApplyTileSnapshot(snapshot.Tiles);

        runtime.SetHierarchicalLayout(quad);
        runtime.RebuildComposition();
    }
}