using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TypeScriptGeneration
{
    public class DiscriminatorConverter : JsonConverter
    {
        private readonly Action<Exception> _onException;
        private Dictionary<Type, SubTypesAndDiscriminator> _parentTypes;

        public DiscriminatorConverter(InheritanceDiscriminatorConfiguration config, Action<Exception> onException = null)
        {
            _onException = onException;
            _parentTypes = config._config.Where(x => x.Value.SubTypesWithDiscriminatorValue.Any())
                .ToDictionary(x => x.Key, x => x.Value);
        }
        
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite { get; } = false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {

                var subTypes = _parentTypes[objectType];
                var jToken = JToken.ReadFrom(reader);
                if (jToken is JObject jObject &&
                    jObject.TryGetValue(subTypes.DiscriminatorProperty.Name,
                        StringComparison.CurrentCultureIgnoreCase,
                        out var discriminatorValue))
                {
                    var deserialized =
                        discriminatorValue.ToObject(subTypes.DiscriminatorProperty.PropertyType, serializer);
                    var subTypesFiltered = subTypes.SubTypesWithDiscriminatorValue.Where(x => Equals(x.Value, deserialized)).ToArray();
                    if (subTypesFiltered.Any())
                    {
                        var subType = subTypesFiltered.First();
                        if (subType.Value == objectType)
                        {
                            return DeserializeBaseType(objectType, serializer, jObject);
                        }
                        else
                        {
                            return DeserializeSubType(serializer, jObject, subType);
                        }
                    }
                }

                return jToken.ToObject(objectType);
            }
            catch (Exception e)
            {
                _onException(e);
                throw;
            }
        }

        private static object DeserializeSubType(JsonSerializer serializer, JObject jObject, KeyValuePair<Type, object> subType)
        {
            return jObject.ToObject(subType.Key, serializer);
        }

        private static object DeserializeBaseType(Type objectType, JsonSerializer serializer, JObject jObject)
        {
            var result = Activator.CreateInstance(objectType);
            foreach (var prop in objectType.GetProperties())
            {
                if (!prop.CanWrite) continue;
                var value = jObject.GetValue(prop.Name, StringComparison.CurrentCultureIgnoreCase);
                if (value != null)
                {
                    prop.SetValue(result, value.ToObject(prop.PropertyType, serializer), null);
                }
            }

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return _parentTypes.ContainsKey(objectType);
        }
    }
}