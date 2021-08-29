namespace Umph.Core
{
    [System.Serializable]
    [UmphComponentMenu("Nested Component", "Core/Nested Component")]
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
