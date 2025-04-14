#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System;

// Handles logic for creating Bhaptics animation layers and parameters.
public static class CreateBhapticsAnimationHandler
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

    // Creates Bhaptics animation layers and parameters.
    public static void CreateBhapticsLayersAndParams(AnimatorController controller)
    {
        if (controller == null) return;
        bool somethingCreated = false;
        foreach (var config in bHapticsLayerConfigs)
        {
            AnimatorStateMachine layer = GetOrCreateAnimatorLayer(controller, config.LayerName);
            if (!controller.parameters.Any(p => p.name.Equals(config.ParameterName, StringComparison.OrdinalIgnoreCase)))
            {
                EnsureAnimatorParameterExists(controller, config.ParameterName, AnimatorControllerParameterType.Bool);
                somethingCreated = true;
            }
            IntegrateToggleLogic(controller, layer, config.ParameterName, config.AnimationPrefix);
        }
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }

    // Gets or creates animation layer.
    private static AnimatorStateMachine GetOrCreateAnimatorLayer(AnimatorController controller, string layerName)
    {
        AnimatorControllerLayer existingLayer = controller.layers.FirstOrDefault(layer => layer.name.Equals(layerName, StringComparison.OrdinalIgnoreCase));
        if (existingLayer != null) return existingLayer.stateMachine;
        AnimatorControllerLayer newLayer = new AnimatorControllerLayer { name = layerName, stateMachine = new AnimatorStateMachine(), defaultWeight = 1f };
        controller.AddLayer(newLayer);
        return newLayer.stateMachine;
    }

    // Ensures animator parameter exists.
    private static void EnsureAnimatorParameterExists(AnimatorController controller, string paramName, AnimatorControllerParameterType type)
    {
        if (!controller.parameters.Any(p => p.name.Equals(paramName, StringComparison.OrdinalIgnoreCase)))
            controller.AddParameter(paramName, type);
    }

    // Integrates toggle logic into the layer.
    private static void IntegrateToggleLogic(AnimatorController controller, AnimatorStateMachine stateMachine, string parameterName, string animationPrefix)
    {
        if (stateMachine == null) return;
        string offAnimName = animationPrefix + "_Off";
        string onAnimName = animationPrefix + "_On";
        AnimationClip LoadAnimation(string animName) => AssetDatabase.LoadAssetAtPath<AnimationClip>($"Assets/bHaptics Avatar Setup by Armounet/Animation/{animName}.anim");
        AnimationClip offAnim = LoadAnimation(offAnimName);
        AnimationClip onAnim = LoadAnimation(onAnimName);
        if (offAnim != null && onAnim != null)
        {
            AnimatorState GetOrCreateState(string stateName, Motion motion)
            {
                var state = stateMachine.states.FirstOrDefault(s => s.state.name.Equals(stateName, StringComparison.OrdinalIgnoreCase)).state;
                if (state == null)
                {
                    // Explicitly use the AddState overload that takes a Motion.
                    state = stateMachine.AddState(stateName);
                    state.motion = motion;
                }
                return state;
            }
            AnimatorState offState = GetOrCreateState(offAnimName, offAnim);
            AnimatorState onState = GetOrCreateState(onAnimName, onAnim);
            AddTransition(offState, onState, parameterName, AnimatorConditionMode.If);
            AddTransition(onState, offState, parameterName, AnimatorConditionMode.IfNot);
            if (stateMachine.defaultState == null) stateMachine.defaultState = offState;
        }
    }

    // Adds a transition.
    private static void AddTransition(AnimatorState fromState, AnimatorState toState, string parameterName, AnimatorConditionMode mode)
    {
        if (!fromState.transitions.Any(t => t.destinationState == toState && t.conditions.Any(c => c.mode == mode && c.parameter.Equals(parameterName, StringComparison.OrdinalIgnoreCase))))
        {
            AnimatorStateTransition transition = fromState.AddTransition(toState);
            transition.AddCondition(mode, 0, parameterName);
            transition.hasExitTime = false;
            transition.duration = 0.25f;
            transition.offset = 0f;
            transition.interruptionSource = TransitionInterruptionSource.None;
        }
    }
}
#endif