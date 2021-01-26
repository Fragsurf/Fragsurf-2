using System;
using Src.Systems;
using UnityEngine;

namespace UIForia.Rendering {

    internal class UIForiaMaterialPool {

        public Material small;
        public Material medium;
        public Material large;
        public Material huge;
        public Material massive;

        // todo -- get stats on how often each is used 
        private readonly UIForiaPropertyBlock smallBlock;
        private readonly UIForiaPropertyBlock mediumBlock;
        private readonly UIForiaPropertyBlock largeBlock;
        private readonly UIForiaPropertyBlock hugeBlock;
        private readonly UIForiaPropertyBlock massiveBlock;

        public UIForiaMaterialPool(Material material) {
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

            this.smallBlock = new UIForiaPropertyBlock(small, RenderContext.k_ObjectCount_Small);
            this.mediumBlock = new UIForiaPropertyBlock(medium, RenderContext.k_ObjectCount_Medium);
            this.largeBlock = new UIForiaPropertyBlock(large, RenderContext.k_ObjectCount_Large);
            this.hugeBlock = new UIForiaPropertyBlock(huge, RenderContext.k_ObjectCount_Huge);
            this.massiveBlock = new UIForiaPropertyBlock(massive, RenderContext.k_ObjectCount_Massive);

            FixedRenderState state = FixedRenderState.Default;

           MaterialUtil.SetupState(small, state);
           MaterialUtil.SetupState(medium, state);
           MaterialUtil.SetupState(large, state);
           MaterialUtil.SetupState(huge, state);
           MaterialUtil.SetupState(massive, state);
        }

        public UIForiaPropertyBlock GetPropertyBlock(int objectCount) {
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
            UnityEngine.Object.Destroy(small);
            UnityEngine.Object.Destroy(medium);
            UnityEngine.Object.Destroy(large);
            UnityEngine.Object.Destroy(huge);
            UnityEngine.Object.Destroy(massive);
        }

    }

}