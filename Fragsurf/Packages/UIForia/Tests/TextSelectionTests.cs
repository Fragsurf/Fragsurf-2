using NUnit.Framework;
using UIForia.Text;

[TestFixture]
public class TextSelectionTests {

    private SelectionRange GetSelectionFromCharacters(ref string input) {
        int idx0 = input.IndexOf('|');
        input = input.Remove(idx0, 1);
        int idx1 = input.IndexOf('|');
        input = input.Remove(idx1, 1);
        return new SelectionRange(idx0, idx1);
    }
    
    private SelectionRange GetCursorFromCharacter(ref string input, bool defaultToBefore = false) {
        int idx = input.IndexOf('|');
        if (idx < 0) {
            if (defaultToBefore) {
                return new SelectionRange(0);
            }
            return new SelectionRange(input.Length);
        }

        if (idx == input.Length - 1) {
            input = input.Remove(idx, 1);  
            return new SelectionRange(input.Length);
        }
        
        input = input.Remove(idx, 1);
        SelectionRange range = new SelectionRange(idx);
        return range;
    }

    [Test]
    public void AppendTextToEmptyString() {
        string text = "|";

        SelectionRange range = GetCursorFromCharacter(ref text);

        string toAdd = "this is added";

        string result = SelectionRangeUtil.InsertText(text, ref range, toAdd);

        Assert.AreEqual(toAdd, result);
        AssertRangesEqual(new SelectionRange(toAdd.Length), range);
    }
    
    [Test]
    public void AppendTextToNonEmptyString() {
        string text = "CONTENT |";

        SelectionRange range = GetCursorFromCharacter(ref text);

        string toAdd = "this is added";

        string result = SelectionRangeUtil.InsertText(text, ref range, toAdd);

        Assert.AreEqual(text + toAdd, result);
        AssertRangesEqual(new SelectionRange((text + toAdd).Length), range);
    }

    [Test]
    public void PrependTextToNonEmptyString() {
        string text = "|CONTENT";

        SelectionRange range = GetCursorFromCharacter(ref text);

        string toAdd = "this is added";

        string result = SelectionRangeUtil.InsertText(text, ref range, toAdd);

        Assert.AreEqual(toAdd + text, result);
        AssertSelection(result, range, "this is added|CONTENT");
    }

    [Test]
    public void InsertTextToNonEmptyString() {
        string text = "INSERT|HERE";

        SelectionRange range = GetCursorFromCharacter(ref text);

        string toAdd = "this is added";

        string expected = "INSERTthis is addedHERE";
        string result = SelectionRangeUtil.InsertText(text, ref range, toAdd);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "INSERTthis is added|HERE");
    }

    [Test]
    public void InsertWhitespaceToNonEmptyString() {
        string text = "Randomword|";

        SelectionRange range = GetCursorFromCharacter(ref text);

        string toAdd = " ";

        string expected = "Randomword another";
        string result = SelectionRangeUtil.InsertText(text, ref range, toAdd);
        result = SelectionRangeUtil.InsertText(result, ref range, "another");

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "Randomword another|");
    }

    [Test]
    public void DeleteTextRangeForwardFromStart() {
        string text = "|Delete|Keep";

        SelectionRange range = GetSelectionFromCharacters(ref text);
        
        string expected = "Keep";
        string result = SelectionRangeUtil.DeleteTextForwards(text, ref range);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "|Keep");
    }
    
    [Test]
    public void DeleteTextRangeForwardFromEnd() {
        string text = "Keep|Delete|";

        SelectionRange range = GetSelectionFromCharacters(ref text);
        
        string expected = "Keep";
        string result = SelectionRangeUtil.DeleteTextForwards(text, ref range);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "Keep|");
        
        string toAdd = " ";
        result = SelectionRangeUtil.InsertText(result, ref range, toAdd);

        Assert.AreEqual("Keep ", result);
        AssertSelection(result, range, "Keep |");
    }
    
    [Test]
    public void DeleteTextRangeForwardFromMiddle() {
        string text = "Keep|Delete|Keep";

        SelectionRange range = GetSelectionFromCharacters(ref text);
        
        string expected = "KeepKeep";
        string result = SelectionRangeUtil.DeleteTextForwards(text, ref range);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "Keep|Keep");
    }
    
    [Test]
    public void DeleteTextForwardFromStart() {
        string text = "|AKeep";

        SelectionRange range = GetCursorFromCharacter(ref text);
        
        string expected = "Keep";
        string result = SelectionRangeUtil.DeleteTextForwards(text, ref range);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "|Keep");
    }
    
    [Test]
    public void DeleteTextFromMiddle() {
        string text = "Keep|DKeep";

        SelectionRange range = GetCursorFromCharacter(ref text);
        
        string expected = "KeepKeep";
        string result = SelectionRangeUtil.DeleteTextForwards(text, ref range);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "Keep|Keep");
    }
    
    [Test]
    public void DeleteTextForwardFromEnd() {
        string text = "Keep|";

        SelectionRange range = GetCursorFromCharacter(ref text);
        
        string expected = "Keep";
        string result = SelectionRangeUtil.DeleteTextForwards(text, ref range);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "Keep|");
    }
    
    [Test]
    public void DeleteTextBackwardFromStart() {
        string text = "|Keep";

        SelectionRange range = GetCursorFromCharacter(ref text);
        
        string expected = "Keep";
        string result = SelectionRangeUtil.DeleteTextBackwards(text, ref range);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "|Keep");
    }
    
    [Test]
    public void DeleteTextBackwardFromMiddle() {
        string text = "KeepD|Keep";

        SelectionRange range = GetCursorFromCharacter(ref text);
        
        string expected = "KeepKeep";
        string result = SelectionRangeUtil.DeleteTextBackwards(text, ref range);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "Keep|Keep");
    }
    
    [Test]
    public void DeleteTextBackwardFromEnd() {
        string text = "KeepA|";

        SelectionRange range = GetCursorFromCharacter(ref text);
        
        string expected = "Keep";
        string result = SelectionRangeUtil.DeleteTextBackwards(text, ref range);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "Keep|");
    }
    
    [Test]
    public void DeleteTextSelectionOnInsert() {
        string text = "Keep|Delete|Keep";

        SelectionRange range = GetSelectionFromCharacters(ref text);
        
        string expected = "KeepInsertedKeep";
        string result = SelectionRangeUtil.InsertText(text, ref range, "Inserted");

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "KeepInserted|Keep");
    }
    
    [Test]
    public void DeleteTextRangeBackwardFromStart() {
        string text = "|Delete|Keep";

        SelectionRange range = GetSelectionFromCharacters(ref text);
        
        string expected = "Keep";
        string result = SelectionRangeUtil.DeleteTextBackwards(text, ref range);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "|Keep");
    }
    
    [Test]
    public void DeleteTextRangeBackwardFromEnd() {
        string text = "Keep|Delete|";

        SelectionRange range = GetSelectionFromCharacters(ref text);
        
        string expected = "Keep";
        string result = SelectionRangeUtil.DeleteTextBackwards(text, ref range);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "Keep|");
    }
    
    [Test]
    public void DeleteTextRangeBackwardFromMiddle() {
        string text = "Keep|Delete|Keep";

        SelectionRange range = GetSelectionFromCharacters(ref text);
        
        string expected = "KeepKeep";
        string result = SelectionRangeUtil.DeleteTextBackwards(text, ref range);

        Assert.AreEqual(expected, result);
        AssertSelection(result, range, "Keep|Keep");
    }
    
    
    public void AssertSelection(string result, in SelectionRange range, string expected) {
        int cursorIdx = expected.IndexOf('|');
        
        if (cursorIdx >= 0) {
            expected = expected.Remove(cursorIdx, 1);
        }

        Assert.AreEqual(expected, result);
        AssertRangesEqual(new SelectionRange(cursorIdx), range);
    }
    
    public void AssertRangesEqual(in SelectionRange expected, in SelectionRange actual) {
        Assert.AreEqual(expected.cursorIndex, actual.cursorIndex);
        Assert.AreEqual(expected.selectIndex, actual.selectIndex);
    }

}