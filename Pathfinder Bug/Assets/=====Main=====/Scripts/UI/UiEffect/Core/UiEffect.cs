using Sirenix.OdinInspector;
using UnityEngine;

namespace UiEffectSystem
{
    public abstract class UIEffect : MonoBehaviour
    {
        public abstract void Execute();
        public abstract void Cancel();
        #if UNITY_EDITOR
        [Sirenix.OdinInspector.Button(ButtonSizes.Large)]
        [Sirenix.OdinInspector.GUIColor(0, 1, 0)]
        public void EXCUTE()
        {
            this.Execute();
        }
        [Sirenix.OdinInspector.Button(ButtonSizes.Large)]
        [Sirenix.OdinInspector.GUIColor(0, 1, 0)]
        public void DISPOSE()
        {
            this.Cancel();
        }
#endif

    }
}