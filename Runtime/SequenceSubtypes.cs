using System.Collections.Generic;

namespace Umph.Core
{
    public partial class Sequence
    {
        // TODO :: pool me
        private static List<ISequenceEffectWrapper> GetEffectWrapperList()
        {
            return new List<ISequenceEffectWrapper>(6);
        }

        private interface ISequenceEffectWrapper
        {
            bool IsCompleted { get; }
            bool IsPlaying { get; }
            float Duration { get; }

            void Play();

            void Pause();

            bool Update(float deltaTime);

            void Skip();

            void Reset();
        }

        private class SequenceEffectBatchWrapper : ISequenceEffectWrapper
        {
            public List<ISequenceEffectWrapper> Effects;

            public bool IsCompleted
            {
                get
                {
                    var isComplete = true;
                    foreach (var subeffect in Effects)
                    {
                        isComplete = isComplete && subeffect.IsCompleted;
                    }
                    return isComplete;
                }
            }

            public bool IsPlaying
            {
                get; private set;
            }

            public float Duration
            {
                get
                {
                    float d = 0f;
                    foreach (var subeffect in Effects)
                    {
                        d += subeffect.Duration;
                    }
                    return d;
                }
            }

            public void Play()
            {
                IsPlaying = true;
                foreach (var subeffect in Effects)
                {
                    subeffect.Play();
                }
            }

            public void Pause()
            {
                IsPlaying = false;
                foreach (var subeffect in Effects)
                {
                    subeffect.Pause();
                }
            }

            public bool Update(float deltaTime)
            {
                var isComplete = true;
                foreach (var subeffect in Effects)
                {
                    var subEffectIsComplete = subeffect.Update(deltaTime);
                    isComplete = isComplete && subEffectIsComplete;
                }
                return isComplete;
            }

            public void Skip()
            {
                IsPlaying = false;
                foreach (var subeffect in Effects)
                {
                    subeffect.Skip();
                }
            }

            public void Reset()
            {
                foreach (var subeffect in Effects)
                {
                    subeffect.Reset();
                }
            }
        }

        private class SequenceEffectWrapper : ISequenceEffectWrapper
        {
            public float Delay;
            public float DelayRemaining;

            public IEffect Effect;

            public bool IsCompleted
            {
                get
                {
                    return Effect.IsCompleted;
                }
            }

            public bool IsPlaying
            {
                get; private set;
            }

            public float Duration
            {
                get
                {
                    return Effect.Duration;
                }
            }

            public void Play()
            {
                IsPlaying = true;
                if (DelayRemaining <= 0f)
                {
                    Effect.Play();
                }
            }

            public void Pause()
            {
                IsPlaying = false;
                Effect.Pause();
            }

            public bool Update(float deltaTime)
            {
                if (Effect.IsCompleted) return true;

                if (DelayRemaining > 0f)
                {
                    DelayRemaining -= deltaTime;

                    if (DelayRemaining <= 0f)
                        Effect.Play();
                }
                else if (Effect.RequiresUpdates)
                {
                    Effect.Update(deltaTime);
                }

                return Effect.IsCompleted;
            }

            public void Skip()
            {
                IsPlaying = false;
                if (!Effect.IsCompleted)
                {
                    DelayRemaining = 0f;
                    Effect.Skip();
                }
            }

            public void Reset()
            {
                Effect.Reset();
                DelayRemaining = Delay;
            }
        }
    }
}
