/*
 * DynamicHScrollView.cs
 * 
 * @author mosframe / https://github.com/mosframe
 * 
 */

 namespace Mosframe {

    using UnityEngine;

    /// <summary>
    /// Dynamic Horizontal Scroll View
    /// </summary>
    [AddComponentMenu("UI/Dynamic H Scroll View")]
    public class DynamicHScrollView : DynamicScrollView {

        protected override float contentAnchoredPosition    { get { return this.contentRect.anchoredPosition.x; } set { this.contentRect.anchoredPosition = new Vector2( value, this.contentRect.anchoredPosition.y ); } }
	    protected override float contentSize                { get { return this.contentRect.rect.width; } }
	    protected override float viewportSize               { get { return this.viewportRect.rect.width; } }
	    protected override float itemSize                   { get { return this.itemPrototype.rect.width;} }

        public override void init () {

            this.direction = Direction.Horizontal;
            base.init();
        }
        protected override void Awake() {

            base.Awake();
            this.direction = Direction.Horizontal;
        }
        protected override void Start () {

            base.Start();
        }
    }
}
