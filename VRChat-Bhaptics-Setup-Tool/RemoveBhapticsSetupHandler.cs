#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.Linq;
using System;
using System.Collections.Generic;

// Handles logic for removing Bhaptics setup.
public static class RemoveBhapticsSetupHandler
{
    private const string BhapticsMenuName = "Bhaptics Menu"; // Submenu name.
    private const string vestVisibleParamName = "BhapticsVestVisibleToggle"; // Vest visibility parameter.
    private const string vibrationToggleParamName = "BhapticsVibrationToggle"; // General vibration parameter.
    private const string selfVibrationParamName = "BhapticsSelfVibrationToggle"; // Self vibration parameter.

    // Removes Bhaptics setup from the avatar.
    public static void RemoveBhapticsSetup(GameObject avatar)
    {
        var (avatarDescriptor, rootMenu) = CreateBhapticsSetupHandler.GetAvatarComponents(avatar);
        if (avatarDescriptor == null || rootMenu == null) return;

        if (!DoesBhapticsMenuExist(rootMenu) && !DoBhapticsParametersExist(avatarDescriptor)) return;

        RemoveBhapticsMenu(rootMenu, avatar);
        RemoveBhapticsParameters(avatarDescriptor);
    }

    // Checks if Bhaptics menu exists.
    static bool DoesBhapticsMenuExist(VRCExpressionsMenu parentMenu) => parentMenu?.controls?.Any(c => c.name.Equals(BhapticsMenuName, StringComparison.OrdinalIgnoreCase) && c.type == VRCExpressionsMenu.Control.ControlType.SubMenu) ?? false;

    // Checks if Bhaptics parameters exist.
    static bool DoBhapticsParametersExist(VRCAvatarDescriptor avatarDescriptor) => avatarDescriptor?.expressionParameters?.parameters?.Any(p => p.name.Equals(vestVisibleParamName, StringComparison.OrdinalIgnoreCase)) == true &&
                                                                                     avatarDescriptor?.expressionParameters?.parameters?.Any(p => p.name.Equals(vibrationToggleParamName, StringComparison.OrdinalIgnoreCase)) == true &&
                                                                                     avatarDescriptor?.expressionParameters?.parameters?.Any(p => p.name.Equals(selfVibrationParamName, StringComparison.OrdinalIgnoreCase)) == true;

    // Removes Bhaptics menu.
    static void RemoveBhapticsMenu(VRCExpressionsMenu parentMenu, GameObject avatar)
    {
        if (parentMenu?.controls == null) return;
        int indexToRemove = parentMenu.controls.FindIndex(control => control.name.Equals(BhapticsMenuName, StringComparison.OrdinalIgnoreCase) && control.type == VRCExpressionsMenu.Control.ControlType.SubMenu);
        if (indexToRemove != -1)
        {
            VRCExpressionsMenu menuToRemove = parentMenu.controls[indexToRemove].subMenu;
            parentMenu.controls.RemoveAt(indexToRemove);
            BhapticsSetupHelperWindowUtils.MarkDirtyAndSave(parentMenu);
            BhapticsSetupHelperWindowUtils.DeleteAsset(menuToRemove);
        }
    }

    // Removes Bhaptics parameters.
    static void RemoveBhapticsParameters(VRCAvatarDescriptor avatarDescriptor)
    {
        VRCExpressionParameters parameters = avatarDescriptor?.expressionParameters;
        if (parameters?.parameters == null) return;
        List<string> parametersToRemove = new List<string> { vestVisibleParamName, vibrationToggleParamName, selfVibrationParamName };
        parameters.parameters = parameters.parameters.Where(p => !parametersToRemove.Contains(p.name)).ToArray();
        BhapticsSetupHelperWindowUtils.MarkDirtyAndSave(parameters);
    }
}
#endif