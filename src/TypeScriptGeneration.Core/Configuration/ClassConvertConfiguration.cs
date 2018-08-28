using System;

namespace TypeScriptGeneration
{
    public class ClassConvertConfiguration
    {
        /// <summary>
        /// When true, the generator will create a inheritance structure, if false, it will flatten classes
        /// </summary>
        public bool ApplyInheritance { get; set; } = true;
        
        /// <summary>
        /// Generate as interface instead of typescript classes
        /// </summary>
        public bool GenerateAsInterface { get; set; } = false;

        public GenerateConstructorType GenerateConstructorType { get; set; } = GenerateConstructorType.ArgumentPerProperty;
        
        public InheritanceDiscriminatorConfiguration InheritanceConfig { get; } = new InheritanceDiscriminatorConfiguration();
        
    }
}