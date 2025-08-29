using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace USave.Data
{
    public interface IPersistentData<T> where T : class
    {
        event Action<T> Updated;
        event Action Deleted;
        UniTask<bool> Save(CancellationToken ct = default);
        UniTask<T> Load(CancellationToken ct = default);
        UniTask<bool> Exists(CancellationToken ct = default);
        UniTask Delete(CancellationToken ct = default);
        UniTask Reset(CancellationToken ct = default);
        UniTask Update(Action<T> action, bool autoSave = true, CancellationToken ct = default);
    }
}