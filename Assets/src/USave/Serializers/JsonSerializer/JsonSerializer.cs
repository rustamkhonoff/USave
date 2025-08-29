using System;
using System.Text;
using System.Threading;
using UnityEngine;

namespace USave.Serializers
{
    public class JsonSerializer : ISerializer
    {
        public ReadOnlyMemory<byte> Serialize<T>(T data, CancellationToken ct = default)
        {
            string json = JsonUtility.ToJson(data);
            byte[] bytes = Encoding.ASCII.GetBytes(json);
            return new ReadOnlyMemory<byte>(bytes);
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> bytes, CancellationToken ct = default)
        {
            string json = Encoding.ASCII.GetString(bytes.Span);
            return JsonUtility.FromJson<T>(json);
        }
    }

    public static class Extensions
    {
        public static ModuleBuilder WithJsonSerializer(this ModuleBuilder builder)
            => builder.SetSerializer(new JsonSerializer());
    }
}