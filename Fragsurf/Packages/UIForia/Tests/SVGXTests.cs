using NUnit.Framework;
using SVGX;
using UIForia.Util;

[TestFixture]
public class SVGXTests {

    [Test]
    public void Triangulate() {
        LightList<int> output = Earcut.Tessellate(new LightList<float>(new float[] {
            10,0,
            0,50,
            60,60,
            70,10
        }), null);
        
        Assert.AreEqual(output[0], 1);
        Assert.AreEqual(output[1], 0);
        Assert.AreEqual(output[2], 3);
        
        Assert.AreEqual(output[3], 3);
        Assert.AreEqual(output[4], 2);
        Assert.AreEqual(output[5], 1);
        
    }

}