using UnityEngine;
using FMODUnity;
using Fragsurf.Shared;

namespace Fragsurf.Client
{
    public static class SoundManager
    {

        private static FMOD.Studio.EventInstance _ambientInstance;
        public static void PlayAmbience2D(string path, float volume)
        {
            StopAmbience2D();
            path = SoundEmitter.FixPath(path);
            _ambientInstance = RuntimeManager.CreateInstance(path);
            _ambientInstance.setVolume(volume);
            RuntimeManager.AttachInstanceToGameObject(_ambientInstance, Camera.main.transform, rigidBody: null);
            _ambientInstance.start();
            _ambientInstance.release();
        }

        public static void StopAmbience2D()
        {
            _ambientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        public static void PlaySound2D(string path, float volume)
        {
            PlaySoundAttached(path, volume, Camera.main.transform);
        }

        public static void PlaySoundAttached(string path, float volume, Transform transform, string parameterName = null, float parameterValue = 0f)
        {
            path = SoundEmitter.FixPath(path);
            var instance = RuntimeManager.CreateInstance(path);

            if(parameterName != null)
            {
                instance.setParameterValue(parameterName, parameterValue);
            }

            instance.setVolume(volume);
            RuntimeManager.AttachInstanceToGameObject(instance, transform, rigidBody: null);
            instance.start();
            instance.release();
        }

    }
}

