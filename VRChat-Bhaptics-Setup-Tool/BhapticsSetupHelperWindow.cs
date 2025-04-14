#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

// Main editor window for the Bhaptics Setup tool.
public class BhapticsSetupHelperWindow : EditorWindow
{
    private bool isVestSetupVisible = false; // Show/hide Vest setup.
    private bool isAnimationSetupVisible = false; // Show/hide Animation setup.

    private GameObject avatarForSetup; // Selected avatar.
    private Texture2D MenuIconToAdd; // Optional submenu icon.
    private AnimatorController animatorController; // Selected animator.

    private GUIStyle titleStyle; // Style for title.
    private GUIStyle subTitleStyle; // Style for subtitle.
    private GUIStyle buttonStyle; // Style for button.

    private bool stylesInitialized = false; // Styles initialized flag.

    // Opens the Bhaptics Setup window.
    [MenuItem("VRChat Bhaptics Setup/Setup")]
    public static void OpenSetupWindow() => GetWindow<BhapticsSetupHelperWindow>("VRChat Bhaptics Setup").Show();

    // Handles the UI drawing and logic.
    void OnGUI()
    {
        if (!stylesInitialized) InitializeStyles();

        GUILayout.Label("VRChat Bhaptics Setup", titleStyle);
        EditorGUILayout.Space();

        if (!isVestSetupVisible && !isAnimationSetupVisible) DrawMainMenu();
        else if (isVestSetupVisible) DrawBhapticsVestSetup();
        else if (isAnimationSetupVisible) DrawBhapticsAnimationSetup();

        GUILayout.FlexibleSpace();
        GUILayout.Label("By Armounet", new GUIStyle(EditorStyles.label) { fontSize = 12, alignment = TextAnchor.MiddleRight });
    }

    // Initializes GUI styles for the window.
    private void InitializeStyles()
    {
        titleStyle = new GUIStyle(EditorStyles.largeLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 24, fontStyle = FontStyle.Bold };
        subTitleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 };
        buttonStyle = new GUIStyle(GUI.skin.button) { padding = new RectOffset(15, 15, 10, 10), fontSize = 14 };
    }

    // Draws the main menu with setup options.
    void DrawMainMenu()
    {
        EditorGUILayout.Space();
        if (GUILayout.Button(new GUIContent("Bhaptics Setup", EditorGUIUtility.FindTexture("AvatarPivot")), buttonStyle)) isVestSetupVisible = true;
        EditorGUILayout.Space();
        if (GUILayout.Button(new GUIContent("Animation Setup", EditorGUIUtility.FindTexture("SettingsIcon")), buttonStyle)) isAnimationSetupVisible = true;
    }

    // Draws the UI for Bhaptics Vest setup.
    void DrawBhapticsVestSetup()
    {
        GUILayout.Label("Bhaptics Setup", subTitleStyle);
        EditorGUILayout.Space();

        avatarForSetup = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Avatar", "Select the avatar."), avatarForSetup, typeof(GameObject), true);
        MenuIconToAdd = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Submenu Icon (Optional)", "Optional icon."), MenuIconToAdd, typeof(Texture2D), false);

        EditorGUILayout.Space();
        if (GUILayout.Button("Create Bhaptics Setup", buttonStyle))
            if (avatarForSetup == null) EditorGUILayout.HelpBox("Select an avatar.", MessageType.Warning); else CreateBhapticsSetupHandler.PerformBhapticsSetup(avatarForSetup, MenuIconToAdd);
        if (GUILayout.Button("Remove Bhaptics Setup", buttonStyle))
            if (avatarForSetup == null) EditorGUILayout.HelpBox("Select an avatar.", MessageType.Warning); else RemoveBhapticsSetupHandler.RemoveBhapticsSetup(avatarForSetup);
        EditorGUILayout.Space();
        if (GUILayout.Button("Back", buttonStyle)) isVestSetupVisible = false;
    }

    // Draws the UI for Bhaptics Animation setup.
    void DrawBhapticsAnimationSetup()
    {
        GUILayout.Label("Bhaptics Animation Setup", subTitleStyle);
        EditorGUILayout.Space();

        animatorController = (AnimatorController)EditorGUILayout.ObjectField(new GUIContent("Animator Controller"), animatorController, typeof(AnimatorController), false);

        EditorGUILayout.Space();
        if (GUILayout.Button("Create Bhaptics Layers & Parameters", buttonStyle))
            if (animatorController == null) EditorGUILayout.HelpBox("Select an Animator Controller.", MessageType.Warning); else CreateBhapticsAnimationHandler.CreateBhapticsLayersAndParams(animatorController);
        if (GUILayout.Button("Remove Bhaptics Layers & Parameters", buttonStyle))
            if (animatorController == null) EditorGUILayout.HelpBox("Select an Animator Controller.", MessageType.Warning); else RemoveBhapticsAnimationHandler.RemoveBhapticsLayers(animatorController);
        EditorGUILayout.Space();
        if (GUILayout.Button("Back", buttonStyle)) isAnimationSetupVisible = false;
    }
}
#endif