using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Clouds.UI.Animation;

namespace UiEffectSystem
{
    public class ParticleEffects : UIEffect
    {
        [Header("Handlers")]
        [SerializeField] private List<ParticleImageHandler> _imageHandlers = new List<ParticleImageHandler>();
        [SerializeField] private List<ParticleSystemHandler> _systemHandlers = new List<ParticleSystemHandler>();
        [SerializeField] private List<InterfaceReference<IUIAnimation>> CustomizeUIAnimation  = new List<InterfaceReference<IUIAnimation>>();
        public override void Execute()
        {
            foreach (var h in _imageHandlers) h.Play();
            foreach (var h in _systemHandlers) h.Play();
            foreach(var ele in CustomizeUIAnimation) ele.Value.Play();
        }

        public override void Cancel()
        {
            foreach (var h in _imageHandlers) h.Stop();
            foreach (var h in _systemHandlers) h.Stop();
            foreach(var ele in CustomizeUIAnimation) ele.Value.Stop();
        }

        public void ResetAll()
        {
            foreach (var h in _imageHandlers) h.Reset();
            foreach (var h in _systemHandlers) h.Reset();
            foreach(var ele in CustomizeUIAnimation) ele.Value.Restart();
        }

        public void SetupImage(string id, Transform attractor = null)
        {
            var handler = _imageHandlers.FirstOrDefault(h => h.UniqueId == id);
            if (handler != null)
            {
                handler.Setup(attractor);
            }
            else
            {
                Debug.LogWarning($"[CompositeUiEffect] ParticleImageHandler with ID '{id}' not found on {gameObject.name}");
            }
        }

        public void SetupSystem(string id, Color? color = null)
        {
            var handler = _systemHandlers.FirstOrDefault(h => h.UniqueId == id);
            if (handler != null)
            {
                handler.Setup(color);
            }
            else
            {
                Debug.LogWarning($"[CompositeUiEffect] ParticleSystemHandler with ID '{id}' not found on {gameObject.name}");
            }
        }
        public void SetupAllImages(Transform attractor)
        {
            foreach (var h in _imageHandlers)
            {
                h.Setup(attractor);
            }
        }
    }
}
