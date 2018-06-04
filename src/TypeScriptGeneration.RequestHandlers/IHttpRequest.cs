using System.Collections.Generic;

namespace Common.RequestHandlers
{
    interface IHttpRequest<TResponse>
    {
        string Method { get; set; }
        string Route { get; set; }
        object Body { get; set; }
        Dictionary<string, string> QueryString { get; set; }
    }
}