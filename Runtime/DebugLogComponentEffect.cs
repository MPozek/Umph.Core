namespace Umph.Core
{
    [System.Serializable]
    public class DebugLogComponentEffect : UmphComponentEffect
    {
        public string Message;

        public override IEffect ConstructEffect()
        {
            return new DebugLogEffect(Message);
        }
        private class DebugLogEffect : IEffect
        {
            private readonly string _message;

            public float Duration => 0f;

            public bool RequiresUpdates => false;

            public bool IsCompleted { get; private set; }

            public DebugLogEffect() : this("") { }

            public DebugLogEffect(string message)
            {
                this._message = message;
            }

            public void Play()
            {
                UnityEngine.Debug.Log(_message);
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
