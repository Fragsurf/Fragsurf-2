using System.IO;
using NUnit.Framework;
using UIForia;
using UIForia.Animation;
using UIForia.Compilers.Style;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Parsing.Style;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;
using UIForia.Sound;
using UIForia.Util;
using UnityEngine;
using Application = UnityEngine.Application;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

[TestFixture]
public class StyleSheetCompilerTests {

    public static StyleSheetCompiler NewStyleSheetCompiler() {
        TemplateSettings templateSettings = new TemplateSettings {
            templateResolutionBasePath = Path.Combine(Application.dataPath, "..", "Packages", "UIForia", "Tests")
        };
        return new StyleSheetCompiler(new StyleSheetImporter(templateSettings, new ResourceManager()), new ResourceManager());
    }

    [Test]
    public void CompileEmptyStyle() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        Assert.IsNotNull(styleSheet);
        Assert.AreEqual(0, styleSheet.styleGroupContainers.Length);
    }

    [Test]
    public void CompileBackgroundImage() {
        var nodes = StyleParser.Parse(@"

const path = ""testimg/cat"";
export const img1 = url(@path);
  
style image1 { BackgroundImage = @img1; }
style image2 { BackgroundImage = url(@path); }
style image3 { BackgroundImage = url(testimg/cat); }

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(3, containers.Length);

        Assert.AreEqual("cat", containers[0].groups[0].normal.style.BackgroundImage.name);
        Assert.AreEqual("cat", containers[1].groups[0].normal.style.BackgroundImage.name);
        Assert.AreEqual("cat", containers[2].groups[0].normal.style.BackgroundImage.name);
    }

    [Test]
    public void CompileCursor() {
        var nodes = StyleParser.Parse(@"

const path = ""testimg/Cursor1"";
export const cursor1 = url(@path);

style image1 { Cursor = @cursor1 1 2; }
style image2 { Cursor = url(@path) 20; }
style image3 { Cursor = url(testimg/Cursor1); }

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(3, containers.Length);

        Assert.AreEqual("Cursor1", containers[0].groups[0].normal.style.Cursor.texture.name);
        Assert.AreEqual(new Vector2(1, 2), containers[0].groups[0].normal.style.Cursor.hotSpot);

        Assert.AreEqual("Cursor1", containers[1].groups[0].normal.style.Cursor.texture.name);
        Assert.AreEqual(new Vector2(20, 20), containers[1].groups[0].normal.style.Cursor.hotSpot);

        Assert.AreEqual("Cursor1", containers[2].groups[0].normal.style.Cursor.texture.name);
        Assert.AreEqual(new Vector2(0, 0), containers[2].groups[0].normal.style.Cursor.hotSpot);
    }

    [Test]
    public void CompileVisibility() {
        var nodes = StyleParser.Parse(@"

const v1 = Visible;
export const v2 = hidden;

style visi1 { Visibility = @v1; }
style visi2 { Visibility = @v2; }
style visi3 { Visibility = Visible; }

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(3, containers.Length);

        Assert.AreEqual(Visibility.Visible, containers[0].groups[0].normal.style.Visibility);
        Assert.AreEqual(Visibility.Hidden, containers[1].groups[0].normal.style.Visibility);
        Assert.AreEqual(Visibility.Visible, containers[2].groups[0].normal.style.Visibility);
    }

    [Test]
    public void CompileOverflow() {
        var nodes = StyleParser.Parse(@"

const o1 = hidden;
const o2 = Scroll;

style overflow1 { Overflow = @o1 @o2; }
style overflow2 { Overflow = @o2; }
style overflow3 { OverflowX = @o2; }
style overflow4 { OverflowY = @o1; }
style overflow5 {
    Overflow = hidden; 
    OverflowY = Scroll;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(5, containers.Length);

        Assert.AreEqual(Overflow.Hidden, containers[0].groups[0].normal.style.OverflowX);
        Assert.AreEqual(Overflow.Scroll, containers[0].groups[0].normal.style.OverflowY);

        Assert.AreEqual(Overflow.Scroll, containers[1].groups[0].normal.style.OverflowX);
        Assert.AreEqual(Overflow.Scroll, containers[1].groups[0].normal.style.OverflowY);

        Assert.AreEqual(Overflow.Scroll, containers[2].groups[0].normal.style.OverflowX);
        Assert.AreEqual(Overflow.Unset, containers[2].groups[0].normal.style.OverflowY);

        Assert.AreEqual(Overflow.Unset, containers[3].groups[0].normal.style.OverflowX);
        Assert.AreEqual(Overflow.Hidden, containers[3].groups[0].normal.style.OverflowY);

        Assert.AreEqual(Overflow.Hidden, containers[4].groups[0].normal.style.OverflowX);
        Assert.AreEqual(Overflow.Scroll, containers[4].groups[0].normal.style.OverflowY);
    }

    [Test]
    public void CompileBackgroundColor() {
        var nodes = StyleParser.Parse(@"
            
const alpha = 255;
const redChannel = 255.000;

export const color0 = rgba(@redChannel, 0, 0, @alpha);
            
style myStyle {
    BackgroundColor = @color0;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.AreEqual(Color.red, containers[0].groups[0].normal.style.BackgroundColor);
    }

    [Test]
    public void CreateAttributeGroupsWithMeasurements() {
        var nodes = StyleParser.Parse(@"

export const m1 = 10%;

style myStyle {
    MarginTop = @m1;
    [hover] {
        MarginLeft = 20px;
    }
    [attr:myAttr=""val""] {
        MarginTop = 20px; 
    }
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);
        Assert.AreEqual(2, containers[0].groups.Length);

        Assert.IsTrue(Mathf.Approximately(0.1f, containers[0].groups[0].normal.style.MarginTop.value));
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.style.MarginTop.unit);
        Assert.AreEqual(20, containers[0].groups[0].hover.style.MarginLeft.value);
        Assert.AreEqual(20, containers[0].groups[1].normal.style.MarginTop.value);
    }

    [Test]
    public void UseMarginPropertyShorthand() {
        var nodes = StyleParser.Parse(@"
            
export const m1 = 10%;
export const m2 = @m3;
export const m3 = 10%;
export const m4 = @m2;
            
style myStyle {
    Margin = @m1 @m2 10px @m4;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.AreEqual(0.1f,  containers[0].groups[0].normal.style.MarginTop.value);
        Assert.AreEqual(0.1f, containers[0].groups[0].normal.style.MarginRight.value);
        Assert.AreEqual(10, containers[0].groups[0].normal.style.MarginBottom.value);
        Assert.AreEqual(0.1f, containers[0].groups[0].normal.style.MarginLeft.value);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.style.MarginTop.unit);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.style.MarginRight.unit);
        Assert.AreEqual(UIFixedUnit.Pixel, containers[0].groups[0].normal.style.MarginBottom.unit);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.style.MarginLeft.unit);
    }

    [Test]
    public void UsePaddingPropertyShorthand() {
        var nodes = StyleParser.Parse(@"

export const p1 = 10%;
export const p2 = @p3;
export const p3 = 10%;
export const p4 = @p2;

style myStyle {
    Padding = @p1 @p2 20px @p4;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.IsTrue(Mathf.Approximately(10 * 0.01f, containers[0].groups[0].normal.style.PaddingTop.value));
        Assert.IsTrue(Mathf.Approximately(10 * 0.01f, containers[0].groups[0].normal.style.PaddingRight.value));
        Assert.AreEqual(20, containers[0].groups[0].normal.style.PaddingBottom.value);
        Assert.IsTrue(Mathf.Approximately(10 * 0.01f, containers[0].groups[0].normal.style.PaddingLeft.value));
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.style.PaddingTop.unit);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.style.PaddingRight.unit);
        Assert.AreEqual(UIFixedUnit.Pixel, containers[0].groups[0].normal.style.PaddingBottom.unit);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.style.PaddingLeft.unit);
    }

    [Test]
    public void UseBorderPropertyShorthand() {
        var nodes = StyleParser.Parse(@"

export const b1 = 10%;
export const b2 = @b3;
export const b3 = 10%;
export const b4 = @b2;

style myStyle {
    Border = @b1 @b2 20px @b4;
    BorderColor = black red rgba(100,200,20,250) #ffffff;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.IsTrue(Mathf.Approximately(10 * 0.01f, containers[0].groups[0].normal.style.BorderTop.value));
        Assert.IsTrue(Mathf.Approximately(10 * 0.01f, containers[0].groups[0].normal.style.BorderRight.value));
        Assert.AreEqual(20, containers[0].groups[0].normal.style.BorderBottom.value);
        Assert.IsTrue(Mathf.Approximately(10 * 0.01f, containers[0].groups[0].normal.style.BorderLeft.value));
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.style.BorderTop.unit);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.style.BorderRight.unit);
        Assert.AreEqual(UIFixedUnit.Pixel, containers[0].groups[0].normal.style.BorderBottom.unit);
        Assert.AreEqual(UIFixedUnit.Percent, containers[0].groups[0].normal.style.BorderLeft.unit);

        Assert.AreEqual(Color.black, containers[0].groups[0].normal.style.BorderColorTop);
        Assert.AreEqual(Color.red, containers[0].groups[0].normal.style.BorderColorRight);
        Assert.AreEqual(new Color( 100f / 255f, 200f / 255f,20f / 255f,250f / 255f), containers[0].groups[0].normal.style.BorderColorBottom);
        Assert.AreEqual(Color.white, containers[0].groups[0].normal.style.BorderColorLeft);
    }

    [Test]
    public void CompileVisibilty() {
        var nodes = StyleParser.Parse(@"

const v = hidden;

style myStyle {
    Visibility = visible;
    [attr:disabled=""disabled""] {
        Visibility = @v;
    }
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);
        Assert.AreEqual(2, containers[0].groups.Length);

        Assert.AreEqual(Visibility.Visible, containers[0].groups[0].normal.style.Visibility);
        Assert.AreEqual(Visibility.Hidden, containers[0].groups[1].normal.style.Visibility);
        Assert.AreEqual("disabled", containers[0].groups[1].rule.attributeName);
        Assert.AreEqual("disabled", containers[0].groups[1].rule.attributeValue);
        Assert.AreEqual(false, containers[0].groups[1].rule.invert);
    }

    [Test]
    public void CompileGridItemColAndRowProperties() {
        var nodes = StyleParser.Parse(@"

const rowStart = 2;

style myStyle {
    GridItemX = 0;
    GridItemWidth = 4;
    GridItemY = @rowStart;
    GridItemHeight = 5;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.AreEqual(new GridItemPlacement(0), containers[0].groups[0].normal.style.GridItemX);
        Assert.AreEqual(new GridItemPlacement(4), containers[0].groups[0].normal.style.GridItemWidth);
        Assert.AreEqual(new GridItemPlacement(2), containers[0].groups[0].normal.style.GridItemY);
        Assert.AreEqual(new GridItemPlacement(5), containers[0].groups[0].normal.style.GridItemHeight);
    }

    [Test]
    public void CompileGridAxisAlignmentProperties() {
        var nodes = StyleParser.Parse(@"

const colSelfAlignment = Center;

style myStyle {
    GridLayoutColAlignment = Shrink;
    GridLayoutRowAlignment = fit;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.AreEqual(GridAxisAlignment.Shrink, containers[0].groups[0].normal.style.GridLayoutColAlignment);
        Assert.AreEqual(GridAxisAlignment.Fit, containers[0].groups[0].normal.style.GridLayoutRowAlignment);
    }

    [Test]
    public void CompileGridLayoutDensity() {
        var nodes = StyleParser.Parse(@"

const density = dense;

style myStyle {
    GridLayoutDensity = @density;
    [hover] { GridLayoutDensity = sparse; }
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var containers = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, containers.Length);

        Assert.AreEqual(GridLayoutDensity.Dense, containers[0].groups[0].normal.style.GridLayoutDensity);
        Assert.AreEqual(GridLayoutDensity.Sparse, containers[0].groups[0].hover.style.GridLayoutDensity);
    }

    [Test]
    public void CompileGridLayoutDirection() {
        var nodes = StyleParser.Parse(@"

const dir = Horizontal;

style myStyle {
    GridLayoutDirection = @dir;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup.Length);
        Assert.AreEqual(LayoutDirection.Horizontal, styleGroup[0].groups[0].normal.style.GridLayoutDirection);
    }

    [Test]
    public void CompileFlexLayoutDirection() {
        var nodes = StyleParser.Parse(@"

const dir = Vertical;

style myStyle {
    FlexLayoutDirection = @dir;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup.Length);
        Assert.AreEqual(LayoutDirection.Vertical, styleGroup[0].groups[0].normal.style.FlexLayoutDirection);
    }

//    [Test]
//    public void CompileGridLayoutColTemplate() {
//        var nodes = StyleParser.Parse(@"
//
//const colOne = 1mx;
//
//style myStyle {
//    GridLayoutColTemplate = @colOne 1mx 2fr 480px;
//}
//
//        ".Trim());
//
//        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
//
//        var styleGroup = styleSheet.styleGroupContainers;
//        Assert.AreEqual(1, styleGroup.Length);
//
//        
//        Assert.AreEqual(4, styleGroup[0].groups[0].normal.style.GridLayoutColTemplate.Count);
//        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.MaxContent), styleGroup[0].groups[0].normal.style.GridLayoutColTemplate[0]);
//        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.MaxContent), styleGroup[0].groups[0].normal.style.GridLayoutColTemplate[1]);
//        Assert.AreEqual(new GridTrackSize(2, GridTemplateUnit.FractionalRemaining), styleGroup[0].groups[0].normal.style.GridLayoutColTemplate[2]);
//        Assert.AreEqual(new GridTrackSize(new GridCellDefinition() {
//            
//        })
//    //480, GridTemplateUnit.Pixel), styleGroup[0].groups[0].normal.style.GridLayoutColTemplate[3]);
//    }
//
//    [Test]
//    public void CompileGridLayoutRowTemplate() {
//        var nodes = StyleParser.Parse(@"
//
//const colOne = 1mx;
//
//style myStyle {
//    GridLayoutRowTemplate = @colOne 1mx 2fr 480px;
//}
//
//        ".Trim());
//
//        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
//
//        var styleGroup = styleSheet.styleGroupContainers;
//        Assert.AreEqual(1, styleGroup.Length);
//
//        Assert.AreEqual(4, styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate.Count);
//        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.MaxContent), styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate[0]);
//        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.MaxContent), styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate[1]);
//        Assert.AreEqual(new GridTrackSize(2, GridTemplateUnit.FractionalRemaining), styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate[2]);
//        Assert.AreEqual(new GridTrackSize(480, GridTemplateUnit.Pixel), styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate[3]);
//    }

//    [Test]
//    public void CompileGridLayoutAxisAutoSize() {
//        var nodes = StyleParser.Parse(@"
//const main = 1fr;
//
//style myStyle {
//    GridLayoutColAutoSize = @main;
//    GridLayoutRowAutoSize = 42px;
//}
//        ".Trim());
//
//        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
//
//        var styleGroup = styleSheet.styleGroupContainers;
//        Assert.AreEqual(1, styleGroup.Length);
//
//        Assert.AreEqual(new GridTrackSize(1, GridTemplateUnit.FractionalRemaining), styleGroup[0].groups[0].normal.style.GridLayoutColAutoSize[0]);
//        Assert.AreEqual(new GridTrackSize(42, GridTemplateUnit.Pixel), styleGroup[0].groups[0].normal.style.GridLayoutRowAutoSize[0]);
//    }

    [Test]
    public void CompileGridLayoutGaps() {
        var nodes = StyleParser.Parse(@"
const colGap = 9;

style myStyle {
    GridLayoutColGap = @colGap;
    GridLayoutRowGap = 42.01f;
}
        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup.Length);

        Assert.AreEqual(9, styleGroup[0].groups[0].normal.style.GridLayoutColGap);
        Assert.AreEqual(42.01f, styleGroup[0].groups[0].normal.style.GridLayoutRowGap);
    }

//    [Test]
//    public void CompileGridRepeatWithConstant() {
//        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
//
//        style myStyle {
//            GridLayoutRowTemplate = repeat(3, 200px);
//        }
//
//        ".Trim());
//
//        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
//
//        UIStyleGroupContainer[] styleGroup = styleSheet.styleGroupContainers;
//        Assert.AreEqual(1, styleGroup.Length);
//        Assert.AreEqual(1, styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate.Count);
//
//        GridTrackSize actual = styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate[0];
//
//        Assert.AreEqual(GridTrackSizeType.Repeat, actual.type);
//        Assert.AreEqual(3, actual.value);
//        Assert.AreEqual(1, actual.pattern.Length);
//        Assert.AreEqual(GridTrackSizeType.Value, actual.pattern[0].type);
//        Assert.AreEqual(200f, actual.pattern[0].value);
//        Assert.AreEqual(GridTemplateUnit.Pixel, actual.pattern[0].unit);
//    }
    
//    [Test]
//    public void CompileGridRepeatWithFill() {
//        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
//
//        style myStyle {
//            GridLayoutRowTemplate = repeat(fill, 200px);
//        }
//
//        ".Trim());
//
//        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
//
//        UIStyleGroupContainer[] styleGroup = styleSheet.styleGroupContainers;
//        Assert.AreEqual(1, styleGroup.Length);
//        Assert.AreEqual(1, styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate.Count);
//
//        GridTrackSize actual = styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate[0];
//
//        Assert.AreEqual(GridTrackSizeType.RepeatFill, actual.type);
//        Assert.AreEqual(1, actual.pattern.Length);
//        Assert.AreEqual(GridTrackSizeType.Value, actual.pattern[0].type);
//        Assert.AreEqual(200f, actual.pattern[0].value);
//        Assert.AreEqual(GridTemplateUnit.Pixel, actual.pattern[0].unit);
//    }
//    
//    [Test]
//    public void CompileGridRepeatWithFit() {
//        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
//
//        style myStyle {
//            GridLayoutRowTemplate = repeat(fit, 200px);
//        }
//
//        ".Trim());
//
//        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
//
//        UIStyleGroupContainer[] styleGroup = styleSheet.styleGroupContainers;
//        Assert.AreEqual(1, styleGroup.Length);
//        Assert.AreEqual(1, styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate.Count);
//
//        GridTrackSize actual = styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate[0];
//
//        Assert.AreEqual(GridTrackSizeType.RepeatFit, actual.type);
//        Assert.AreEqual(1, actual.pattern.Length);
//        Assert.AreEqual(GridTrackSizeType.Value, actual.pattern[0].type);
//        Assert.AreEqual(200f, actual.pattern[0].value);
//        Assert.AreEqual(GridTemplateUnit.Pixel, actual.pattern[0].unit);
//    }
    
//    [Test]
//    public void CompileGridRepeatWithFitAndFillAndEverything() {
//        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
//
//        const aSize = 100px;
//
//        style myStyle {
//            GridLayoutRowTemplate = 
//                repeat(fit, @aSize) 
//                repeat(5, 10px 10px minmax(1mx, 10px)) 
//                minmax(1mx, @aSize) 
//                1mx;
//        }
//
//        ".Trim());
//
//        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
//
//        UIStyleGroupContainer[] styleGroup = styleSheet.styleGroupContainers;
//        Assert.AreEqual(1, styleGroup.Length);
//        Assert.AreEqual(4, styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate.Count);
//
//        GridTrackSize actual1 = styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate[0];
//
//        Assert.AreEqual(GridTrackSizeType.RepeatFit, actual1.type);
//        Assert.AreEqual(1, actual1.pattern.Length);
//        Assert.AreEqual(GridTrackSizeType.Value, actual1.pattern[0].type);
//        Assert.AreEqual(100f, actual1.pattern[0].value);
//        Assert.AreEqual(GridTemplateUnit.Pixel, actual1.pattern[0].unit);
//
//        GridTrackSize actual2 = styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate[1];
//
//        Assert.AreEqual(GridTrackSizeType.Repeat, actual2.type);
//        Assert.AreEqual(5, actual2.value);
//        Assert.AreEqual(3, actual2.pattern.Length);
//        // arg 1
//        Assert.AreEqual(GridTrackSizeType.Value, actual2.pattern[0].type);
//        Assert.AreEqual(10f, actual2.pattern[0].value);
//        Assert.AreEqual(GridTemplateUnit.Pixel, actual2.pattern[0].unit);
//        // arg 2
//        Assert.AreEqual(GridTrackSizeType.Value, actual2.pattern[1].type);
//        Assert.AreEqual(10f, actual2.pattern[1].value);
//        Assert.AreEqual(GridTemplateUnit.Pixel, actual2.pattern[1].unit);
//        // arg 3
//        Assert.AreEqual(GridTrackSizeType.MinMax, actual2.pattern[2].type);
//        Assert.AreEqual(2, actual2.pattern[2].pattern.Length);
//        Assert.AreEqual(1, actual2.pattern[2].pattern[0].value);
//        Assert.AreEqual(GridTemplateUnit.MaxContent, actual2.pattern[2].pattern[0].unit);
//        Assert.AreEqual(10f, actual2.pattern[2].pattern[1].value);
//        Assert.AreEqual(GridTemplateUnit.Pixel, actual2.pattern[2].pattern[1].unit);
//        
//        GridTrackSize actual3 = styleGroup[0].groups[0].normal.style.GridLayoutRowTemplate[2];
//
//        Assert.AreEqual(GridTrackSizeType.MinMax, actual3.type);
//        Assert.AreEqual(2, actual3.pattern.Length);
//        Assert.AreEqual(GridTrackSizeType.Value, actual3.pattern[0].type);
//        Assert.AreEqual(1, actual3.pattern[0].value);
//        Assert.AreEqual(GridTemplateUnit.MaxContent, actual3.pattern[0].unit);
//        
//        Assert.AreEqual(GridTrackSizeType.Value, actual3.pattern[1].type);
//        Assert.AreEqual(100f, actual3.pattern[1].value);
//        Assert.AreEqual(GridTemplateUnit.Pixel, actual3.pattern[1].unit);
//    }
    
    [Test]
    public void CompileFlexProperties() {
        var nodes = StyleParser.Parse(@"
export const wrap = WrapHorizontal;
export const grow = 1;

style myStyle {
    FlexItemGrow = @grow;
    FlexItemShrink = 0;
    FlexLayoutWrap = @wrap;
}
        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup.Length);

        Assert.AreEqual(1, styleGroup[0].groups[0].normal.style.FlexItemGrow);
        Assert.AreEqual(0, styleGroup[0].groups[0].normal.style.FlexItemShrink);
    }

    [Test]
    public void CompileBorder() {
        var nodes = StyleParser.Parse(@"
export const brtl = 1px;
export const brtr = 2%;
export const brbr = 3vw;
export const brbl = 4em;

style border1 {
    Border = @brtl @brtr @brbr @brbl;
}

style border2 {
    Border = @brtl 20px @brbr;
}

style border3 {
    Border = @brtl @brtr;
}

style border4 {
    Border = 5px;
}
style border5 {
    BorderTop = 1px;
    BorderRight = 20vh;
    BorderBottom = 2em;
    BorderLeft = 4px;
}
        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(5, styleGroup.Length);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[0].groups[0].normal.style.BorderTop);
        Assert.AreEqual(new UIFixedLength(0.02f, UIFixedUnit.Percent), styleGroup[0].groups[0].normal.style.BorderRight);
        Assert.AreEqual(new UIFixedLength(3, UIFixedUnit.ViewportWidth), styleGroup[0].groups[0].normal.style.BorderBottom);
        Assert.AreEqual(new UIFixedLength(4, UIFixedUnit.Em), styleGroup[0].groups[0].normal.style.BorderLeft);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[1].groups[0].normal.style.BorderTop);
        Assert.AreEqual(new UIFixedLength(20), styleGroup[1].groups[0].normal.style.BorderRight);
        Assert.AreEqual(new UIFixedLength(3, UIFixedUnit.ViewportWidth), styleGroup[1].groups[0].normal.style.BorderBottom);
        Assert.AreEqual(new UIFixedLength(20), styleGroup[1].groups[0].normal.style.BorderLeft);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[2].groups[0].normal.style.BorderTop);
        Assert.AreEqual(new UIFixedLength(0.02f, UIFixedUnit.Percent), styleGroup[2].groups[0].normal.style.BorderRight);
        Assert.AreEqual(new UIFixedLength(1), styleGroup[2].groups[0].normal.style.BorderBottom);
        Assert.AreEqual(new UIFixedLength(0.02f, UIFixedUnit.Percent), styleGroup[2].groups[0].normal.style.BorderLeft);

        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.style.BorderTop);
        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.style.BorderRight);
        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.style.BorderBottom);
        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.style.BorderLeft);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[4].groups[0].normal.style.BorderTop);
        Assert.AreEqual(new UIFixedLength(20, UIFixedUnit.ViewportHeight), styleGroup[4].groups[0].normal.style.BorderRight);
        Assert.AreEqual(new UIFixedLength(2, UIFixedUnit.Em), styleGroup[4].groups[0].normal.style.BorderBottom);
        Assert.AreEqual(new UIFixedLength(4), styleGroup[4].groups[0].normal.style.BorderLeft);
    }

    [Test]
    public void CompilBorderRadius() {
        var nodes = StyleParser.Parse(@"
export const brtl = 1px;
export const brtr = 2%;
export const brbr = 3vw;
export const brbl = 4em;

style border1 {
    BorderRadius = @brtl @brtr @brbr @brbl;
}

style border2 {
    BorderRadius = @brtl 20px @brbr;
}

style border3 {
    BorderRadius = @brtl @brtr;
}

style border4 {
    BorderRadius = 5px;
}
style border5 {
    BorderRadiusTopLeft = 1px;
    BorderRadiusTopRight = 20vh;
    BorderRadiusBottomRight = 2em;
    BorderRadiusBottomLeft = 4px;
}
        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(5, styleGroup.Length);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[0].groups[0].normal.style.BorderRadiusTopLeft);
        Assert.AreEqual(new UIFixedLength(0.02f, UIFixedUnit.Percent), styleGroup[0].groups[0].normal.style.BorderRadiusTopRight);
        Assert.AreEqual(new UIFixedLength(3, UIFixedUnit.ViewportWidth), styleGroup[0].groups[0].normal.style.BorderRadiusBottomRight);
        Assert.AreEqual(new UIFixedLength(4, UIFixedUnit.Em), styleGroup[0].groups[0].normal.style.BorderRadiusBottomLeft);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[1].groups[0].normal.style.BorderRadiusTopLeft);
        Assert.AreEqual(new UIFixedLength(20), styleGroup[1].groups[0].normal.style.BorderRadiusTopRight);
        Assert.AreEqual(new UIFixedLength(3, UIFixedUnit.ViewportWidth), styleGroup[1].groups[0].normal.style.BorderRadiusBottomRight);
        Assert.AreEqual(new UIFixedLength(20), styleGroup[1].groups[0].normal.style.BorderRadiusBottomLeft);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[2].groups[0].normal.style.BorderRadiusTopLeft);
        Assert.AreEqual(new UIFixedLength(0.02f, UIFixedUnit.Percent), styleGroup[2].groups[0].normal.style.BorderRadiusTopRight);
        Assert.AreEqual(new UIFixedLength(1), styleGroup[2].groups[0].normal.style.BorderRadiusBottomRight);
        Assert.AreEqual(new UIFixedLength(0.02f, UIFixedUnit.Percent), styleGroup[2].groups[0].normal.style.BorderRadiusBottomLeft);

        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.style.BorderRadiusTopLeft);
        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.style.BorderRadiusTopRight);
        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.style.BorderRadiusBottomRight);
        Assert.AreEqual(new UIFixedLength(5), styleGroup[3].groups[0].normal.style.BorderRadiusBottomLeft);

        Assert.AreEqual(new UIFixedLength(1), styleGroup[4].groups[0].normal.style.BorderRadiusTopLeft);
        Assert.AreEqual(new UIFixedLength(20, UIFixedUnit.ViewportHeight), styleGroup[4].groups[0].normal.style.BorderRadiusTopRight);
        Assert.AreEqual(new UIFixedLength(2, UIFixedUnit.Em), styleGroup[4].groups[0].normal.style.BorderRadiusBottomRight);
        Assert.AreEqual(new UIFixedLength(4), styleGroup[4].groups[0].normal.style.BorderRadiusBottomLeft);
    }

    [Test]
    public void CompileTransformPosition() {
        var nodes = StyleParser.Parse(@"
export const x = 20sw;
export const y = 10cah;

style trans1 { TransformPosition = @x @y; }
style trans2 { TransformPosition = @x; }
style trans3 { TransformPositionX = @x; }
style trans4 { TransformPositionY = 15h; }

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(4, styleGroup.Length);

        Assert.AreEqual(new OffsetMeasurement(20, OffsetMeasurementUnit.ScreenWidth), styleGroup[0].groups[0].normal.style.TransformPositionX);
        Assert.AreEqual(new OffsetMeasurement(10, OffsetMeasurementUnit.ContentAreaHeight), styleGroup[0].groups[0].normal.style.TransformPositionY);

        Assert.AreEqual(new OffsetMeasurement(20, OffsetMeasurementUnit.ScreenWidth), styleGroup[1].groups[0].normal.style.TransformPositionX);
        Assert.AreEqual(new OffsetMeasurement(20, OffsetMeasurementUnit.ScreenWidth), styleGroup[1].groups[0].normal.style.TransformPositionY);

        Assert.AreEqual(new OffsetMeasurement(20, OffsetMeasurementUnit.ScreenWidth), styleGroup[2].groups[0].normal.style.TransformPositionX);
        Assert.AreEqual(OffsetMeasurement.Unset, styleGroup[2].groups[0].normal.style.TransformPositionY);

        Assert.AreEqual(OffsetMeasurement.Unset, styleGroup[3].groups[0].normal.style.TransformPositionX);
        Assert.AreEqual(new OffsetMeasurement(15, OffsetMeasurementUnit.ActualHeight), styleGroup[3].groups[0].normal.style.TransformPositionY);
    }

    [Test]
    public void CompileTransformProperties() {
        var nodes = StyleParser.Parse(@"
export const x = 1;
export const y = 2;

style trans1 { TransformScale = @x @y; }
style trans2 { TransformScaleX = 3; }
style trans3 { TransformScaleY = 4; }

const pivot = 10%;

style pivot1 { TransformPivot = @pivot 10px; }
style pivot2 { TransformPivotX = @pivot; }
style pivot3 { TransformPivotY = 20px; }

style rotate1 { TransformRotation = 90; }

const pivotOffset = PivotOffset;


        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);

        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(1, styleGroup[0].groups[0].normal.style.TransformScaleX);
        Assert.AreEqual(2, styleGroup[0].groups[0].normal.style.TransformScaleY);

        Assert.AreEqual(3, styleGroup[1].groups[0].normal.style.TransformScaleX);
        Assert.AreEqual(float.NaN, styleGroup[1].groups[0].normal.style.TransformScaleY);

        Assert.AreEqual(float.NaN, styleGroup[2].groups[0].normal.style.TransformScaleX);
        Assert.AreEqual(4, styleGroup[2].groups[0].normal.style.TransformScaleY);

        Assert.AreEqual(new UIFixedLength(0.1f, UIFixedUnit.Percent), styleGroup[3].groups[0].normal.style.TransformPivotX);
        Assert.AreEqual(new UIFixedLength(10), styleGroup[3].groups[0].normal.style.TransformPivotY);

        Assert.AreEqual(new UIFixedLength(0.1f, UIFixedUnit.Percent), styleGroup[4].groups[0].normal.style.TransformPivotX);
        Assert.AreEqual(UIFixedLength.Unset, styleGroup[4].groups[0].normal.style.TransformPivotY);

        Assert.AreEqual(UIFixedLength.Unset, styleGroup[5].groups[0].normal.style.TransformPivotX);
        Assert.AreEqual(new UIFixedLength(20), styleGroup[5].groups[0].normal.style.TransformPivotY);

        Assert.AreEqual(90, styleGroup[6].groups[0].normal.style.TransformRotation);

    
    }

    [Test]
    public void CompileSizes() {
        var nodes = StyleParser.Parse(@"
export const x = 1pca;
export const y = 2;

style size1 { 
    MinWidth = @x;
    MinHeight = 300px;
    PreferredWidth = 20px;
    PreferredHeight = 1000px;
    MaxWidth = 400px;
    MaxHeight = @y;
}
style size2 { 
    PreferredSize = 1000px 1111px;
    MinSize = 200px;
    MaxSize = 1500px 1200px;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        var styleContainer = styleSheet.styleGroupContainers;
        Assert.AreEqual(new UIMeasurement(1, UIMeasurementUnit.ParentContentArea), styleContainer[0].groups[0].normal.style.MinWidth);
        Assert.AreEqual(new UIMeasurement(300), styleContainer[0].groups[0].normal.style.MinHeight);
        Assert.AreEqual(new UIMeasurement(20), styleContainer[0].groups[0].normal.style.PreferredWidth);
        Assert.AreEqual(new UIMeasurement(1000), styleContainer[0].groups[0].normal.style.PreferredHeight);
        Assert.AreEqual(new UIMeasurement(400), styleContainer[0].groups[0].normal.style.MaxWidth);
        Assert.AreEqual(new UIMeasurement(2), styleContainer[0].groups[0].normal.style.MaxHeight);

        Assert.AreEqual(new UIMeasurement(1000), styleContainer[1].groups[0].normal.style.PreferredWidth);
        Assert.AreEqual(new UIMeasurement(1111), styleContainer[1].groups[0].normal.style.PreferredHeight);
        Assert.AreEqual(new UIMeasurement(200), styleContainer[1].groups[0].normal.style.MinWidth);
        Assert.AreEqual(new UIMeasurement(200), styleContainer[1].groups[0].normal.style.MinHeight);
        Assert.AreEqual(new UIMeasurement(1500), styleContainer[1].groups[0].normal.style.MaxWidth);
        Assert.AreEqual(new UIMeasurement(1200), styleContainer[1].groups[0].normal.style.MaxHeight);
    }

    [Test]
    public void CompileAnchoring() {
        var nodes = StyleParser.Parse(@"
export const layout = Flex;

style anchoring { 
    LayoutType = @layout;
    LayoutBehavior = Ignored;
    ZIndex = 3;
    RenderLayer = Screen;
    RenderLayerOffset = 22;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(LayoutBehavior.Ignored, styleGroup[0].groups[0].normal.style.LayoutBehavior);
        Assert.AreEqual(3, styleGroup[0].groups[0].normal.style.ZIndex);
        Assert.AreEqual(RenderLayer.Screen, styleGroup[0].groups[0].normal.style.RenderLayer);
        Assert.AreEqual(22, styleGroup[0].groups[0].normal.style.RenderLayerOffset);
    }

    [Test]
    public void CompileText() {
        // note: because of possible spaces in paths we have to support string values for urls
        var nodes = StyleParser.Parse(@"
export const red = red;

style teXt { 
    TextColor = @red;
    TextFontAsset = url(""Fonts/GothamNarrow-Medium SDF"");
    TextFontStyle = bold italic superscript underline highlight smallcaps;
    TextFontSize = 14;
    TextAlignment = Center;
}

        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(Color.red, styleGroup[0].groups[0].normal.style.TextColor);
        Assert.AreEqual("GothamNarrow-Medium SDF", styleGroup[0].groups[0].normal.style.TextFontAsset.name);
        Assert.AreEqual(FontStyle.Normal
                        | FontStyle.Bold
                        | FontStyle.Italic
                        | FontStyle.Underline, styleGroup[0].groups[0].normal.style.TextFontStyle);
        Assert.AreEqual(TextAlignment.Center, styleGroup[0].groups[0].normal.style.TextAlignment);
        Assert.AreEqual(new UIFixedLength(14), styleGroup[0].groups[0].normal.style.TextFontSize);
    }

    [Test]
    public void CompileImport() {
        // note: because of possible spaces in paths we have to support string values for urls
        var nodes = StyleParser.Parse(@"
import ""Data/Styles/ImportFromMe.style"" as importedThings;

style xyz {
    BackgroundColor = @importedThings.colorRed;
}
        ".Trim());

        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        var styleGroup = styleSheet.styleGroupContainers;
        Assert.AreEqual(Color.red, styleGroup[0].groups[0].normal.style.BackgroundColor);
    }

    [Test]
    public void StyleSheetContainers() {
        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
          export const red = red;

          style styleRoot {
               
              TextColor = @red;
              TextFontAsset = url(""Gotham-Medium SDF"");
              TextFontStyle = bold italic superscript underline highlight smallcaps;
              TextFontSize = 14;
              TextAlignment = Center;
              [attr:attr0] {
              
                BackgroundColor = #ff0000aa;

              }

              [hover] {
                TextFontSize = 18;
              }
          }

        ".Trim());
        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        Assert.AreEqual(1, styleSheet.styleGroupContainers.Length);
        Assert.AreEqual(2, styleSheet.styleGroupContainers[0].groups.Length);
    }

    [Test]
    public void CompileAnimation() {
        var nodes = StyleParser.Parse(@"
            animation anim1 {
                [keyframes] {
                    0% { 
                        BackgroundColor = red; 
                        BackgroundColor = red; 
                    }
                    50% {
                        TextFontSize = 11;
                        BackgroundColor = green; 
                    }
                    60% {
                        PreferredSize = 40px, 30px;
                    }
                    100% {
                         BackgroundColor = green; 
                    }
                }
            }
        ".Trim());
        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        Assert.AreEqual(1, styleSheet.animations.Length);
        AnimationData animationData = styleSheet.animations[0];
        Assert.AreEqual("anim1", animationData.name);
        Assert.AreEqual(4, animationData.frames.Count);
        StyleAnimationKeyFrame frame0 = animationData.frames[0];
        Assert.AreEqual(1, frame0.properties.Count);
        Assert.AreEqual(0, frame0.key);
        Assert.AreEqual(StylePropertyId.BackgroundColor, frame0.properties[0].propertyId);
        Assert.AreEqual(Color.red, frame0.properties[0].styleProperty.AsColor);

        StyleAnimationKeyFrame frame1 = animationData.frames[1];
        Assert.AreEqual(2, frame1.properties.Count);
        Assert.AreEqual(0.5f, frame1.key);
        Assert.AreEqual(StylePropertyId.TextFontSize, frame1.properties[0].propertyId);
        Assert.AreEqual(StylePropertyId.BackgroundColor, frame1.properties[1].propertyId);

        StyleAnimationKeyFrame frame2 = animationData.frames[2];
        Assert.AreEqual(2, frame2.properties.Count);
        Assert.AreEqual(0.6f, frame2.key);
        Assert.AreEqual(StylePropertyId.PreferredWidth, frame2.properties[0].propertyId);
        Assert.AreEqual(StylePropertyId.PreferredHeight, frame2.properties[1].propertyId);

        StyleAnimationKeyFrame frame3 = animationData.frames[3];
        Assert.AreEqual(1, frame3.properties.Count);
        Assert.AreEqual(1, frame3.key);
        Assert.AreEqual(StylePropertyId.BackgroundColor, frame3.properties[0].propertyId);
    }

    [Test]
    public void CompileAnimationOptions() {
        var nodes = StyleParser.Parse(@"
            animation anim1 {

                [options] {
                    delay = 1000;
                    duration = 3000;
                    timingFunction = SineEaseOut;
                }

                [keyframes] {
                    0% { 
                        BackgroundColor = red; 
                        BackgroundColor = red; 
                    }
                    50% {
                        TextFontSize = 11;
                        BackgroundColor = green; 
                    }
                    60% {
                        PreferredSize = 40px, 30px;
                    }
                    100% {
                         BackgroundColor = green; 
                    }
                }
            }
        ".Trim());
        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        Assert.AreEqual(1, styleSheet.animations.Length);
        AnimationData animationData = styleSheet.animations[0];
        Assert.AreEqual(new UITimeMeasurement(1000), animationData.options.delay);
        Assert.AreEqual(new UITimeMeasurement(3000), animationData.options.duration);
        Assert.AreEqual(EasingFunction.SineEaseOut, animationData.options.timingFunction);
    }
    
    [Test]
    public void ReferenceAnimationInStyle() {
        var nodes = StyleParser.Parse(@"
            animation anim1 {
                [options] {
                    duration = 500;
                }

                [keyframes] {
                    0% { BackgroundColor = @regularColor; }
                    20% { BackgroundColor = yellow; }
                    80% { BackgroundColor = yellow; }
                    100% { BackgroundColor = @regularColor;}
                }
            }

            style someStyle {
                run animation(anim1);
                [hover] {
                    run animation(anim1);
                }
            }

            const regularColor = red;
        ".Trim());
        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        Assert.AreEqual(1, styleSheet.styleGroupContainers.Length);
        UIStyleGroupContainer container = styleSheet.styleGroupContainers[0];
        Assert.AreEqual("someStyle", container.name);
        Assert.AreEqual(1, container.groups[0].normal.runCommands.Count);
        Assert.AreEqual(1, container.groups[0].hover.runCommands.Count);
    }

//    [Test]
//    public void CompileBackgroundImageFromSpriteAtlas() {
//        var nodes = StyleParser.Parse(@"
//            style fromatlas {
//                BackgroundImage = url(""/some/image"", ""spriteName1""); 
//            }
//        ".Trim());
//        
//        
//        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
//        
//        var styleContainer = styleSheet.styleGroupContainers;
//        Assert.IsInstanceOf<Sprite>(styleContainer[0].groups[0].normal.style.BackgroundImage);
//    }
// Pitch = 0.4;
// PitchRange = range(0.1, 0.5);
// Tempo = 23.9;
// Duration = 50%; //%
// Duration = 2s; //%
// Iterations = 1; 
// Iterations = Infinite; // -1
// MixerGroup = stringhere;
// MixerGroup = "Master Group 1";
// Volume 
    [Test]
    public void RunSound() {
        var nodes = StyleParser.Parse(@"
                sound notification {
                    Asset = ""sounds/notification1"";
                    MixerGroup = ""Master Group 1"";
                    Duration = 2s;
                    Iterations = Infinite;
                    Pitch = -0.4;
                    PitchRange = range(0.1, 0.4);
                    Tempo = 23.9;
                    Volume = 0.9;
                }

                style someStyle {
                    run sound(notification);
                }
            ".Trim());
        StyleSheet styleSheet = NewStyleSheetCompiler().Compile("test", nodes);
        Assert.AreEqual(1, styleSheet.styleGroupContainers.Length);
        UIStyleGroupContainer container = styleSheet.styleGroupContainers[0];
        Assert.AreEqual("someStyle", container.name);
        Assert.AreEqual(1, container.groups[0].normal.runCommands.Count);
        
        Assert.AreEqual(1, styleSheet.sounds.Length);
        UISoundData soundData = styleSheet.sounds[0];
        
        Assert.AreEqual("sounds/notification1", soundData.asset);
        Assert.AreEqual("Master Group 1", soundData.mixerGroup);
        Assert.AreEqual(UITimeMeasurementUnit.Seconds, soundData.duration.unit);
        Assert.AreEqual(2f, soundData.duration.value);
        Assert.AreEqual(-1, soundData.iterations);
        Assert.AreEqual(-0.4f, soundData.pitch);
        Assert.AreEqual(0.1f, soundData.pitchRange.Min);
        Assert.AreEqual(0.4f, soundData.pitchRange.Max);
        Assert.AreEqual(23.9f, soundData.tempo);
        Assert.AreEqual(0.9f, soundData.volume);
    }
}
