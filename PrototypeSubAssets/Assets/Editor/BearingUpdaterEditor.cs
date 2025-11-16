using PrototypeSubMod.Puzzles.BearingPuzzle;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BearingUpdater))]
public class BearingUpdaterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var updater = (BearingUpdater)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("Refresh sprites"))
        {
            var symbolIndicators = updater.GetComponentsInChildren<BearingSymbolIndicator>(true);
            Undo.RecordObjects(symbolIndicators, "SymbolIndicators");
            foreach (var indicator in symbolIndicators)
            {
                indicator.RefreshSprite();
                
            }
            SceneView.RepaintAll();
            Canvas.ForceUpdateCanvases();
        }
    }
}
