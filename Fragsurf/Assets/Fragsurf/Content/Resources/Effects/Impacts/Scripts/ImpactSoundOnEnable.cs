using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ImpactSoundOnEnable : MonoBehaviour
{

    [SerializeField]
    private AudioClip[] _clips;

    private AudioSource _src;

    private void Awake()
    {
        _src = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if(_clips.Length == 0)
        {
            _src.Play();
            return;
        }
        _src.clip = _clips[Random.Range(0, _clips.Length)];
        _src.Play();
    }

}

