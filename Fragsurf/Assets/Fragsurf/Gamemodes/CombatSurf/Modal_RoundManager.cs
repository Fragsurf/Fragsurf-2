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
        [SerializeField]
        private TMP_Text _team1Name;
        [SerializeField]
        private TMP_Text _team2Name;

        private RoundManager _rm;
        private MatchStates _prevMatchState;
        private RoundStates _prevRoundState;
        private int _prevTime;
        private int _prevRoundNumber = -1;
        private int _prevDefaultWinner = -1;
        private int _prevTeam1Score;
        private int _prevTeam2Score;

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

            if(_prevDefaultWinner != _rm.DefaultWinner)
            {
                switch (_rm.DefaultWinner)
                {
                    case 1:
                        _team1Name.color = Color.green;
                        _team2Name.color = Color.white;
                        break;
                    case 2:
                        _team1Name.color = Color.white;
                        _team2Name.color = Color.green;
                        break;
                }
                _prevDefaultWinner = _rm.DefaultWinner;
            }

            var t = (int)_rm.Timer;
            if(_prevTime != t)
            {
                _timer.text = TimeSpan.FromSeconds(t).ToString(TimerFormat);
                _prevTime = t;
            }

            var t1score = _rm.GetTeamScore(1);
            if(t1score != _prevTeam1Score)
            {
                _team1Score.text = t1score.ToString();
                _prevTeam1Score = t1score;
            }

            var t2score = _rm.GetTeamScore(2);
            if (t2score != _prevTeam2Score)
            {
                _team2Score.text = t2score.ToString();
                _prevTeam2Score = t2score;
            }
        }

    }
}

