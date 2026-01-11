using PrototypeSubMod.MiscMonobehaviors.SubSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(SFXButton))]
public class SFXButtonEditor : Editor
{
    private SerializedProperty onClick;

    public override void OnInspectorGUI()
    {
        var button = (SFXButton)target;
        EditorGUI.BeginChangeCheck();
        Undo.RecordObject(target, "SFX Button Editor");

        if (onClick == null)
        {
            onClick = serializedObject.FindProperty("onClickWrapper");
        }

        button.interactable = EditorGUILayout.Toggle("Interactable", button.interactable);
        button.transition = (Selectable.Transition)EditorGUILayout.EnumPopup("Transition", button.transition);

        EditorGUI.indentLevel++;
        button.targetGraphic = (Graphic)EditorGUILayout.ObjectField("Target Graphic", button.targetGraphic, typeof(Graphic), true);
        switch (button.transition)
        {
            case Selectable.Transition.None:
                break;
            case Selectable.Transition.ColorTint:
                var normalColor = EditorGUILayout.ColorField("Normal Color", button.colors.normalColor);
                var highlightedColor = EditorGUILayout.ColorField("Highlighted Color", button.colors.highlightedColor);
                var pressedColor = EditorGUILayout.ColorField("Pressed Color", button.colors.pressedColor);
                var selectedColor = EditorGUILayout.ColorField("Selected Color", button.colors.selectedColor);
                var disabledColor = EditorGUILayout.ColorField("Disabled Color", button.colors.disabledColor);
                var colorMultiplier = EditorGUILayout.Slider("Color Multiplier", button.colors.colorMultiplier, 1, 5);
                var fadeDuration = EditorGUILayout.FloatField("Fade Duration", button.colors.fadeDuration);
                button.colors = new ColorBlock()
                {
                    normalColor = normalColor,
                    highlightedColor = highlightedColor,
                    pressedColor = pressedColor,
                    selectedColor = selectedColor,
                    disabledColor = disabledColor,
                    colorMultiplier = colorMultiplier,
                    fadeDuration = fadeDuration
                };
                break;
            case Selectable.Transition.SpriteSwap:
                var highlightedSprite = (Sprite)EditorGUILayout.ObjectField("Highlighted Sprite", button.spriteState.highlightedSprite, typeof(Sprite), false);
                var pressedSprite = (Sprite)EditorGUILayout.ObjectField("Pressed Sprite", button.spriteState.pressedSprite, typeof(Sprite), false);
                var selectedSprite = (Sprite)EditorGUILayout.ObjectField("Selected Sprite", button.spriteState.selectedSprite, typeof(Sprite), false);
                var disabledSprite = (Sprite)EditorGUILayout.ObjectField("Disabled Sprite", button.spriteState.disabledSprite, typeof(Sprite), false);
                button.spriteState = new SpriteState()
                {
                    highlightedSprite = highlightedSprite,
                    pressedSprite = pressedSprite,
                    selectedSprite = selectedSprite,
                    disabledSprite = disabledSprite
                };
                break;
            case Selectable.Transition.Animation:
                var normalTrigger = EditorGUILayout.TextField("Normal Trigger", button.animationTriggers.normalTrigger);
                var highlightedTrigger = EditorGUILayout.TextField("Highlited Trigger", button.animationTriggers.highlightedTrigger);
                var pressedTrigger = EditorGUILayout.TextField("Pressed Trigger", button.animationTriggers.pressedTrigger);
                var selectedTrigger = EditorGUILayout.TextField("Selected Trigger", button.animationTriggers.selectedTrigger);
                var disabledTrigger = EditorGUILayout.TextField("Disabled Trigger", button.animationTriggers.disabledTrigger);
                button.animationTriggers = new AnimationTriggers()
                {
                    normalTrigger = normalTrigger,
                    highlightedTrigger = highlightedTrigger,
                    pressedTrigger = pressedTrigger,
                    selectedTrigger = selectedTrigger,
                    disabledTrigger = disabledTrigger
                };
                break;
        }

        EditorGUILayout.Space(5);
        EditorGUI.indentLevel--;

        var navMode = (Navigation.Mode)EditorGUILayout.EnumPopup("Navigation", button.navigation.mode);
        if (navMode == Navigation.Mode.Explicit)
        {
            var selectOnLeft = (Selectable)EditorGUILayout.ObjectField("Select On Left", button.navigation.selectOnLeft, typeof(Selectable), true);
            var selectOnRight = (Selectable)EditorGUILayout.ObjectField("Select On Right", button.navigation.selectOnRight, typeof(Selectable), true);
            var selectOnUp = (Selectable)EditorGUILayout.ObjectField("Select On Up", button.navigation.selectOnUp, typeof(Selectable), true);
            var selectOnDown = (Selectable)EditorGUILayout.ObjectField("Select On Down", button.navigation.selectOnDown, typeof(Selectable), true);
            button.navigation = new Navigation()
            {
                selectOnLeft = selectOnLeft,
                selectOnRight = selectOnRight,
                selectOnUp = selectOnUp,
                selectOnDown = selectOnDown,
                mode = navMode
            };
        }
        else
        {
            button.navigation = new Navigation()
            {
                selectOnLeft = button.navigation.selectOnLeft,
                selectOnRight = button.navigation.selectOnRight,
                selectOnUp = button.navigation.selectOnUp,
                selectOnDown = button.navigation.selectOnDown,
                mode = navMode
            };
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(onClick);

        button.onEnterFX = (FMODAsset)EditorGUILayout.ObjectField("On Enter SFX", button.onEnterFX, typeof(FMODAsset), false);
        button.onExitFX = (FMODAsset)EditorGUILayout.ObjectField("On Exit SFX", button.onExitFX, typeof(FMODAsset), false);
        button.onClickFX = (FMODAsset)EditorGUILayout.ObjectField("On Click SFX", button.onClickFX, typeof(FMODAsset), false);
        button.volume = EditorGUILayout.FloatField("Volume", button.volume);
        button.minDistForSound = EditorGUILayout.FloatField("Min Dist For Sound", button.minDistForSound);

        if (!serializedObject.hasModifiedProperties) return;
        
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(button);
        EditorSceneManager.MarkSceneDirty(button.gameObject.scene);
    }
}
