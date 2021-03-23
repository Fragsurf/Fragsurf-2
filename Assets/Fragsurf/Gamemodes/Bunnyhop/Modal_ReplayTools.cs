using Fragsurf.Client;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class Modal_ReplayTools : UGuiModal
    {

        [SerializeField]
        private Button _kick;
        [SerializeField]
        private Button _spawn;
        [SerializeField]
        private Button _replayList;
        [SerializeField]
        private Slider _timeline;
        [SerializeField]
        private Toggle _drawPath;
        [SerializeField]
        private TMP_InputField _speed;
        [SerializeField]
        private Button _pause;

        private TMP_Text _pauseText;

        private void Start()
        {
            _pauseText = _pause.GetComponentInChildren<TMP_Text>();

            _kick.onClick.AddListener(() =>
            {
                var target = FSGameLoop.GetGameInstance(false).Get<SpectateController>().TargetHuman;
                if(!(target is ReplayHuman replay))
                {
                    return;
                }
                replay.Delete();
            });

            _spawn.onClick.AddListener(async () =>
            {
                var target = FSGameLoop.GetGameInstance(false).Get<SpectateController>().TargetHuman;
                var pos = target != null ? target.Origin : Vector3.zero;
                var angles = target != null ? target.Angles : Vector3.zero;
                FSGameLoop.GetGameInstance(false).TextChat.MessageAll("/r");
                var timer = 1000f;
                while(Human.Local == null && timer > 0)
                {
                    await Task.Delay(50);
                    timer -= 50;
                }
                if(Human.Local != null && (pos + angles != Vector3.zero))
                {
                    FSGameLoop.GetGameInstance(false).TextChat.MessageAll($"/tele {pos.x},{pos.y},{pos.z} {angles.x},{angles.y},{angles.z}");
                }
            });

            _pause.onClick.AddListener(() =>
            {
                var target = FSGameLoop.GetGameInstance(false).Get<SpectateController>().TargetHuman as ReplayHuman;
                if(target == null)
                {
                    return;
                }
                target.Timeline.Paused = !target.Timeline.Paused;
            });

            _replayList.onClick.AddListener(() =>
            {
                UGuiManager.Instance.ToggleModal<Modal_ReplayList>();
            });

            _timeline.onValueChanged.AddListener((v) =>
            {
                var target = FSGameLoop.GetGameInstance(false).Get<SpectateController>().TargetHuman;
                if(target != null 
                    && target.Timeline is BunnyhopTimeline bhop
                    && bhop.Mode == TimelineMode.Replay)
                {
                    bhop.Paused = true;
                    bhop.SetReplayPosition(v);
                }
            });

            _drawPath.onValueChanged.AddListener((v) =>
            {
                var target = FSGameLoop.GetGameInstance(false).Get<SpectateController>().TargetHuman as ReplayHuman;
                if (target == null)
                {
                    return;
                }
                target.DrawPath = v;
            });

            _speed.onValueChanged.AddListener((v) =>
            {
                if ((FSGameLoop.GetGameInstance(false).Get<SpectateController>().TargetHuman is ReplayHuman target)
                    && float.TryParse(v, out float speed)
                    && target.Timeline is BunnyhopTimeline bhop
                    && bhop.Mode == TimelineMode.Replay)
                {
                    target.PlaybackSpeed = speed;
                }
            });
        }

        private void Update()
        {
            var cl = FSGameLoop.GetGameInstance(false);
            if(!cl 
                || !cl.TryGet(out SpectateController spec)
                || !(spec.TargetHuman is ReplayHuman target)
                || !(spec.TargetHuman.Timeline is BunnyhopTimeline bhop))
            {
                Close();
                return;
            }

            _timeline.SetValueWithoutNotify(bhop.GetReplayPosition());
            _pauseText.text = target.Timeline.Paused ? "Play" : "Pause";
            _drawPath.SetIsOnWithoutNotify(target.DrawPath);

            if (!_speed.isFocused)
            {
                _speed.SetTextWithoutNotify(target.PlaybackSpeed.ToString());
            }
        }

    }
}

