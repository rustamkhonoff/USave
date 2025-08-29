using UnityEngine.Assertions;

namespace USave
{
    public class ModuleBuilder
    {
        private ISerializer Serializer { get; set; }
        private IStorage Storage { get; set; }

        public ModuleBuilder SetStorage(IStorage storage)
        {
            Storage = storage;
            return this;
        }

        public ModuleBuilder SetSerializer(ISerializer serializer)
        {
            Serializer = serializer;
            return this;
        }

        internal IPersistenceModule Build()
        {
            Assert.IsNotNull(Serializer, "Serializer != null");
            Assert.IsNotNull(Storage, "Storage != null");

            return new PersistenceModule(Serializer, Storage);
        }
    }
}