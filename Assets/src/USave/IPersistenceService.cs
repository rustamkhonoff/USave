using System.Threading;
using Cysharp.Threading.Tasks;

namespace USave
{
    public interface IPersistenceService
    {
        UniTask SaveAsync<T>(string key, T data, CancellationToken ct) where T : class;
        UniTask<T> LoadAsync<T>(string key, CancellationToken ct) where T : class;
        UniTask<bool> ExistsAsync<T>(string key, CancellationToken ct);
        UniTask DeleteAsync<T>(string key, CancellationToken ct) where T : class;
    }
}