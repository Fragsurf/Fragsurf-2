using UIForia.Animation;
using UIForia.Rendering;
using UIForia.Sound;

namespace UIForia.Compilers.Style {

    public class StyleSheet {

        public string path;
        internal string source;
        internal readonly int id;
        internal readonly StyleConstant[] constants;
        internal readonly AnimationData[] animations;
        internal readonly UIStyleGroupContainer[] styleGroupContainers;
        internal UISoundData[] sounds;

        internal StyleSheet(StyleConstant[] constants, UIStyleGroupContainer[] styleGroupContainers, AnimationData[] animations, UISoundData[] sounds) {
            this.id = 0;
            this.constants = constants;
            this.styleGroupContainers = styleGroupContainers;
            this.animations = animations;
        }

        internal StyleSheet(int id, StyleConstant[] constants, UIStyleGroupContainer[] styleGroupContainers, AnimationData[] animations, UISoundData[] sounds) {
            this.id = id;
            this.constants = constants;
            this.styleGroupContainers = styleGroupContainers;
            this.animations = animations;
            this.sounds = sounds;
        }

        public bool TryGetAnimationData(string name, out AnimationData retn) {
            if (animations == null) {
                retn = default;
                return false;
            }

            for (int i = 0; i < animations.Length; i++) {
                if (animations[i].name == name) {
                    retn = animations[i];
                    return true;
                }
            }

            retn = default;
            return false;
        }

//        public UIStyleGroupContainer GetStyleGroupsByTagName(string tagName) {
//            if (styleGroupContainers == null) return null;
//            for (int i = 0; i < styleGroupContainers.Length; i++) {
//                UIStyleGroupContainer container = styleGroupContainers[i];
//                if (container.styleType == StyleType.Implicit && container.name == tagName) {
//                    return container;
//                }
//            }
//
//            return null;
//        }
//
//        public UIStyleGroupContainer GetStyleGroupByStyleName(string styleName) {
//            if (styleGroupContainers == null) return null;
//            for (int i = 0; i < styleGroupContainers.Length; i++) {
//                UIStyleGroupContainer container = styleGroupContainers[i];
//                if (container.styleType == StyleType.Shared && container.name == styleName) {
//                    return container;
//                }
//            }
//
//            return null;
//        }

        public bool TryResolveStyleName(string name, out UIStyleGroupContainer retn) {
            if (styleGroupContainers == null) {
                retn = null;
                return false;
            }

            for (int i = 0; i < styleGroupContainers.Length; i++) {
                UIStyleGroupContainer container = styleGroupContainers[i];
                if (container.name == name) {
                    retn = container;
                    return true;
                }
            }

            retn = null;
            return false;
        }

        public UIStyleGroupContainer GetStyleByName(string name) {
            for (int i = 0; i < styleGroupContainers.Length; i++) {
                if (styleGroupContainers[i].name == name) {
                    return styleGroupContainers[i];
                }
            }

            return null;
        }

        public bool TryResolveStyleByTagName(string tagName, out int id) {
            for (int i = 0; i < styleGroupContainers.Length; i++) {
                if (styleGroupContainers[i].styleType == StyleType.Implicit) {
                    if (styleGroupContainers[i].name == tagName) {
                        id = styleGroupContainers[i].id;
                        return true;
                    }
                }
            }

            id = -1;
            return false;
        }

    }

}