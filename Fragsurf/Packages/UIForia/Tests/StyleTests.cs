using System;
using System.Reflection;
using NUnit.Framework;
using Tests.Mocks;
using TMPro;
using UIForia;
using UIForia.Attributes;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

[TestFixture]
public class StyleTests {

    [Template("StyleTests.xml")]
    public class StyleSetTestThing : UIElement {

        public string marginStyle = "MyMargin";

    }

    public T ComputedValue<T>(UIElement obj, string propertyName) {
        return (T) typeof(UIStyleSet).GetProperty(propertyName).GetValue(obj.style);
    }

    public T DefaultValue<T>(string propertyName) {
        return (T) typeof(DefaultStyleValues_Generated).GetField(propertyName, BindingFlags.Static | BindingFlags.Public).GetValue(null);
    }

    public void CallMethod(object target, string methodName, object arg0, object arg1) {
        target.GetType().GetMethod(methodName).Invoke(target, new object[] {arg0, arg1});
    }

    private void SetStyleValue<T>(UIStyle style, string propertyName, T value) {
        typeof(UIStyle).GetProperty(propertyName).SetValue(style, value);
    }

    private void RunIntTests(Action<string, string, string> TestBody) {
        TestBody(
            nameof(UIStyleSet.SetFlexItemGrow),
            nameof(UIStyleSet.FlexItemGrow),
            nameof(DefaultStyleValues_Generated.FlexItemGrow)
        );

        TestBody(
            nameof(UIStyleSet.SetFlexItemShrink),
            nameof(UIStyleSet.FlexItemShrink),
            nameof(DefaultStyleValues_Generated.FlexItemShrink)
        );
    }

    private void RunSizeTests(Action<string, string, string> TestBody) {
        TestBody(nameof(UIStyleSet.SetMinWidth), nameof(UIStyleSet.MinWidth), nameof(DefaultStyleValues_Generated.MinWidth));
        TestBody(nameof(UIStyleSet.SetMaxWidth), nameof(UIStyleSet.MaxWidth), nameof(DefaultStyleValues_Generated.MaxWidth));
        TestBody(nameof(UIStyleSet.SetPreferredWidth), nameof(UIStyleSet.PreferredWidth), nameof(DefaultStyleValues_Generated.PreferredWidth));

        TestBody(nameof(UIStyleSet.SetMinHeight), nameof(UIStyleSet.MinHeight), nameof(DefaultStyleValues_Generated.MinHeight));
        TestBody(nameof(UIStyleSet.SetMaxHeight), nameof(UIStyleSet.MaxHeight), nameof(DefaultStyleValues_Generated.MaxHeight));
        TestBody(nameof(UIStyleSet.SetPreferredHeight), nameof(UIStyleSet.PreferredHeight), nameof(DefaultStyleValues_Generated.PreferredHeight));
    }

    [Test]
    public void IntProperties_UpdateComputedStyleWithValue() {
        Action<string, string, string> TestBody = (setFnName, computedPropertyName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();
            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            Assert.AreEqual(DefaultValue<int>(defaultName), ComputedValue<int>(root, computedPropertyName));

            CallMethod(root.style, setFnName, ComputedValue<int>(root, computedPropertyName) + 1, StyleState.Normal);

            Assert.AreEqual(DefaultValue<int>(defaultName) + 1, ComputedValue<int>(root, computedPropertyName));
        };

        RunIntTests(TestBody);
    }

    [Test]
    public void IntProperties_UpdateComputedStyleWithUnsetValue() {
        Action<string, string, string> TestBody = (setFnName, computedPropertyName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();
            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            Assert.AreEqual(DefaultValue<int>(defaultName), ComputedValue<int>(root, computedPropertyName));

            CallMethod(root.style, setFnName, DefaultValue<int>(defaultName) + 1, StyleState.Normal);
            CallMethod(root.style, setFnName, null, StyleState.Normal);

            Assert.AreEqual(DefaultValue<int>(defaultName), ComputedValue<int>(root, computedPropertyName));
        };

        RunIntTests(TestBody);
    }

    [Test]
    public void IntProperties_EnterState() {
        Action<string, string, string> TestBody = (setFnName, propName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            CallMethod(root.style, setFnName, DefaultValue<int>(defaultName) + 1, StyleState.Hover);

            Assert.AreEqual(DefaultValue<int>(defaultName), ComputedValue<int>(root, propName));

            root.style.EnterState(StyleState.Hover);

            Assert.AreEqual(DefaultValue<int>(defaultName) + 1, ComputedValue<int>(root, propName));
        };

        RunIntTests(TestBody);
    }

    [Test]
    public void IntProperties_EnterSecondState() {
        Action<string, string, string> TestBody = (setFnName, propName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            CallMethod(root.style, setFnName, DefaultValue<int>(defaultName) + 1, StyleState.Hover);
            CallMethod(root.style, setFnName, DefaultValue<int>(defaultName) + 5, StyleState.Focused);

            root.style.EnterState(StyleState.Hover);

            Assert.AreEqual(DefaultValue<int>(defaultName) + 1, ComputedValue<int>(root, propName));

            root.style.EnterState(StyleState.Focused);

            Assert.AreEqual(DefaultValue<int>(defaultName) + 5, ComputedValue<int>(root, propName));

            root.style.ExitState(StyleState.Focused);

            Assert.AreEqual(DefaultValue<int>(defaultName) + 1, ComputedValue<int>(root, propName));
        };
        RunIntTests(TestBody);
    }

    [Test]
    public void IntProperties_ExitState() {
        Action<string, string, string> TestBody = (setFnName, propName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            CallMethod(root.style, setFnName, DefaultValue<int>(defaultName) + 1, StyleState.Normal);
            CallMethod(root.style, setFnName, DefaultValue<int>(defaultName) + 2, StyleState.Hover);

            Assert.AreEqual(DefaultValue<int>(defaultName) + 1, ComputedValue<int>(root, propName));

            root.style.EnterState(StyleState.Hover);

            Assert.AreEqual(DefaultValue<int>(defaultName) + 2, ComputedValue<int>(root, propName));

            root.style.ExitState(StyleState.Hover);

            Assert.AreEqual(DefaultValue<int>(defaultName) + 1, ComputedValue<int>(root, propName));
        };
        RunIntTests(TestBody);
    }

    [Test]
    public void IntProperties_FromBaseStyle() {
        Action<string, string, string> TestBody = (setFnName, propName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            UIStyle baseStyle = new UIStyle();
            SetStyleValue(baseStyle, propName, 5);

            UIStyleGroup group = new UIStyleGroup() {
                name = "Name",
                normal = new UIStyleRunCommand() {
                    style = baseStyle
                }
            };
            UIStyleGroupContainer container = new UIStyleGroupContainer(0, "Name", StyleType.Shared, new[] {group});
            root.style.AddStyleGroupContainer(container);

            Assert.AreEqual(5, ComputedValue<int>(root, propName));

            root.style.RemoveStyleGroupContainer(container);

            Assert.AreEqual(DefaultValue<int>(defaultName), ComputedValue<int>(root, propName));
        };

        RunIntTests(TestBody);
    }

    [Test]
    public void IntProperties_OverrideBaseStyle() {
        Action<string, string, string> TestBody = (setFnName, propName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            UIStyle baseStyle = new UIStyle();
            SetStyleValue(baseStyle, propName, 5);

            CallMethod(root.style, setFnName, 15, StyleState.Normal);
            UIStyleGroup group = new UIStyleGroup() {
                name = "Name",
                normal = new UIStyleRunCommand() {
                    style = baseStyle
                }
            };
            group.styleType = StyleType.Shared;

            UIStyleGroupContainer container = new UIStyleGroupContainer(0, "Name", StyleType.Shared, new[] {group});
            root.style.AddStyleGroupContainer(container);

            Assert.AreEqual(15, ComputedValue<int>(root, propName));

            root.style.RemoveStyleGroupContainer(container);

            Assert.AreEqual(15, ComputedValue<int>(root, propName));
        };
        RunIntTests(TestBody);
    }

    [Test]
    public void IntProperties_OverrideBaseStyleInState() {
        Action<string, string, string> TestBody = (setFnName, propName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            UIStyle baseStyle = new UIStyle();
            SetStyleValue(baseStyle, propName, 5);

            CallMethod(root.style, setFnName, 15, StyleState.Hover);
            UIStyleGroup group = new UIStyleGroup() {
                name = "Name",
                normal = new UIStyleRunCommand() {
                    style = baseStyle
                }
            };
            UIStyleGroupContainer container = new UIStyleGroupContainer(0, "Name", StyleType.Shared, new[] {group});
            root.style.AddStyleGroupContainer(container);

            Assert.AreEqual(5, ComputedValue<int>(root, propName));

            root.style.EnterState(StyleState.Hover);
            Assert.AreEqual(15, ComputedValue<int>(root, propName));
        };
        RunIntTests(TestBody);
    }

    [Test]
    public void SizeProperties_UpdateComputedValue() {
        Action<string, string, string> TestBody = (setFnName, computedPropertyName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            Assert.AreEqual(DefaultValue<UIMeasurement>(defaultName), ComputedValue<UIMeasurement>(root, computedPropertyName));

            CallMethod(root.style, setFnName, new UIMeasurement(99999), StyleState.Normal);

            Assert.AreEqual(new UIMeasurement(99999), ComputedValue<UIMeasurement>(root, computedPropertyName));
        };

        RunSizeTests(TestBody);
    }

    [Test]
    public void SizeProperties_UpdateComputedStyleWithUnsetValue() {
        Action<string, string, string> TestBody = (setFnName, computedPropertyName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            Assert.AreEqual(DefaultValue<UIMeasurement>(defaultName), ComputedValue<UIMeasurement>(root, computedPropertyName));

            CallMethod(root.style, setFnName, new UIMeasurement(99999), StyleState.Normal);
            CallMethod(root.style, setFnName, null, StyleState.Normal);

            Assert.AreEqual(DefaultValue<UIMeasurement>(defaultName), ComputedValue<UIMeasurement>(root, computedPropertyName));
        };

        RunSizeTests(TestBody);
    }

    [Test]
    public void SizeProperties_EnterState() {
        Action<string, string, string> TestBody = (setFnName, propName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            CallMethod(root.style, setFnName, new UIMeasurement(99999), StyleState.Hover);

            Assert.AreEqual(DefaultValue<UIMeasurement>(defaultName), ComputedValue<UIMeasurement>(root, propName));

            root.style.EnterState(StyleState.Hover);

            Assert.AreEqual(new UIMeasurement(99999), ComputedValue<UIMeasurement>(root, propName));
        };

        RunSizeTests(TestBody);
    }

    [Test]
    public void SizeProperties_EnterSecondState() {
        Action<string, string, string> TestBody = (setFnName, propName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            CallMethod(root.style, setFnName, new UIMeasurement(1000), StyleState.Hover);
            CallMethod(root.style, setFnName, new UIMeasurement(5000), StyleState.Focused);

            root.style.EnterState(StyleState.Hover);

            Assert.AreEqual(new UIMeasurement(1000), ComputedValue<UIMeasurement>(root, propName));

            root.style.EnterState(StyleState.Focused);

            Assert.AreEqual(new UIMeasurement(5000), ComputedValue<UIMeasurement>(root, propName));

            root.style.ExitState(StyleState.Focused);

            Assert.AreEqual(new UIMeasurement(1000), ComputedValue<UIMeasurement>(root, propName));
        };
        RunSizeTests(TestBody);
    }

    [Test]
    public void SizeProperties_ExitState() {
        Action<string, string, string> TestBody = (setFnName, propName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            CallMethod(root.style, setFnName, new UIMeasurement(1000), StyleState.Normal);
            CallMethod(root.style, setFnName, new UIMeasurement(2000), StyleState.Hover);

            Assert.AreEqual(new UIMeasurement(1000), ComputedValue<UIMeasurement>(root, propName));

            root.style.EnterState(StyleState.Hover);

            Assert.AreEqual(new UIMeasurement(2000), ComputedValue<UIMeasurement>(root, propName));

            root.style.ExitState(StyleState.Hover);

            Assert.AreEqual(new UIMeasurement(1000), ComputedValue<UIMeasurement>(root, propName));
        };
        RunSizeTests(TestBody);
    }

    [Test]
    public void SizeProperties_FromBaseStyle() {
        Action<string, string, string> TestBody = (setFnName, propName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            UIStyle baseStyle = new UIStyle();
            SetStyleValue(baseStyle, propName, new UIMeasurement(5000));

            UIStyleGroup group = new UIStyleGroup() {
                name = "Name",
                normal = new UIStyleRunCommand() {
                    style = baseStyle
                }
            };
            UIStyleGroupContainer container = new UIStyleGroupContainer(0, "Name", StyleType.Shared, new[] {group});
            root.style.AddStyleGroupContainer(container);
            Assert.AreEqual(new UIMeasurement(5000), ComputedValue<UIMeasurement>(root, propName));

            root.style.RemoveStyleGroupContainer(container);

            Assert.AreEqual(DefaultValue<UIMeasurement>(defaultName), ComputedValue<UIMeasurement>(root, propName));
        };

        RunSizeTests(TestBody);
    }

    [Test]
    public void SizeProperties_OverrideBaseStyle() {
        Action<string, string, string> TestBody = (setFnName, propName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            UIStyle baseStyle = new UIStyle();
            SetStyleValue(baseStyle, propName, new UIMeasurement(5000));

            CallMethod(root.style, setFnName, new UIMeasurement(1500), StyleState.Normal);
            UIStyleGroup group = new UIStyleGroup() {
                name = "Name",
                normal = new UIStyleRunCommand() {
                    style = baseStyle
                }
            };
            UIStyleGroupContainer container = new UIStyleGroupContainer(0, "Name", StyleType.Shared, new[] {group});
            root.style.AddStyleGroupContainer(container);
            Assert.AreEqual(new UIMeasurement(1500), ComputedValue<UIMeasurement>(root, propName));

            root.style.RemoveStyleGroupContainer(container);

            Assert.AreEqual(new UIMeasurement(1500), ComputedValue<UIMeasurement>(root, propName));
        };
        RunSizeTests(TestBody);
    }

    [Test]
    public void SizeProperties_OverrideBaseStyleInState() {
        Action<string, string, string> TestBody = (setFnName, propName, defaultName) => {
            MockApplication view = MockApplication.Setup<StyleSetTestThing>();

            StyleSetTestThing root = (StyleSetTestThing) view.RootElement;

            UIStyle baseStyle = new UIStyle();
            SetStyleValue(baseStyle, propName, new UIMeasurement(500));

            CallMethod(root.style, setFnName, new UIMeasurement(1500), StyleState.Hover);
            UIStyleGroup group = new UIStyleGroup() {
                name = "Name",
                normal = new UIStyleRunCommand() {
                    style = baseStyle
                }
            };
            UIStyleGroupContainer container = new UIStyleGroupContainer(0, "Name", StyleType.Shared, new[] {group});
            root.style.AddStyleGroupContainer(container);
            Assert.AreEqual(new UIMeasurement(500), ComputedValue<UIMeasurement>(root, propName));

            root.style.EnterState(StyleState.Hover);
            Assert.AreEqual(new UIMeasurement(1500), ComputedValue<UIMeasurement>(root, propName));
        };
        RunSizeTests(TestBody);
    }

    [Test]
    public void ConvertColorToStyleColor() {
        Color32 c = new Color32(128, 128, 128, 1);
        StyleColor styleColor = new StyleColor(c);
        Assert.AreEqual((Color) c, (Color) styleColor);
    }

    [Test]
    public void Inherit_FontSize() {
        MockApplication app = MockApplication.Setup<StyleSetTestThing>();
        StyleSetTestThing root = (StyleSetTestThing) app.RootElement;
        UIStyleSetStateProxy normal = root.style.Normal;
        normal.TextFontSize = 8;
        app.Update();
        Assert.AreEqual(new UIFixedLength(8), root.FindById("group1").style.TextFontSize);
        Assert.AreEqual(new UIFixedLength(8), root.FindById("group2").style.TextFontSize);
        root.FindById("group1").style.SetTextFontSize(12, StyleState.Normal);
        app.Update();
        Assert.AreEqual(new UIFixedLength(12), root.FindById("group1").style.TextFontSize);
        Assert.AreEqual(new UIFixedLength(12), root.FindById("group2").style.TextFontSize);
        root.FindById("group1").style.SetTextFontSize(null, StyleState.Normal);
        app.Update();
        Assert.AreEqual(new UIFixedLength(8), root.FindById("group1").style.TextFontSize);
        Assert.AreEqual(new UIFixedLength(8), root.FindById("group2").style.TextFontSize);
    }

    [Test]
    public void Inherit_TextColor() {
        MockApplication app = MockApplication.Setup<StyleSetTestThing>();
        StyleSetTestThing root = (StyleSetTestThing) app.RootElement;
        UIStyleSetStateProxy normal = root.style.Normal;
        normal.TextColor = Color.red;
        app.Update();
        Assert.AreEqual(Color.red, root.FindById("group1").style.TextColor);
        Assert.AreEqual(Color.red, root.FindById("group2").style.TextColor);
        root.FindById("group1").style.SetTextColor(Color.blue, StyleState.Normal);
        app.Update();
        Assert.AreEqual(Color.blue, root.FindById("group1").style.TextColor);
        Assert.AreEqual(Color.blue, root.FindById("group2").style.TextColor);
        root.FindById("group1").style.SetTextColor(null, StyleState.Normal);
        app.Update();
        Assert.AreEqual(Color.red, root.FindById("group1").style.TextColor);
        Assert.AreEqual(Color.red, root.FindById("group2").style.TextColor);
    }

    [Test]
    public void Inherit_FontAsset() {
        MockApplication app = MockApplication.Setup<StyleSetTestThing>();
        StyleSetTestThing root = (StyleSetTestThing) app.RootElement;
        UIStyleSetStateProxy normal = root.style.Normal;
        // var font0 = new FontAsset(TMP_FontAsset.defaultFontAsset);
        // var font1 = new FontAsset(TMP_FontAsset.defaultFontAsset);
        // normal.TextFontAsset = font0;
        // app.Update();
        // Assert.AreEqual(font0, root.FindById("group1").style.TextFontAsset);
        // Assert.AreEqual(font0, root.FindById("group2").style.TextFontAsset);
        // root.FindById("group1").style.SetTextFontAsset(font1, StyleState.Normal);
        // app.Update();
        // Assert.AreEqual(font1, root.FindById("group1").style.TextFontAsset);
        // Assert.AreEqual(font1, root.FindById("group2").style.TextFontAsset);
        // root.FindById("group1").style.SetTextFontAsset(null, StyleState.Normal);
        // app.Update();
        // Assert.AreEqual(font0, root.FindById("group1").style.TextFontAsset);
        // Assert.AreEqual(font0, root.FindById("group2").style.TextFontAsset);
    }

    [Test]
    public void ApplyDynamicStyleBinding() {
        MockApplication app = MockApplication.Setup<StyleSetTestThing>();
        StyleSetTestThing root = (StyleSetTestThing) app.RootElement;
        app.Update();
        Assert.AreEqual(100, root.FindById("dynamic").style.MarginTop.value);
    }


    [Template("StyleTests.xml#heading")]
    public class HeadingStyleElement : UIElement { }

    [Test]
    public void ApplyHeadingStyles() {
        MockApplication app = MockApplication.Setup<HeadingStyleElement>();
        HeadingStyleElement root = (HeadingStyleElement) app.RootElement;
        app.Update();
        Assert.AreEqual(new UIFixedLength(100), root.FindById("myHeading").style.TextFontSize);
    }

    [Template("StyleTests.xml#inherited")]
    public class InheritStyleElement : UIElement {

        public System.Collections.Generic.List<string> list;

    }

    [Test]
    public void StylesAreInheritedForDynamicallyCreatedElements() {
        MockApplication app = MockApplication.Setup<InheritStyleElement>();
        InheritStyleElement target = app.RootElement as InheritStyleElement;
        target.list = new System.Collections.Generic.List<string>(new[] {
            "one", "two", "three"
        });
        app.Update();
        StyleProperty fontSize = target.style.GetComputedStyleProperty(StylePropertyId.TextFontSize);
        Assert.AreEqual(100, fontSize.AsUIFixedLength.value);
    }

    [Template("StyleTests.xml#inherited_enable")]
    public class InheritEnable : UIElement { }

    [Test]
    public void StylesAreInheritedWhenEnabled() {
        MockApplication app = MockApplication.Setup<InheritEnable>();
        InheritEnable target = app.RootElement as InheritEnable;

        app.Update();

        Assert.AreEqual(new UIFixedLength(18), target[0].style.TextFontSize);

        Assert.IsTrue(target[0].isDisabled);

        target[0].SetEnabled(true);

        Assert.AreEqual(new UIFixedLength(100), target[0].style.TextFontSize);
    }

}