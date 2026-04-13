using GameLib.Alg;
using UnityEditor;
using UnityEngine;

namespace Truchet
{
    [InitializeOnLoad]
    public static class TileInteractionSceneTool
    {
        static TileInteractionSceneTool()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            var controller = Object.FindObjectOfType<TileInteractionController>();
            if (controller == null || controller.Pointer == null)
                return;

            if (!controller.EditingEnabled)
                return;

            Event e = Event.current;

            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    case KeyCode.R:
                        controller.SetMode(TileInteractionController.InteractionMode.Random);
                        e.Use();
                        break;

                    case KeyCode.T:
                        controller.SetMode(TileInteractionController.InteractionMode.Turn);
                        e.Use();
                        break;

                    case KeyCode.S:
                        controller.SetMode(TileInteractionController.InteractionMode.Split);
                        e.Use();
                        break;

                    case KeyCode.M:
                        controller.SetMode(TileInteractionController.InteractionMode.Merge);
                        e.Use();
                        break;

                    case KeyCode.E:
                        controller.SetMode(TileInteractionController.InteractionMode.Erase);
                        e.Use();
                        break;
                }
            }

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Vector2 screenPos = HandleUtility.GUIPointToScreenPixelCoordinate(e.mousePosition);

                if (controller.Pointer.TryGetUV(screenPos, out var uv))
                {
                    controller.ApplyAtUV(uv);
                    e.Use();
                }
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            DrawOverlay(sceneView, controller);
        }

        private static void DrawOverlay(SceneView sceneView, TileInteractionController controller)
        {
            Handles.BeginGUI();

            float width = 440f;
            float height = 150f;

            Rect rect = new Rect(
                (sceneView.position.width - width) * 0.5f,
                10f,
                width,
                height
            );

            GUILayout.BeginArea(rect, GUI.skin.box);

            GUILayout.Label($"Mode: {controller.Mode}", EditorStyles.boldLabel);

            var provider = controller.Pointer as UIPointerProvider;
            if (provider != null)
            {
                var rectTransform = provider.GetTargetRect();
                if (rectTransform != null)
                {
                    GUILayout.Label(rectTransform.transform.GetDebugName(), EditorStyles.miniLabel);
                }
            }

            GUILayout.Space(6);

            GUILayout.BeginHorizontal();

            DrawModeButton(controller, TileInteractionController.InteractionMode.Random, "R");
            DrawModeButton(controller, TileInteractionController.InteractionMode.Turn, "T");
            DrawModeButton(controller, TileInteractionController.InteractionMode.Split, "S");
            DrawModeButton(controller, TileInteractionController.InteractionMode.Merge, "M");
            DrawModeButton(controller, TileInteractionController.InteractionMode.Erase, "E");

            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Shortcuts: R, T, S, M, E", EditorStyles.miniLabel);

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Disable Editing", GUILayout.Height(24)))
            {
                controller.SetEditingEnabled(false);
            }

            if (GUILayout.Button("Bake State", GUILayout.Height(24)))
            {
                controller.BakeState();
            }

            if (GUILayout.Button("Bake Structure", GUILayout.Height(24)))
            {
                controller.BakeStructure();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            Handles.EndGUI();
        }

        private static void DrawModeButton(
            TileInteractionController controller,
            TileInteractionController.InteractionMode mode,
            string label)
        {
            bool active = controller.Mode == mode;

            Color prev = GUI.backgroundColor;
            if (active)
                GUI.backgroundColor = Color.green;

            if (GUILayout.Button(label, GUILayout.Height(24)))
            {
                controller.SetMode(mode);
            }

            GUI.backgroundColor = prev;
        }
    }
}