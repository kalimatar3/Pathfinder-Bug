using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI; // Cần thiết để sử dụng CanvasScaler

public abstract class baseCanvas : MyBehaviour, IPopupHolder
{
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public CanvasScaler canvasScaler;
    public Dictionary<string, BasePopup> Popups => _popups;
    protected Dictionary<string, BasePopup> _popups;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        canvasScaler = GetComponent<CanvasScaler>();
        canvas = GetComponent<Canvas>();
    }
    protected override void Awake()
    {
        base.Awake();
        if (canvasScaler != null && canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            float screenRatio = (float)Screen.width / Screen.height;
            float referenceRatio = canvasScaler.referenceResolution.x / canvasScaler.referenceResolution.y;
            if (screenRatio > referenceRatio)
            {
                canvasScaler.matchWidthOrHeight = 1f; // Match Height
            }
            else
            {
                canvasScaler.matchWidthOrHeight = 0; // Match Width
            }
            Debug.Log($"CanvasScaler Match set to: {canvasScaler.matchWidthOrHeight} (0=Height, 1=Width) for screen ratio {screenRatio:F2} vs reference ratio {referenceRatio:F2}");
        }
    }
}