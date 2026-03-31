using System.Collections.Generic;
using Clouds.Ultilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
[DefaultExecutionOrder(-1)]
public class DataManager : Singleton<DataManager>,IStageDataControler
{
    [SerializeField] protected StageDynamicData _stageDynamicData;
    protected bool isStageDataLoadCompleted = false;
    [SerializeField] protected StageListAsset stageListAsset;
    public bool ISLAODCOMPLETE => isStageDataLoadCompleted;
    StageDynamicData IStageDataControler.stageDynamicData { get => _stageDynamicData; set => _stageDynamicData = value; }
    void IStageDataControler.SaveData() => SaveStageData();
    List<StageData> IStageDataControler.RandomStageData() => RandomStageData();
    
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        this.LoadData();
    }
    protected List<StageData> RandomStageData()
    {
        List<StageData> allStageData = stageListAsset.AllStages;
        int stage_unlocked = Random.Range(0, allStageData.Count);

        for (int i = 0; i < allStageData.Count; i++)
        {
            if (i < stage_unlocked)
            {
                allStageData[i].isLock = false;
                allStageData[i].StarGot = Random.Range(1, 4); // Random.Range(min, max) is exclusive for int max
            }
            else
            {
                allStageData[i].isLock = true;
                allStageData[i].StarGot = 0;
            }
        }
        return allStageData;
    }
    protected async void LoadData()
    {
        while(!LSManager.LoadDataFromPlayerPref(Config.STAGEDATA,new StageDynamicData(RandomStageData()),out _stageDynamicData)) await UniTask.DelayFrame(1);
        this.SaveStageData();
        isStageDataLoadCompleted = true;
    }
    public void SaveStageData()
    {
        LSManager.SaveDataToPlayerPref(Config.STAGEDATA,_stageDynamicData);
    }

}
