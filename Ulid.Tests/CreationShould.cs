using Xunit;

namespace CalvinReed.Tests
{
    public class CreationShould
    {
        [Fact]
        public void BeMonotonic()
        {
            var before = Ulid.Create();
            for (var i = 0; i < 10_000; i++)
            {
                var after = Ulid.Create();
                Assert.True(before < after, $"{i} {before} {after}");
                before = after;
            }
        }
    }
}
