using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HierarchyColorizer
{
    static HierarchyColorizer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        HierarchyColor hc = obj.GetComponent<HierarchyColor>();
        if (hc == null || string.IsNullOrEmpty(hc.tagLabel)) return;

        GUIStyle tagStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = new GUIStyleState() { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        Vector2 size = tagStyle.CalcSize(new GUIContent(hc.tagLabel));
        Rect tagRect = new Rect(selectionRect.xMax - size.x - 6f, selectionRect.y + 1f, size.x + 4f, selectionRect.height - 2f);

        EditorGUI.DrawRect(tagRect, hc.tagColor);
        EditorGUI.LabelField(tagRect, hc.tagLabel, tagStyle);
    }
}
