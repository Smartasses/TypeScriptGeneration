namespace Common.RequestHandlers
{
    interface IRequestDispatcher
    {
        TResponse Execute<TResponse>(IHttpRequest<TResponse> request);
    }
}