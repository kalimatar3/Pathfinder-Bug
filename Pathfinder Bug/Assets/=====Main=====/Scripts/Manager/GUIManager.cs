using Clouds.Ultilities;
using UnityEngine;
using UnityEngine.UIElements;

public class GUIManager : Singleton<GUIManager>
{
    public IPopupHolder Popups;
    public BackGroundCanvas BackGroundCanvas;
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    public void SwitchCanvas(baseCanvas canvas)
    {
        this.Popups = canvas;
    }   
    public void ShowPopup(string PopupName)
    {
        Popups.Popups[PopupName].ShowPopup();
    }
    public void ClosePopup(string PopupName)
    {
        Popups.Popups[PopupName].ClosePopup();
    }

}
