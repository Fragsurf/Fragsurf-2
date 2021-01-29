using Fragsurf.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Mapping
{
    public class TestGamemode : BaseGamemode
    {
        public override string Name => "TestGamemode";

        protected override void _Load(FSGameLoop game)
        {
            Debug.Log("Test Gamemode Loaded");
        }

        protected override void _Unload(FSGameLoop game)
        {
            Debug.Log("Test Gamemode Unloaded");
        }
    }
}

