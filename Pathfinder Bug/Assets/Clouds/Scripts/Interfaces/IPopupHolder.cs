using UnityEngine; 
using Clouds.Ultilities;
using System.Collections.Generic;
public interface IPopupHolder
{
    Dictionary<string,BasePopup> Popups {get;}
}