/**
 * Item2.cs
 * 
 * @author mosframe / https://github.com/mosframe
 * 
 */

namespace Mosframe {

    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class Item2 : UIBehaviour, IDynamicScrollViewItem 
    {
	    private readonly Color[] colors = new Color[] {
		    Color.cyan,
		    Color.green,
	    };

	    public Image    icon;
	    public Text     title;
        public Button   button;
	    public Image    background;

        private int     dataIndex = -1;

        protected override void OnEnable () {

            base.OnEnable();
            this.button.onClick.AddListener( this.onClick );
        }

        protected override void OnDisable () {

            base.OnDisable();
            this.button.onClick.RemoveListener( this.onClick );
        }

        public void onUpdateItem( int index ) {

            if( RealTimeInsertItemExample.I.data.Count > index ) {

                this.dataIndex = index;
                this.updateItem();
            }
        }

        public void onClick () {

            if( this.dataIndex == -1 ) return;
            var data = RealTimeInsertItemExample.I.data[ this.dataIndex ];
            data.on = !data.on;

            this.updateItem();
        }
        

        private void updateItem () {

            if( this.dataIndex == -1 ) return;

            var data = RealTimeInsertItemExample.I.data[ this.dataIndex ];

		    this.background.color   = this.colors[Mathf.Abs(this.dataIndex) % this.colors.Length];
		    this.icon.sprite        = Resources.Load<Sprite>( (Mathf.Abs(this.dataIndex) % 20 + 1).ToString("icon_00") );

            if( data.on ) {
                this.title.text = data.name + "(" + data.value +")";
            } else {
                this.title.text = data.name + "(" + this.dataIndex.ToString("000") +")";
            }

            var buttonText = this.button.GetComponentInChildren<Text>();
            if( buttonText != null ) {
                buttonText.text = data.on ? "ON" : "OFF";
            }
        }
    }
}