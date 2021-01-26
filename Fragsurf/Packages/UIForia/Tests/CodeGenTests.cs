using System;
using NUnit.Framework;
using UIForia.Editor;
using UIForia.Rendering;

[TestFixture]
public class CodeGenTests {

    [Test]
    public void AssertAllStylePropertiesAreMapped() {
        foreach (var propId in Enum.GetValues(typeof(StylePropertyId))) {
            bool found = false;
            foreach (var prop in CodeGen.properties) {
                if (propId.Equals(prop.propertyId)) {
                    found = true;
                }
            }

            if (!found && !propId.ToString().StartsWith("__")) {
                Assert.Fail("No mapping for style property found: " + propId);
            }
        }
    }
}
