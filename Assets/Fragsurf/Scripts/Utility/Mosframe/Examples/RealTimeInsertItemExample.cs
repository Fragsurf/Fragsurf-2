/**
 * RealTimeInsertItemExample.cs
 *
 * @author mosframe / https://github.com/mosframe
 *
 */

namespace Mosframe {

    using System;
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// RealTimeInsertItemExample
    /// </summary>
    public class RealTimeInsertItemExample : MonoBehaviour {

        public static RealTimeInsertItemExample I;

        public class CustomData {

            public string   name;
            public string   value;
            public bool     on;
        }

        public List<CustomData>     data = new List<CustomData>();

        public DynamicScrollView    scrollView;
        //public  int                 dataIndex    = 1;
        //public  string              dataName     = "Insert01";
        //public  int                 dataValue    = 100;


        public InputField           indexInput;
        public InputField           titleInput;
        public InputField           valueInput;
        public Button               insertButton;


        private void Awake () {
            I = this;
        }


        private void Start() {

            // sample insert

            this.insertItem( 0, new CustomData{ name="data00", value="value0", on=true } );
            this.insertItem( 0, new CustomData{ name="data01", value="value1", on=true } );

            // register click event to InsertButton

            this.insertButton.onClick.AddListener( this.onClick_InsertButton );
        }

        public void insertItem( int index, CustomData data ) {

            // set custom data

            I.data.Insert( index, data );

            this.scrollView.totalItemCount = I.data.Count;
        }

        public void onClick_InsertButton () {

            this.insertItem( int.Parse(this.indexInput.text), new CustomData{ name=this.titleInput.text, value=this.valueInput.text, on=true } );
        }

    }

   //#if UNITY_EDITOR
   //
   //[UnityEditor.CustomEditor(typeof(RealTimeInsertItemExample))]
   //public class RealTimeAddItemExampleEditor : UnityEditor.Editor {
   //
   //    public override void OnInspectorGUI() {
   //        base.OnInspectorGUI();
   //
   //        if( Application.isPlaying ) {
   //
   //            if( GUILayout.Button("Insert") ) {
   //
   //                var example = (RealTimeInsertItemExample)this.target;
   //                example.insertItem(example.dataIndex, new RealTimeInsertItemExample.CustomData{ name=example.dataName, value=example.dataValue } );
   //            }
   //        }
   //    }
   //}
   //
   //#endif
}