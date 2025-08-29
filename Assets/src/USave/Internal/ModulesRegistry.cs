using System;
using System.Collections.Generic;
using System.Text;

namespace USave.Internal
{
    public class ModulesRegistry
    {
        public string DefaultModuleKey { get; set; } = "default";

        public IPersistenceModule GetDefault() => m_modules[DefaultModuleKey];
        public IPersistenceModule Get(string key) => m_modules[key];
        public void Set(string key, IPersistenceModule module) => m_modules[key] = module;

        private readonly Dictionary<string, IPersistenceModule> m_modules = new();
        private readonly Dictionary<Type, string> m_typeModuleMap = new();

        public void SetModuleForType<T>(string key)
        {
            m_typeModuleMap[typeof(T)] = key;
        }
        public bool TryGetCustomModuleFor(Type type, out string key) => m_typeModuleMap.TryGetValue(type, out key);
        public bool HasModule(string key) => m_modules.ContainsKey(key);

        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            foreach ((string key, IPersistenceModule value) in m_modules) stringBuilder.AppendLine(key + ":" + value);
            return stringBuilder.ToString();
        }
    }
}