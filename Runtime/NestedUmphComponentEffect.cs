namespace Umph.Core
{
    [System.Serializable]
    public class NestedUmphComponentEffect : UmphComponentEffect
    {
        public UmphComponent Nested;

        public override IEffect ConstructEffect()
        {
            Nested.Initialize();
            return Nested.Sequence;
        }
    }
}
