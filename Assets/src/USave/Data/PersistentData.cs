using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace USave.Data
{
    [Serializable]
    public sealed class PersistentData<T> : IPersistentData<T> where T : class
    {
        public event Action<T> Updated;
        public event Action Deleted;

        [SerializeField] private T m_value;

        private readonly IPersistenceService m_persistenceService;
        private readonly IGlobalDataRegistry m_dataRegistry;
        private readonly ILogger m_logger;
        private volatile bool m_loaded;

        private readonly SemaphoreSlim m_gate = new(1, 1);

        public PersistentData(IPersistenceService persistenceService, IGlobalDataRegistry dataRegistry, ILogger logger)
        {
            m_persistenceService = persistenceService;
            m_dataRegistry = dataRegistry;
            m_logger = logger;
        }

        public async UniTask<bool> Save(CancellationToken ct = default)
        {
            if (!m_dataRegistry.TryGet<T>(out Entry registry))
            {
                m_logger.LogError($"[USave] No default factory for {typeof(T)}");
                return false;
            }

            await EnsureLoadedAsync(ct);
            await m_persistenceService.SaveAsync(registry.Key, m_value, ct);

            return true;
        }

        public async UniTask<T> Load(CancellationToken ct = default)
        {
            if (!m_dataRegistry.TryGet<T>(out Entry registry))
            {
                m_logger.LogError($"[USave] No default factory for {typeof(T)}");
                return null;
            }

            if (m_loaded) return m_value;

            await m_gate.WaitAsync(ct);
            try
            {
                if (m_loaded) return m_value;

                if (await m_persistenceService.ExistsAsync<T>(registry.Key, ct))
                {
                    T data = await m_persistenceService.LoadAsync<T>(registry.Key, ct);
                    m_value = data;
                    m_loaded = true;
                    return m_value;
                }

                T newInstance = ((Func<T>)registry.Factory)();
                m_value = newInstance;
                m_loaded = true;
                await Save(ct);
                return m_value;
            }
            finally
            {
                m_gate.Release();
            }
        }

        public UniTask<bool> Exists(CancellationToken ct = default)
        {
            return m_dataRegistry.TryGet<T>(out Entry registry)
                ? m_persistenceService.ExistsAsync<T>(registry.Key, ct)
                : UniTask.FromResult(false);
        }

        public async UniTask Delete(CancellationToken ct = default)
        {
            await m_gate.WaitAsync(ct);
            try
            {
                m_value = null;
                m_loaded = false;
                Updated?.Invoke(null);
                Deleted?.Invoke();

                if (m_dataRegistry.TryGet<T>(out Entry registry))
                    await m_persistenceService.DeleteAsync<T>(registry.Key, ct);
            }
            finally
            {
                m_gate.Release();
            }
        }

        public async UniTask Reset(CancellationToken ct = default)
        {
            if (!m_dataRegistry.TryGet<T>(out Entry registry))
            {
                m_logger.LogError($"[USave] No default factory for {typeof(T)}");
                return;
            }

            await m_gate.WaitAsync(ct);
            try
            {
                m_value = ((Func<T>)registry.Factory)();
                m_loaded = true;
                await m_persistenceService.SaveAsync(registry.Key, m_value, ct);
                Updated?.Invoke(m_value);
            }
            finally
            {
                m_gate.Release();
            }
        }

        public async UniTask Update(Action<T> action, bool autoSave = true, CancellationToken ct = default)
        {
            if (!m_dataRegistry.Has<T>())
            {
                m_logger.LogError($"[USave] No default factory for {typeof(T)}");
                return;
            }

            await EnsureLoadedAsync(ct);

            action?.Invoke(m_value);

            if (autoSave) await Save(ct);

            Updated?.Invoke(m_value);
        }

        private UniTask EnsureLoadedAsync(CancellationToken ct) => m_loaded ? UniTask.CompletedTask : Load(ct);
    }
}