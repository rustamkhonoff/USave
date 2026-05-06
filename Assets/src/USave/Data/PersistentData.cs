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

        private readonly SemaphoreSlim m_gate = new(1, 1);

        private bool m_loaded;

        public PersistentData(
            IPersistenceService persistenceService,
            IGlobalDataRegistry dataRegistry,
            ILogger logger
        )
        {
            m_persistenceService = persistenceService;
            m_dataRegistry = dataRegistry;
            m_logger = logger;
        }

        public async UniTask<bool> Save(CancellationToken ct = default)
        {
            if (!TryGetRegistry(out Entry registry))
                return false;

            await m_gate.WaitAsync(ct);

            try
            {
                await EnsureLoadedCore(registry, saveIfCreated: false, ct);
                await SaveCore(registry, m_value, ct);

                return true;
            }
            finally
            {
                m_gate.Release();
            }
        }

        public async UniTask<T> Load(CancellationToken ct = default)
        {
            if (!TryGetRegistry(out Entry registry))
                return null;

            await m_gate.WaitAsync(ct);

            try
            {
                await EnsureLoadedCore(registry, saveIfCreated: true, ct);
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
            bool shouldNotify = false;

            await m_gate.WaitAsync(ct);

            try
            {
                if (m_dataRegistry.TryGet<T>(out Entry registry))
                    await m_persistenceService.DeleteAsync<T>(registry.Key, ct);

                m_value = null;
                m_loaded = false;

                shouldNotify = true;
            }
            finally
            {
                m_gate.Release();
            }

            if (shouldNotify)
            {
                Updated?.Invoke(null);
                Deleted?.Invoke();
            }
        }

        public async UniTask Reset(CancellationToken ct = default)
        {
            if (!TryGetRegistry(out Entry registry))
                return;

            T newValue;

            await m_gate.WaitAsync(ct);

            try
            {
                newValue = CreateDefault(registry);

                await SaveCore(registry, newValue, ct);

                m_value = newValue;
                m_loaded = true;
            }
            finally
            {
                m_gate.Release();
            }

            Updated?.Invoke(newValue);
        }

        public async UniTask Update(Action<T> action, bool autoSave = true, CancellationToken ct = default)
        {
            if (action == null)
                return;

            if (!TryGetRegistry(out Entry registry))
                return;

            T updatedValue;

            await m_gate.WaitAsync(ct);

            try
            {
                await EnsureLoadedCore(registry, saveIfCreated: true, ct);

                action.Invoke(m_value);

                if (autoSave)
                    await SaveCore(registry, m_value, ct);

                updatedValue = m_value;
            }
            finally
            {
                m_gate.Release();
            }

            Updated?.Invoke(updatedValue);
        }

        public async UniTask Update<TState>(TState state, Action<T, TState> action, bool autoSave = true, CancellationToken ct = default)
        {
            if (action == null)
                return;

            if (!TryGetRegistry(out Entry registry))
                return;

            T updatedValue;

            await m_gate.WaitAsync(ct);

            try
            {
                await EnsureLoadedCore(registry, saveIfCreated: true, ct);

                action.Invoke(m_value, state);

                if (autoSave)
                    await SaveCore(registry, m_value, ct);

                updatedValue = m_value;
            }
            finally
            {
                m_gate.Release();
            }

            Updated?.Invoke(updatedValue);
        }

        public async UniTask Update<TState1, TState2>(TState1 state1, TState2 state2, Action<T, TState1, TState2> action, bool autoSave = true, CancellationToken ct = default)
        {
            if (action == null)
                return;

            if (!TryGetRegistry(out Entry registry))
                return;

            T updatedValue;

            await m_gate.WaitAsync(ct);

            try
            {
                await EnsureLoadedCore(registry, saveIfCreated: true, ct);

                action.Invoke(m_value, state1, state2);

                if (autoSave)
                    await SaveCore(registry, m_value, ct);

                updatedValue = m_value;
            }
            finally
            {
                m_gate.Release();
            }

            Updated?.Invoke(updatedValue);
        }

        private async UniTask EnsureLoadedCore(Entry registry, bool saveIfCreated, CancellationToken ct)
        {
            if (m_loaded)
                return;

            if (await m_persistenceService.ExistsAsync<T>(registry.Key, ct))
            {
                T loadedValue = await m_persistenceService.LoadAsync<T>(
                    registry.Key,
                    ct
                );

                if (loadedValue != null)
                {
                    m_value = loadedValue;
                    m_loaded = true;
                    return;
                }

                m_logger.LogError(
                    $"[USave] Loaded data is null for {typeof(T)}. Creating default instance."
                );
            }

            m_value = CreateDefault(registry);
            m_loaded = true;

            if (saveIfCreated)
                await SaveCore(registry, m_value, ct);
        }

        private UniTask SaveCore(Entry registry, T value, CancellationToken ct)
        {
            if (value == null)
                throw new InvalidOperationException(
                    $"[USave] Cannot save null value for {typeof(T)}"
                );

            return m_persistenceService.SaveAsync(registry.Key, value, ct);
        }

        private T CreateDefault(Entry registry)
        {
            if (registry.Factory is not Func<T> factory)
            {
                throw new InvalidOperationException(
                    $"[USave] Invalid default factory for {typeof(T)}"
                );
            }

            T instance = factory.Invoke();

            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"[USave] Default factory returned null for {typeof(T)}"
                );
            }

            return instance;
        }

        private bool TryGetRegistry(out Entry registry)
        {
            if (m_dataRegistry.TryGet<T>(out registry))
                return true;

            m_logger.LogError($"[USave] No default factory for {typeof(T)}");
            return false;
        }
    }
}