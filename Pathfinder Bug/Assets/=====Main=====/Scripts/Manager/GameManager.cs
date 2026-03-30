using Clouds.Ultilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        this.LoadtoMenu();
    }
    protected async void LoadtoMenu()
    {
        while(!DataManager.Instance.ISLAODCOMPLETE) await UniTask.DelayFrame(1);
        await SceneManager.LoadSceneAsync("Menu");
    }
}
