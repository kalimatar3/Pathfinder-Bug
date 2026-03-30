using System.Collections.Generic;
using Clouds.UI.Animation;
using UnityEngine;
using UnityEngine.UI;

public class BackGroundCanvas : baseCanvas,ICameraHolder,IBackGroundUIData
{
    [SerializeField] protected Camera _camera;
    [SerializeField] protected MonoBehaviour CirlceAppearAnimaion;
    [SerializeField] protected Image backGroundImage;
    public Camera Camera => _camera;
    public IUIAnimation AppearAnimaiton => CirlceAppearAnimaion.GetComponent<IUIAnimation>();
    public Image BackGroundImage => backGroundImage;
}
