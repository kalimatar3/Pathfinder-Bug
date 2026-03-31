using UnityEngine;
using Clouds.Ultilities;
using System;

namespace Clouds.UI.Animation
{
    public enum Ease
    {
        Linear,
        InSine, OutSine, InOutSine,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InQuart, OutQuart, InOutQuart,
        InQuint, OutQuint, InOutQuint,
        InExpo, OutExpo, InOutExpo,
        InCirc, OutCirc, InOutCirc,
        InElastic, OutElastic, InOutElastic,
        InBack, OutBack, InOutBack,
        InBounce, OutBounce, InOutBounce
    }
    public enum LoopType
    {
        Restart,
        Yoyo
    }
    public interface IUIAnimation
    {
        void Play();
        void Stop();
        void Restart();
        bool IsPlaying { get; }
        object NativeAnimation { get; }
        event Action OnComplete;
        event Action OnStart;
    }

    public interface IUIAnimationFactory
    {
        IUIAnimation CreateMove(RectTransform rect, UIEffectData effect, IUIElement data = null, bool ignoreTimeScale = false);
        IUIAnimation CreateRotate(RectTransform rect, UIEffectData effect, IUIElement data= null, bool ignoreTimeScale = false);
        IUIAnimation CreateScale(RectTransform rect, UIEffectData effect, IUIElement data= null, bool ignoreTimeScale=  false);
        IUIAnimation CreateShake(RectTransform rect, UIEffectData effect, IUIElement data = null, bool ignoreTimeScale = false);
        IUIAnimation CreatePunch(RectTransform rect, UIEffectData effect, IUIElement data = null, bool ignoreTimeScale = false);
        IUIAnimation CreateFade(CanvasGroup canvas, UIEffectData effect, IUIElement data = null, bool ignoreTimeScale = false);
    }
}