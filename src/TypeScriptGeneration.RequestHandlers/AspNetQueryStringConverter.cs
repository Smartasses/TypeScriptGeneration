using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TypeScriptGeneration.Converters;

namespace TypeScriptGeneration.RequestHandlers
{
    /*
     An implementation for the default .NET query string binder.
     E.g.:
     Given the following request GET-method signature where all parameters are bound as Query type:
       MyMethod(string name, Filter filter)
     with the Filter class looking like:
       public class Filter
       {
           public string Search { get; set; }
           public Page Page { get; set; }
       }
     and the Page class looking like:
       public class Page
       {
           public int Index { get; set; }
           public int Size { get; set; }
       }
     
     will result in the following query parameters:
       - Name
       - Filter.Search
       - Filter.Page.Index
       - Filter.Page.Size
     (notice the lack of Filter prefix anywhere)
    */
    public static class AspNetQueryStringConverter
    {
        public static string GetQueryStringLines(IEnumerable<ClassConverter.Property> properties, string queryStringPropertyName, ILocalConvertContext context)
        {
            var queryStringBuilder = new StringBuilder();

            foreach (var property in properties)
            {
                queryStringBuilder.AppendQueryStringLines("    ", property.PropertyInfo, context.Imports, context.Configuration.GetPropertyName,
                    property.Name, queryStringPropertyName);
            }

            return queryStringBuilder.ToString();
        }

        private static void AppendQueryStringLines(this StringBuilder builder, string linePrefix, PropertyInfo propertyInfo,
            IDictionary<Type, TypeScriptResult> imports, Func<Type, PropertyInfo, string> getTypescriptPropertyName,
            string propertyName, string queryStringPropertyName)
        {
            var defaultCondition = $"if (this.{propertyName})";
            var defaultLeftHandSide = $"req.{queryStringPropertyName}['{propertyName}']";
            var propertyType = propertyInfo.PropertyType;
            var isEnumerableOrArray = propertyType.IsArray || typeof(IEnumerable).IsAssignableFrom(propertyType);
            
            if (propertyType == typeof(string))
            {
                builder.AppendLine($"{linePrefix}{defaultCondition} {defaultLeftHandSide} = this.{propertyName};");
            }
            else if (isEnumerableOrArray)
            {
                builder.AppendLine($"{linePrefix}{defaultCondition} {defaultLeftHandSide} = this.{propertyName}.map(i => i ? i.toString() : null).filter(i => typeof i === 'string');");
            }
            else if (imports.ContainsKey(propertyType))
            {
                var import = imports[propertyType];

                builder.AppendLine($"{linePrefix}{defaultCondition} {{");
                var innerProperties = propertyInfo.PropertyType.GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.SetProperty |
                    BindingFlags.GetProperty);
                foreach (var innerPropertyInfo in innerProperties)
                {
                    var innerImports = import.Imports.ToDictionary(x => x.Type, x => x);
                    var newPropertyNamePart = getTypescriptPropertyName(propertyType, innerPropertyInfo);
                    var innerPropertyName = $"{propertyName}.{newPropertyNamePart}";
                    
                    builder.AppendQueryStringLines($"{linePrefix}    ",innerPropertyInfo, innerImports, getTypescriptPropertyName,
                        innerPropertyName, queryStringPropertyName);
                }

                builder.AppendLine($"{linePrefix}}}");
            }
            else
            {
                builder.AppendLine($"{linePrefix}{defaultCondition} {defaultLeftHandSide} = this.{propertyName}.toString();");
            }
        }
    }
}