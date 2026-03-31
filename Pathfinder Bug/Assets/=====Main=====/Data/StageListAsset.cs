using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "StageList", menuName = "Game Data/Stage List")]
public class StageListAsset : ScriptableObject
{
    [SerializeField]
    private List<StageData> allStages = new List<StageData>();
    public List<StageData> AllStages => allStages;
    [Button(ButtonSizes.Large)]
    void GenerateDummyStages()
    {
        allStages.Clear();
        for (int i = 0; i < 999; i++)
        {
            allStages.Add(new StageData { 
                stageindex = i, 
                isLock = (i % 5 != 0 && i != 0) // Example: stages 1, 6, 11... are unlocked, first stage is unlocked
            });
        }
        // Mark the asset as dirty so Unity Editor saves changes
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
        Debug.Log($"Generated {allStages.Count} dummy stages.");
    }

    // Function to update data for a specific stage (e.g., after player completes it)
    public void UpdateStageData(int stageIndex, StageData newData)
    {
        if (stageIndex >= 0 && stageIndex < allStages.Count)
        {
            allStages[stageIndex] = newData;
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
}