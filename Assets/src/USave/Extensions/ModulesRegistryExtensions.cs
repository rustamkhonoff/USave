using System;
using UnityEngine;
using USave.Data;
using USave.Internal;

namespace USave
{
    public static class USaveExtensions
    {
        public static ModulesRegistry Register(this ModulesRegistry registry, string key, ISerializer serializer, IStorage storage)
        {
            registry.Set(key, new PersistenceModule(serializer, storage));
            return registry;
        }

        public static ModulesRegistry RegisterDefault(this ModulesRegistry registry, ISerializer serializer, IStorage storage)
        {
            registry.Set("default", new PersistenceModule(serializer, storage));
            return registry;
        }

        public static ModulesRegistry Register(this ModulesRegistry registry, string key, ModuleBuilder builder)
        {
            registry.Set(key, builder.Build());
            return registry;
        }

        public static ModulesRegistry RegisterDefault(this ModulesRegistry registry, ModuleBuilder builder)
        {
            registry.Set("default", builder.Build());
            return registry;
        }

        public static IGlobalDataRegistry Register<T>(this IGlobalDataRegistry registry, string key) where T : class
        {
            Type type = typeof(T);
            if (type.GetConstructor(Type.EmptyTypes) != null)
                registry.Register(key, static () => Activator.CreateInstance<T>());
            else
                Debug.LogError($"Can't register default factory for type {type}, there is no default constructor");
            return registry;
        }
    }
}