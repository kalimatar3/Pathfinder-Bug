#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
[CustomEditor(typeof(baseUI), true)]
public class BaseUIEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        var ui = (baseUI)target;
        var UIid = ui.GetComponent<UniqueIDComponent>();
        EditorGUILayout.LabelField("Unique ID", UIid.UniqueID.ToString());
        base.OnInspectorGUI();
    }
}
#endif
