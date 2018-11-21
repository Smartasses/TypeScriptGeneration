using System;
using Xunit;

namespace TypeScriptGeneration.RequestHandlers.Tests
{
    public class DiscriminatorTests
    {
        private abstract class MyAbstractClass
        {
            protected MyAbstractClass(string discriminator)
            {
                if (string.IsNullOrEmpty(discriminator))
                    throw new ArgumentNullException(nameof(discriminator));
                
                Discriminator = discriminator;
            }
            
            public string Discriminator { get; set; }
        }

        private class MyImplementationWithPublicEmptyConstructor : MyAbstractClass
        {
            public MyImplementationWithPublicEmptyConstructor()
                : base(nameof(MyImplementationWithPublicEmptyConstructor))
            {
            }
        }
        
        private class MyImplementationWithPrivateEmptyConstructor : MyAbstractClass
        {
            private MyImplementationWithPrivateEmptyConstructor()
                : base(nameof(MyImplementationWithPrivateEmptyConstructor))
            {
            }
        }
        
        private class MyImplementationWithPublicEmptyConstructorAndOthers : MyAbstractClass
        {
            public MyImplementationWithPublicEmptyConstructorAndOthers()
                : base(nameof(MyImplementationWithPublicEmptyConstructorAndOthers))
            {
            }
            
            public MyImplementationWithPublicEmptyConstructorAndOthers(object shouldNotBeNull)
                : base(nameof(MyImplementationWithPublicEmptyConstructorAndOthers))
            {
                if (shouldNotBeNull == null)
                    throw new ArgumentNullException(nameof(shouldNotBeNull));
            }
        }
        
        private class MyImplementationWithPrivateEmptyConstructorAndOthers : MyAbstractClass
        {
            private MyImplementationWithPrivateEmptyConstructorAndOthers()
                : base(nameof(MyImplementationWithPrivateEmptyConstructorAndOthers))
            {
            }

            public MyImplementationWithPrivateEmptyConstructorAndOthers(object shouldNotBeNull)
                : base(nameof(MyImplementationWithPrivateEmptyConstructorAndOthers))
            {
                if (shouldNotBeNull == null)
                    throw new ArgumentNullException(nameof(shouldNotBeNull));
            }
        }
        
        private class MyImplementationWithConstructorContainingValidParameters : MyAbstractClass
        {
            public MyImplementationWithConstructorContainingValidParameters(object canBeNull)
                : base(nameof(MyImplementationWithConstructorContainingValidParameters))
            {
                MyObject = canBeNull;
            }

            public object MyObject { get; set; }
        }
        
        [Fact]
        public void PublicEmptyConstructorTest()
        {
            var discriminatorConfiguration = new InheritanceDiscriminatorConfiguration();
            discriminatorConfiguration.Add<MyAbstractClass>(x => x.Discriminator, typeof(MyImplementationWithPublicEmptyConstructor));
        }
        
        [Fact]
        public void PrivateEmptyConstructorTest()
        {
            var discriminatorConfiguration = new InheritanceDiscriminatorConfiguration();
            discriminatorConfiguration.Add<MyAbstractClass>(x => x.Discriminator, typeof(MyImplementationWithPrivateEmptyConstructor));
        }
        
        [Fact]
        public void PublicEmptyConstructorAndOthersTest()
        {
            var discriminatorConfiguration = new InheritanceDiscriminatorConfiguration();
            discriminatorConfiguration.Add<MyAbstractClass>(x => x.Discriminator, typeof(MyImplementationWithPublicEmptyConstructorAndOthers));
        }
        
        [Fact]
        public void PrivateEmptyConstructorAndOthersTest()
        {
            var discriminatorConfiguration = new InheritanceDiscriminatorConfiguration();
            discriminatorConfiguration.Add<MyAbstractClass>(x => x.Discriminator, typeof(MyImplementationWithPrivateEmptyConstructorAndOthers));
        }
        
        [Fact]
        public void ConstructorContainingValidParametersTest()
        {
            var discriminatorConfiguration = new InheritanceDiscriminatorConfiguration();
            discriminatorConfiguration.Add<MyAbstractClass>(x => x.Discriminator, typeof(MyImplementationWithConstructorContainingValidParameters));
        }
    }
}