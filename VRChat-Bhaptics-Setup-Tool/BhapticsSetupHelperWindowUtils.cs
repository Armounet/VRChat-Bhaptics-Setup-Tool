#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Utility functions for the Bhaptics Setup Helper Window.
public static class BhapticsSetupHelperWindowUtils
{
    // Gets asset path.
    public static string GetAssetPath(UnityEngine.Object parentObject, string assetName)
    {
        string assetPath = AssetDatabase.GetAssetPath(parentObject);
        string directory = System.IO.Path.GetDirectoryName(assetPath);
        return System.IO.Path.Combine(directory, assetName);
    }

    // Marks dirty and saves asset.
    public static void MarkDirtyAndSave(UnityEngine.Object obj)
    {
        EditorUtility.SetDirty(obj);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // Deletes asset.
    public static void DeleteAsset(UnityEngine.Object obj)
    {
        if (obj == null) return;
        string assetPath = AssetDatabase.GetAssetPath(obj);
        AssetDatabase.DeleteAsset(assetPath);
    }
}
#endif