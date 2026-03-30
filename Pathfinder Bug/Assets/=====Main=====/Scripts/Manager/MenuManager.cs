using System;
using Clouds.Ultilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : Singleton<MenuManager>,IMenuController
{
    [SerializeField] protected baseCanvas MenuCanvas;
    IBackGroundUIData backGroundUIData;
    public override void Awake()
    {
        base.Awake();
        GUIManager.Instance.SwitchCanvas(MenuCanvas);
        backGroundUIData = GUIManager.Instance.BackGroundCanvas;
    }
    protected void Start()
    {
        backGroundUIData.BackGroundImage.material.SetVector("_RevealCenter",new Vector4(.5f,.5f,0,0));
    }
    public async void Play()
    {
        backGroundUIData.AppearAnimaiton.Play();
        do
        {
            await UniTask.DelayFrame(1);
        }
        while( backGroundUIData.AppearAnimaiton.IsPlaying) ;
        await SceneManager.LoadSceneAsync("Play");
        backGroundUIData.AppearAnimaiton.Restart();
    }
}
