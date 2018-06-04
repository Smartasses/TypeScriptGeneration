using System;
using Common.RequestHandlers;
using TypeScriptGeneration.Converters;
using IRequestDispatcher = RequestHandlers.IRequestDispatcher;

namespace TypeScriptGeneration.RequestHandlers
{
    public class RequestDispatcherConverter : IConverter
    {
        public bool CanConvertType(Type type)
        {
            return type == typeof(IRequestDispatcher);
        }

        public string ConvertType(Type type, ILocalConvertContext context)
        {
            return $@"export interface {context.Configuration.GetTypeName(type)} {{
    execute<TResponse>(request: {
                    context.GetTypeScriptType(typeof(IHttpRequest<string>)).ToTypeScriptType()
                        .Replace("string", "TResponse")
                });
}}";
        }
    }
}