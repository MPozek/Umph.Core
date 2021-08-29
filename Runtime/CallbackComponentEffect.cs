namespace Umph.Core
{
    [System.Serializable]
    [UmphComponentMenu("Callback", "Core/Callback")]
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
            public bool IsPlaying { get; private set; }

            public CallbackEffect(UnityEngine.Events.UnityEvent callback)
            {
                _callback = callback;
            }

            public void Play()
            {
                if (!IsPlaying)
                {
                    IsPlaying = true;
                    _callback.Invoke();
                    IsPlaying = false;
                    IsCompleted = true;
                }
            }

            public void Pause()
            {
                IsPlaying = false;
            }

            public void Reset()
            {
                IsCompleted = false;
                IsPlaying = false;
            }

            public void Skip()
            {
                Play();
                IsPlaying = false;
            }

            public void Update(float deltaTime)
            {
            }
        }
    }
}
