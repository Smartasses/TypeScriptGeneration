namespace TypeScriptGeneration.RequestHandlers
{
    public static class GenerationConfigurationExtensions
    {
        public static ConvertConfiguration AddRequestHandlerGeneration(this ConvertConfiguration config)
        {
            config.AddConverter(new HttpRequestInterfaceConverter());
            config.AddConverter(new RequestDispatcherConverter());
            config.AddConverter(new RequestConverter());
            return config;
        }
    }
}