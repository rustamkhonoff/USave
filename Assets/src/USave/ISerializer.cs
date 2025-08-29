using System;
using System.Threading;

namespace USave
{
    public interface ISerializer
    {
        ReadOnlyMemory<byte> Serialize<T>(T data, CancellationToken ct = default);
        T Deserialize<T>(ReadOnlyMemory<byte> bytes, CancellationToken ct = default);
    }
}