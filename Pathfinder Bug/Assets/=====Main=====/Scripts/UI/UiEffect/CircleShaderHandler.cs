using System;
using Clouds.UI.Animation;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CircleShaderHandler : MonoBehaviour, IUIAnimation
{
    public bool IsPlaying => Isplaying;
    public object NativeAnimation => seq;
    public event Action OnComplete;
    public event Action OnStart;
    [SerializeField] protected bool Isplaying;
    [SerializeField] protected Image Image;
     Sequence seq;
    [Button(ButtonSizes.Large)]
    public void Play()
    {
        Image.gameObject.SetActive(true);
        Isplaying = true;
        Material material = Image.material;
        seq?.Kill(); 
        seq = DOTween.Sequence();
        seq.OnStart(() =>
        {
            OnStart?.Invoke();
        });

        seq.Append(DOTween.To(() => 0f,
            x => material.SetFloat("_RevealProgress", x),
            2f, .6f).SetEase(DG.Tweening.Ease.OutQuad));
        seq.OnComplete(() =>
        {
            Isplaying = false;
            OnComplete?.Invoke();
        });
    }
    public void Restart()
    {
        Material material = Image.material;
        seq?.Kill();
        Isplaying = true;        
        seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => material.GetFloat("_RevealProgress"),
            x => material.SetFloat("_RevealProgress", x),
            0f, .6f).SetEase(DG.Tweening.Ease.OutQuad));
        seq.OnComplete(() =>
        {
            Isplaying = false;
            Image.gameObject.SetActive(false);
        });
    }

    public void Stop()
    {
        seq?.Kill();
        Isplaying = false;
    }
}
