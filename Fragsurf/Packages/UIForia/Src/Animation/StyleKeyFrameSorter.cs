using System.Collections.Generic;

namespace UIForia.Animation {

    public class StyleKeyFrameSorter : IComparer<ProcessedStyleKeyFrame> {

        public int Compare(ProcessedStyleKeyFrame x, ProcessedStyleKeyFrame y) {
            if (x.time == y.time) return 0;
            return x.time > y.time ? 1 : -1;
        }

    }
    
    public class MaterialKeyFrameSorter : IComparer<ProcessedMaterialKeyFrame> {

        public int Compare(ProcessedMaterialKeyFrame x, ProcessedMaterialKeyFrame y) {
            if (x.time == y.time) return 0;
            return x.time > y.time ? 1 : -1;
        }

    }

}