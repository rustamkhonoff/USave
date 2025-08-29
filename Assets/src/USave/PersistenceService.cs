using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using USave.Internal;


[assembly: InternalsVisibleTo("USave.Zenject")]

namespace USave
{
    public class PersistenceService : IPersistenceService
    {
        private readonly ILogger m_logger;
        private readonly IPersistenceModule m_defaultModule;
        private readonly ModulesRegistry m_modulesRegistry;

        public PersistenceService(ModulesRegistry modulesRegistry, ILogger logger)
        {
            m_modulesRegistry = modulesRegistry;
            m_logger = logger;
            m_defaultModule = m_modulesRegistry.GetDefault();
            if (m_defaultModule == null) m_logger.LogError("[USAVE] There is no default module!");
        }

        public UniTask SaveAsync<T>(string key, T data, CancellationToken ct) where T : class
        {
            IPersistenceModule module = GetModule(typeof(T));
            return module.SaveAsync(key, data, ct);
        }

        public UniTask<T> LoadAsync<T>(string key, CancellationToken ct) where T : class
        {
            IPersistenceModule module = GetModule(typeof(T));
            return module.LoadAsync<T>(key, ct);
        }

        public UniTask<bool> ExistsAsync<T>(string key, CancellationToken ct)
        {
            IPersistenceModule module = GetModule(typeof(T));
            return module.ExistsAsync(key, ct);
        }

        public UniTask DeleteAsync<T>(string key, CancellationToken ct) where T : class
        {
            IPersistenceModule module = GetModule(typeof(T));
            return module.DeleteAsync(key, ct);
        }

        private IPersistenceModule GetModule(Type type)
        {
            if (!m_modulesRegistry.TryGetCustomModuleFor(type, out string key)) return m_defaultModule;

            if (m_modulesRegistry.HasModule(key)) return m_modulesRegistry.Get(key);

            m_logger.LogError(new NullReferenceException($"There is no module with key {key}").Message);

            return m_defaultModule;
        }
    }
}