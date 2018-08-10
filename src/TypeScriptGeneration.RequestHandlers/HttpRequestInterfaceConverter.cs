using System;
using System.Collections.Generic;
using System.Reflection;
using Common.RequestHandlers;
using TypeScriptGeneration.Converters;

namespace TypeScriptGeneration.RequestHandlers
{
    public class HttpRequestInterfaceConverter : IConverter
    {
        public bool CanConvertType(Type type)
        {
            return type == typeof(IHttpRequest<>);
        }

        public string ConvertType(Type type, ILocalConvertContext context)
        {
            return $@"export interface {context.Configuration.GetTypeName(type)}<TResponse> {{
    {PropName(context, type, nameof(IHttpRequest<object>.Method))}: {
                    context.GetTypeScriptType(typeof(string)).ToTypeScriptType()
                };
    {PropName(context, type, nameof(IHttpRequest<object>.Route))}: {
                    context.GetTypeScriptType(typeof(string)).ToTypeScriptType()
                };
    {PropName(context, type, nameof(IHttpRequest<object>.Body))}: {
                    context.GetTypeScriptType(typeof(object)).ToTypeScriptType()
                };
    {PropName(context, type, nameof(IHttpRequest<object>.QueryString))}: {{
        [key: string]: string | string[];
    }};
}}";
        }

        private static string PropName(ILocalConvertContext context, Type httpRequestType, string name)
        {
            return context.Configuration.GetPropertyName(httpRequestType, httpRequestType.GetTypeInfo().GetProperty(name));
        }
    }
}