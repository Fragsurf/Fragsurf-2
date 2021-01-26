using System.Collections.Generic;
using UIForia.Util;

namespace UIForia.Animation {

    public struct StyleAnimationKeyFrame {

        public readonly float key;
        public StructList<StyleKeyFrameValue> properties; // todo -- to IList<T>

        public StyleAnimationKeyFrame(float key) {
            this.key = key;
            this.properties = null;
        }

        public StyleAnimationKeyFrame(float key, StyleKeyFrameValue p0) {
            this.key = key;
            this.properties = StructList<StyleKeyFrameValue>.Get();
            this.properties.EnsureCapacity(1);
            this.properties[0] = p0;
            this.properties.Count = 1;
        }

        public StyleAnimationKeyFrame(float key, StyleKeyFrameValue p0, StyleKeyFrameValue p1) {
            this.key = key;
            this.properties = StructList<StyleKeyFrameValue>.Get();
            this.properties.EnsureCapacity(2);
            this.properties[0] = p0;
            this.properties[1] = p1;
            this.properties.Count = 2;
        }

        public StyleAnimationKeyFrame(float key, StyleKeyFrameValue p0, StyleKeyFrameValue p1, StyleKeyFrameValue p2) {
            this.key = key;
            this.properties = StructList<StyleKeyFrameValue>.Get();
            this.properties.EnsureCapacity(3);
            this.properties[0] = p0;
            this.properties[1] = p1;
            this.properties[2] = p2;
            this.properties.Count = 3;
        }

        public StyleAnimationKeyFrame(float key, StyleKeyFrameValue p0, StyleKeyFrameValue p1, StyleKeyFrameValue p2, StyleKeyFrameValue p3) {
            this.key = key;
            this.properties = StructList<StyleKeyFrameValue>.Get();
            this.properties.EnsureCapacity(4);
            this.properties[0] = p0;
            this.properties[1] = p1;
            this.properties[2] = p2;
            this.properties[3] = p3;
            this.properties.Count = 4;
        }

        public StyleAnimationKeyFrame(float key, params StyleKeyFrameValue[] properties) {
            this.key = key;
            this.properties = StructList<StyleKeyFrameValue>.Get();
            this.properties.EnsureCapacity(properties.Length);
            for (int i = 0; i < properties.Length; i++) {
                this.properties[i] = properties[i];
            }

            this.properties.Count = properties.Length;
        }

        public StyleAnimationKeyFrame(float key, IList<StyleKeyFrameValue> properties) {
            this.key = key;
            this.properties = StructList<StyleKeyFrameValue>.Get();
            this.properties.EnsureCapacity(properties.Count);
            for (int i = 0; i < properties.Count; i++) {
                this.properties[i] = properties[i];
            }

            this.properties.Count = properties.Count;
        }

        public void Release() {
            StructList<StyleKeyFrameValue>.Release(ref properties);
        }

    }

}