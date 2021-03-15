using Fragsurf.Shared;
using Fragsurf.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    public class Modal_RoundManager : UGuiModal
    {
        private const string TimerFormat = @"mm\:ss";

        [SerializeField]
        private TMP_Text _roundNumber;
        [SerializeField]
        private TMP_Text _matchState;
        [SerializeField]
        private TMP_Text _roundState;
        [SerializeField]
        private TMP_Text _timer;
        [SerializeField]
        private TMP_Text _team1Score;
        [SerializeField]
        private TMP_Text _team2Score;

        private RoundManager _rm;
        private MatchStates _prevMatchState;
        private RoundStates _prevRoundState;
        private int _prevTime;
        private int _prevRoundNumber = -1;

        private void Start()
        {
            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl)
            {
                return;
            }
            _rm = cl.Get<RoundManager>();
        }

        private void Update()
        {
            if (!_rm)
            {
                return;
            }

            if(_rm.MatchState == MatchStates.Live)
            {
                _matchState.gameObject.SetActive(false);
            }
            else if(_prevMatchState != _rm.MatchState)
            {
                _matchState.gameObject.SetActive(true);
                _matchState.text = _rm.MatchState.ToString();
                _prevMatchState = _rm.MatchState;
            }

            if(_rm.RoundState != _prevRoundState)
            {
                _roundState.text = $"{_rm.RoundState}";
                _prevRoundState = _rm.RoundState;
            }

            if(_prevRoundNumber != _rm.CurrentRound)
            {
                _roundNumber.text = $"Round {_rm.CurrentRound}";
                _prevRoundNumber = _rm.CurrentRound;
            }

            var t = (int)_rm.Timer;
            if(_prevTime != t)
            {
                _timer.text = TimeSpan.FromSeconds(t).ToString(TimerFormat);
                _prevTime = t;
            }

            _team1Score.text = 0.ToString();
            _team2Score.text = 0.ToString();
        }

    }
}

