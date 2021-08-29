namespace Umph.Core
{
    public interface IEffect
    {
        /// <summary>
        /// How long does the effect take, no delay is taken into account
        /// </summary>
        float Duration { get; }

        /// <summary>
        /// Does the effect require constant frame by frame updates, if false Update will not be called
        /// </summary>
        bool RequiresUpdates { get; }

        /// <summary>
        /// Is the effect completed, if true the effect was started and completed it's execution
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Is the effect currently playing, true when the effect was started but is not yet completed
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Starts playback of the effect
        /// </summary>
        void Play();

        /// <summary>
        /// Pauses the playback of the effect
        /// </summary>
        void Pause();

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
