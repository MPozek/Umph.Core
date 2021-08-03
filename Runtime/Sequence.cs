using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Umph.Core
{
    public class Sequence : IEffect
    {
        private interface ISequenceEffectWrapper
        {
            bool IsCompleted { get; }
            float Duration { get; }

            void Play();

            bool Update(float deltaTime);

            void Skip();

            void Reset();
        }

        private class SequenceEffectBatchWrapper : ISequenceEffectWrapper
        {
            public List<SequenceEffectWrapper> Effects;

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

            public float Duration
            {
                get
                {
                    float d = 0f;
                    foreach (var subeffect in Effects)
                    {
                        d += subeffect.Effect.Duration + subeffect.Delay;
                    }
                    return d;
                }
            }

            public void Play()
            {
                foreach (var subeffect in Effects)
                {
                    subeffect.Play();
                }
            }

            public bool Update(float deltaTime)
            {
                var isComplete = true;
                foreach (var subeffect in Effects)
                {
                    isComplete = isComplete && subeffect.Update(deltaTime);
                }
                return isComplete;
            }

            public void Skip()
            {
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

            public float Duration
            {
                get
                {
                    return Effect.Duration;
                }
            }

            public void Play()
            {
                if (DelayRemaining <= 0f)
                {
                    Effect.Play();
                }
            }

            public bool Update(float deltaTime)
            {
                if (Effect.IsCompleted) return true;

                if (DelayRemaining > 0f)
                {
                    DelayRemaining -= deltaTime;
                }
                else if (Effect.RequiresUpdates)
                {
                    Effect.Update(deltaTime);
                }

                return Effect.IsCompleted;
            }

            public void Skip()
            {
                DelayRemaining = 0f;
                Effect.Skip();   
            }

            public void Reset()
            {
                Effect.Reset();
                DelayRemaining = Delay;
            }
        }

        private struct SequenceMetaData
        {
            public bool IsParallel;
            public float Delay;
            public float DelayRemaining;
        }

        private int _currentEffectIndex;

        private List<ISequenceEffectWrapper> _effects;

        public float Duration 
        { 
            get
            {
                var d = 0f;
                foreach (var effect in _effects)
                {
                    d += effect.Duration;
                }
                return d;
            }
        }

        public bool RequiresUpdates => true;

        public bool IsCompleted => _currentEffectIndex >= _effects.Count;

        public Sequence(int effectCapacity = 6)
        {
            _effects = new List<ISequenceEffectWrapper>(effectCapacity);
        }

        /// <summary>
        /// Add an effect to the end of the sequence
        /// </summary>
        /// <param name="effect">The effect to be added</param>
        /// <param name="delay">The delay in seconds since the last effect batch</param>
        public Sequence Append(IEffect effect, float delay = 0f)
        {
            _effects.Add(new SequenceEffectWrapper
            {
                Effect = effect,
                Delay = delay,
                DelayRemaining = delay
            });

            return this;
        }

        /// <summary>
        /// Add an effect to the sequence so that it executes in parallel with the last added effect
        /// </summary>
        /// <param name="effect">The effect to be added</param>
        /// <param name="delay">The delay in seconds since the start of the batch</param>
        public Sequence Parallel(IEffect effect, float delay = 0f)
        {
            if (_effects.Count == 0)
            {
                Append(effect, delay);
                return this;
            }

            var effectWrapper = _effects[_effects.Count - 1];
            if (effectWrapper is SequenceEffectBatchWrapper batchWrapper)
            {
                batchWrapper.Effects.Add(new SequenceEffectWrapper
                {
                    Effect = effect,
                    Delay = delay,
                    DelayRemaining = delay
                });
            }
            else
            {
                effectWrapper = new SequenceEffectBatchWrapper
                {
                    Effects = new List<SequenceEffectWrapper>() { 
                        (SequenceEffectWrapper) effectWrapper, 
                        new SequenceEffectWrapper
                        {
                            Effect = effect,
                            Delay = delay,
                            DelayRemaining = delay
                        }
                    }
                };

                _effects[_effects.Count - 1] = effectWrapper;
            }

            return this;
        }

        public void Play()
        {
            Reset();

            PlayCurrentBatch();
        }

        public void Reset()
        {
            for (int i = Mathf.Min(_currentEffectIndex, _effects.Count - 1); i >= 0; i--)
            {
                _effects[i].Reset();
            }

            _currentEffectIndex = 0;
        }

        public void Skip()
        {
            foreach (var effect in _effects)
            {
                effect.Skip();
            }

            _currentEffectIndex = _effects.Count;
        }

        public void Update(float deltaTime)
        {
            if (IsCompleted) return;

            var current = _effects[_currentEffectIndex];

            if (current.IsCompleted || current.Update(deltaTime))
            {
                _currentEffectIndex++;

                PlayCurrentBatch();
            }
        }

        /// <summary>
        /// Calls play on all effects in a batch that don't have a delay
        /// </summary>
        private void PlayCurrentBatch()
        {
            if (_currentEffectIndex < _effects.Count)
            {
                _effects[_currentEffectIndex].Play();
            }
        }
    }
}
