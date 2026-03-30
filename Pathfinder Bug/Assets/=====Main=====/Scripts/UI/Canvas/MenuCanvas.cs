using System.Collections.Generic;
using UnityEngine;

public class MenuCanvas : baseCanvas, IPopupHolder,ICameraHolder
{
    [SerializeField] protected Camera _camera;
    public Camera Camera => _camera;
}
