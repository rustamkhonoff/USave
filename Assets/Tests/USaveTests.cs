using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using USave;
using USave.Data;
using USave.Internal;
using USave.Serializers;
using USave.Storage;
using USave.Zenject;
using Zenject;

namespace Tests
{
    public class USaveTests : ZenjectUnitTestFixture
    {
        [Serializable]
        public class SavedData
        {
            public int A;
            public string B;

            public SavedData() { }

            public SavedData(int a)
            {
                A = a;
                B = a.ToString();
            }

            public bool IsSame(SavedData savedData) => savedData.A == A && savedData.B == B;
        }

        [Serializable]
        public class GlobalSavedData
        {
            public int A;
            public GlobalSavedData(int a) => A = a;
            public GlobalSavedData() => A = -1000;
        }

        [SetUp]
        public void Install()
        {
            Container.AddUSave(r => r.RegisterDefault(new ModuleBuilder().SetSerializer(new JsonSerializer()).SetStorage(new FileStorage(".sav"))));
        }

        [UnityTest]
        public IEnumerator Is_Saved_And_Loaded_Data_Equal_ReturnsTrue()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                SavedData data = new(1);
                IPersistenceService service = Container.Resolve<IPersistenceService>();

                await service.SaveAsync("save", data, default);
                SavedData loadedData = await service.LoadAsync<SavedData>("save", default);

                Assert.IsTrue(data.IsSame(loadedData));
            });
        }

        [UnityTest]
        public IEnumerator Is_PersistentData_Saving_AndExistsAfterSave_ReturnsTrue()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                Container.Resolve<IGlobalDataRegistry>().Register("global_data", () => new GlobalSavedData(-999));

                IPersistentData<GlobalSavedData> persistentData = Container.Resolve<IPersistentData<GlobalSavedData>>();

                await persistentData.Update(a => a.A = 150);
                bool exists = await persistentData.Exists();

                Assert.IsTrue(exists);
            });
        }

        [UnityTest]
        public IEnumerator Is_PersistentData_Deleting_ReturnsTrue()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                Container.Resolve<IGlobalDataRegistry>().Register("global_data", () => new GlobalSavedData(-999));

                IPersistentData<GlobalSavedData> persistentData = Container.Resolve<IPersistentData<GlobalSavedData>>();

                await persistentData.Delete();
                bool exists = await persistentData.Exists();

                Assert.IsFalse(exists);
            });
        }

        [UnityTest]
        public IEnumerator Is_DefaultPersistentData_CreatingIfNotExists_ReturnsTrue()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                Container.Resolve<IGlobalDataRegistry>().Register("global_data", () => new GlobalSavedData(-999));

                IPersistentData<GlobalSavedData> persistentData = Container.Resolve<IPersistentData<GlobalSavedData>>();
                await persistentData.Delete();

                GlobalSavedData x = await persistentData.Load();

                Assert.IsTrue(x.A.Equals(-999));
            });
        }

        [UnityTest]
        public IEnumerator Is_UsingDefaultConstructor_If_NotRegisteredInRegistry_ReturnsTrue()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                IPersistentData<GlobalSavedData> persistentData = Container.Resolve<IPersistentData<GlobalSavedData>>();
                await persistentData.Delete();

                GlobalSavedData x = await persistentData.Load();

                Assert.IsTrue(x.A.Equals(-1000));
            });
        }

        [UnityTest]
        public IEnumerator Set_CustomSaveFileName_SaveWithCustomNameExists()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                PlayerPrefs.DeleteKey("custom");

                ModulesRegistry registry = Container.Resolve<ModulesRegistry>();
                registry.Register("prefs", new JsonSerializer(), new PrefsStorage());
                registry.SetModuleForType<GlobalSavedData>("prefs");

                string CustomFileNameFunc(Type type) => "custom";
                PersistentDataGlobal.DefaultKeyFunc = CustomFileNameFunc;

                IPersistentData<GlobalSavedData> persistentData = Container.Resolve<IPersistentData<GlobalSavedData>>();
                await persistentData.Save();

                bool hasKey = PlayerPrefs.HasKey("custom");

                PersistentDataGlobal.DefaultKeyFunc = PersistentDataGlobal.DefaultKeyFuncDefault;
                Assert.IsTrue(hasKey);
            });
        }
    }
}