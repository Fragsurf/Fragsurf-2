using NUnit.Framework;
using UIForia.Util;

namespace Util {

    public class StringUtilTests {

        [Test]
        public void CharStringBuilder_Append() {
            CharStringBuilder builder = new CharStringBuilder();
            builder.Append("content");
            builder.Append("-");
            builder.Append("content");
            builder.Append("-");
            builder.Append("content");
            builder.Append("superlongstringhereohmygoshthisislong");
            Assert.AreEqual("content-content-contentsuperlongstringhereohmygoshthisislong", builder.ToString());
        }

        [Test]
        public void StringUtil_RangeEquals() {
            string a = "--hello--";
            string b = "hello";

            Assert.IsTrue(StringUtil.EqualsRangeUnsafe(a.ToCharArray(), 2, b.ToCharArray(), 0, b.Length));
        }

        [Test]
        public void StringUtil_FindMatchingIndex() {
            string input = "hello[this[is] more]here";
            int idx = StringUtil.FindMatchingIndex(input, '[', ']');
            Assert.AreEqual(input.Length - 4, idx);
        }
    }

}