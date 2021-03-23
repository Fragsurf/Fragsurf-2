//using System.Linq;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.LowLevel;

//public class PlayerLoopReducer
//{
//    private static List<string> systemsToRemove = new List<string>()
//    {
//                "PollPlayerConnection",
//                "XREarlyUpdate",
//                "AnalyticsCoreStatsUpdate",
//                "XRUpdate",
//                "ProcessRemoteInput",
//                "TangoUpdate",
//                "DeliverIosPlatformEvents",
//                "PhysicsResetInterpolatedTransformPosition",
//                "SpriteAtlasManagerUpdate",
//                "PerformanceAnalyticsUpdate",
//                "Physics2DUpdate",
//                //"IMGUISendQueuedEvents",
//                "AIUpdate",
//                "ExecuteGameCenterCallbacks",
//                "WindUpdate",
//                "UpdateAudio",
//                "AIUpdatePostScript",
//                "UNetUpdate",
//                "PhysicsSkinnedClothBeginUpdate",
//                "UpdateAudio",
//                "UpdateVideoTextures",
//                "UpdateVideo",
//                "PhysicsSkinnedClothFinishUpdate",
//                // MAKE SURE THIS ISN'T ACTUALLY NEEDED
//                //"ScriptRunDelayedTasks"
//    };

//    [RuntimeInitializeOnLoadMethod]
//    private static void AppStart()
//    {
//        var defaultSystems = PlayerLoop.GetDefaultPlayerLoop();
//        foreach (var s in systemsToRemove)
//        {
//            defaultSystems = Filtered(defaultSystems, s);
//        }
//        PlayerLoop.SetPlayerLoop(defaultSystems);
//    }

//    private static PlayerLoopSystem Filtered(PlayerLoopSystem root, string name)
//    {
//        if (root.subSystemList != null)
//        {
//            var list = root.subSystemList.ToList();
//            list.RemoveAll(x => x.type.Name == name);
//            root.subSystemList = list.ToArray();
//            for (int i = root.subSystemList.Length - 1; i >= 0; i--)
//            {
//                root.subSystemList[i] = Filtered(root.subSystemList[i], name);
//            }
//        }
//        return root;
//    }
//}
