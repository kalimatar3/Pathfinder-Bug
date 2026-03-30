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

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        this.LoadData();
    }
    protected async void LoadData()
    {
        while(!LSManager.LoadDataFromPlayerPref(Config.STAGEDATA,new StageDynamicData(stageListAsset.AllStages),out _stageDynamicData)) await UniTask.DelayFrame(1);
        isStageDataLoadCompleted = true;
    }
    public void SaveData()
    {
        LSManager.SaveDataToPlayerPref(Config.STAGEDATA,_stageDynamicData);
    }
}
