using System;
using System.Collections.Generic;

namespace USave.Data
{
    public sealed class Entry
    {
        public string Key { get; }
        public Delegate Factory { get; }

        public Entry(string key, Delegate factory)
        {
            Key = key;
            Factory = factory;
        }

        public static readonly Entry Default = new(string.Empty, null);
    }

    public interface IGlobalDataRegistry
    {
        void Register<T>(string key, Func<T> defaultFactory) where T : class;
        bool Unregister<T>() where T : class;
        bool Has<T>() where T : class;
        bool TryGet<T>(out Entry entry) where T : class;
        IEnumerable<Type> GetRegisteredTypes();
    }
}