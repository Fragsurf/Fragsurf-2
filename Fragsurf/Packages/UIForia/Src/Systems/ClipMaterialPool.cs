using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UIForia.Rendering {
    
    internal class ClipMaterialPool {

        public Material small;
        public Material medium;
        public Material large;
        public Material huge;
        public Material massive;

        // todo -- get stats on how often each is used 
        private readonly ClipPropertyBlock smallBlock;
        private readonly ClipPropertyBlock mediumBlock;
        private readonly ClipPropertyBlock largeBlock;
        private readonly ClipPropertyBlock hugeBlock;
        private readonly ClipPropertyBlock massiveBlock;

        public ClipMaterialPool(Material material) {
            this.small = new Material(material);
            this.medium = new Material(material);
            this.large = new Material(material);
            this.huge = new Material(material);
            this.massive = new Material(material);

            this.small.EnableKeyword("BATCH_SIZE_SMALL");
            this.medium.EnableKeyword("BATCH_SIZE_MEDIUM");
            this.large.EnableKeyword("BATCH_SIZE_LARGE");
            this.huge.EnableKeyword("BATCH_SIZE_HUGE");
            this.massive.EnableKeyword("BATCH_SIZE_MASSIVE");

            this.smallBlock = new ClipPropertyBlock(small, RenderContext.k_ObjectCount_Small);
            this.mediumBlock = new ClipPropertyBlock(medium, RenderContext.k_ObjectCount_Medium);
            this.largeBlock = new ClipPropertyBlock(large, RenderContext.k_ObjectCount_Large);
            this.hugeBlock = new ClipPropertyBlock(huge, RenderContext.k_ObjectCount_Huge);
            this.massiveBlock = new ClipPropertyBlock(massive, RenderContext.k_ObjectCount_Massive);
        }
        
        public ClipPropertyBlock GetPropertyBlock(int objectCount) {
            if (objectCount <= RenderContext.k_ObjectCount_Small) {
                return smallBlock;
            }

            if (objectCount <= RenderContext.k_ObjectCount_Medium) {
                return mediumBlock;
            }

            if (objectCount <= RenderContext.k_ObjectCount_Large) {
                return largeBlock;
            }

            if (objectCount <= RenderContext.k_ObjectCount_Huge) {
                return hugeBlock;
            }

            if (objectCount <= RenderContext.k_ObjectCount_Massive) {
                return massiveBlock;
            }

            throw new Exception($"Batch size is too big. Tried to draw {objectCount} objects but batching supports at most {RenderContext.k_ObjectCount_Massive}");
        }

        public void Destroy() {
            Object.Destroy(small);
            Object.Destroy(medium);
            Object.Destroy(large);
            Object.Destroy(huge);
            Object.Destroy(massive);
        }

    }

}