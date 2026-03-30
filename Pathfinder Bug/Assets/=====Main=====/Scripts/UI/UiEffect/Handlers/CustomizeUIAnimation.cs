using System;
using System.Collections.Generic;
using Clouds.UI.Animation;
using Clouds.UI.Settings;
using Clouds.Ultilities;
using UnityEngine;

public class CustomizeUIAnimation : MonoBehaviour, IUIAnimation
{
    public bool IsPlaying => _isplaying;
    public object NativeAnimation => animations;
    public string UniqueId => transform.name;
    public event Action OnComplete;
    public event Action OnStart;

    List<IUIAnimation> animations = new List<IUIAnimation>();
    public List<UIAnimationElement> Elements;
    [SerializeField] protected bool ignoreTimeScale;
    protected bool _isplaying;
    public void CreateUIAnimation()
    {
        foreach (UIAnimationElement data in Elements)
        {
            if (data.UIObj == null) continue;
            RectTransform rt = data.UIObj.GetComponent<RectTransform>();
            CanvasGroup cg = data.UIObj.GetComponent<CanvasGroup>();

            foreach (UIEffectData effect in data.UIAnimationData.Effects)
            {
                IUIAnimation anim = null;
                switch (effect.type)
                {
                    case TRIGGEREFFECT.Move:
                        anim = UISetting.Instance.GetFactory().CreateMove(rt, effect, data, ignoreTimeScale);
                        break;
                    case TRIGGEREFFECT.Rotate:
                        anim = UISetting.Instance.GetFactory().CreateRotate(rt, effect, data, ignoreTimeScale);
                        break;
                    case TRIGGEREFFECT.Scale:
                        anim = UISetting.Instance.GetFactory().CreateScale(rt, effect, data, ignoreTimeScale);
                        break;
                    case TRIGGEREFFECT.Shake:
                        anim = UISetting.Instance.GetFactory().CreateShake(rt, effect, data, ignoreTimeScale);
                        break;
                    case TRIGGEREFFECT.Punch:
                        anim = UISetting.Instance.GetFactory().CreatePunch(rt, effect, data, ignoreTimeScale);
                        break;
                    case TRIGGEREFFECT.Fade:
                        if (cg != null) anim = UISetting.Instance.GetFactory().CreateFade(cg, effect, data, ignoreTimeScale);
                        break;
                }
                if (anim != null) animations.Add(anim);
            }
        }
    }
    protected void Awake()
    {
        this.CreateUIAnimation();
    }
    public void Play()
    {
        foreach(var ele in animations) 
        {
            ele.Play();
        }
    }

    public void Restart()
    {
        foreach(var ele in animations) ele.Restart();
    }

    public void Stop()
    {
        foreach(var ele in animations) ele.Stop();
    }
}
