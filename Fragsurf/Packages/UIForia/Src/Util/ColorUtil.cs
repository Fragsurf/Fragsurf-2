// using System.Diagnostics;
// using UIForia.Extensions;
// using UnityEngine;
//
// namespace UIForia.Util {
//
//     public static class ColorUtil {
//
//         public static readonly Color UnsetValue = new Color(-1, -1, -1, -1);
//
//         [DebuggerStepThrough]
//         public static bool IsDefined(Color color) {
//             return color.IsDefined();
//         }
//
//         // doesn't seem to handle alpha properly, might need to be a uint
//         public static float ColorToFloat(Color c) {
//             int color = (int) (c.r * 255) |
//                         (int) (c.g * 255) << 8 |
//                         (int) (c.b * 255) << 16 |
//                         (int) (c.a * 255) << 24;
//             return FloatUtil.DecodeToFloat(color);
//         }
//         
//     }
//
// }