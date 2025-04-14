#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System;

// Handles logic for removing Bhaptics animation layers and parameters.
public static class RemoveBhapticsAnimationHandler
{
    private struct BhapticsLayerConfig
    {
        public string LayerName;
        public string AnimationPrefix;
        public string ParameterName;
    }
    
    private static readonly BhapticsLayerConfig[] bHapticsLayerConfigs = new BhapticsLayerConfig[]
    {
        new BhapticsLayerConfig { LayerName = "BhapticsSelfVibrationToggle", AnimationPrefix = "Bhaptics_Self_Vibration", ParameterName = "BhapticsSelfVibrationToggle" },
        new BhapticsLayerConfig { LayerName = "BhapticsVestVisibleToggle", AnimationPrefix = "Bhaptics_Vest_Visible", ParameterName = "BhapticsVestVisibleToggle" },
        new BhapticsLayerConfig { LayerName = "BhapticsVibrationToggle", AnimationPrefix = "Bhaptics_Vibration", ParameterName = "BhapticsVibrationToggle" }
    };

    // Removes Bhaptics animation layers and parameters.
    public static void RemoveBhapticsLayers(AnimatorController controller)
    {
        if (controller == null) return;
        bool somethingRemoved = false;
        foreach (var config in bHapticsLayerConfigs)
        {
            for (int i = controller.layers.Length - 1; i >= 0; i--)
                if (controller.layers[i].name.Equals(config.LayerName, StringComparison.OrdinalIgnoreCase)) { controller.RemoveLayer(i); somethingRemoved = true; break; }
            for (int i = controller.parameters.Length - 1; i >= 0; i--)
                if (controller.parameters[i].name.Equals(config.ParameterName, StringComparison.OrdinalIgnoreCase)) { controller.RemoveParameter(controller.parameters[i]); somethingRemoved = true; break; }
        }
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    // Removes animator layer by name.
    private static void RemoveAnimatorLayer(AnimatorController controller, string layerName)
    {
        for (int i = controller.layers.Length - 1; i >= 0; i--)
            if (controller.layers[i].name.Equals(layerName, StringComparison.OrdinalIgnoreCase))
                controller.RemoveLayer(i);
    }

    // Removes animator parameter by name.
    private static void RemoveAnimatorParameter(AnimatorController controller, string paramName)
    {
        for (int i = controller.parameters.Length - 1; i >= 0; i--)
            if (controller.parameters[i].name.Equals(paramName, StringComparison.OrdinalIgnoreCase))
                controller.RemoveParameter(controller.parameters[i]);
    }
}
#endif