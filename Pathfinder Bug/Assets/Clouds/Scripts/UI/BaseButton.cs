using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Clouds.SignalSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using Clouds.UI.Animation;
using Clouds.Ultilities;
using System;
using Clouds.UI.Settings;

public abstract class BaseButton : MyBehaviour, ISignalSender
{
    private static IUIAnimationFactory _factory;
    public static IUIAnimationFactory AnimationFactory
    {
        get
        {
            if (_factory == null) _factory = UISetting.Instance.GetFactory();
            return _factory;
        }
    }
    [SerializeField] protected Button thisbutton;
    [SerializeField] protected bool Permission = true;
    [ToggleGroup("UseOnPointerClick")] [SerializeField] private bool UseOnPointerClick;
    [ToggleGroup("UseOnPointerClick")] public UIAnimationData OnPointerClick_AnimationBehavior;
    [ToggleGroup("UseOnPointerClick")] public Action OnPointerClickCallBack_Start;
    [ToggleGroup("UseOnPointerClick")] public Action OnPointerClickCallBack_Completed;
    protected List<IUIAnimation> animations = new List<IUIAnimation>();
    [Header("Signals to Emit (Templates)")]
    [HideInInlineEditors] public List<SignalMessage.SignalType> signalsToEmit = new List<SignalMessage.SignalType >();

    [Header("Targeting (Direct Link)")]
    [HideInInlineEditors] public List<baseUI> targetReceivers = new List<baseUI>();

    // Implement ISignalSender
    public List<SignalMessage.SignalType> SignalsToEmit => signalsToEmit;
    public List<ISignalReceiver> Receivers => targetReceivers.ConvertAll(x => (ISignalReceiver)x);

    protected UniqueIDComponent _myID;

    public void setPermission(bool trigger) => this.Permission = trigger;

    protected override void Awake()
    {
        base.Awake();
        _myID = GetComponent<UniqueIDComponent>();
        if (thisbutton == null) thisbutton = GetComponent<Button>();
        this.CreateUIAnimations();
        thisbutton?.onClick.AddListener(OnButtonClicked);
    }

    public void SendSignal(SignalMessage message)
    {
        UISignalBus.Emit(message);
    }

    protected virtual void OnButtonClicked()
    {
        if (!CanAct()) return;
        if(animations.Count <=0) this.SendSignal();
        foreach (var anim in animations)
        {
            if (anim == null) continue;
            anim.Restart();
        }
    }
    protected void SendSignal()
    {
        int sID = _myID != null ? _myID.UniqueID : -1;

        foreach (var template in SignalsToEmit)
        {
            SignalMessage msg = SignalMessage.Create(template, null, sID);
            var targetList = Receivers;
            if (targetList == null || targetList.Count == 0)
            {
                UISignalBus.Emit(msg);
            }
            else
            {
                foreach (var receiver in targetList)
                {
                    if (receiver == null) continue;
                    msg.TargetID = receiver.ID;
                    UISignalBus.Emit(msg);
                }
            }
        }        
    }
    protected void OnEnable()
    {   
        this.Permission = true;
    } 

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.thisbutton = GetComponent<Button>();
    }
    protected virtual bool CanAct() => Permission;
    public void CreateUIAnimations()
    {
        if (animations != null && animations.Count > 0) return;
        animations = new List<IUIAnimation>();
        RectTransform rt = this.GetComponent<RectTransform>();
        CanvasGroup cg = this.GetComponent<CanvasGroup>();
        foreach (UIEffectData effect in OnPointerClick_AnimationBehavior.Effects)
        {
            IUIAnimation anim = null;
            switch (effect.type)
            {
                case TRIGGEREFFECT.Move:
                    anim = AnimationFactory.CreateMove(rt, effect);
                    break;
                case TRIGGEREFFECT.Rotate:
                    anim = AnimationFactory.CreateRotate(rt, effect);
                    break;
                case TRIGGEREFFECT.Scale:
                    anim = AnimationFactory.CreateScale(rt, effect);
                    break;
                case TRIGGEREFFECT.Shake:
                    anim = AnimationFactory.CreateShake(rt, effect);
                    break;
                case TRIGGEREFFECT.Punch:
                    anim = AnimationFactory.CreatePunch(rt, effect);
                    break;
                case TRIGGEREFFECT.Fade:
                    if (cg != null) anim = AnimationFactory.CreateFade(cg, effect);
                    break;
            }
            if (anim != null) animations.Add(anim);
        }
        animations[0].OnStart += OnPointerClickCallBack_Start;
        if(animations.Count > 0) animations[animations.Count -1].OnComplete += ()=>
        {
            this.SendSignal();
            OnPointerClickCallBack_Completed?.Invoke();
        };
    }
}