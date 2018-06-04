using FluentAssertions;
using TypeScriptGeneration.Converters;
using Xunit;

namespace TypeScriptGeneration.Tests.Converters
{
    public class ClassConverterTests
    {
        [Fact]
        public void ConvertSimpleClass()
        {
            var result = new ClassConverter().ConvertType(typeof(SimpleClass), new LocalContext(new ConvertConfiguration(), null, typeof(SimpleClass)));
            result.Should().BeLike(@"
export class SimpleClass {
    constructor(
        public PropertyArray?: Array<string>,
        public StringProperty?: string) {
    }
}");
        }

        class SimpleClass
        {
            public string[] PropertyArray { get; set; }
            public string StringProperty { get; set; }
        }
        
        [Fact]
        public void ConvertAbstractClass()
        {
            var result = new ClassConverter().ConvertType(typeof(AbstractClass), new LocalContext(new ConvertConfiguration(), null, typeof(AbstractClass)));
            result.Should().BeLike(@"
export abstract class AbstractClass {
    constructor() {
    }
}");
        }

        abstract class AbstractClass
        {
        }
        
        [Fact]
        public void ConvertGenericClass()
        {
            var result = new ClassConverter().ConvertType(typeof(GenericClass<,>), new LocalContext(new ConvertConfiguration(), null, typeof(GenericClass<,>)));
            result.Should().BeLike(@"
export class GenericClass<T1, T2> {
    constructor(
        public Prop1?: T1,
        public Prop2?: T2) {
    }
}");
        }

        class GenericClass<T1, T2>
        {
            public T1 Prop1 { get; set; }
            public T2 Prop2 { get; set; }
        }
        
        
        [Fact]
        public void InheritanceTest()
        {
            var ctx = new ConvertContext();
            var result = new ClassConverter().ConvertType(typeof(InheritanceSample), new LocalContext(ctx.Configuration, ctx, typeof(InheritanceSample)));
            result.Should().BeLike(@"
export class InheritanceSample extends SimpleClass {
    constructor(
        PropertyArray?: Array<string>,
        StringProperty?: string,
        public IsAwesome?: boolean) {
        super(PropertyArray, StringProperty);
    }
}");

            ctx.GeneratedResults.Should().HaveCount(1);
        }

        class InheritanceSample : SimpleClass
        {
            public bool IsAwesome { get; set; }
        }
    }
}