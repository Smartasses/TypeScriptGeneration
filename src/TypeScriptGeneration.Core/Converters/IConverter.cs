using System;

namespace TypeScriptGeneration.Converters
{
    public interface IConverter
    {
        bool CanConvertType(Type type);
        string ConvertType(Type type, ILocalConvertContext context);
    }
}