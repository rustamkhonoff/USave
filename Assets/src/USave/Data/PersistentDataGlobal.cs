using System;

namespace USave.Data
{
    public static class PersistentDataGlobal
    {
        public readonly static Func<Type, string> DefaultKeyFuncDefault = a => a.FullName;
        public static Func<Type, string> DefaultKeyFunc { get; set; } = DefaultKeyFuncDefault;
    }
}