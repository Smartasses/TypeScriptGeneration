using FluentAssertions;
using FluentAssertions.Primitives;

namespace TypeScriptGeneration.Tests
{
    static class StringAssertionsHelper
    {
        public static AndConstraint<StringAssertions> BeLike(this StringAssertions assertion, string expected, string because = "", params object[] becauseArgs)
        {
            var actualForReals = assertion.Subject.Replace("\r\n", "\n").Trim();
            var expectedForReals = expected.Replace("\r\n", "\n").Trim();
            return actualForReals.Should().Be(expectedForReals, because, becauseArgs);
        }        
    }
}