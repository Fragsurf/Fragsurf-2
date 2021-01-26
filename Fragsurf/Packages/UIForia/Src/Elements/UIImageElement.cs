using UIForia.Attributes;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Elements {

    [TemplateTagName("Image")]
    public class UIImageElement : UIContainerElement {

        public ImageLocator? src;
        internal Texture texture;
        private Mesh mesh;

        public float Width;
        public float Height;
        
        public UIImageElement() {
            flags |= UIElementFlags.Primitive;
        }
        
        [OnPropertyChanged(nameof(src))]
        public void OnSrcChanged() {
            SetupBackground();
        }

        public override void OnEnable() {
            SetupBackground();
        }

        private void SetupBackground() {
            if (src == null) {
                style.SetBackgroundImage(null, StyleState.Normal);
                return;
            }

            texture = src.Value.texture ?? application.ResourceManager.GetTexture(src.Value.imagePath);
            style.SetBackgroundImage((Texture2D)texture, StyleState.Normal);
            if (Width > 0) {
                style.SetPreferredHeight(Width * texture.height / texture.width, StyleState.Normal);
                style.SetPreferredWidth(Width, StyleState.Normal);
            }

            if (Height > 0) {
                style.SetPreferredWidth(Height * texture.width / texture.height, StyleState.Normal);
                style.SetPreferredHeight(Height, StyleState.Normal);
            } 
        }

        public override string GetDisplayName() {
            return "Image";
        }

    }

}