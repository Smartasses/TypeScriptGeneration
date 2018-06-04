using System;
using System.Linq;
using System.Reflection;

namespace TypeScriptGeneration.Converters
{
    public class EnumConverter : IConverter
    {
        public bool CanConvertType(Type type) => GetNonNullable(type).GetTypeInfo().IsEnum;

        private static Type GetNonNullable(Type type) => Nullable.GetUnderlyingType(type) ?? type;

        public string ConvertType(Type type, ILocalConvertContext context)
        {
            var nonNullable = GetNonNullable(type);
            var enumValues = Enum.GetNames(nonNullable).Select(x => new
            {
                Name = context.Configuration.GetEnumValueName(nonNullable, x),
                Value = Convert.ChangeType(Enum.Parse(nonNullable, x), System.Enum.GetUnderlyingType(nonNullable))
            });
            
            var generated = $@"
export enum {context.Configuration.GetTypeName(nonNullable)} {{{_.Foreach(enumValues, val => $@"
    {val.Name} = {val.Value},").TrimEnd(',')}
}}";
            return generated;
        }
    }
}