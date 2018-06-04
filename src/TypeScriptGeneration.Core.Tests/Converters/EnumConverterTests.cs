using FluentAssertions;
using TypeScriptGeneration.Converters;
using Xunit;

namespace TypeScriptGeneration.Tests.Converters
{
    public class EnumConverterTests
    {
        [Fact]
        public void ConvertTest()
        {
            var converter = new EnumConverter();
            converter.ConvertType(typeof(TestEnum), new LocalContext(new ConvertConfiguration(), null, typeof(TestEnum))).Should().BeLike(@"
export enum TestEnum {
    Val1 = 0,
    Val2 = 1
}");
        }

        enum TestEnum
        {
            Val1,
            Val2
        }
    }
}