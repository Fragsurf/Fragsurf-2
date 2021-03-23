using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
//using Aws.GameLift.Server;
//using Aws.GameLift.Server.Model;
using Fragsurf.Utility;
using Fragsurf.Shared.Player;

namespace Fragsurf.Server
{
    public class GameLiftServer : FSServerScript
    {

        ///// Fields /////

        //private GameSession _gameSession;
        private bool _shutdownInitiated;
        private float _shutdownDelay;
        private bool _canShutdown;
        private bool _isGameLiftServer;

        ///// Methods /////

        protected override void _Start()
        {
            //base._Ready();

            //_isGameLiftServer = Game.LaunchParams.Contains("gamelift");

            //if (!_isGameLiftServer)
            //    return;

            //// wait for GameLift to initiate the session
            //ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
            //var listeningPort = 42020;
            //var initSDKOutcome = GameLiftServerAPI.InitSDK();
            //if (initSDKOutcome.Success)
            //{
            //    ProcessParameters processParameters = new ProcessParameters(ActivateGameSession,
            //        OnAWSProcessTerminate,
            //        HealthCheck,
            //        listeningPort,
            //        new LogParameters());

            //    //Calling ProcessReady tells GameLift this game server is ready to receive incoming game sessions!
            //    var processReadyOutcome = GameLiftServerAPI.ProcessReady(processParameters);
            //    if (processReadyOutcome.Success)
            //    {
            //        Debug.Log("ProcessReady success.");
            //    }
            //    else
            //    {
            //        Debug.Log("ProcessReady failure : " + processReadyOutcome.Error.ToString());
            //    }
            //}
            //else
            //{
            //    Debug.LogError("InitSDK failure : " + initSDKOutcome.Error.ToString());
            //}
        }

        //protected override void OnPlayerJoiningGame(PlayerJoiningEventArgs e)
        //{
        //    if (!_isGameLiftServer)
        //        return;

        //    var outcome = GameLiftServerAPI.AcceptPlayerSession(e.Player.SteamID.ToString());
        //    if (!outcome.Success)
        //    {
        //        e.JoiningState = PlayerJoiningState.Denied;
        //        e.DenyReason = "Amazon said no";
        //    }
        //}

        //protected override void OnPlayerDisconnected(IPlayer player)
        //{
        //    if (!_isGameLiftServer)
        //        return;

        //    GameLiftServerAPI.RemovePlayerSession(player.SteamID.ToString());
        //}

        //protected override void OnGameLoaded()
        //{
        //    if (!_isGameLiftServer)
        //        return;

        //    // yeet it out there
        //    GameLiftServerAPI.ActivateGameSession();
        //    _shutdownDelay = 120f;
        //}

        //protected override void _Destroy()
        //{
        //    base._Destroy();

        //    if (!_isGameLiftServer)
        //        return;

        //    GameLiftServerAPI.Destroy();
        //}

        //private bool _gameSessionInitialized;

        //protected override void _Update()
        //{
        //    if (!_isGameLiftServer)
        //        return;

        //    if (_gameSession != null && !_gameSessionInitialized)
        //    {
        //        //var map = _gameSession.GameProperties.Find(x => x.Key == "map").Value;
        //        string map = string.Empty;
        //        string gamemode = string.Empty;

        //        try
        //        {
        //            map = GetAttributeWithHighestTally("mapPreference", _gameSession.MatchmakerData);
        //            gamemode = GetAttributeWithHighestTally("gameMode", _gameSession.MatchmakerData);
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError(e);
        //            Debug.LogError("Matchmaker Data: " + _gameSession.MatchmakerData);
        //        }

        //        if (string.IsNullOrEmpty(map))
        //        {
        //            map = "dtest";
        //            Debug.LogError("Couldn't determine map: \n" + _gameSession.MatchmakerData);
        //        }

        //        if (string.IsNullOrEmpty(gamemode))
        //        {
        //            gamemode = "Deathmatch";
        //            Debug.LogError("Couldn't determine gamemode: \n" + _gameSession.MatchmakerData);
        //        }

        //        var rankedProp = _gameSession.GameProperties.Find(x => x.Key == "ranked");
        //        var ranked = rankedProp != null ? rankedProp.Value : "false";

        //        FSLog.ExecuteLine("server.broadcastip " + _gameSession.IpAddress);
        //        FSLog.ExecuteLine("server.port " + _gameSession.Port);
        //        FSLog.ExecuteLine("map.default " + map);
        //        FSLog.ExecuteLine("game.mode " + gamemode);
        //        FSLog.ExecuteLine("game.ranked " + ranked);

        //        ServerLoop.Instance.BetterGameServer.StartServer();
        //        _gameSessionInitialized = true;
        //    }

        //    if (_shutdownDelay > 0)
        //    {
        //        _shutdownDelay -= Time.deltaTime;
        //        if (_shutdownDelay <= 0)
        //        {
        //            _canShutdown = true;
        //        }
        //    }

        //    if (_canShutdown && Game.PlayerMan.PlayerCount == 0)
        //    {
        //        Terminate();
        //        _shutdownInitiated = true;
        //    }
        //}

        // sample matchmaker data
        //{
        //   "matchId":"c1750132-10b0-4f39-83be-bd6f3ccf9083",
        //   "matchmakingConfigurationArn":"arn:aws:gamelift:us-west-2:439878155947:matchmakingconfiguration/fssolo-config3",
        //   "teams":[
        //      {
        //         "name":"red",
        //         "players":[
        //            {
        //               "playerId":"76561197979190498",
        //               "attributes":{
        //                  "mapPreference":{
        //                     "attributeType":"STRING_LIST",
        //                     "valueAttribute":[
        //                        "dtest"
        //                     ]
        //    },
        //                  "skill":{
        //                     "attributeType":"DOUBLE",
        //                     "valueAttribute":10
        //                  },
        //                  "gameMode":{
        //                     "attributeType":"STRING",
        //                     "valueAttribute":"trikz"
        //                  }
        //               }
        //            }
        //         ]
        //      }
        //   ],
        //   "autoBackfillTicketId":null
        //}

        private string GetAttributeWithHighestTally(string attribute, string matchmakerData)
        {
            var data = JSON.Parse(matchmakerData);
            var teams = data["teams"].AsArray;
            var mapTally = new Dictionary<string, int>();

            foreach (JSONNode t in teams)
            {
                var players = t["players"].AsArray;
                foreach (JSONNode player in players)
                {
                    var attrValue = player["attributes"][attribute]["valueAttribute"];
                    if (attrValue.IsArray)
                    {
                        foreach (JSONNode v in player["attributes"][attribute]["valueAttribute"])
                        {
                            if (!mapTally.ContainsKey(v))
                                mapTally.Add(v, 0);
                            mapTally[v]++;
                        }
                    }
                    else
                    {
                        if (!mapTally.ContainsKey(attrValue.Value))
                            mapTally.Add(attrValue.Value, 0);
                        mapTally[attrValue.Value]++;
                    }
                }
            }

            return mapTally.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        }

        //private void Terminate()
        //{
        //    Game.Destroy();
        //    GameLiftServerAPI.ProcessEnding();
        //    Application.Quit();
        //}

        //private void ActivateGameSession(GameSession gameSession)
        //{
        //    //Respond to new game session activation request. GameLift sends activation request 
        //    //to the game server along with a game session object containing game properties 
        //    //and other settings. Once the game server is ready to receive player connections, 
        //    //invoke GameLiftServerAPI.ActivateGameSession()
        //    _gameSession = gameSession;
        //    _gameSessionInitialized = false;
        //}

        //private void OnAWSProcessTerminate()
        //{
        //    //OnProcessTerminate callback. GameLift invokes this callback before shutting down 
        //    //an instance hosting this game server. It gives this game server a chance to save
        //    //its state, communicate with services, etc., before being shut down. 
        //    //In this case, we simply tell GameLift we are indeed going to shut down.
        //    Terminate();
        //}

        private bool HealthCheck()
        {
            //This is the HealthCheck callback.
            //GameLift invokes this callback every 60 seconds or so.
            //Here, a game server might want to check the health of dependencies and such.
            //Simply return true if healthy, false otherwise.
            //The game server has 60 seconds to respond with its health status. 
            //GameLift will default to 'false' if the game server doesn't respond in time.
            //In this case, we're always healthy!
            return true;
        }

        private bool MyRemoteCertificateValidationCallback(System.Object sender,
            X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain,
            // look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        continue;
                    }
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                        break;
                    }
                }
            }
            return isOk;
        }

    }
}
