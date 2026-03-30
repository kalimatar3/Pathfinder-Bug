using System.Collections;
using System.Collections.Generic;
using Clouds.Ultilities;
using Clouds.UI.Animation;
using Clouds.UI.Settings;
using UnityEngine;
using Clouds.SignalSystem;
[RequireComponent(typeof(UniqueIDComponent))]
public abstract class BasePopup : baseUI
{
    [Header("Groups Behavior")]
    public SerializableDictionary<SignalMessage.SignalType, UIAnimationGroup> AnimationGroupDics = new SerializableDictionary<SignalMessage.SignalType, UIAnimationGroup>();
    public SerializableDictionary<SignalMessage.SignalType, UIContentGroup> ContentUpdateGroupDics = new SerializableDictionary<SignalMessage.SignalType, UIContentGroup>();

    protected override void Awake()
    {
        base.Awake();
        foreach (var ele in AnimationGroupDics)
        {
            this.CreateUIAnimations(ele.Key);
        }
    }
    public virtual void ShowPopup()
    {
        SignalMessage signal = SignalMessage.Create(SignalMessage.SignalType.OnShow);
        PlayAnimations(signal);
    }
    public virtual void ClosePopup()
    {
        SignalMessage signal = SignalMessage.Create(SignalMessage.SignalType.OnClose);
        PlayAnimations(signal);        
    }
    public void PlayAnimations(SignalMessage message)
    {
        if (message.Type == SignalMessage.SignalType.None) return;
        if (AnimationGroupDics == null || !AnimationGroupDics.ContainsKey(message.Type)) return;
        
        UIAnimationGroup group = AnimationGroupDics[message.Type];
        if (group.animations == null) return;

        foreach (var anim in group.animations)
        {
            if (anim == null) continue;
            anim.Restart();
        }
    }
    public void UpdateContents(SignalMessage message)
    {
        if (message.Type == SignalMessage.SignalType.None) return;
        if (ContentUpdateGroupDics == null || !ContentUpdateGroupDics.ContainsKey(message.Type)) return;
        
        UIContentGroup group = ContentUpdateGroupDics[message.Type];
        if (group.Elements == null) return;

        foreach (var element in group.Elements)
        {
            if (element.ConEffects == null) continue;
            
                        foreach (var effect in element.ConEffects)
            {
                switch (effect.type)
                {
                    case CONTINUOSEFFECT.FillText:
                        if (message.Value is string textValue)
                            UIUtility.FillText(effect, textValue);
                        break;
                    case CONTINUOSEFFECT.ChangeOpacity:
                        if (message.Value is float fValue)
                            UIUtility.ChangeOpacity(effect, fValue);
                        else if (message.Value is int iValue)
                            UIUtility.ChangeOpacity(effect, (float)iValue);
                        break;
                }
            }
        }
    }

    public override void UpdateVirtual(SignalMessage message)
    {
        if (message.Type == SignalMessage.SignalType.None) return;
        this.PlayAnimations(message);
        this.UpdateContents(message);
    }



    public void CreateUIAnimations(SignalMessage.SignalType signalType, bool ignoreTimeScale = false)
    {
        if (!AnimationGroupDics.ContainsKey(signalType)) return;

        UIAnimationGroup group = AnimationGroupDics[signalType];
        
        // Tranh trung lap khi goi nhieu lan
        if (group.animations != null && group.animations.Count > 0) return;

        group.animations = new List<IUIAnimation>();

        foreach (UIAnimationElement data in group.Elements)
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
                        anim = AnimationFactory.CreateMove(rt, effect, data, ignoreTimeScale);
                        break;
                    case TRIGGEREFFECT.Rotate:
                        anim = AnimationFactory.CreateRotate(rt, effect, data, ignoreTimeScale);
                        break;
                    case TRIGGEREFFECT.Scale:
                        anim = AnimationFactory.CreateScale(rt, effect, data, ignoreTimeScale);
                        break;
                    case TRIGGEREFFECT.Shake:
                        anim = AnimationFactory.CreateShake(rt, effect, data, ignoreTimeScale);
                        break;
                    case TRIGGEREFFECT.Punch:
                        anim = AnimationFactory.CreatePunch(rt, effect, data, ignoreTimeScale);
                        break;
                    case TRIGGEREFFECT.Fade:
                        if (cg != null) anim = AnimationFactory.CreateFade(cg, effect, data, ignoreTimeScale);
                        break;
                }

                if (anim != null) group.animations.Add(anim);
            }
        }
    }
}