using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MissingScriptFinder : EditorWindow
{
    [MenuItem("Tools/Check Missing Scripts in Scene")]
    static void FindMissingScriptsInScene()
    {
        int goCount = 0;
        int componentsCount = 0;
        int missingCount = 0;

        // Duyệt toàn bộ các GameObject trong scene, bao gồm cả inactive
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);

        foreach (GameObject go in allObjects)
        {
            goCount++;
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                componentsCount++;
                if (components[i] == null)
                {
                    missingCount++;
                    Debug.LogWarning(
                        $"⚠️ Missing script on: {GetFullPath(go)}",
                        go
                    );
                }
            }
        }

        Debug.Log($"✅ Scan complete. Checked {goCount} GameObjects, {componentsCount} Components, found {missingCount} missing scripts.");
    }

    // Hàm giúp tạo đường dẫn đầy đủ của GameObject trong hierarchy
    static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform current = go.transform;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }
        return path;
    }
    [MenuItem("Tools/Remove Missing Scripts in Scene")]
    static void RemoveMissingScriptsInScene()
    {
        int count = 0;
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);

        foreach (GameObject go in allObjects)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                Debug.Log($"🧹 Removed {removed} missing script(s) from: {GetFullPath(go)}", go);
                count += removed;
            }
        }

        Debug.Log($"✅ Done! Removed {count} missing scripts in total.");
    }
}
