namespace Umph.Core
{
    [System.Serializable]
    public abstract class UmphComponentEffect
    {
        public float Delay;
        public bool IsParallel;

#if UNITY_EDITOR
        public virtual void EDITOR_Initialize(UnityEngine.GameObject ownerObject) { }
#endif

        public abstract IEffect ConstructEffect();
    }
}
