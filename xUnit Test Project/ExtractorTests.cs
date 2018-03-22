using System;
using Xunit;
using LorenzoExtractor;
using LorenzoExtractor.Helpers;
using System.Text.RegularExpressions;
using System.Threading;
namespace xUnit_Test_Project
{
    public class SeperatorTests
    {
        [Fact]
        private void Seperator_Empty()
        {
            Assert.Equal(new string[0], Extractor.Seperate(String.Empty, String.Empty));
        }
        [Fact]
        private void Seperator_One()
        {
            Assert.Equal(new[] { "h", "i" }, Extractor.Seperate("h;i", ";"));
        }
        [Fact]
        private void Seperator_NewLine()
        {
            Assert.Equal(new[] { "h", "i" }, Extractor.Seperate(String.Format("h{0}i", Environment.NewLine), Extractor.NEW_LINE_COMBOBOX));
        }
        [Fact]
        private void Multiple_Seperators()
        {
            Assert.Equal(new[] { "h", "i", "there" }, Extractor.Seperate(String.Format("h{0}i;there", Environment.NewLine),
                String.Format("{0}{1};", Extractor.NEW_LINE_COMBOBOX, Environment.NewLine)));
        }
    }
    public class StartsWithTests
    {
        [Fact]
        private void Seperator_Empty()
        {
            Assert.Equal(new[] { String.Empty }, Extractor.StartsWith(new[] { String.Empty }, String.Empty, StringComparison.Ordinal, CancellationToken.None));
        }
        [Fact]
        private void Seperator_Hey()
        {
            Assert.Equal(new string[0], Extractor.StartsWith(new[] { "hey" }, "e", StringComparison.Ordinal, CancellationToken.None));
        }
        [Fact]
        private void Seperator_Hey_Correct()
        {
            Assert.Equal(new[] { "hey" }, Extractor.StartsWith(new[] { "hey" }, "h", StringComparison.Ordinal, CancellationToken.None));
            Assert.Equal(new[] { "hey" }, Extractor.StartsWith(new[] { "hey" }, "he", StringComparison.Ordinal, CancellationToken.None));
        }
        [Fact]
        private void Seperator_Hey_Multiline()
        {
            Assert.Equal(new[] { "hey" }, Extractor.StartsWith(new[] { "hey", "hoo" }, "he", StringComparison.Ordinal, CancellationToken.None));
            Assert.Equal(new[] { "hoo" }, Extractor.StartsWith(new[] { "hey", "hoo" }, "ho", StringComparison.Ordinal, CancellationToken.None));
            Assert.Equal(new[] { "hey", "hoo" }, Extractor.StartsWith(new[] { "hey", "hoo" }, "h", StringComparison.Ordinal, CancellationToken.None));
        }
    }
    public class ContainsTests
    {
        [Fact]
        private void Seperator_Empty()
        {
            Assert.Equal(new[] { String.Empty }, Extractor.Contains(new[] { String.Empty }, String.Empty, StringComparison.Ordinal, CancellationToken.None));
        }
        [Fact]
        private void Seperator_Hey()
        {
            Assert.Equal(new string[0], Extractor.Contains(new[] { "hey" }, "o", StringComparison.Ordinal, CancellationToken.None));
        }
        [Fact]
        private void Seperator_Hey_Correct()
        {
            Assert.Equal(new[] { "hey" }, Extractor.Contains(new[] { "hey" }, "h", StringComparison.Ordinal, CancellationToken.None));
            Assert.Equal(new[] { "hey" }, Extractor.Contains(new[] { "hey" }, "e", StringComparison.Ordinal, CancellationToken.None));
        }
        [Fact]
        private void Seperator_Hey_Multiline()
        {
            Assert.Equal(new[] { "hey" }, Extractor.Contains(new[] { "hey", "hoo" }, "he", StringComparison.Ordinal, CancellationToken.None));
            Assert.Equal(new[] { "hoo" }, Extractor.Contains(new[] { "hey", "hoo" }, "ho", StringComparison.Ordinal, CancellationToken.None));
            Assert.Equal(new[] { "hey", "hoo" }, Extractor.Contains(new[] { "hey", "hoo" }, "h", StringComparison.Ordinal, CancellationToken.None));
        }
        [Fact]
        private void Seperator_Hoey_Multiline()
        {
            Assert.Equal(new[] { "hoey", "hoo" }, Extractor.Contains(new[] { "hoey", "hoo" }, "o", StringComparison.Ordinal, CancellationToken.None));
        }
    }
    public class RegexTests
    {
        [Fact]
        private void Seperator_Empty()
        {
            Assert.Equal(new[] { String.Empty }, Extractor.SearchRegex(new[] { String.Empty }, String.Empty, RegexOptions.IgnoreCase, CancellationToken.None));
        }
        [Fact]
        private void Seperator_Hey()
        {
            Assert.Equal(new string[0], Extractor.SearchRegex(new[] { "hey" }, "o", RegexOptions.IgnoreCase, CancellationToken.None));
        }
        [Fact]
        private void Seperator_Hey_Correct()
        {
            Assert.Equal(new[] { "hey" }, Extractor.SearchRegex(new[] { "hey" }, "h", RegexOptions.IgnoreCase, CancellationToken.None));
            Assert.Equal(new[] { "hey" }, Extractor.SearchRegex(new[] { "hey" }, "e", RegexOptions.IgnoreCase, CancellationToken.None));
        }
        [Fact]
        private void Seperator_Hey_Multiline()
        {
            Assert.Equal(new[] { "hey" }, Extractor.SearchRegex(new[] { "hey", "hoo" }, "he", RegexOptions.IgnoreCase, CancellationToken.None));
            Assert.Equal(new[] { "hoo" }, Extractor.SearchRegex(new[] { "hey", "hoo" }, "ho", RegexOptions.IgnoreCase, CancellationToken.None));
            Assert.Equal(new[] { "hey", "hoo" }, Extractor.SearchRegex(new[] { "hey", "hoo" }, "h", RegexOptions.IgnoreCase, CancellationToken.None));
        }
        [Fact]
        private void Seperator_Hoey_Multiline()
        {
            Assert.Equal(new[] { "hoey", "hoo" }, Extractor.SearchRegex(new[] { "hoey", "hoo" }, "o", RegexOptions.IgnoreCase, CancellationToken.None));
        }
    }
}
