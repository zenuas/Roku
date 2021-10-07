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
        public void CountTest()
        {
            var s = "abcあいうアイウ亜伊宇ｱｲｳ";
            var sjis = System.Text.Encoding.GetEncoding(932);

            Assert.IsTrue(s.CountAsByte(0, sjis) == 0);
            Assert.IsTrue(s.CountAsByte(1, sjis) == 1);
            Assert.IsTrue(s.CountAsByte(2, sjis) == 2);
            Assert.IsTrue(s.CountAsByte(3, sjis) == 3);
            Assert.IsTrue(s.CountAsByte(4, sjis) == 3);
            Assert.IsTrue(s.CountAsByte(5, sjis) == 4);
            Assert.IsTrue(s.CountAsByte(6, sjis) == 4);
            Assert.IsTrue(s.CountAsByte(7, sjis) == 5);
            Assert.IsTrue(s.CountAsByte(8, sjis) == 5);
            Assert.IsTrue(s.CountAsByte(9, sjis) == 6);
            Assert.IsTrue(s.CountAsByte(10, sjis) == 6);
            Assert.IsTrue(s.CountAsByte(11, sjis) == 7);
            Assert.IsTrue(s.CountAsByte(12, sjis) == 7);
            Assert.IsTrue(s.CountAsByte(13, sjis) == 8);
            Assert.IsTrue(s.CountAsByte(14, sjis) == 8);
            Assert.IsTrue(s.CountAsByte(15, sjis) == 9);
            Assert.IsTrue(s.CountAsByte(16, sjis) == 9);
            Assert.IsTrue(s.CountAsByte(17, sjis) == 10);
            Assert.IsTrue(s.CountAsByte(18, sjis) == 10);
            Assert.IsTrue(s.CountAsByte(19, sjis) == 11);
            Assert.IsTrue(s.CountAsByte(20, sjis) == 11);
            Assert.IsTrue(s.CountAsByte(21, sjis) == 12);
            Assert.IsTrue(s.CountAsByte(22, sjis) == 13);
            Assert.IsTrue(s.CountAsByte(23, sjis) == 14);
            Assert.IsTrue(s.CountAsByte(24, sjis) == 15);
            Assert.IsTrue(s.CountAsByte(25, sjis) == 15);
        }

        [Test]
        public void ByteTest()
        {
            var s = "abcあいうアイウ亜伊宇ｱｲｳ";
            var sjis = System.Text.Encoding.GetEncoding(932);

            Assert.IsTrue(s.SubstringAsByte(0, 0, sjis) == "");
            Assert.IsTrue(s.SubstringAsByte(0, 1, sjis) == "a");
            Assert.IsTrue(s.SubstringAsByte(0, 2, sjis) == "ab");
            Assert.IsTrue(s.SubstringAsByte(0, 3, sjis) == "abc");
            Assert.IsTrue(s.SubstringAsByte(0, 4, sjis) == "abc");
            Assert.IsTrue(s.SubstringAsByte(0, 5, sjis) == "abcあ");
            Assert.IsTrue(s.SubstringAsByte(0, 6, sjis) == "abcあ");
            Assert.IsTrue(s.SubstringAsByte(0, 7, sjis) == "abcあい");
            Assert.IsTrue(s.SubstringAsByte(0, 8, sjis) == "abcあい");
            Assert.IsTrue(s.SubstringAsByte(0, 9, sjis) == "abcあいう");
            Assert.IsTrue(s.SubstringAsByte(0, 10, sjis) == "abcあいう");
            Assert.IsTrue(s.SubstringAsByte(0, 11, sjis) == "abcあいうア");
            Assert.IsTrue(s.SubstringAsByte(0, 12, sjis) == "abcあいうア");
            Assert.IsTrue(s.SubstringAsByte(0, 13, sjis) == "abcあいうアイ");
            Assert.IsTrue(s.SubstringAsByte(0, 14, sjis) == "abcあいうアイ");
            Assert.IsTrue(s.SubstringAsByte(0, 15, sjis) == "abcあいうアイウ");
            Assert.IsTrue(s.SubstringAsByte(0, 16, sjis) == "abcあいうアイウ");
            Assert.IsTrue(s.SubstringAsByte(0, 17, sjis) == "abcあいうアイウ亜");
            Assert.IsTrue(s.SubstringAsByte(0, 18, sjis) == "abcあいうアイウ亜");
            Assert.IsTrue(s.SubstringAsByte(0, 19, sjis) == "abcあいうアイウ亜伊");
            Assert.IsTrue(s.SubstringAsByte(0, 20, sjis) == "abcあいうアイウ亜伊");
            Assert.IsTrue(s.SubstringAsByte(0, 21, sjis) == "abcあいうアイウ亜伊宇");
            Assert.IsTrue(s.SubstringAsByte(0, 22, sjis) == "abcあいうアイウ亜伊宇ｱ");
            Assert.IsTrue(s.SubstringAsByte(0, 23, sjis) == "abcあいうアイウ亜伊宇ｱｲ");
            Assert.IsTrue(s.SubstringAsByte(0, 24, sjis) == "abcあいうアイウ亜伊宇ｱｲｳ");
            Assert.IsTrue(s.SubstringAsByte(0, 25, sjis) == "abcあいうアイウ亜伊宇ｱｲｳ");

            Assert.IsTrue(s.SubstringAsByte(0, 3, sjis) == "abc");
            Assert.IsTrue(s.SubstringAsByte(1, 3, sjis) == "bc");
            Assert.IsTrue(s.SubstringAsByte(2, 3, sjis) == "cあ");
            Assert.IsTrue(s.SubstringAsByte(3, 3, sjis) == "あ");
            Assert.IsTrue(s.SubstringAsByte(4, 3, sjis) == "あ");
            Assert.IsTrue(s.SubstringAsByte(5, 3, sjis) == "い");
            Assert.IsTrue(s.SubstringAsByte(6, 3, sjis) == "い");
            Assert.IsTrue(s.SubstringAsByte(7, 3, sjis) == "う");
            Assert.IsTrue(s.SubstringAsByte(8, 3, sjis) == "う");
            Assert.IsTrue(s.SubstringAsByte(9, 3, sjis) == "ア");
            Assert.IsTrue(s.SubstringAsByte(10, 3, sjis) == "ア");
            Assert.IsTrue(s.SubstringAsByte(11, 3, sjis) == "イ");
            Assert.IsTrue(s.SubstringAsByte(12, 3, sjis) == "イ");
            Assert.IsTrue(s.SubstringAsByte(13, 3, sjis) == "ウ");
            Assert.IsTrue(s.SubstringAsByte(14, 3, sjis) == "ウ");
            Assert.IsTrue(s.SubstringAsByte(15, 3, sjis) == "亜");
            Assert.IsTrue(s.SubstringAsByte(16, 3, sjis) == "亜");
            Assert.IsTrue(s.SubstringAsByte(17, 3, sjis) == "伊");
            Assert.IsTrue(s.SubstringAsByte(18, 3, sjis) == "伊");
            Assert.IsTrue(s.SubstringAsByte(19, 3, sjis) == "宇ｱ");
            Assert.IsTrue(s.SubstringAsByte(20, 3, sjis) == "宇ｱ");
            Assert.IsTrue(s.SubstringAsByte(21, 3, sjis) == "ｱｲｳ");
            Assert.IsTrue(s.SubstringAsByte(22, 3, sjis) == "ｲｳ");
            Assert.IsTrue(s.SubstringAsByte(23, 3, sjis) == "ｳ");
            Assert.IsTrue(s.SubstringAsByte(24, 3, sjis) == "");
        }
    }
}
