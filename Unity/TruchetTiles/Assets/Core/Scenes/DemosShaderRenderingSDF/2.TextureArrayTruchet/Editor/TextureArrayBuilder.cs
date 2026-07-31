using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureArrayBuilder : EditorWindow
{
    // Adds a right-click context menu item in the Project Window
    [MenuItem("Assets/Create/Texture2D Array From Selection", false, 19)]
    // Adds a fallback option in the top main menu bar
    [MenuItem("Tools/Create Texture2D Array From Selection")]
    public static void CreateTextureArray()
    {
        // Gather all selected Texture2D assets
        Object[] selectedObjects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
        
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Please select at least one Texture2D asset in your Project window.", "OK");
            return;
        }

        // Use the first texture to define the settings for the entire array
        Texture2D firstTex = selectedObjects[0] as Texture2D;
        int width = firstTex.width;
        int height = firstTex.height;
        TextureFormat format = firstTex.format;

        // Initialize the Texture2DArray object (mipChain set to false for SDFs)
        Texture2DArray texArray = new Texture2DArray(width, height, selectedObjects.Length, format, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        // Loop through the selection and copy them into the array slices
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            Texture2D tex = selectedObjects[i] as Texture2D;
            
            // Validate that all textures match the exact same resolution
            if (tex.width != width || tex.height != height)
            {
                Debug.LogError($"Texture '{tex.name}' dimensions ({tex.width}x{tex.height}) do not match the first texture ({width}x{height}). All textures in an array must be identical in resolution.");
                return;
            }

            // Copy pixels into the corresponding slice of the Texture2DArray on the GPU
            Graphics.CopyTexture(tex, 0, texArray, i);
        }

        // Determine where to save the new asset based on the first selected texture
        string path = AssetDatabase.GetAssetPath(selectedObjects[0]);
        string directory = Path.GetDirectoryName(path);
        string savePath = Path.Combine(directory, "TruchetTextureArray.asset");
        
        // Save the asset to the project
        AssetDatabase.CreateAsset(texArray, savePath);
        AssetDatabase.SaveAssets();
        
        // Highlight the newly created asset in the editor
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = texArray;

        Debug.Log($"<color=green><b>Texture2DArray successfully created with {selectedObjects.Length} slices at: {savePath}</b></color>");
    }
}