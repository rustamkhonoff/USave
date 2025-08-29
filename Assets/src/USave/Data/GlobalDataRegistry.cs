using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace USave.Data
{
    public class GlobalDataRegistry : IGlobalDataRegistry
    {
        private readonly ConcurrentDictionary<Type, Entry> m_registry = new();
        private readonly ILogger m_logger;

        public GlobalDataRegistry(ILogger logger)
        {
            m_logger = logger;
        }

        public void Register<T>(string key, Func<T> defaultFactory) where T : class
        {
            Type type = typeof(T);

            if (string.IsNullOrEmpty(key) || defaultFactory == null)
            {
                m_logger.LogError("Empty key or invalid default factory, aborting operation");
                return;
            }

            if (m_registry.ContainsKey(type))
                m_logger.Log($"Global data with key {key} for type {type} exists, overriding data");

            m_registry[type] = new Entry(key, defaultFactory);
        }


        public bool Unregister<T>() where T : class
        {
            return m_registry.TryRemove(typeof(T), out _);
        }

        public bool Has<T>() where T : class
        {
            return m_registry.ContainsKey(typeof(T));
        }

        public bool TryGet<T>(out Entry entry) where T : class
        {
            Type type = typeof(T);

            if (m_registry.TryGetValue(type, out Entry foundEntry))
            {
                entry = foundEntry;
                return true;
            }

            ConstructorInfo info = type.GetConstructor(Type.EmptyTypes);
            if (info == null)
            {
                m_logger.LogError($"There is no default constructor for {type}, aborting");
                entry = Entry.Default;
                return false;
            }

            string key = PersistentDataGlobal.DefaultKeyFunc(type);
            Entry newEntry = new(key, (Func<T>)Activator.CreateInstance<T>);
            m_registry[type] = newEntry;
            m_logger.Log($"Added entry for type {type} with key {key}");
            entry = newEntry;
            return true;
        }

        public IEnumerable<Type> GetRegisteredTypes() => m_registry.Keys;
    }
}