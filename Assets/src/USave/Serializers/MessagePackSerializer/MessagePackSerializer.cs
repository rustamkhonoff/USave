#if USAVE_MESSAGE_PACK
using System;
using System.Threading;
using MessagePack;
using MessagePack.Resolvers;

namespace USave.Serializers
{
    public class MessagePackSerializer : ISerializer
    {
        public ReadOnlyMemory<byte> Serialize<T>(T data, CancellationToken ct = default)
        {
            MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard;

            if (!Attribute.IsDefined(typeof(T), typeof(MessagePackObjectAttribute)))
                options = ContractlessStandardResolver.Options;

            byte[] bytes = MessagePack.MessagePackSerializer.Serialize(data, options, ct);

            return new ReadOnlyMemory<byte>(bytes);
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> bytes, CancellationToken ct = default)
        {
            MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard;

            if (!Attribute.IsDefined(typeof(T), typeof(MessagePackObjectAttribute)))
                options = ContractlessStandardResolver.Options;

            T data = MessagePack.MessagePackSerializer.Deserialize<T>(bytes, options, ct);

            return data;
        }
    }

    public static class Extensions
    {
        public static ModuleBuilder WithMessagePackSerializer(this ModuleBuilder builder)
            => builder.SetSerializer(new MessagePackSerializer());
    }
}
#endif