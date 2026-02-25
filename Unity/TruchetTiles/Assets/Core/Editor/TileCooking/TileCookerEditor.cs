// TODO ROADMAP:
// [x] Add cook button
// [x] Remove folder popup
// [ ] Add preview panel
// [ ] Add batch cook window
// [ ] Add atlas cook option

using UnityEditor;
using UnityEngine;

namespace Truchet.TileCooking
{
    [CustomEditor(typeof(TileCookDefinition))]
    public class TileCookerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var def = (TileCookDefinition)target;

            GUILayout.Space(10);

            if (GUILayout.Button("Cook Tile"))
            {
                TileTextureCooker.Cook(def);
            }
        }
    }
}