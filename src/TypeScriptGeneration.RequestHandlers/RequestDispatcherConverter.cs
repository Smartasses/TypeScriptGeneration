using System;
using Common.RequestHandlers;
using TypeScriptGeneration.Converters;
using TypeScriptGeneration.TypeScriptTypes;
using IRequestDispatcher = RequestHandlers.IRequestDispatcher;

namespace TypeScriptGeneration.RequestHandlers
{
    public class RequestDispatcherConverter : IConverter
    {
        private readonly DispatcherResponseType _responseType;

        public RequestDispatcherConverter(DispatcherResponseType responseType)
        {
            _responseType = responseType;
        }
        public bool CanConvertType(Type type)
        {
            return type == typeof(IRequestDispatcher);
        }

        public string ConvertType(Type type, ILocalConvertContext context)
        {
            if (_responseType == DispatcherResponseType.Observable)
            {
                context.ExternalImports.Add(new [] { new BuiltInTypeScriptType("Observable") }, "rxjs/index");
            }
            return $@"export interface {context.Configuration.GetTypeName(type)} {{
    execute<TResponse>(request: {
                    context.GetTypeScriptType(typeof(IHttpRequest<string>)).ToTypeScriptType()
                        .Replace("string", "TResponse")
                }): {(_responseType == DispatcherResponseType.Promise ? "Promise<TResponse>" : "Observable<TResponse>")};
}}";
        }
    }

    public enum DispatcherResponseType
    {
        Observable,
        Promise
    }
}