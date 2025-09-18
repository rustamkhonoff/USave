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
        UniTask Update<TState>(Action<T, TState> action, TState state, bool autoSave = true, CancellationToken ct = default);

        UniTask Update<TState1, TState2>(Action<T, TState1, TState2> action, TState1 state1, TState2 state2, bool autoSave = true,
            CancellationToken ct = default);
    }
}