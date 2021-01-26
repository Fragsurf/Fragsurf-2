//using JetBrains.Annotations;
//using UIForia.Elements;
//using UIForia.Layout.LayoutTypes;
//
//namespace UIForia.Layout {
//
//    [LayoutBoxName("Hex")]
//    public class HexLayoutBox : LayoutBox, IPoolableLayoutBox {
//
//        // style.HexGridOffset = 1;
//        public HexLayoutBox([NotNull] UIElement element) : base(element) { }
//
//        public override Size RunLayout(LayoutBoxSize[] output) {
//            throw new System.NotImplementedException();
//            // return a size
//            // set a size for each child
//            children.SetAllocatedWidth(40);
//        }
//
//        protected override float ComputeContentWidth() {
//            throw new System.NotImplementedException();
//        }
//
//        protected override float ComputeContentHeight(float width) {
//            throw new System.NotImplementedException();
//        }
//
//        protected override void OnChildrenChanged() {
//            throw new System.NotImplementedException();
//        }
//
//        public void OnSpawn(UIElement element) { }
//
//        public void OnRelease() { }
//
//    }
//
//}