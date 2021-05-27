/*
 * DynamicVScrollView.cs
 * 
 * @author mosframe / https://github.com/mosframe
 * 
 */

 namespace Mosframe {

    using UnityEngine;

    /// <summary>
    /// Dynamic Vertical Scroll View
    /// </summary>
    [AddComponentMenu("UI/Dynamic V Scroll View")]
    public class DynamicVScrollView : DynamicScrollView {

        protected override float contentAnchoredPosition    { get { return -this.contentRect.anchoredPosition.y; } set { this.contentRect.anchoredPosition = new Vector2( this.contentRect.anchoredPosition.x, -value ); } }
	    protected override float contentSize                { get { return this.contentRect.rect.height; } }
	    protected override float viewportSize               { get { return this.viewportRect.rect.height;} }
	    protected override float itemSize                   { get { return this.itemPrototype.rect.height;} }

        public override void init () {

            this.direction = Direction.Vertical;
            base.init();
        }
        protected override void Awake() {

            base.Awake();
            this.direction = Direction.Vertical;
        }
        protected override void Start () {

            base.Start();
        }
    }
}
