using Newtonsoft.Json;

namespace SalesApp.Core.Services.Person.Json
{
    public class Serializer
    {
        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

    }
}
