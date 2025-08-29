using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace USave.Internal
{
    public static class PersistenceServiceExtensions
    {
        public static async UniTask<T> LoadOrCreateAsync<T>(this IPersistenceService service,
            string key, Func<T> createFunc, bool saveNewCreated = true)
            where T : class
        {
            if (await service.ExistsAsync<T>(key)) return await service.LoadAsync<T>(key);
            T instance = createFunc();
            if (saveNewCreated) await service.SaveAsync(key, instance);
            return instance;
        }

        [HideInCallstack]
        public static UniTask<T> LoadAsync<T>(this IPersistenceService module, string key) where T : class => module.LoadAsync<T>(key, default);

        [HideInCallstack]
        public static UniTask SaveAsync<T>(this IPersistenceService module, string key, T data) where T : class =>
            module.SaveAsync(key, data, default);

        [HideInCallstack]
        public static UniTask<bool> ExistsAsync<T>(this IPersistenceService module, string key) where T : class =>
            module.ExistsAsync<T>(key, default);

        [HideInCallstack]
        public static UniTask DeleteAsync<T>(this IPersistenceService module, string key) where T : class => module.DeleteAsync<T>(key, default);
    }
}