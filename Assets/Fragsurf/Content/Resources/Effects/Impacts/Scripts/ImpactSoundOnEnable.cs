using Fragsurf.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ImpactSoundOnEnable : MonoBehaviour
{

    [SerializeField]
    private AudioClip[] _clips;

    private AudioSource _src;
    private GameAudioSource _gameSrc;

    private void Awake()
    {
        _src = GetComponent<AudioSource>();
        _src.minDistance = .05f;
        _src.maxDistance = 10f;
        _src.volume = .9f;
        if(!TryGetComponent(out _gameSrc))
        {
            _gameSrc = gameObject.AddComponent<GameAudioSource>();
        }
        _gameSrc.Category = SoundCategory.Effects;
    }

    private void OnEnable()
    {
        if(_clips.Length == 0)
        {
            _gameSrc.PlayClip(_src.clip);
            return;
        }
        _gameSrc.PlayClip(_clips[Random.Range(0, _clips.Length)]);
    }

}

