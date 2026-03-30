using System.Collections.Generic;
[System.Serializable]
public class StageDynamicData : DynamicData
{
    public List<StageData> stageDatas;
    public StageDynamicData(List<StageData> allStages)
    {
        this.stageDatas = allStages;
    }
}