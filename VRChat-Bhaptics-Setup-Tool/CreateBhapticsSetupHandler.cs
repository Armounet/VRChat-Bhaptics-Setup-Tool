#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.Linq;
using System;
using System.Collections.Generic;

// Handles logic for creating Bhaptics setup.
public static class CreateBhapticsSetupHandler
{
    private const string BhapticsMenuName = "Bhaptics Menu"; // Submenu name.
    private const string vestVisibleParamName = "BhapticsVestVisibleToggle"; // Vest visibility parameter.
    private const string vibrationToggleParamName = "BhapticsVibrationToggle"; // General vibration parameter.
    private const string selfVibrationParamName = "BhapticsSelfVibrationToggle"; // Self vibration parameter.

    // Performs Bhaptics setup on the avatar.
    public static void PerformBhapticsSetup(GameObject avatar, Texture2D menuIcon)
    {
        var (avatarDescriptor, rootMenu) = GetAvatarComponents(avatar);
        if (avatarDescriptor == null || rootMenu == null) return;

        if (DoesBhapticsMenuExist(rootMenu) && DoBhapticsParametersExist(avatarDescriptor)) return;

        VRCExpressionsMenu BhapticsMenu = GetOrCreateBhapticsMenu(rootMenu, avatar, menuIcon);
        EnsureBhapticsParametersExist(avatarDescriptor);

        if (BhapticsMenu != null) AddBhapticsMenuControls(BhapticsMenu, avatarDescriptor);
    }

    // Gets avatar components.
    public static (VRCAvatarDescriptor, VRCExpressionsMenu) GetAvatarComponents(GameObject avatar) // Made public static
    {
        if (avatar == null || !avatar.TryGetComponent<VRCAvatarDescriptor>(out var descriptor) || descriptor.expressionsMenu == null) return (null, null);
        return (descriptor, descriptor.expressionsMenu);
    }

    // Checks if Bhaptics menu exists.
    static bool DoesBhapticsMenuExist(VRCExpressionsMenu parentMenu) => parentMenu?.controls?.Any(c => c.name.Equals(BhapticsMenuName, StringComparison.OrdinalIgnoreCase) && c.type == VRCExpressionsMenu.Control.ControlType.SubMenu) ?? false;

    // Checks if Bhaptics parameters exist.
    static bool DoBhapticsParametersExist(VRCAvatarDescriptor avatarDescriptor) => avatarDescriptor?.expressionParameters?.parameters?.Any(p => p.name.Equals(vestVisibleParamName, StringComparison.OrdinalIgnoreCase)) == true &&
                                                                                     avatarDescriptor?.expressionParameters?.parameters?.Any(p => p.name.Equals(vibrationToggleParamName, StringComparison.OrdinalIgnoreCase)) == true &&
                                                                                     avatarDescriptor?.expressionParameters?.parameters?.Any(p => p.name.Equals(selfVibrationParamName, StringComparison.OrdinalIgnoreCase)) == true;

    // Gets or creates Bhaptics menu.
    static VRCExpressionsMenu GetOrCreateBhapticsMenu(VRCExpressionsMenu parentMenu, GameObject avatar, Texture2D menuIcon) => DoesBhapticsMenuExist(parentMenu) ? parentMenu.controls.First(control => control.name.Equals(BhapticsMenuName, StringComparison.OrdinalIgnoreCase)).subMenu : CreateBhapticsSubmenu(parentMenu, avatar, menuIcon);

    // Creates Bhaptics submenu.
    static VRCExpressionsMenu CreateBhapticsSubmenu(VRCExpressionsMenu parentMenu, GameObject avatar, Texture2D menuIcon)
    {
        VRCExpressionsMenu newMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
        newMenu.name = parentMenu.name + "_" + BhapticsMenuName;
        AssetDatabase.CreateAsset(newMenu, BhapticsSetupHelperWindowUtils.GetAssetPath(parentMenu, newMenu.name + ".asset"));
        BhapticsSetupHelperWindowUtils.MarkDirtyAndSave(newMenu);
        AddSubmenuControlToParent(parentMenu, newMenu, avatar, menuIcon);
        return newMenu;
    }

    // Adds submenu control to parent.
    static void AddSubmenuControlToParent(VRCExpressionsMenu parentMenu, VRCExpressionsMenu submenu, GameObject avatar, Texture2D menuIcon)
    {
        VRCExpressionsMenu.Control newControl = new VRCExpressionsMenu.Control { name = BhapticsMenuName, type = VRCExpressionsMenu.Control.ControlType.SubMenu, subMenu = submenu, icon = menuIcon };
        if (parentMenu.controls == null) parentMenu.controls = new List<VRCExpressionsMenu.Control>();
        parentMenu.controls.Add(newControl);
        BhapticsSetupHelperWindowUtils.MarkDirtyAndSave(parentMenu);
    }

    // Adds Bhaptics menu controls.
    static void AddBhapticsMenuControls(VRCExpressionsMenu menu, VRCAvatarDescriptor avatarDescriptor)
    {
        if (menu == null) return;
        menu.controls = new List<VRCExpressionsMenu.Control> { CreateToggleControl("Vest Visibility Toggle", vestVisibleParamName), CreateToggleControl("Vest Vibration Toggle", vibrationToggleParamName), CreateToggleControl("Self Vibration Toggle", selfVibrationParamName) };
        BhapticsSetupHelperWindowUtils.MarkDirtyAndSave(menu);
    }

    // Creates a toggle control.
    static VRCExpressionsMenu.Control CreateToggleControl(string name, string parameterName) => new VRCExpressionsMenu.Control { name = name, type = VRCExpressionsMenu.Control.ControlType.Toggle, parameter = new VRCExpressionsMenu.Control.Parameter { name = parameterName }, value = 1f };

    // Ensures Bhaptics parameters exist.
    static void EnsureBhapticsParametersExist(VRCAvatarDescriptor avatarDescriptor)
    {
        VRCExpressionParameters parameters = avatarDescriptor.expressionParameters;
        if (parameters == null)
        {
            parameters = ScriptableObject.CreateInstance<VRCExpressionParameters>();
            parameters.name = avatarDescriptor.name + "_HapticsParameters";
            AssetDatabase.CreateAsset(parameters, BhapticsSetupHelperWindowUtils.GetAssetPath(avatarDescriptor, parameters.name + ".asset"));
            BhapticsSetupHelperWindowUtils.MarkDirtyAndSave(parameters);
            avatarDescriptor.expressionParameters = parameters;
            BhapticsSetupHelperWindowUtils.MarkDirtyAndSave(avatarDescriptor);
        }

        List<VRCExpressionParameters.Parameter> parameterList = parameters.parameters.ToList();
        AddParameterIfMissing(parameterList, vestVisibleParamName, VRCExpressionParameters.ValueType.Bool, true);
        AddParameterIfMissing(parameterList, vibrationToggleParamName, VRCExpressionParameters.ValueType.Bool, true);
        AddParameterIfMissing(parameterList, selfVibrationParamName, VRCExpressionParameters.ValueType.Bool, true);
        parameters.parameters = parameterList.ToArray();
        BhapticsSetupHelperWindowUtils.MarkDirtyAndSave(parameters);
    }

    // Adds parameter if missing.
    static bool AddParameterIfMissing(List<VRCExpressionParameters.Parameter> parameterList, string name, VRCExpressionParameters.ValueType type, bool saved)
    {
        if (!parameterList.Any(p => p.name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            parameterList.Add(new VRCExpressionParameters.Parameter { name = name, valueType = type, saved = saved });
            return true;
        }
        return false;
    }
}
#endif