using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace USave
{
    public interface IStorage
    {
        UniTask<bool> Exist(string key, CancellationToken ct = default);
        UniTask<bool> Save(string key, ReadOnlyMemory<byte> data, CancellationToken ct = default);
        UniTask<ReadOnlyMemory<byte>> Load<T>(string key, CancellationToken ct = default);
        UniTask<bool> Delete(string key, CancellationToken ct = default);
    }
}