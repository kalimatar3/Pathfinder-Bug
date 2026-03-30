using UnityEngine;
using Clouds.UI.Animation;
using Clouds.Ultilities;
using DG.Tweening;
using System;

namespace Clouds.UI.Animation
{
    public class DOTweenUIAnimation : IUIAnimation
    {
        private Tween _tween;
        public event Action OnComplete;
        public event Action OnStart;

        public DOTweenUIAnimation(Tween tween)
        {
            _tween = tween;
            if (_tween != null) 
            {
                _tween.OnComplete(() => OnComplete?.Invoke());
                _tween.OnStart(() => OnStart?.Invoke());
            }
        }

        public void Play() => _tween?.Play();
        public void Stop() => _tween?.Kill();
        public void Restart() => _tween?.Restart();
        public bool IsPlaying => _tween != null && _tween.IsPlaying();
        public object NativeAnimation => _tween; // Returns the actual Tween for the Editor
    }

    public class DOTweenAnimationFactory : IUIAnimationFactory
    {
        // Helper function to map Ease
        private DG.Tweening.Ease MapEase(Clouds.UI.Animation.Ease ease)
        {
            return (DG.Tweening.Ease)Enum.Parse(typeof(DG.Tweening.Ease), ease.ToString());
        }
        // Helper function to map LoopType
        private DG.Tweening.LoopType MapLoop(Clouds.UI.Animation.LoopType loopType) {
            return (DG.Tweening.LoopType)Enum.Parse(typeof(DG.Tweening.LoopType),loopType.ToString());
        }
        public IUIAnimation CreateMove(RectTransform rect, UIEffectData effect, IUIElement data, bool ignoreTimeScale)
        {
            Tween tween = null;
            Vector2 targetPos = new Vector2(data.initPos.x, data.initPos.y);
            DG.Tweening.Ease dotweenEase = MapEase(effect.easeMove);

            switch (effect.moveType)
            {
                case MOVEEFFECT.Custom:
                    Vector2 startPos = new Vector2(effect.moveFrom.localPosition.x, effect.moveFrom.localPosition.y);
                    Sequence seq = DOTween.Sequence()
                        .SetUpdate(ignoreTimeScale)
                        .SetId(1)
                        .SetAutoKill(false)
                        .Pause()
                        .SetLink(rect.gameObject, LinkBehaviour.KillOnDestroy); // Added SetLink
                    seq.AppendInterval(effect.Delay);
                    seq.AppendCallback(() => rect.localPosition = startPos);
                    seq.Append(rect.DOLocalMove(data.initPos, effect.timeMove).SetEase(dotweenEase).SetLoops(effect.loopMove ? effect.loopCircleMove : 0));
                    return new DOTweenUIAnimation(seq);

                case MOVEEFFECT.FromAbove:
                    rect.localPosition = new Vector2(rect.localPosition.x, (Screen.height / 2) + (rect.sizeDelta.y / 2) + 100);
                    tween = rect.DOLocalMove(targetPos, effect.timeMove).SetEase(dotweenEase);
                    break;
                case MOVEEFFECT.FromBelow:
                    rect.localPosition = new Vector2(rect.localPosition.x, -((Screen.height / 2) + (rect.sizeDelta.y / 2) + 100));
                    tween = rect.DOLocalMove(targetPos, effect.timeMove).SetEase(dotweenEase);
                    break;
                case MOVEEFFECT.FromLeft:
                    rect.localPosition = new Vector2(-((Screen.width / 2) + (rect.sizeDelta.x / 2) + 100), rect.localPosition.y);
                    tween = rect.DOLocalMove(targetPos, effect.timeMove).SetEase(dotweenEase);
                    break;
                case MOVEEFFECT.FromRight:
                    rect.localPosition = new Vector2((Screen.width / 2) + (rect.sizeDelta.x / 2) + 100, rect.localPosition.y);
                    tween = rect.DOLocalMove(targetPos, effect.timeMove).SetEase(dotweenEase);
                    break;
            }
            
            if (tween != null)
            {
                if (effect.loopMove) tween.SetLoops(effect.loopCircleMove);
                tween.SetId(1)
                     .SetUpdate(ignoreTimeScale)
                     .SetDelay(effect.Delay)
                     .SetAutoKill(false)
                     .Pause()
                     .SetLink(rect.gameObject, LinkBehaviour.KillOnDestroy); // Added SetLink
            }
            return new DOTweenUIAnimation(tween);
        }

        public IUIAnimation CreateRotate(RectTransform rect, UIEffectData effect, IUIElement data, bool ignoreTimeScale)
        {
            var tween = rect.DOLocalRotate(effect.rotateTo, effect.timeRotate, RotateMode.FastBeyond360)
                .SetEase(MapEase(effect.easeRotate))
                .SetId(1)
                .SetUpdate(ignoreTimeScale)
                .SetDelay(effect.Delay)
                .SetAutoKill(false)
                .Pause()
                .SetLink(rect.gameObject, LinkBehaviour.KillOnDestroy); // Added SetLink
            if (effect.loopRotate) tween.SetLoops(effect.loopCircleRotate);
            return new DOTweenUIAnimation(tween);
        }

        public IUIAnimation CreateScale(RectTransform rect, UIEffectData effect, IUIElement data, bool ignoreTimeScale)
        {
           var tween = rect.DOScale(effect.scaleTo, effect.timeScale)
            .SetLoops(effect.loopCircleScale,MapLoop(effect.ScaleLoopType))
            .SetEase(MapEase(effect.easeScale))
            .SetId(1)
            .SetUpdate(ignoreTimeScale)
            .SetDelay(effect.Delay)
            .SetAutoKill(false)
            .Pause()
            .SetLink(rect.gameObject, LinkBehaviour.KillOnDestroy);
            return new DOTweenUIAnimation(tween);
        }

        public IUIAnimation CreateShake(RectTransform rect, UIEffectData effect, IUIElement data, bool ignoreTimeScale)
        {
            Tween tween = null;
            if (effect.shakePosition) tween = rect.DOShakePosition(effect.timeShake, effect.shakeStrength, effect.shakeVibrate, effect.shakeRandomness);
            else if (effect.shakeRotation) tween = rect.DOShakeRotation(effect.timeShake, effect.shakeStrength, effect.shakeVibrate, effect.shakeRandomness);
            else if (effect.shakeScale) tween = rect.DOShakeScale(effect.timeShake, effect.shakeStrength, effect.shakeVibrate, effect.shakeRandomness);
            
            if (tween != null)
            {
                tween.SetEase(MapEase(effect.easeShake))
                     .SetId(1)
                     .SetUpdate(ignoreTimeScale)
                     .SetDelay(effect.Delay)
                     .SetAutoKill(false)
                     .Pause()
                     .SetLink(rect.gameObject, LinkBehaviour.KillOnDestroy); // Added SetLink
                if (effect.loopShake) tween.SetLoops(effect.loopCircleShake);
            }
            return new DOTweenUIAnimation(tween);
        }

        public IUIAnimation CreatePunch(RectTransform rect, UIEffectData effect, IUIElement data, bool ignoreTimeScale)
        {
            Tween tween = null;
            if (effect.punchPostion) tween = rect.DOPunchPosition(effect.punchTo, effect.timePunch, effect.punchVibrate, effect.punchElasticity);
            else if (effect.punchRotation) tween = rect.DOPunchRotation(effect.punchTo, effect.timePunch, effect.punchVibrate, effect.punchElasticity);
            else if (effect.punchScale) tween = rect.DOPunchScale(effect.punchTo, effect.timePunch, effect.punchVibrate, effect.punchElasticity);

            if (tween != null)
            {
                tween.SetEase(MapEase(effect.easePunch))
                     .SetId(1)
                     .SetUpdate(ignoreTimeScale)
                     .SetDelay(effect.Delay)
                     .SetAutoKill(false)
                     .Pause()
                     .SetLink(rect.gameObject, LinkBehaviour.KillOnDestroy); // Added SetLink
                if (effect.loopPunch) tween.SetLoops(effect.loopCirclePunch);
            }
            return new DOTweenUIAnimation(tween);
        }

        public IUIAnimation CreateFade(CanvasGroup canvas, UIEffectData effect, IUIElement data, bool ignoreTimeScale)
        {
            var tween = canvas.DOFade(effect.fadeTo, effect.timeFade)
                .SetEase(MapEase(effect.easeFade))
                .SetId(1)
                .SetUpdate(ignoreTimeScale)
                .SetDelay(effect.Delay)
                .SetAutoKill(false)
                .Pause()
                .SetLink(canvas.gameObject, LinkBehaviour.KillOnDestroy); // Added SetLink, linking to canvas.gameObject
            if (effect.loopFade) tween.SetLoops(effect.loopCircleFade);
            return new DOTweenUIAnimation(tween);
        }
    }
}