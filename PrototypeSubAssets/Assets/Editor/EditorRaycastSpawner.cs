using UnityEditor;
using UnityEngine;

public class EditorRaycastSpawner : EditorWindow
{
    private static bool active;
    private static bool forwardOppositeToNormal;
    private static Transform parent;
    private static Material material;

    [MenuItem("Tools/Editor raycast spawner")]
    private static void Init()
    {
        var window = (EditorRaycastSpawner)GetWindow(typeof(EditorRaycastSpawner));
        window.Show();
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }

    private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    private void OnSceneGUI(SceneView view)
    {
        if (!active) return;

        if (Event.current.type == EventType.MouseDown && parent)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            if (Physics.Raycast(ray, out var hitInfo))
            {
                var point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.transform.position = hitInfo.point + hitInfo.normal * 0.25f;
                point.transform.forward = hitInfo.normal * (forwardOppositeToNormal ? -1 : 1);
                point.transform.localScale = Vector3.one * 0.2f;
                point.transform.SetParent(parent);
                DestroyImmediate(point.GetComponent<Collider>());

                point.name = $"SpawnedPoint ({parent.childCount})";

                if (material != null)
                {
                    point.GetComponent<Renderer>().material = material;
                }
            }
        }
        
        Event.current.Use();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Toggle Raycasting"))
        {
            active = !active;
        }

        GUILayout.Label("Active = " + active);
        forwardOppositeToNormal = GUILayout.Toggle(forwardOppositeToNormal, "Forward is opposite to normal");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Attach parent ");
        parent = (Transform)EditorGUILayout.ObjectField(parent, typeof(Transform), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Spawned transform material ");
        material = (Material)EditorGUILayout.ObjectField(material, typeof(Material), false);
        GUILayout.EndHorizontal();
    }
}
