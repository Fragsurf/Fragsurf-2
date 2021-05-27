/*
 * DynamicScrollViewEditor.cs
 * 
 * @author mosframe / https://github.com/mosframe
 * 
 */
namespace Mosframe {

    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// <see cref="DynamicScrollView"/> Editor
    /// </summary>
    public class DynamicScrollViewEditor {


        [MenuItem( "GameObject/UI/Dynamic H Scroll View" )]
        public static void CreateHorizontal () {

            var go = new GameObject( "Horizontal Scroll View", typeof(RectTransform) );
            go.transform.SetParent( Selection.activeTransform, false );
            go.AddComponent<DynamicHScrollView>().init();
        }

        [MenuItem( "GameObject/UI/Dynamic V Scroll View" )]
        public static void CreateVertical () {

            var go = new GameObject( "Vertical Scroll View", typeof(RectTransform) );
            go.transform.SetParent( Selection.activeTransform, false );
            go.AddComponent<DynamicVScrollView>().init();
        }
    }
}
