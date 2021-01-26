using UnityEngine;

namespace UIForia.Elements {

    public struct ImageLocator {

        public readonly Texture texture;
        public readonly string imagePath;

        public bool isValid => texture != null || imagePath != null;

        public ImageLocator(Texture texture) {
            this.texture = texture;
            this.imagePath = null;
        }

        public ImageLocator(string imagePath) {
            this.texture = null;
            this.imagePath = imagePath;
        }
        
        public static implicit operator ImageLocator(Texture texture) {
            return new ImageLocator(texture);
        }
        
        public static implicit operator ImageLocator(Texture2D texture) {
            return new ImageLocator(texture);
        }
        
        public static implicit operator ImageLocator(RenderTexture texture) {
            return new ImageLocator(texture);
        }

        public static implicit operator ImageLocator(string imagePath) {
            return new ImageLocator(imagePath);
        }

        public static bool operator ==(ImageLocator a, ImageLocator b) {
            return ReferenceEquals(a.texture, b.texture) && a.imagePath == b.imagePath;
        }
        
        public static bool operator !=(ImageLocator a, ImageLocator b) {
            return !ReferenceEquals(a.texture, b.texture) || a.imagePath != b.imagePath;
        }
        
    }
}