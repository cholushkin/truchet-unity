// TODO ROADMAP:
// [x] Implement full snapshot restore
// [x] Implement structure-only restore
// [x] Implement capture (full + structure)
// [ ] Add snapshot versioning
// [ ] Add validation / compatibility checks

using UnityEngine;

namespace Truchet
{
    public class TilemapStateController : MonoBehaviour
    {
        [SerializeField] private byte[] _fullSnapshot;
        [SerializeField] private byte[] _structureSnapshot;

        // --------------------------------------------------
        // STATE CHECKS
        // --------------------------------------------------

        public bool HasState()
        {
            return _fullSnapshot != null && _fullSnapshot.Length > 0;
        }

        public bool HasStructure()
        {
            return _structureSnapshot != null && _structureSnapshot.Length > 0;
        }

        // --------------------------------------------------
        // CAPTURE
        // --------------------------------------------------

        public void Capture(TruchetEngine engine)
        {
            var quad = engine.GetQuadTree();
            if (quad == null)
                return;

            var structure = quad.CreateStructureSnapshot();
            var tiles = quad.CreateTileSnapshot();

            var snapshot = new QuadTreeSnapshot
            {
                Structure = structure,
                Tiles = tiles
            };

            _fullSnapshot = SnapshotsController.Serialize(snapshot);
        }

        public void CaptureStructure(TruchetEngine engine)
        {
            var quad = engine.GetQuadTree();
            if (quad == null)
                return;

            var structure = quad.CreateStructureSnapshot();

            _structureSnapshot = SnapshotsController.SerializeStructure(structure);
        }

        // --------------------------------------------------
        // APPLY FULL SNAPSHOT
        // --------------------------------------------------

        public void Apply(TruchetEngine engine)
        {
            if (_fullSnapshot == null || _fullSnapshot.Length == 0)
            {
                Debug.LogWarning("No snapshot to apply.");
                return;
            }

            var snapshot = SnapshotsController.Deserialize(_fullSnapshot);

            var quad = new QuadTree(snapshot.Structure.RootSize);

            quad.ApplyStructureSnapshot(snapshot.Structure);
            quad.ApplyTileSnapshot(snapshot.Tiles);

            engine.SetQuadTree(quad);
            engine.RebuildComposition();
        }

        // --------------------------------------------------
        // APPLY STRUCTURE ONLY
        // --------------------------------------------------

        public void ApplyStructure(TruchetEngine engine)
        {
            var structure = SnapshotsController.DeserializeStructure(_structureSnapshot);

            var quad = new QuadTree(structure.RootSize);

            quad.ApplyStructureSnapshot(structure);

            engine.SetQuadTree(quad);

            engine.ReinitRng();
            engine.FillTiles();

            engine.RebuildComposition();
        }
    }
}