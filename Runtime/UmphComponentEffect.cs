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

        public BasicEffectSettings Settings;
        public float Delay => Settings.Delay;
        public bool IsParallel => Settings.IsParallel;

#if UNITY_EDITOR
        public virtual void EDITOR_Initialize(UnityEngine.GameObject ownerObject) { }
#endif

        public abstract IEffect ConstructEffect();

        public UmphComponentEffect Clone() => (UmphComponentEffect)MemberwiseClone();
    }
}
