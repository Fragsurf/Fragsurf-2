using UnityEngine;
using Fragsurf.Shared;

namespace Fragsurf.Client
{
    public static class SoundManager
    {

        public static void PlayAmbience2D(string path, float volume)
        {
            throw new System.NotImplementedException();
        }

        public static void StopAmbience2D()
        {
            throw new System.NotImplementedException();
        }

        public static void PlaySound2D(AudioClip clip, float volume)
        {
            PlaySoundAttached(clip, volume, GameCamera.Camera.transform);
        }

        public static void PlaySoundAttached(AudioClip clip, float volume, Transform transform, string parameterName = null, float parameterValue = 0f)
        {
            throw new System.NotImplementedException();
        }

    }
}

