using System.Collections.Generic;
using UnityEngine.UIElements;

public interface IStageDataControler
{
    public StageDynamicData stageDynamicData {get; set;}
    public bool ISLAODCOMPLETE {get;}
    void SaveData();
    List<StageData> RandomStageData();
}