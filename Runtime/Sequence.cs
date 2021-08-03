using System.Collections.Generic;
using UnityEngine;

namespace Umph.Core
{
    public partial class Sequence : IEffect
    {
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

        public Sequence()
        {
            _effects = GetEffectWrapperList();
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
                batchWrapper = new SequenceEffectBatchWrapper
                {
                    Effects = GetEffectWrapperList()
                };
                batchWrapper.Effects.Add(effectWrapper);
                batchWrapper.Effects.Add(
                    new SequenceEffectWrapper
                    {
                        Effect = effect,
                        Delay = delay,
                        DelayRemaining = delay
                    }
                );

                _effects[_effects.Count - 1] = batchWrapper;
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
