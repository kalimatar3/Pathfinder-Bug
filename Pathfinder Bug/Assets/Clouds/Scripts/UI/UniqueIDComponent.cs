using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class UniqueIDComponent : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private int uniqueID;

    [SerializeField, HideInInspector]
    private int lastInstanceID;

    public int UniqueID => uniqueID;

#if UNITY_EDITOR
    private void OnValidate()
    {
        uniqueID = GetInstanceID();
    }

    private void Reset()
    {
        uniqueID = GetInstanceID();
    }
#endif
}
