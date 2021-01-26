using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    internal static partial class StyleASTNodeFactory {
            
        internal static readonly ObjectPool<AnimationRootNode> s_AnimationRootNodePool = new ObjectPool<AnimationRootNode>();
        
        internal static AnimationRootNode AnimationRootNode(string animName) {
            AnimationRootNode retn = s_AnimationRootNodePool.Get();
            retn.animName = animName;
            retn.type = StyleASTNodeType.AnimationDeclaration;
            return retn;
        }
    }

    public class AnimationRootNode : StyleASTNode {

        public string animName;

        public LightList<VariableDefinitionNode> variableNodes;
        public LightList<AnimationOptionNode> optionNodes;
        public LightList<KeyFrameNode> keyframeNodes;

        public void AddVariableNode(VariableDefinitionNode node) {
            variableNodes = variableNodes ?? new LightList<VariableDefinitionNode>(4);
            variableNodes.Add(node);
        }
        
        public void AddOptionNode(AnimationOptionNode optionNode) {
            optionNodes = optionNodes ?? new LightList<AnimationOptionNode>(4);
            optionNodes.Add(optionNode);
        }

        public void AddKeyFrameNode(KeyFrameNode keyFrameNode) {
            keyframeNodes = keyframeNodes ?? new LightList<KeyFrameNode>(4);
            keyframeNodes.Add(keyFrameNode);
        }

        public override void Release() {
            if (variableNodes != null) {
                for (int i = 0; i < variableNodes.Count; i++) {
                    variableNodes[i].Release();
                }
                variableNodes.Clear();
                LightList<VariableDefinitionNode>.Release(ref variableNodes);
            }
            
            if (optionNodes != null) {
                for (int i = 0; i < optionNodes.Count; i++) {
                    optionNodes[i].Release();
                }
                optionNodes.Clear();
                LightList<AnimationOptionNode>.Release(ref optionNodes);
            }
            
            if (keyframeNodes != null) {
                for (int i = 0; i < keyframeNodes.Count; i++) {
                    keyframeNodes[i].Release();
                }
                keyframeNodes.Clear();
                LightList<KeyFrameNode>.Release(ref keyframeNodes);
            }
            
            StyleASTNodeFactory.s_AnimationRootNodePool.Release(this);
        }

    }

}