using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace USave
{
    public class PersistenceModule : IPersistenceModule
    {
        private readonly ISerializer m_serializer;
        private readonly IStorage m_storage;

        public PersistenceModule(ISerializer serializer, IStorage storage)
        {
            m_serializer = serializer;
            m_storage = storage;
        }

        public UniTask<bool> SaveAsync(string key, object data, CancellationToken ct = default)
        {
            ReadOnlyMemory<byte> bytes = m_serializer.Serialize(data, ct);
            return m_storage.Save(key, bytes, ct);
        }

        public async UniTask<T> LoadAsync<T>(string key, CancellationToken ct = default)
        {
            ReadOnlyMemory<byte> bytes = await m_storage.Load<T>(key, ct);
            return m_serializer.Deserialize<T>(bytes, ct);
        }

        public UniTask<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            return m_storage.Exist(key, ct);
        }

        public UniTask DeleteAsync(string key, CancellationToken ct = default)
        {
            return m_storage.Delete(key, ct);
        }
    }
}