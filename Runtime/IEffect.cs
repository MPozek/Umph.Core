namespace Umph.Core
{
    public interface IEffect
    {
        float Duration { get; }
        bool RequiresUpdates { get; }
        bool IsCompleted { get; }
        bool IsPlaying { get; }

        /// <summary>
        /// Starts playback of the effect
        /// </summary>
        void Play();

        /// <summary>
        /// Updates the effect state by frame delta time
        /// </summary>
        /// <param name="deltaTime">Time passed since last update</param>
        void Update(float deltaTime);

        /// <summary>
        /// Resets the internal state of the effect
        /// </summary>
        void Reset();

        /// <summary>
        /// Sets the internal state of the effect to the end position
        /// </summary>
        void Skip();
    }
}
