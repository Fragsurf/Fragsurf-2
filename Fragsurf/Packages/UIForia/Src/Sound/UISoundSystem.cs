using System;
using UIForia.Elements;

namespace UIForia.Sound {
    public class UISoundSystem {

        public event Action<UISoundEvent> onSoundPlayed;
        public event Action<UISoundEvent> onSoundPaused;
        public event Action<UISoundEvent> onSoundResumed;
        public event Action<UISoundEvent> onSoundStopped;

        public void PlaySound(UIElement origin, UISoundData soundData) {
            onSoundPlayed?.Invoke(new UISoundEvent() {Origin = origin, SoundData = soundData});
        }

        public void PauseSound(UIElement origin, UISoundData soundData) {
            onSoundPaused?.Invoke(new UISoundEvent() {Origin = origin, SoundData = soundData});
        }

        public void SoundResumed(UIElement origin, UISoundData soundData) {
            onSoundResumed?.Invoke(new UISoundEvent() {Origin = origin, SoundData = soundData});
        }

        public void StopSound(UIElement origin, UISoundData soundData) {
            onSoundStopped?.Invoke(new UISoundEvent() {Origin = origin, SoundData = soundData});
        }
    }
}
