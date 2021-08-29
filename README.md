# Umph - effect sequencing for Unity

Umph is a tool that allows for sequencing of different effects (like tweens and callbacks) directly from it's component.
It aims to offer an easy to use interface for adding simple animations and polish to your game and to be designer friendly while doing it.

![umph_usage](https://user-images.githubusercontent.com/15526815/131263391-242563a5-a7d8-4ab8-9463-5be80d1b23dc.gif)

## How to use Umph

It's simple, you add the UmphComponent component to any GameObject you'd like and add different premade and configurable effects to your sequence.
UmphComponent can play automatically (on start or enable) or be triggered by other code using it's public API.

## What's included in Core?

Umph.Core contains the Editor functionality and the base components required to make Umph work.
Some premade effects are included:

- Log - calls Debug.Log with the supplied message
- Callback - exposes an UnityEvent that gets called when the effect starts
- Nested Umph Sequence - plays another Umph component as a part of the current sequence

For more Effect types check out the currently available extensions:

- [DOTween extension](https://github.com/MPozek/Umph.DOTween)

## Instalation
1. open the Unity Package Manager (Windows/Package Manager)
2. click the add package button and choose the "add package from git URL" option

![image](https://user-images.githubusercontent.com/15526815/130480877-e7b244be-7a24-4bf7-b008-ca214f090ba5.png)

3. paste the git url `https://github.com/MPozek/Umph.Core.git` and click Add

That's it! Unity should handle the rest of downloading and adding Pickle to your project.

## How do I Extend Umph?

An Umph effect is defined via implementing the `IEffect` interface. The interface requires the implementation of multiple properties:

- Duration (float get) - the time in seconds it takes for the effect to complete
- RequiresUpdates (bool get) - does this effect require frame by frame update calls (usually true for custom effects)
- IsCompleted (bool get) - state variable, true if the effect was started and has been completed
- IsPlaying (bool get) - state variable, true if the effect was started, is not paused, but has not yet been completed

- Play (void method) - starts playing the effect, called when the effect should start playing, set IsPlaying to true here
- Pause (void method) - pauses the execution of the effect, when this is called the effect should stop doing it's purpose and wait for another Play call
- Update (void method) - called every frame on the effect if RequiresUpdates returns true
- Reset (void method) - implement this and reset all state variables of the effect
- Skip (void method) - skip the effect, set the internal state to the end state of the effect if this gets called

When you implement the interface you need to create a wrapper for defining it within the Umph component. You can do this by creating a new class that inherits from the abstract `UmphComponentEffect` class.
Make sure to make the class serializable by marking it with the `[System.Serializable]` attribute.
You can declare any user exposed parameters in this class just as you would in a `MonoBehaviour` and use the `ConstructEffect` method to create a new instance of your IEffect class, populate it with the required parameters and return it as a result of the method call.
To make the effect display nicely in the add menu, you can add an `UmphComponentMenu` and customize the display name and the create menu path (using `/` as a submenu separator).

### Example
TODO, though you can try to learn from finished code, here's how you'd make a `PlayAudioSource` effect:

```cs
using Umph.Core;
using UnityEngine;

namespace UmphExample
{
    [System.Serializable]
    [UmphComponentMenu("Play Audio Source", "Audio/Play Audio Source")]
    public class PlayAudioSourceComponentEffect : UmphComponentEffect
    {
        [SerializeField] private AudioSource _targetSource;

        public override IEffect ConstructEffect()
        {
            return new PlayAudioSourceEffect(_targetSource);
        }

        public class PlayAudioSourceEffect : IEffect
        {
            private AudioSource _source;

            public PlayAudioSourceEffect(AudioSource source)
            {
                _source = source;
            }

            public float Duration => _source.clip.length;

            public bool RequiresUpdates => false;

            public bool IsCompleted => _source.time >= _source.clip.length;

            public bool IsPlaying => _source.isPlaying;

            public void Play()
            {
                _source.Play();
            }

            public void Pause()
            {
                _source.Pause();
            }

            public void Reset()
            {
                var wasPlaying = _source.isPlaying;
                _source.Stop();

                if (wasPlaying)
                {
                    _source.Play();
                }
            }

            public void Skip()
            {
                _source.time = _source.clip.length;
                _source.Stop();
            }

            public void Update(float deltaTime)
            {
            }
        }
    }
}
```
