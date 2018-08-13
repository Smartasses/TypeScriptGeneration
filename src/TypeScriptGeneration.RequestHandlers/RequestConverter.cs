using System;
using System.Linq;
using System.Reflection;
using Common.RequestHandlers;
using RequestHandlers;
using RequestHandlers.Http;
using TypeScriptGeneration.Converters;
using TypeScriptGeneration.TypeScriptTypes;
using IRequestDispatcher = RequestHandlers.IRequestDispatcher;

namespace TypeScriptGeneration.RequestHandlers
{
    public class RequestConverter : ClassConverter
    {
        public override bool CanConvertType(Type type)
        {
            return base.CanConvertType(type) && type.GetTypeInfo().GetCustomAttribute<HttpRequestAttribute>() != null;
        }

        protected override void AdditionalGeneration(Data data, Type type, ILocalConvertContext context)
        {
            var attr = type.GetTypeInfo().GetCustomAttribute<HttpRequestAttribute>();
            if (attr == null)
            {
                throw new ArgumentNullException("attr cannot be null");
            }
            var parsed = new HttpRequestHandlerDefinition(attr, new RequestAndResponse(type));


            var queryStringParameters = parsed.Parameters.Where(x => x.BindingType == BindingType.FromQuery)
                .Select(x => new
                {
                    Original = x,
                    Parsed = data.Properties.Single(p => p.PropertyInfo == x.PropertyInfo)
                }).ToArray();
            var routeParameters = parsed.Parameters.Where(x => x.BindingType == BindingType.FromRoute)
                .Select(x => new
                {
                    Original = x,
                    Parsed = data.Properties.Single(p => p.PropertyInfo == x.PropertyInfo)
                }).ToArray();
            var bodyParameters = parsed.Parameters.Where(x => x.BindingType == BindingType.FromBody)
                .Select(x => new
                {
                    Original = x,
                    Parsed = data.Properties.Single(p => p.PropertyInfo == x.PropertyInfo)
                }).ToArray();

            var httpRequestType = typeof(IHttpRequest<>).MakeGenericType(parsed.Definition.ResponseType);

            var replaceRouteArgs = _.Foreach(routeParameters, prop =>
                $".replace('{{{prop.Original.PropertyName}}}', this.{prop.Parsed.Name} ? this.{prop.Parsed.Name}.toString() : '')");
            var hasBody = parsed.HttpMethod == HttpMethod.Patch || parsed.HttpMethod == HttpMethod.Post ||
                          parsed.HttpMethod == HttpMethod.Put;
            var body = $@"{{{
                    _.Foreach(bodyParameters, prop => $@"
        {prop.Parsed.Name}: this.{prop.Parsed.Name},").TrimEnd(',')
                }
    }}";
            var queryStringPropertyName = PropName(context, httpRequestType, nameof(IHttpRequest<object>.QueryString));
            var methodPropertyName = PropName(context, httpRequestType, nameof(IHttpRequest<object>.Method));
            var routePropertyName = PropName(context, httpRequestType, nameof(IHttpRequest<object>.Route));
            var bodyPropertyName = PropName(context, httpRequestType, nameof(IHttpRequest<object>.Body));
            var code = $@"
public __name = '{context.Configuration.GetTypeName(type)}';
private __request = () => {{
    const req: {context.GetTypeScriptType(httpRequestType).ToTypeScriptType()} = {{
        {methodPropertyName}: '{parsed.HttpMethod.ToString().ToLower()}',
        {routePropertyName}: '{parsed.Route}'{replaceRouteArgs},
        {bodyPropertyName}: {(hasBody ? body : "undefined")},
        {queryStringPropertyName}: {{}}
    }};{_.Foreach(queryStringParameters, prop => $@"
    req.{queryStringPropertyName}['{prop.Parsed.Name}'] = {GetQueryStringValue(prop.Parsed)}")}
    return req;
}}
public execute = (dispatcher: {
                    context.GetTypeScriptType(typeof(IRequestDispatcher)).ToTypeScriptType()
                }) => dispatcher.execute(this.__request());";
            data.Body.AddRange(code.Replace("\r\n", "\n").Split('\n'));
        }

        private static string GetQueryStringValue(Property parsed)
        {
            if (parsed.TypeScriptType is ArrayTypeScriptType)
            {
                return $"this.{parsed.Name} ? this.{parsed.Name}.map(i => i ? i.toString() : null).filter(i => typeof i === 'string') : null;";
            }
            return $"this.{parsed.Name} ? this.{parsed.Name}.toString() : null;";
        }

        private static string PropName(ILocalConvertContext context, Type httpRequestType, string name)
        {
            return context.Configuration.GetPropertyName(httpRequestType, httpRequestType.GetProperty(name));
        }

        class RequestAndResponse : IRequestDefinition
        {
            public RequestAndResponse(Type type)
            {
                RequestType = type;
                ResponseType = type.GetInterfaces()
                                   .SingleOrDefault(x =>
                                       x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IReturn<>))
                                   ?.GetGenericArguments()?.First() ?? typeof(object);
            }

            public Type RequestType { get; set; }
            public Type ResponseType { get; set; }
        }
    }
}

namespace Common.RequestHandlers
{
}