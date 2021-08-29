using UnityEngine;
using System.Collections.Generic;

namespace Umph.Core
{

    /// <summary>
    /// Core component of the Umph plugin, used to edit and run Umph Sequences of effects.
    /// </summary>
    public class UmphComponent : MonoBehaviour
    {
        private Sequence _constructedSequence;

        public bool IsPlaying => _isPlaying;
        public bool IsCompleted => _constructedSequence != null && _constructedSequence.IsCompleted;

        public Sequence Sequence => _constructedSequence;

        [SerializeField, Tooltip("When true, will start playing the sequence on Unity's Start callback")]
        private bool _playOnStart;

        /* TODO :: have to add the option to run effects without timescale as well
        [SerializeField, Tooltip("When true, will use unscaled deltaTime to run effects")]
        private bool _useUnscaledTime;
        */

        [SerializeField, SerializeReference] private List<UmphComponentEffect> _effects = new List<UmphComponentEffect>();

        private bool _isPlaying = false;

        /// <summary>
        /// 
        /// </summary>
        public void Play(bool reset = false)
        {
            if (reset)
            {
                _constructedSequence.Reset();
            }
            else
            {
                if (_isPlaying)
                    return;
            }

            _isPlaying = true;
            _constructedSequence.Play();
        }

        public void DoReset()
        {
            _constructedSequence.Reset();
        }

        public void Pause()
        {
            _isPlaying = false;
            _constructedSequence.Pause();
        }

        public void Skip()
        {
            if (_isPlaying)
            {
                _constructedSequence.Skip();
                _isPlaying = false;
            }
        }

        public void Initialize(bool doForce = false)
        {
            if (doForce || _constructedSequence == null)
            {
                // cache and build out the sequence of umph effects
                _constructedSequence = new Sequence();
                foreach (var effect in _effects)
                {
                    if (effect.IsParallel)
                    {
                        _constructedSequence.Parallel(effect.ConstructEffect(), effect.Delay);
                    }
                    else
                    {
                        _constructedSequence.Append(effect.ConstructEffect(), effect.Delay);
                    }
                }
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnDisable()
        {
            if (_isPlaying)
            {
                Pause();
            }
            if (_constructedSequence != null)
                _constructedSequence.Reset();
        }

        private void Start()
        {
            // start playback if required
            if (_playOnStart)
            {
                Play();
            }
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            if (_constructedSequence.IsCompleted)
            {
                _isPlaying = false;
                return;
            }

            // run update to reduce delay counters and update the manually updating effects
            _constructedSequence.Update(Time.deltaTime);
        }
    }
}
