using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TypeScriptGeneration.TypeScriptTypes;

namespace TypeScriptGeneration.Converters
{
    public class ClassConverter : IConverter
    {
        public class Data
        {
            public Data()
            {
                Properties = new List<Property>();
                ConstructorLines = new List<string>();
                Body = new List<string>();
                ClassSuffix = "";
            }
            public List<Property> Properties { get; set; }
            public List<string> ConstructorLines { get; set; }
            public List<string> Body { get; set; }
            public string ClassSuffix { get; set; }
        }
        
        public class Property
        {
            public bool IsDeclared { get; set; }
            public string Name { get; set; }
            public TypeScriptType TypeScriptType { get; set; }
            public PropertyInfo PropertyInfo { get; set; }
        }
        
        public virtual bool CanConvertType(Type type)
        {
            return type.IsClass || type.IsValueType;
        }
        
        public string ConvertType(Type type, ILocalConvertContext context)
        {
            var data = new Data();
            var name = context.Configuration.GetTypeName(type);
            
            AddBase(data, type, context, name);
            AddProperties(data, type, context);
            GenerateDiscriminator(data, type, context);
            
            if (type.IsGenericTypeDefinition)
            {
                var args = type.GetGenericArguments();
                name += $"<{string.Join(", ", args.Select(x => x.Name))}>";
            }

            AdditionalGeneration(data, type, context);

            var constructorArguments = GeneratePropertiesAndConstructor(context, data);

            var generated = $@"
export{(type.IsAbstract ? " abstract" : "")} class {name}{data.ClassSuffix} {{
    constructor({constructorArguments}) {{{_.Foreach(data.ConstructorLines, line => $@"
        {line}")}
    }}{_.Foreach(data.Body, line => string.IsNullOrEmpty(line) ? @"
" : $@"
    {line}")}
}}";
            return generated;
        }

        private void GenerateDiscriminator(Data data, Type type, ILocalConvertContext context)
        {
            var inheritanceConfig = context.Configuration.ClassConfiguration.InheritanceConfig;

            if(inheritanceConfig.TryGetValue(type, out var subTypesAndDiscriminator))
            {
                var propertyName = context.Configuration.GetPropertyName(type, subTypesAndDiscriminator.DiscriminatorProperty);
                var tsType = context.GetTypeScriptType(subTypesAndDiscriminator.DiscriminatorProperty.PropertyType);
                if (subTypesAndDiscriminator.DiscriminatorValue != null)
                {
                    if (subTypesAndDiscriminator.GenerateStaticTypeProperty)
                    {
                        data.Body.Insert(0, $"public static {propertyName}: {tsType.ToTypeScriptType()} = {GetTypeScriptValue(subTypesAndDiscriminator.DiscriminatorValue, subTypesAndDiscriminator.DiscriminatorProperty.PropertyType, context)};");
                    }
                    data.Body.Add($"public {propertyName}: {tsType.ToTypeScriptType()} = {GetTypeScriptValue(subTypesAndDiscriminator.DiscriminatorValue, subTypesAndDiscriminator.DiscriminatorProperty.PropertyType, context)};");
                }
                else
                {
                    data.Body.Add($"public {propertyName}: {tsType.ToTypeScriptType()};");
                }
                
                
                foreach (var subTypes in subTypesAndDiscriminator.SubTypesWithDiscriminatorValue)
                {
                    context.GetTypeScriptType(subTypes.Key, false);
                }
            }
        }

        private string GetTypeScriptValue(object discriminatorValue, Type type, ILocalConvertContext context)
        {
            var typeScriptType = context.GetTypeScriptType(type);
            var typeScriptTypeStr = typeScriptType.ToTypeScriptType();
            if (type.IsEnum)
            {
                return $"{typeScriptTypeStr}.{discriminatorValue}";
            }
            else if (typeScriptTypeStr == "string")
            {
                return $"'{((string) discriminatorValue).Replace("'", "\\'")}'";
            }
            else if (typeScriptTypeStr == "numeric")
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}", discriminatorValue);
            }
            else if (typeScriptTypeStr == "boolean")
            {
                return Equals(discriminatorValue, true) ? "true" : "false";
            }
            throw new NotSupportedException();
        }

        private static string GeneratePropertiesAndConstructor(ILocalConvertContext context, Data data)
        {
            bool generateProperties = true;
            string constructorArguments = null;
            if (context.Configuration.ClassConfiguration.GenerateConstructorType
                == GenerateConstructorType.ObjectInitializer)
            {
                if (data.Properties.Any())
                {
                    constructorArguments = $@"init?: {{{_.Foreach(data.Properties, arg => $@"
        {arg.Name}?: {arg.TypeScriptType.ToTypeScriptType()},").TrimEnd(',')}
    }}";

                    data.ConstructorLines.AddRange(
                        new[] {"if (init) {"}
                            .Concat(data.Properties.Where(x => x.IsDeclared).Select(x => $"    this.{x.Name} = init.{x.Name};"))
                            .Concat(new[] {"}"}));
                }
            }
            else if (context.Configuration.ClassConfiguration.GenerateConstructorType ==
                     GenerateConstructorType.ArgumentPerProperty)
            {
                constructorArguments = _.Foreach(data.Properties, arg => $@"
        {(arg.IsDeclared ? "public " : "")}{arg.Name}?: {arg.TypeScriptType.ToTypeScriptType()},").TrimEnd(',');
                generateProperties = false;
            }

            if (generateProperties)
            {
                data.Body.InsertRange(0,
                    data.Properties
                        .Where(x => x.IsDeclared)
                        .Select(property =>
                            $"public {property.Name}: {property.TypeScriptType.ToTypeScriptType()};"));
            }

            return string.IsNullOrEmpty(constructorArguments) ? "" : constructorArguments;
        }

        protected virtual void AdditionalGeneration(Data data, Type type, ILocalConvertContext context)
        {
        }

        protected void AddProperties(Data data, Type type, ILocalConvertContext context)
        {
            var properties = type.GetProperties(
                    BindingFlags.DeclaredOnly | 
                    BindingFlags.Instance | 
                    BindingFlags.Public | 
                    BindingFlags.SetProperty | 
                    BindingFlags.GetProperty)
                .Where(x => context.Configuration.ShouldConvertProperty(type, x))
                .Select(baseProperty => new Property
                {
                    Name = context.Configuration.GetPropertyName(type.BaseType, baseProperty),
                    TypeScriptType = context.GetTypeScriptType(baseProperty.PropertyType),
                    PropertyInfo = baseProperty,
                    IsDeclared = true
                })
                .ToArray();
            foreach (var baseProperty in properties)
            {
                data.Properties.Add(baseProperty);
            }
        }

        protected virtual void AddBase(Data data, Type type, ILocalConvertContext context, string className)
        {
            if (type.BaseType != typeof(object))
            {
                var baseProperties = type.BaseType.GetProperties(
                        BindingFlags.Instance | 
                        BindingFlags.Public | 
                        BindingFlags.SetProperty | 
                        BindingFlags.GetProperty)
                    .Where(x => context.Configuration.ShouldConvertProperty(type, x))
                    .Select(baseProperty => new Property
                    {
                        Name = context.Configuration.GetPropertyName(type.BaseType, baseProperty),
                        TypeScriptType = context.GetTypeScriptType(baseProperty.PropertyType),
                        PropertyInfo = baseProperty,
                        IsDeclared = !context.Configuration.ClassConfiguration.ApplyInheritance
                    })
                    .ToArray();
                foreach (var baseProperty in baseProperties)
                {
                    data.Properties.Add(baseProperty);
                }
                
                if (context.Configuration.ClassConfiguration.ApplyInheritance)
                {
                    string baseParameters;
                    if (context.Configuration.ClassConfiguration.GenerateConstructorType == GenerateConstructorType.ObjectInitializer)
                    {
                        baseParameters = baseProperties.Any() ? "init" : "";
                    }
                    else
                    {
                        baseParameters = string.Join(", ", baseProperties.Select(x => x.Name));
                    }
                    
                    var superCall = $"super({baseParameters});";
                    data.ConstructorLines.Add(superCall);
                    
                    var baseType = context.GetTypeScriptType(type.BaseType);
                    var baseTypescriptType = baseType.ToTypeScriptType();
                    if (baseTypescriptType.Equals(className))
                    {
                        baseTypescriptType += "Base";
                        if (context.Imports.ContainsKey(type.BaseType))
                        {
                            context.Imports[type.BaseType].Alias = baseTypescriptType;
                        }
                    }

                    data.ClassSuffix += $" extends {baseTypescriptType}";                    
                }
            }
        }
    }

}