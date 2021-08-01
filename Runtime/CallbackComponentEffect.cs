namespace Umph.Core
{
    [System.Serializable]
    public class CallbackComponentEffect : UmphComponentEffect
    {
        public UnityEngine.Events.UnityEvent Callback = new UnityEngine.Events.UnityEvent();

        public override IEffect ConstructEffect()
        {
            return new CallbackEffect(Callback);
        }
        private class CallbackEffect : IEffect
        {
            private readonly UnityEngine.Events.UnityEvent _callback = new UnityEngine.Events.UnityEvent();

            public float Duration => 0f;

            public bool RequiresUpdates => false;

            public bool IsCompleted { get; private set; }

            public CallbackEffect(UnityEngine.Events.UnityEvent callback)
            {
                _callback = callback;
            }

            public void Play()
            {
                _callback.Invoke();
                IsCompleted = true;
            }

            public void Reset()
            {
                IsCompleted = false;
            }

            public void Skip()
            {
                Play();
            }

            public void Update(float deltaTime)
            {
            }
        }
    }
}
