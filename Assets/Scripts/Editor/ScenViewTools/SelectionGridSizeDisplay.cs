using UnityEditor;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public static class SelectionGridSizeDisplay
    {
        static SelectionGridSizeDisplay()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            var selected = Selection.gameObjects;
            if (selected.Length == 0)
                return;

            Bounds? totalBounds = null;

            foreach (var go in selected)
            {
                if (go.TryGetBounds(out Bounds bounds))
                {
                    if (totalBounds == null)
                        totalBounds = bounds;

                    EncapsulateBounds(totalBounds.Value, bounds);
                }
            }

            if (!totalBounds.HasValue)
                return;

            Vector3 size = totalBounds.Value.size;

            var titleStyle = new GUIStyle(GUI.skin.window)
            {
                alignment = TextAnchor.UpperLeft,
            };

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };

            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(40, 5, 90, 80), "Bounds Size", titleStyle);
            GUILayout.Label($"W:  {FormatSmart(size.x)}", labelStyle);
            GUILayout.Label($"H: {FormatSmart(size.y)}", labelStyle);
            GUILayout.Label($"D:  {FormatSmart(size.z)}", labelStyle);
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        private static Bounds EncapsulateBounds(Bounds a, Bounds b)
        {
            a.Encapsulate(b.min);
            a.Encapsulate(b.max);
            return a;
        }

        private static string FormatSmart(float value)
        {
            return Mathf.Approximately(value % 1f, 0f) ? value.ToString("F0") : value.ToString("F2");
        }
    }
}