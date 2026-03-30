using UnityEngine;
using System;
using Clouds.UI.Animation;

namespace UiEffectSystem
{
    [Serializable]
    public class ParticleSystemHandler : IUIAnimation
    {
        [SerializeField] private ParticleSystem _particleSystem;
        public bool IsPlaying => throw new NotImplementedException();

        public object NativeAnimation => this;

        public string UniqueId => id;
        [SerializeField] protected string id; 

        [Header("Defaults")]
        [SerializeField] private Color _defaultColor = Color.white;

        public event Action OnComplete;
        public event Action OnStart;

        public void Setup(Color? color = null)
        {
            if (_particleSystem == null) return;
            var main = _particleSystem.main;
            main.startColor = color ?? _defaultColor;
        }

        public void Play()
        {
            if (_particleSystem != null) _particleSystem.Play();
        }

        public void Stop()
        {
            if (_particleSystem != null) _particleSystem.Stop();
        }

        public void Reset()
        {
            if (_particleSystem != null) _particleSystem.Clear();
        }

        public void Restart()
        {
            throw new NotImplementedException();
        }

    }
}