using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using Clouds.UI.Animation;
using Clouds.Ultilities;

namespace Clouds.Ultilities
{
    [System.Serializable]
    public struct UIEffectData
    {
        public TRIGGEREFFECT type;
        [Range(0, 5)] public float Delay;
        
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] public MOVEEFFECT moveType;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] [EnableIf("@this.type == TRIGGEREFFECT.Move && this.moveType == MOVEEFFECT.Custom")] public RectTransform moveFrom;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] public float timeMove;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] public ACTIVATETYPE moveActivate;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] [ShowIf("moveActivate", ACTIVATETYPE.Delay)] public float delayTimeMove;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] public Ease easeMove;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] public bool loopMove;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] public LoopType MoveLoopType;
        [BoxGroup("Move Settings")] [ShowIf("type", TRIGGEREFFECT.Move)] [ShowIf("loopMove")] public int loopCircleMove;

        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] public Vector3 rotateTo;
        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] public float timeRotate;
        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] public ACTIVATETYPE rotateActivate;
        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] [ShowIf("rotateActivate", ACTIVATETYPE.Delay)] public float delayTimeRotate;
        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] public Ease easeRotate;
        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] public bool loopRotate;
        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] public LoopType RotateLoopType;
        [BoxGroup("Rotate Settings")] [ShowIf("type", TRIGGEREFFECT.Rotate)] [ShowIf("loopRotate")] public int loopCircleRotate;

        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public Vector3 scaleTo;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public float timeScale;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public ACTIVATETYPE scaleActivate;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] [ShowIf("scaleActivate", ACTIVATETYPE.Delay)] public float delayTimeScale;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public Ease easeScale;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public bool loopScale;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] public LoopType ScaleLoopType;
        [BoxGroup("Scale Settings")] [ShowIf("type", TRIGGEREFFECT.Scale)] [ShowIf("loopScale")] public int loopCircleScale;

        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] public bool shakePosition, shakeRotation, shakeScale;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] public float shakeStrength, shakeRandomness, timeShake;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] public int shakeVibrate;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] public ACTIVATETYPE shakeActivate;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] [ShowIf("shakeActivate", ACTIVATETYPE.Delay)] public float delayTimeShake;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] public Ease easeShake;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] public bool loopShake;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] public LoopType ShakeLoopType;
        [BoxGroup("Shake Settings")] [ShowIf("type", TRIGGEREFFECT.Shake)] [ShowIf("loopShake")] public int loopCircleShake;

        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public bool punchPostion, punchRotation, punchScale;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public Vector2 punchTo;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public int punchVibrate;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public float punchElasticity, timePunch;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public ACTIVATETYPE punchActivate;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] [ShowIf("punchActivate", ACTIVATETYPE.Delay)] public float delayTimePunch;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public Ease easePunch;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public bool loopPunch;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] public LoopType PunchLoopType;
        [BoxGroup("Punch Settings")] [ShowIf("type", TRIGGEREFFECT.Punch)] [ShowIf("loopPunch")] public int loopCirclePunch;

        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] [Range(0, 1)] public float fadeTo;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] public float timeFade;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] public ACTIVATETYPE fadeActivate;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] [ShowIf("fadeActivate", ACTIVATETYPE.Delay)] public float delayTimeFade;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] public Ease easeFade;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] public bool loopFade;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] public LoopType FadeLooptype;
        [BoxGroup("Fade Settings")] [ShowIf("type", TRIGGEREFFECT.Fade)] [ShowIf("loopFade")] public int loopCircleFade;
    }

    [System.Serializable]
    public struct UIContinousEffectdata
    {
        public CONTINUOSEFFECT type;
        [ShowIf("type", CONTINUOSEFFECT.FillText)] public string Text;
        [ShowIf("type", CONTINUOSEFFECT.FillText)] public TextMeshProUGUI TextComponent;
        [ShowIf("type", CONTINUOSEFFECT.ChangeOpacity)] public Image Image;
        [ShowIf("type", CONTINUOSEFFECT.ChangeOpacity)] public float Opacity;
    }

    [System.Serializable]
    public class UIAnimationElement : IUIElement
    {
        [HideInInspector] public Vector3 realPos;
        [HideInInspector] public Vector3 initPos { get; set; }
        [HideInInspector] public Vector3 initScale { get; set; }
        [HideInInspector] public float initFade { get; set; }
        [HideInInspector] public Transform initParent;
        [Range(0, 5)] public float Delay;
        public bool ignoreTimeScale;
        public GameObject UIObj; 
        GameObject IUIElement.UIObj => UIObj;

        [EnumToggleButtons] public UIAnimationData UIAnimationData;
    }

    [System.Serializable]
    public class UIContentElement
    {
        public List<UIContinousEffectdata> ConEffects;
    }

    [System.Serializable]
    public class UIAnimationGroup
    {
        public UIAnimationElement[] Elements;
        public List<IUIAnimation> animations = new List<IUIAnimation>();
    }

    [System.Serializable]
    public class UIContentGroup
    {
        public UIContentElement[] Elements;
    }
}