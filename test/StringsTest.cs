using Extensions;
using NUnit.Framework;

namespace Roku.Tests;

public class StringsTest
{
    [OneTimeSetUp]
    public void OneTimeSetUp() => System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

    [Test]
    public void CountTest()
    {
        var s = "abcあいうアイウ亜伊宇ｱｲｳ";
        var sjis = System.Text.Encoding.GetEncoding(932);

        Assert.AreEqual(s.CountAsByte(0, sjis), 0);
        Assert.AreEqual(s.CountAsByte(1, sjis), 1);
        Assert.AreEqual(s.CountAsByte(2, sjis), 2);
        Assert.AreEqual(s.CountAsByte(3, sjis), 3);
        Assert.AreEqual(s.CountAsByte(4, sjis), 3);
        Assert.AreEqual(s.CountAsByte(5, sjis), 4);
        Assert.AreEqual(s.CountAsByte(6, sjis), 4);
        Assert.AreEqual(s.CountAsByte(7, sjis), 5);
        Assert.AreEqual(s.CountAsByte(8, sjis), 5);
        Assert.AreEqual(s.CountAsByte(9, sjis), 6);
        Assert.AreEqual(s.CountAsByte(10, sjis), 6);
        Assert.AreEqual(s.CountAsByte(11, sjis), 7);
        Assert.AreEqual(s.CountAsByte(12, sjis), 7);
        Assert.AreEqual(s.CountAsByte(13, sjis), 8);
        Assert.AreEqual(s.CountAsByte(14, sjis), 8);
        Assert.AreEqual(s.CountAsByte(15, sjis), 9);
        Assert.AreEqual(s.CountAsByte(16, sjis), 9);
        Assert.AreEqual(s.CountAsByte(17, sjis), 10);
        Assert.AreEqual(s.CountAsByte(18, sjis), 10);
        Assert.AreEqual(s.CountAsByte(19, sjis), 11);
        Assert.AreEqual(s.CountAsByte(20, sjis), 11);
        Assert.AreEqual(s.CountAsByte(21, sjis), 12);
        Assert.AreEqual(s.CountAsByte(22, sjis), 13);
        Assert.AreEqual(s.CountAsByte(23, sjis), 14);
        Assert.AreEqual(s.CountAsByte(24, sjis), 15);
        Assert.AreEqual(s.CountAsByte(25, sjis), 15);
    }

    [Test]
    public void SubstringTest()
    {
        var s = "abcあいうアイウ亜伊宇ｱｲｳ";
        var sjis = System.Text.Encoding.GetEncoding(932);

        Assert.AreEqual(s.SubstringAsByte(0, 0, sjis), "");
        Assert.AreEqual(s.SubstringAsByte(0, 1, sjis), "a");
        Assert.AreEqual(s.SubstringAsByte(0, 2, sjis), "ab");
        Assert.AreEqual(s.SubstringAsByte(0, 3, sjis), "abc");
        Assert.AreEqual(s.SubstringAsByte(0, 4, sjis), "abc");
        Assert.AreEqual(s.SubstringAsByte(0, 5, sjis), "abcあ");
        Assert.AreEqual(s.SubstringAsByte(0, 6, sjis), "abcあ");
        Assert.AreEqual(s.SubstringAsByte(0, 7, sjis), "abcあい");
        Assert.AreEqual(s.SubstringAsByte(0, 8, sjis), "abcあい");
        Assert.AreEqual(s.SubstringAsByte(0, 9, sjis), "abcあいう");
        Assert.AreEqual(s.SubstringAsByte(0, 10, sjis), "abcあいう");
        Assert.AreEqual(s.SubstringAsByte(0, 11, sjis), "abcあいうア");
        Assert.AreEqual(s.SubstringAsByte(0, 12, sjis), "abcあいうア");
        Assert.AreEqual(s.SubstringAsByte(0, 13, sjis), "abcあいうアイ");
        Assert.AreEqual(s.SubstringAsByte(0, 14, sjis), "abcあいうアイ");
        Assert.AreEqual(s.SubstringAsByte(0, 15, sjis), "abcあいうアイウ");
        Assert.AreEqual(s.SubstringAsByte(0, 16, sjis), "abcあいうアイウ");
        Assert.AreEqual(s.SubstringAsByte(0, 17, sjis), "abcあいうアイウ亜");
        Assert.AreEqual(s.SubstringAsByte(0, 18, sjis), "abcあいうアイウ亜");
        Assert.AreEqual(s.SubstringAsByte(0, 19, sjis), "abcあいうアイウ亜伊");
        Assert.AreEqual(s.SubstringAsByte(0, 20, sjis), "abcあいうアイウ亜伊");
        Assert.AreEqual(s.SubstringAsByte(0, 21, sjis), "abcあいうアイウ亜伊宇");
        Assert.AreEqual(s.SubstringAsByte(0, 22, sjis), "abcあいうアイウ亜伊宇ｱ");
        Assert.AreEqual(s.SubstringAsByte(0, 23, sjis), "abcあいうアイウ亜伊宇ｱｲ");
        Assert.AreEqual(s.SubstringAsByte(0, 24, sjis), "abcあいうアイウ亜伊宇ｱｲｳ");
        Assert.AreEqual(s.SubstringAsByte(0, 25, sjis), "abcあいうアイウ亜伊宇ｱｲｳ");

        Assert.AreEqual(s.SubstringAsByte(0, 3, sjis), "abc");
        Assert.AreEqual(s.SubstringAsByte(1, 3, sjis), "bc");
        Assert.AreEqual(s.SubstringAsByte(2, 3, sjis), "cあ");
        Assert.AreEqual(s.SubstringAsByte(3, 3, sjis), "あ");
        Assert.AreEqual(s.SubstringAsByte(4, 3, sjis), "あ");
        Assert.AreEqual(s.SubstringAsByte(5, 3, sjis), "い");
        Assert.AreEqual(s.SubstringAsByte(6, 3, sjis), "い");
        Assert.AreEqual(s.SubstringAsByte(7, 3, sjis), "う");
        Assert.AreEqual(s.SubstringAsByte(8, 3, sjis), "う");
        Assert.AreEqual(s.SubstringAsByte(9, 3, sjis), "ア");
        Assert.AreEqual(s.SubstringAsByte(10, 3, sjis), "ア");
        Assert.AreEqual(s.SubstringAsByte(11, 3, sjis), "イ");
        Assert.AreEqual(s.SubstringAsByte(12, 3, sjis), "イ");
        Assert.AreEqual(s.SubstringAsByte(13, 3, sjis), "ウ");
        Assert.AreEqual(s.SubstringAsByte(14, 3, sjis), "ウ");
        Assert.AreEqual(s.SubstringAsByte(15, 3, sjis), "亜");
        Assert.AreEqual(s.SubstringAsByte(16, 3, sjis), "亜");
        Assert.AreEqual(s.SubstringAsByte(17, 3, sjis), "伊");
        Assert.AreEqual(s.SubstringAsByte(18, 3, sjis), "伊");
        Assert.AreEqual(s.SubstringAsByte(19, 3, sjis), "宇ｱ");
        Assert.AreEqual(s.SubstringAsByte(20, 3, sjis), "宇ｱ");
        Assert.AreEqual(s.SubstringAsByte(21, 3, sjis), "ｱｲｳ");
        Assert.AreEqual(s.SubstringAsByte(22, 3, sjis), "ｲｳ");
        Assert.AreEqual(s.SubstringAsByte(23, 3, sjis), "ｳ");
        Assert.AreEqual(s.SubstringAsByte(24, 3, sjis), "");
    }
}
