using System.Collections.Generic;
using UnityEngine;
using Clouds.SignalSystem;
using Sirenix.OdinInspector;
using Clouds.UI.Animation;
using Clouds.UI.Settings;

[RequireComponent(typeof(UniqueIDComponent))]
public abstract class baseUI : MyBehaviour, ISignalReceiver
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

    [HideInInspector] public UniqueIDComponent idComponent;
    public int ID => idComponent != null ? idComponent.UniqueID : -1;
    public abstract void UpdateVirtual(SignalMessage message);

    public virtual void OnSignalReceived(SignalMessage message)
    {
        if (message.TargetID != -1 && message.TargetID != this.ID) return;  
        UpdateVirtual(message);
    }

    protected override void Awake()
    {
        base.Awake();
        if (idComponent == null) idComponent = GetComponent<UniqueIDComponent>();
    }

    protected virtual void OnEnable()
    {
        UISignalBus.Subscribe(this);
    }

    protected virtual void OnDisable()
    {
        UISignalBus.Unsubscribe(this);
    }

    protected virtual void OnDestroy()
    {
        UISignalBus.Unsubscribe(this);
    }
}