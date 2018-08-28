using System.Configuration;
using FluentAssertions;
using TypeScriptGeneration.Converters;
using Xunit;

namespace TypeScriptGeneration.Tests.Converters
{
    public class ClassConverterTests
    {
        [Fact]
        public void ConvertSimpleClass_Arguments()
        {
            var result = new ClassConverter().ConvertType(typeof(SimpleClass), new LocalContext(new ConvertConfiguration(), null, typeof(SimpleClass)));
            result.Should().BeLike(@"
export class SimpleClass {
    constructor(
        public propertyArray?: Array<string>,
        public stringProperty?: string) {
    }
}");
        }
        [Fact]
        public void ConvertSimpleClass_Initializer()
        {
            var result = new ClassConverter().ConvertType(typeof(SimpleClass), new LocalContext(new ConvertConfiguration
            {
                ClassConfiguration = { GenerateConstructorType = GenerateConstructorType.ObjectInitializer}
            }, null, typeof(SimpleClass)));
            result.Should().BeLike(@"
export class SimpleClass {
    constructor(init?: {
        propertyArray?: Array<string>,
        stringProperty?: string
    }) {
        if (init) {
            this.propertyArray = init.propertyArray;
            this.stringProperty = init.stringProperty;
        }
    }
    public propertyArray: Array<string>;
    public stringProperty: string;
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
        public prop1?: T1,
        public prop2?: T2) {
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
        propertyArray?: Array<string>,
        stringProperty?: string,
        public isAwesome?: boolean) {
        super(propertyArray, stringProperty);
    }
}");

            ctx.GeneratedResults.Should().HaveCount(1);
        }

        class InheritanceSample : SimpleClass
        {
            public bool IsAwesome { get; set; }
        }
        
        
        [Fact]
        public void InheritanceDiscriminatorTest()
        {
            var ctx = new ConvertContext();
            ctx.Configuration.ClassConfiguration.InheritanceConfig
                .Add<BaseClassWithDiscriminator>(x => x.Discriminator, 
                    typeof(InheritanceDiscriminatorSample));
            
            var result = new ClassConverter().ConvertType(typeof(InheritanceDiscriminatorSample), new LocalContext(ctx.Configuration, ctx, typeof(InheritanceSample)));
            result.Should().BeLike(@"
export class InheritanceDiscriminatorSample extends BaseClassWithDiscriminator {
    constructor() {
        super();
    }
    public discriminator: string = 'inherit';
}");

            ctx.GeneratedResults.Should().HaveCount(2);
        }

        class InheritanceDiscriminatorSample : BaseClassWithDiscriminator
        {
            public override string Discriminator { get; } = "inherit";
        }

        class BaseClassWithDiscriminator
        {
            public virtual string Discriminator { get; } = "base";
        }
    }
}