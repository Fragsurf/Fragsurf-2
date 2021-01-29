using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public class TestEntity : NetEntity
    {

        public TestEntity(FSGameLoop game) 
            : base(game)
        {
        }

        private float _destructTimer = 5f;

        [NetProperty]
        public Vector3 TestProp { get; set; }
        [NetProperty]
        public Vector4 TestProp2 { get; set; }
        [NetProperty]
        public Vector4 TestProp3 { get; set; }
        [NetProperty]
        public Vector4 TestProp4 { get; set; }
        [NetProperty]
        public Vector4 TestProp5 { get; set; }
        [NetProperty]
        public Vector4 TestProp6 { get; set; }
        [NetProperty]
        public Vector4 TestProp7 { get; set; }
        [NetProperty]
        public Vector4 TestProp8 { get; set; }
        [NetProperty]
        public Vector4 TestProp9 { get; set; }
        [NetProperty]
        public Vector4 TestProp10 { get; set; }
        [NetProperty]
        public Vector4 TestProp11 { get; set; }
        [NetProperty]
        public Vector4 TestProp12 { get; set; }
        [NetProperty]
        public Vector4 TestProp13 { get; set; }

        protected override void _Update()
        {
            if(_destructTimer > 0)
            {
                _destructTimer -= Time.deltaTime;
                if (_destructTimer <= 0)
                {
                    Delete();
                }
            }
        }

        protected override void _Tick()
        {
            TestProp += Vector3.one;
            TestProp2 += Vector4.one;
            TestProp3 += Vector4.one;
            TestProp4 += Vector4.one;
            TestProp5 += Vector4.one;
            TestProp6 += Vector4.one;
            TestProp7 += Vector4.one;
            TestProp8 += Vector4.one;
            TestProp8 += Vector4.one;
            TestProp10 += Vector4.one;
            TestProp11 += Vector4.one;
            TestProp12 += Vector4.one;
            TestProp13 += Vector4.one;
        }

    }
}

