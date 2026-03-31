using System.Collections.Generic;
using UnityEngine.UIElements;

public interface IStageDataControler
{
    public StageDynamicData stageDynamicData {get; set;}
    public bool ISLAODCOMPLETE {get;}
    public void SaveData();
    List<StageData> RandomStageData();
}