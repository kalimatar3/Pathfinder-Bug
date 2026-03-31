using UnityEngine;

namespace Clouds.Ultilities
{
    public interface IUIElement
    {
        Vector3 initPos { get; set; }
        Vector3 initScale { get; set; }
        float initFade { get; set; }
        GameObject UIObj { get; }
    }
}