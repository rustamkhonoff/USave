#if USAVE_NEWTONSOFT
using System;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace USave.Serializers
{
    public class NewtonsoftJsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings m_jsonSerializerSettings;

        public NewtonsoftJsonSerializer(JsonSerializerSettings jsonSerializerSettings = null)
        {
            m_jsonSerializerSettings = jsonSerializerSettings ?? new JsonSerializerSettings();
        }

        public ReadOnlyMemory<byte> Serialize<T>(T data, CancellationToken ct = default)
        {
            string json = JsonConvert.SerializeObject(data, m_jsonSerializerSettings);
            byte[] bytes = Encoding.ASCII.GetBytes(json);
            return new ReadOnlyMemory<byte>(bytes);
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> bytes, CancellationToken ct = default)
        {
            string json = Encoding.ASCII.GetString(bytes.Span);
            return JsonConvert.DeserializeObject<T>(json, m_jsonSerializerSettings);
        }
    }

    public static class Extensions
    {
        public static ModuleBuilder WithNewtonsoftJsonSerializer(this ModuleBuilder builder)
            => builder.SetSerializer(new NewtonsoftJsonSerializer());
    }
}
#endif