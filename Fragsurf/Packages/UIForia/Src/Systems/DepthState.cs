using System;

namespace Src.Systems {

    public struct DepthState {

        private byte m_WriteEnabled;
        private sbyte m_CompareFunction;

        public DepthState(bool writeEnabled, UnityEngine.Rendering.CompareFunction compareFunction = UnityEngine.Rendering.CompareFunction.LessEqual) {
            this.m_WriteEnabled = Convert.ToByte(writeEnabled);
            this.m_CompareFunction = (sbyte) compareFunction;
        }

        public static DepthState Default {
            get { return new DepthState(true); }
        }

        public bool writeEnabled {
            get { return this.m_WriteEnabled == 1; }
            set { this.m_WriteEnabled = (byte) (value ? 1 : 0); }
        }

        public UnityEngine.Rendering.CompareFunction compareFunction {
            get { return (UnityEngine.Rendering.CompareFunction) this.m_CompareFunction; }
            set { this.m_CompareFunction = (sbyte) value; }
        }

    }

}