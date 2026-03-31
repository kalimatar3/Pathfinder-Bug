
using UnityEngine.UIElements;

[System.Serializable]
public class StageData
{
    public int stageindex = 0;
    public int StarGot = 0;
    public bool isLock  = true;
    public StageData()
    {
        
    }
    public StageData(int stageindex)
    {
        this.stageindex = stageindex;
        isLock = false;
        StarGot = 0;
    }
}