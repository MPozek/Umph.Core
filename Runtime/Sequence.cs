using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Umph.Core
{
    public class Sequence : IEffect
    {
        private struct SequenceMetaData
        {
            public bool IsParallel;
            public float Delay;
            public float DelayRemaining;
        }

        private float _lastAddedEffectDuration;
        private int _currentEffectIndex;

        private List<SequenceMetaData> _metaData;
        private List<IEffect> _effects;

        public float Duration { get; private set; }

        public bool RequiresUpdates => true;

        public bool IsCompleted => _currentEffectIndex >= _effects.Count;

        public Sequence(int effectCapacity = 6)
        {
            _metaData = new List<SequenceMetaData>(effectCapacity);
            _effects = new List<IEffect>(effectCapacity);
        }

        /// <summary>
        /// Add an effect to the end of the sequence
        /// </summary>
        /// <param name="effect">The effect to be added</param>
        /// <param name="delay">The delay in seconds since the last effect batch</param>
        public Sequence Append(IEffect effect, float delay = 0f)
        {
            _effects.Add(effect);
            _metaData.Add(new SequenceMetaData
            {
                IsParallel = false,
                Delay = delay,
                DelayRemaining = delay
            });

            _lastAddedEffectDuration = delay + effect.Duration;
            Duration += _lastAddedEffectDuration;

            return this;
        }

        internal void Pause()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Add an effect to the sequence so that it executes in parallel with the last added effect
        /// </summary>
        /// <param name="effect">The effect to be added</param>
        /// <param name="delay">The delay in seconds since the start of the batch</param>
        public Sequence Parallel(IEffect effect, float delay = 0f)
        {
            _effects.Add(effect);
            _metaData.Add(new SequenceMetaData
            {
                IsParallel = true,
                Delay = delay,
                DelayRemaining = delay
            });

            // update sequence duration if required
            var effectDuration = delay + effect.Duration;
            if (effectDuration > _lastAddedEffectDuration)
            {
                Duration -= _lastAddedEffectDuration;
                _lastAddedEffectDuration = effectDuration;
                Duration += _lastAddedEffectDuration;
            }

            return this;
        }

        public void Play()
        {
            _currentEffectIndex = 0;

            PlayCurrentBatch();
        }

        public void Reset()
        {
            foreach (var effect in _effects)
            {
                effect.Reset();
            }

            for (int i = 0; i < _effects.Count; i++)
            {
                var data = _metaData[i];
                data.DelayRemaining = data.Delay;
                _metaData[i] = data;
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

            UpdateCurrentBatch(deltaTime);

            var didChangeEffectIndex = false;
            while (_currentEffectIndex < _effects.Count && _effects[_currentEffectIndex].IsCompleted)
            {
                _currentEffectIndex++;
                didChangeEffectIndex = true;
            }

            if (didChangeEffectIndex && !IsCompleted)
            {
                // did we transition onto the next batch?
                // this is the same as head effect not being parallel
                var currentHeadEffectData = _metaData[_currentEffectIndex];
                
                if (currentHeadEffectData.IsParallel == false)
                {
                    PlayCurrentBatch();
                }
            }
        }

        /// <summary>
        /// Calls play on all effects in a batch that don't have a delay
        /// </summary>
        private void PlayCurrentBatch()
        {
            for (int i = _currentEffectIndex; i < _effects.Count; i++)
            {
                var effectData = _metaData[i];
                if (i > _currentEffectIndex && !effectData.IsParallel)
                    return;

                if (effectData.DelayRemaining <= 0f)
                {
                    _effects[_currentEffectIndex].Play();
                }
            }
        }

        /// <summary>
        /// Decrements delays and updates effects that need updating in the current batch
        /// </summary>
        private void UpdateCurrentBatch(float deltaTime)
        {
            for (int i = _currentEffectIndex; i < _effects.Count; i++)
            {
                var effectData = _metaData[i];
                if (i > _currentEffectIndex && !effectData.IsParallel)
                    return;

                var effect = _effects[i];
                if (effectData.DelayRemaining > 0f)
                {
                    effectData.DelayRemaining -= deltaTime;
                    _metaData[i] = effectData;

                    if (effectData.DelayRemaining > 0f)
                    {
                        continue;
                    }
                    else
                    {
                        effect.Play();
                    }
                }

                if (effect.RequiresUpdates && !effect.IsCompleted)
                {
                    effect.Update(deltaTime);
                }

            }
        }
    }
}
