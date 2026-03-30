using UnityEngine;
using System;
using AssetKits.ParticleImage;
using Clouds.UI.Animation;

namespace UiEffectSystem
{
    [Serializable]
    public class ParticleImageHandler : IUIAnimation
    {
        [SerializeField] private string _id;
        [SerializeField] private ParticleImage _particleImage;

        public string UniqueId => _id;
        public bool IsPlaying => _isplaying;
        public object NativeAnimation => _particleImage;
        protected bool _isplaying;
        [Header("Defaults")]
        [SerializeField] private Transform _defaultAttractor;

        public event Action OnComplete;
        public event Action OnStart;

        public void Setup(Transform attractor = null)
        {
            if (_particleImage == null) return;
            
            // Assign dynamic reference if available, otherwise use default from Inspector
            if (attractor != null || _defaultAttractor != null)
            {
                _particleImage.attractorTarget = attractor ?? _defaultAttractor;
            }
        }

        public void Play()
        {
            if (_particleImage != null) _particleImage.Play();
        }

        public void Stop()
        {
            if (_particleImage != null) _particleImage.Stop();
        }

        public void Reset()
        {
            if (_particleImage != null) _particleImage.Clear();
        }

        public void Restart()
        {
            throw new NotImplementedException();
        }

    }
}