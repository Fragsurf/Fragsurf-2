using Fragsurf.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    [RequireComponent(typeof(GameAudioSource))]
    public class MusicPlayer : MonoBehaviour
    {

        [SerializeField]
        private TMP_Text _song;
        [SerializeField]
        private TMP_Text _time;
        [SerializeField]
        private Button _pause;
        [SerializeField]
        private Button _play;
        [SerializeField]
        private Button _prev;
        [SerializeField]
        private Button _next;
        [SerializeField]
        private Slider _volume;

        [SerializeField]
        private AudioClip[] _songs;

        private GameAudioSource _src;
        private float _prevTime;

        private void Start()
        {
            _src = GetComponent<GameAudioSource>();
            _play.onClick.AddListener(Play);
            _pause.onClick.AddListener(Pause);
            _next.onClick.AddListener(PlayNext);
            _prev.onClick.AddListener(PlayPrevious);
            _volume.onValueChanged.AddListener(SetVolume);
            SetVolume(.15f);
            PlayRandom();
            Play();
        }

        private void OnEnable()
        {
            if(!_src || !_src.Src)
            {
                return;
            }
            _src.Src.time = _prevTime;
        }

        public void Play()
        {
            _src.Src.time = _prevTime;
            _src.Src.Play();
        }

        public void Pause()
        {
            _src.Src.Pause();
        }

        public void PlayNext()
        {
            if(_src.Src.clip == null
                || !_songs.Contains(_src.Src.clip))
            {
                SetSong(_songs[0]);
                return;
            }
            SetSong(_songs.ToList().NextOf(_src.Src.clip));
            Play();
        }

        public void PlayPrevious()
        {
            if (_src.Src.clip == null
                || !_songs.Contains(_src.Src.clip))
            {
                SetSong(_songs[0]);
                return;
            }
            SetSong(_songs.ToList().PreviousOf(_src.Src.clip));
            Play();
        }

        public void PlayRandom()
        {
            SetSong(_songs[UnityEngine.Random.Range(0, _songs.Length)]);
            Play();
        }

        public void SetVolume(float v)
        {
            _src.Src.volume = v;
            _volume.SetValueWithoutNotify(v);
        }

        private void SetSong(AudioClip song)
        {
            _prevTime = 0;
            _src.Src.clip = song;
            _src.Src.time = 0;
        }

        private void Update()
        {
            var cl = FSGameLoop.GetGameInstance(false);
            if (cl && cl.GameLoader.State == GameLoaderState.Playing)
            {
                if (_src.Src.isPlaying)
                {
                    Pause();
                    _time.text = "PAUSED WHILE IN-GAME";
                }
                return;
            }

            var playing = _src.Src.isPlaying;
            _play.gameObject.SetActive(!playing);
            _pause.gameObject.SetActive(playing);
            _prevTime = _src.Src.time;
            _song.text = _src.Src.clip != null ? _src.Src.clip.name : "Nothing playing";
            _time.text = _src.Src.clip != null ? $"{FormatSeconds(_src.Src.time)} / {FormatSeconds(_src.Src.clip.length)}" : "00:00 / 00:00";
        }

        private string FormatSeconds(float s)
        {
            var ts = TimeSpan.FromSeconds(s);
            return ts.ToString(@"mm\:ss");
        }

    }
}

