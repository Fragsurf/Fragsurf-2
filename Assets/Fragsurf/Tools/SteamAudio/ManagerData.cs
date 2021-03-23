//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using System.Collections;
using UnityEngine;

namespace SteamAudio
{
	public class ManagerData
	{
        public ComponentCache      componentCache      = new ComponentCache();
        public GameEngineState     gameEngineState     = new GameEngineState();
        public AudioEngineState    audioEngineState    = null;
        public int                 referenceCount      = 0;

        public void Initialize(byte[] data, GameEngineStateInitReason reason, AudioEngine audioEngine, SimulationSettingsValue simulationValue, string[] sofaFileNames)
        {
            if (referenceCount == 0)
            {
                componentCache.Initialize();
                gameEngineState.Initialize(data, simulationValue, componentCache, reason);

                if (reason == GameEngineStateInitReason.Playing)
                {
                    audioEngineState = AudioEngineStateFactory.Create(audioEngine);
                    audioEngineState.Initialize(componentCache, gameEngineState, sofaFileNames);
                }
            }

            ++referenceCount;
        }

        // Destroys Phonon Manager.
        public void Destroy()
        {
            --referenceCount;

            if (referenceCount == 0)
            {
                if (audioEngineState != null)
                {
                    audioEngineState.Destroy();
                    audioEngineState = null;
                }

                gameEngineState.Destroy();
                componentCache.Destroy();
            }
        }
	}
}