using Extensions;
using NUnit.Framework;
using System;

namespace Roku.Tests
{
    public class StringsTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        [Test]
        public void ByteTest()
        {
            var s = "abcあいうアイウ亜伊宇ｱｲｳ";
            var sjis = System.Text.Encoding.GetEncoding(932);
            Assert.IsTrue(s.SubstringAsByte(0, sjis) == "");
            Assert.IsTrue(s.SubstringAsByte(1, sjis) == "a");
            Assert.IsTrue(s.SubstringAsByte(2, sjis) == "ab");
            Assert.IsTrue(s.SubstringAsByte(3, sjis) == "abc");
            Assert.IsTrue(s.SubstringAsByte(4, sjis) == "abc");
            Assert.IsTrue(s.SubstringAsByte(5, sjis) == "abcあ");
            Assert.IsTrue(s.SubstringAsByte(6, sjis) == "abcあ");
            Assert.IsTrue(s.SubstringAsByte(7, sjis) == "abcあい");
            Assert.IsTrue(s.SubstringAsByte(8, sjis) == "abcあい");
            Assert.IsTrue(s.SubstringAsByte(9, sjis) == "abcあいう");
            Assert.IsTrue(s.SubstringAsByte(10, sjis) == "abcあいう");
            Assert.IsTrue(s.SubstringAsByte(11, sjis) == "abcあいうア");
            Assert.IsTrue(s.SubstringAsByte(12, sjis) == "abcあいうア");
            Assert.IsTrue(s.SubstringAsByte(13, sjis) == "abcあいうアイ");
            Assert.IsTrue(s.SubstringAsByte(14, sjis) == "abcあいうアイ");
            Assert.IsTrue(s.SubstringAsByte(15, sjis) == "abcあいうアイウ");
            Assert.IsTrue(s.SubstringAsByte(16, sjis) == "abcあいうアイウ");
            Assert.IsTrue(s.SubstringAsByte(17, sjis) == "abcあいうアイウ亜");
            Assert.IsTrue(s.SubstringAsByte(18, sjis) == "abcあいうアイウ亜");
            Assert.IsTrue(s.SubstringAsByte(19, sjis) == "abcあいうアイウ亜伊");
            Assert.IsTrue(s.SubstringAsByte(20, sjis) == "abcあいうアイウ亜伊");
            Assert.IsTrue(s.SubstringAsByte(21, sjis) == "abcあいうアイウ亜伊宇");
            Assert.IsTrue(s.SubstringAsByte(22, sjis) == "abcあいうアイウ亜伊宇ｱ");
            Assert.IsTrue(s.SubstringAsByte(23, sjis) == "abcあいうアイウ亜伊宇ｱｲ");
            Assert.IsTrue(s.SubstringAsByte(24, sjis) == "abcあいうアイウ亜伊宇ｱｲｳ");
            Assert.IsTrue(s.SubstringAsByte(25, sjis) == "abcあいうアイウ亜伊宇ｱｲｳ");
        }
    }
}
