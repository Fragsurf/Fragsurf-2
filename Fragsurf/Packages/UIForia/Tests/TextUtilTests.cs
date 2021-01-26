using NUnit.Framework;
using UIForia.Text;
using UIForia.Util;


[TestFixture]
public class TextUtilTests {

    [Test]
    public void BreakCharacterGroupsIntoWords() {
        string input = "some text";
        StructList<WordInfo> result = TextUtil.BreakIntoWords(input.ToCharArray());

        WordInfo[] expected = {
            new WordInfo() {
                type = WordType.Normal,
                charStart = 0,
                charEnd = 4
            },
            new WordInfo() {
                type = WordType.Whitespace,
                charStart = 4,
                charEnd = 5
            },
            new WordInfo() {
                type = WordType.Normal,
                charStart = 5,
                charEnd = 9
            }
        };

        Assert.AreEqual(expected.Length, result.size);

        for (int i = 0; i < result.size; i++) {
            Assert.AreEqual(expected[i], result.array[i]);
        }
    }

    [Test]
    public void BreakCharacterGroupsIntoWordsWithNewLine() {
        string input = "some\ntext";
        StructList<WordInfo> result = TextUtil.BreakIntoWords(input.ToCharArray());

        WordInfo[] expected = {
            new WordInfo() {
                type = WordType.Normal,
                charStart = 0,
                charEnd = 4
            },
            new WordInfo() {
                type = WordType.NewLine,
                charStart = 4,
                charEnd = 5
            },
            new WordInfo() {
                type = WordType.Normal,
                charStart = 5,
                charEnd = 9
            }
        };

        Assert.AreEqual(expected.Length, result.size);

        for (int i = 0; i < result.size; i++) {
            Assert.AreEqual(expected[i], result.array[i]);
        }
    }

    [Test]
    public void BreakCharacterGroupsIntoWordsWithSequentialNewLine() {
        string input = "some\n\ntext";
        StructList<WordInfo> result = TextUtil.BreakIntoWords(input.ToCharArray());

        WordInfo[] expected = {
            new WordInfo() {
                type = WordType.Normal,
                charStart = 0,
                charEnd = 4
            },
            new WordInfo() {
                type = WordType.NewLine,
                charStart = 4,
                charEnd = 5
            },
            new WordInfo() {
                type = WordType.NewLine,
                charStart = 5,
                charEnd = 6
            },
            new WordInfo() {
                type = WordType.Normal,
                charStart = 6,
                charEnd = 10
            }
        };

        Assert.AreEqual(expected.Length, result.size);

        for (int i = 0; i < result.size; i++) {
            Assert.AreEqual(expected[i], result.array[i]);
        }
    }

    [Test]
    public void BreakCharacterGroupsIntoWordsWithSpaceBrokenNewLine() {
        string input = "some\n   \ntext";
        StructList<WordInfo> result = TextUtil.BreakIntoWords(input.ToCharArray());

        WordInfo[] expected = {
            new WordInfo() {
                type = WordType.Normal,
                charStart = 0,
                charEnd = 4
            },
            new WordInfo() {
                type = WordType.NewLine,
                charStart = 4,
                charEnd = 5
            },
            new WordInfo() {
                type = WordType.Whitespace,
                charStart = 5,
                charEnd = 8
            },
            new WordInfo() {
                type = WordType.NewLine,
                charStart = 8,
                charEnd = 9
            },
            new WordInfo() {
                type = WordType.Normal,
                charStart = 9,
                charEnd = 13
            }
        };

        Assert.AreEqual(expected.Length, result.size);

        for (int i = 0; i < result.size; i++) {
            Assert.AreEqual(expected[i], result.array[i]);
        }
    }

    [Test]
    public void BreakCharacterGroupsIntoWordsWithSoftHyphen() {
        string input = "some\u00ADtext";
        StructList<WordInfo> result = TextUtil.BreakIntoWords(input.ToCharArray());

        WordInfo[] expected = {
            new WordInfo() {
                type = WordType.Normal,
                charStart = 0,
                charEnd = 4
            },
            new WordInfo() {
                type = WordType.SoftHyphen,
                charStart = 4,
                charEnd = 5
            },
            new WordInfo() {
                type = WordType.Normal,
                charStart = 5,
                charEnd = 9
            }
        };

        Assert.AreEqual(expected.Length, result.size);

        for (int i = 0; i < result.size; i++) {
            Assert.AreEqual(expected[i], result.array[i]);
        }
    }


    [Test]
    public void Wrap_IdentityInput() {
        char[] buffer = null;
        string input = "nothing should change";
        int outputSize = TextUtil.ProcessWhitespace(WhitespaceMode.PreserveNewLines, ref buffer, input.ToCharArray());
        Assert.AreEqual(input.Length, outputSize);
        Assert.AreEqual(buffer, input.ToCharArray());
    }

    [Test]
    public void Wrap_CollapseSpaces() {
        char[] buffer = null;
        string input = "spaces   should change";
        string expected = "spaces should change";
        int outputSize = TextUtil.ProcessWhitespace(WhitespaceMode.PreserveNewLines | WhitespaceMode.CollapseWhitespace, ref buffer, input.ToCharArray());
        Assert.AreEqual(expected.Length, outputSize);
        for (int i = 0; i < expected.Length; i++) {
            Assert.AreEqual(expected[i], buffer[i]);
        }
    }

    [Test]
    public void Wrap_CollapseNewlines() {
        char[] buffer = null;
        string input = "  \tspaces   \nshould change\n";
        string expected = "spaces should change";
        int outputSize = TextUtil.ProcessWhitespace(WhitespaceMode.TrimStart | WhitespaceMode.CollapseWhitespace, ref buffer, input.ToCharArray());
        Assert.AreEqual(expected.Length, outputSize);
        for (int i = 0; i < expected.Length; i++) {
            Assert.AreEqual(expected[i], buffer[i]);
        }
    }

    [Test]
    public void Wrap_TrimEnd() {
        char[] buffer = null;
        string input = "  \tspaces   \nshould change\n";
        string expected = "spaces \nshould change";
        int outputSize = TextUtil.ProcessWhitespace(WhitespaceMode.PreserveNewLines | WhitespaceMode.CollapseWhitespace | WhitespaceMode.Trim, ref buffer, input.ToCharArray());
        Assert.AreEqual(expected.Length, outputSize);
        for (int i = 0; i < expected.Length; i++) {
            Assert.AreEqual(expected[i], buffer[i]);
        }
    }

    [Test]
    public void Wrap_TrimStart() {
        char[] buffer = null;
        string input = "  \nspaces   \nshould change\n";
        string expected = "spaces   \nshould change\n";
        int outputSize = TextUtil.ProcessWhitespace(WhitespaceMode.TrimStart | WhitespaceMode.PreserveNewLines, ref buffer, input.ToCharArray());
        Assert.AreEqual(expected.Length, outputSize);
        for (int i = 0; i < expected.Length; i++) {
            Assert.AreEqual(expected[i], buffer[i]);
        }
    }

}