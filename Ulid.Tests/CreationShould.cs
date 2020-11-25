using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace CalvinReed.Tests
{
    public class CreationShould
    {
        private readonly ITestOutputHelper output;

        public CreationShould(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void BeMonotonic()
        {
            var before = Ulid.Next();
            for (var i = 0; i < 10_000; i++)
            {
                var after = Ulid.Next();
                Assert.True(before < after, $"{i} {before} {after}");
                before = after;
            }
        }

        [Fact]
        public void BeFast()
        {
            const int count = 10_000_000;
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                Ulid.Next();
            }

            stopwatch.Stop();
            var perMilli = count / stopwatch.ElapsedMilliseconds;
            var perMilliStr = $"{perMilli}";
            output.WriteLine(perMilliStr);
            Assert.True(perMilli > 10000, perMilliStr);
        }
    }
}
