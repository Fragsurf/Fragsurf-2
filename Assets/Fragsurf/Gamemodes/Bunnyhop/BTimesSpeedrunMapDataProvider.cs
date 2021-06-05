using Fragsurf.Actors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class BTimesSpeedrunMapDataProvider
    {

        public bool CreateZones(string mapName)
        {
            var zoneFile = Resources.Load<TextAsset>("btimes_zones");
            if (zoneFile == null)
            {
                return false;
            }

            var obj = JsonConvert.DeserializeObject<MapJsonData>(zoneFile.text);
            var m = obj.maps.FirstOrDefault(x => x.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase));
            if (m == null)
            {
                return false;
            }

            var zones = obj.zones.Where(x => x.MapID.HasValue && x.MapID == m.MapID).ToList();
            if (zones == null || zones.Count == 0)
            {
                return false;
            }

            var mainTrack = new GameObject().AddComponent<FSMTrack>();
            mainTrack.Set(FSMTrackType.Linear, "Main");

            if(Maps.Map.Current != null)
            {
                Maps.Map.Current.Actors.Add(mainTrack);
            }

            foreach (var z in zones)
            {
                var trigObj = new GameObject().AddComponent<FSMTrigger>();
                switch(z.Type)
                {
                    case ZoneType.MainStart:
                        trigObj.ActorName = "Main Start";
                        mainTrack.LinearData.StartTrigger = trigObj;
                        break;
                    case ZoneType.MainEnd:
                        trigObj.ActorName = "Main End";
                        mainTrack.LinearData.EndTrigger = trigObj;
                        break;
                }
                BuildTrigger(trigObj, z);
                if (Maps.Map.Current != null)
                {
                    Maps.Map.Current.Actors.Add(trigObj);
                }
            }

            return true;
        }

        private void BuildTrigger(FSMTrigger trig, Zone z)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(trig.transform);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = Vector3.one;

            var min = new Vector3(z.point00, z.point02, z.point01) * .0254f;
            var max = new Vector3(z.point10, z.point12, z.point11) * .0254f;
            var center = (min + max) / 2;
            var sz = max - min;

            trig.transform.position = center;
            trig.transform.localScale = sz;

            trig.EnsureRenderers();
        }

        private class MapJsonData
        {
            public List<Map> maps;
            public List<Zone> zones;
        }

        private class Map
        {
            public int MapID;
            public string MapName;
            public int Tier;
        }

        private class Zone
        {
            public int? MapID;
            public ZoneType Type;
            public float point00;
            public float point01;
            public float point02;
            public float point10;
            public float point11;
            public float point12;
        }

        private enum ZoneType
        {
            MainStart,
            MainEnd,
            BonusStart,
            BonusEnd,
            AntiCheat,
            Freestyle,
            Slide
        }

        //#define MAIN_START  0
        //#define MAIN_END    1
        //#define BONUS_START 2
        //#define BONUS_END   3
        //#define ANTICHEAT   4
        //#define FREESTYLE   5
        //#define SLIDE       6

    }
}


