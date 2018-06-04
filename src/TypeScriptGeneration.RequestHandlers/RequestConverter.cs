using System;
using System.Linq;
using System.Reflection;
using Common.RequestHandlers;
using RequestHandlers;
using RequestHandlers.Http;
using TypeScriptGeneration.Converters;
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
                    Parsed = data.ConstructorArgs.Single(p => p.PropertyInfo == x.PropertyInfo)
                }).ToArray();
            var routeParameters = parsed.Parameters.Where(x => x.BindingType == BindingType.FromRoute)
                .Select(x => new
                {
                    Original = x,
                    Parsed = data.ConstructorArgs.Single(p => p.PropertyInfo == x.PropertyInfo)
                }).ToArray();
            var bodyParameters = parsed.Parameters.Where(x => x.BindingType == BindingType.FromBody)
                .Select(x => new
                {
                    Original = x,
                    Parsed = data.ConstructorArgs.Single(p => p.PropertyInfo == x.PropertyInfo)
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
            var code = $@"
private __request = () => <{context.GetTypeScriptType(httpRequestType).ToTypeScriptType()}>{{
    {PropName(context, httpRequestType, nameof(IHttpRequest<object>.Method))}: '{
                    parsed.HttpMethod.ToString().ToLower()
                }',
    {PropName(context, httpRequestType, nameof(IHttpRequest<object>.Route))}: '{parsed.Route}'{replaceRouteArgs},
    {PropName(context, httpRequestType, nameof(IHttpRequest<object>.Body))}: {(hasBody ? body : "undefined")},
    {PropName(context, httpRequestType, nameof(IHttpRequest<object>.QueryString))}: {{{
                    _.Foreach(queryStringParameters, prop => $@"
        {prop.Parsed.Name}: this.{prop.Parsed.Name} ? this.{prop.Parsed.Name}.toString() : null,").TrimEnd(',')
                }
    }}
}};
public execute = (dispatcher: {
                    context.GetTypeScriptType(typeof(IRequestDispatcher)).ToTypeScriptType()
                }) => dispatcher.execute(this.__request());";
            data.Body.AddRange(code.Replace("\r\n", "\n").Split('\n'));
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