using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TypeScriptGeneration.TypeScriptTypes;

namespace TypeScriptGeneration.Converters
{
    public class ClassConverter : IConverter
    {
        public class Data
        {
            public Data()
            {
                ConstructorArgs = new List<ConstructorArg>();
                ConstructorLines = new List<string>();
                Body = new List<string>();
                ClassSuffix = "";
            }
            public List<ConstructorArg> ConstructorArgs { get; set; }
            public List<string> ConstructorLines { get; set; }
            public List<string> Body { get; set; }
            public string ClassSuffix { get; set; }
        }
        
        public class ConstructorArg
        {
            public bool Expose { get; set; }
            public string Name { get; set; }
            public TypeScriptType TypeScriptType { get; set; }
            public PropertyInfo PropertyInfo { get; set; }
        }
        
        public virtual bool CanConvertType(Type type)
        {
            return type.GetTypeInfo().IsClass && !type.IsConstructedGenericType;
        }
        
        public string ConvertType(Type type, ILocalConvertContext context)
        {
            var data = new Data();
            AddBase(data, type, context);
            AddProperties(data, type, context);
            
            var name = context.Configuration.GetTypeName(type);
            if (type.GetTypeInfo().IsGenericTypeDefinition)
            {
                var args = type.GetTypeInfo().GetGenericArguments();
                name += $"<{string.Join(", ", args.Select(x => x.Name))}>";
            }

            AdditionalGeneration(data, type, context);
            
            var generated = $@"
export{(type.GetTypeInfo().IsAbstract ? " abstract" : "")} class {name}{data.ClassSuffix} {{
    constructor({_.Foreach(data.ConstructorArgs, arg => $@"
        {(arg.Expose ? "public " : "")}{arg.Name}?: {arg.TypeScriptType.ToTypeScriptType()},").TrimEnd(',')}) {{{_.Foreach(data.ConstructorLines, line => $@"
        {line}")}
    }}{_.Foreach(data.Body, line => string.IsNullOrEmpty(line) ? @"
" : $@"
    {line}")}
}}";
            return generated;
        }

        protected virtual void AdditionalGeneration(Data data, Type type, ILocalConvertContext context)
        {
        }

        protected void AddProperties(Data data, Type type, ILocalConvertContext context)
        {
            var properties = type.GetTypeInfo().GetProperties(
                    BindingFlags.DeclaredOnly | 
                    BindingFlags.Instance | 
                    BindingFlags.Public | 
                    BindingFlags.SetProperty | 
                    BindingFlags.GetProperty)
                .Where(x => context.Configuration.ShouldConvertProperty(type, x))
                .Select(baseProperty => new ConstructorArg
                {
                    Name = context.Configuration.GetPropertyName(type.GetTypeInfo().BaseType, baseProperty),
                    TypeScriptType = context.GetTypeScriptType(baseProperty.PropertyType),
                    PropertyInfo = baseProperty,
                    Expose = true
                })
                .ToArray();
            foreach (var baseProperty in properties)
            {
                data.ConstructorArgs.Add(baseProperty);
            }
        }

        protected virtual void AddBase(Data data, Type type, ILocalConvertContext context)
        {
            if (type.GetTypeInfo().BaseType != typeof(object))
            {
                var baseProperties = type.GetTypeInfo().BaseType.GetTypeInfo().GetProperties(
                        BindingFlags.Instance | 
                        BindingFlags.Public | 
                        BindingFlags.SetProperty | 
                        BindingFlags.GetProperty)
                    .Where(x => context.Configuration.ShouldConvertProperty(type, x))
                    .Select(baseProperty => new ConstructorArg
                    {
                        Name = context.Configuration.GetPropertyName(type.GetTypeInfo().BaseType, baseProperty),
                        TypeScriptType = context.GetTypeScriptType(baseProperty.PropertyType),
                        PropertyInfo = baseProperty,
                        Expose = !context.Configuration.ClassConfiguration.ApplyInheritance
                    })
                    .ToArray();
                foreach (var baseProperty in baseProperties)
                {
                    data.ConstructorArgs.Add(baseProperty);
                }
                
                if (context.Configuration.ClassConfiguration.ApplyInheritance)
                {
                    var superCall = $"super({string.Join(", ", baseProperties.Select(x => x.Name))});";
                    data.ConstructorLines.Add(superCall);
                    var baseType = context.GetTypeScriptType(type.GetTypeInfo().BaseType);
                    data.ClassSuffix += $" extends {baseType.ToTypeScriptType()}";                    
                }
            }
        }
    }

}