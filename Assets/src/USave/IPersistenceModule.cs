using System.Threading;
using Cysharp.Threading.Tasks;

namespace USave
{
    public interface IPersistenceModule
    {
        UniTask<bool> SaveAsync(string key, object data, CancellationToken ct = default);
        UniTask<T> LoadAsync<T>(string key, CancellationToken ct = default);
        UniTask<bool> ExistsAsync(string key, CancellationToken ct = default);
        UniTask DeleteAsync(string key, CancellationToken ct = default);
    }
}