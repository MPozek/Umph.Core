using UnityEngine;

namespace Umph.Core
{
    [System.Serializable]
    public abstract class UmphComponentEffect
    {
        [System.Serializable]
        public struct BasicEffectSettings
        {
            public float Delay;
            public bool IsParallel;
        }


#if UNITY_EDITOR
        [SerializeField] private string _name;
        public virtual void EDITOR_Initialize(UnityEngine.GameObject ownerObject, string name) { _name = name; }
#endif

        public BasicEffectSettings Settings;
        public float Delay => Settings.Delay;
        public bool IsParallel => Settings.IsParallel;

        public abstract IEffect ConstructEffect();

        public UmphComponentEffect Clone() => (UmphComponentEffect)MemberwiseClone();
    }
}
