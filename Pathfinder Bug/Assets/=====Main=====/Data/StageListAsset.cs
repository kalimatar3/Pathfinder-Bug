using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageList", menuName = "Game Data/Stage List")]
public class StageListAsset : ScriptableObject
{
    [SerializeField]
    private List<StageData> allStages = new List<StageData>();

    public List<StageData> AllStages => allStages;

    // Hàm để khởi tạo 999 stage data ban đầu (chỉ chạy trong Editor)
    // Bạn có thể click chuột phải vào asset StageList trong Project view, chọn "Generate 999 Dummy Stages"
    [ContextMenu("Generate 999 Dummy Stages")] 
    void GenerateDummyStages()
    {
        allStages.Clear();
        for (int i = 0; i < 999; i++)
        {
            allStages.Add(new StageData { 
                stageindex = i, 
                isLock = (i % 5 != 0 && i != 0) // Ví dụ: màn 1, 6, 11... không khóa, màn đầu tiên không khóa
            });
        }
        // Đánh dấu asset là đã thay đổi để Unity Editor lưu lại
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
        Debug.Log($"Generated {allStages.Count} dummy stages.");
    }

    // Hàm để cập nhật dữ liệu của một màn chơi (ví dụ: sau khi người chơi hoàn thành)
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