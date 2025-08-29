#if ZENJECT
using System;
using USave.Data;
using USave.Internal;
using Zenject;
using Logger = USave.Internal.Logger;

namespace USave.Zenject
{
    public static class ZenjectExtensions
    {
        public static void AddUSave(this DiContainer diContainer, Action<ModulesRegistry> modulesAction = null)
        {
            ModulesRegistry modulesRegistry = new();
            modulesAction?.Invoke(modulesRegistry);

            diContainer
                .Bind<ModulesRegistry>()
                .FromInstance(modulesRegistry)
                .AsSingle();

            diContainer
                .BindInterfacesTo<Logger>()
                .AsSingle();

            diContainer
                .BindInterfacesTo<GlobalDataRegistry>()
                .AsSingle()
                .NonLazy();

            diContainer
                .BindInterfacesTo<PersistenceService>()
                .AsSingle();

            diContainer
                .Bind(typeof(IPersistentData<>), typeof(PersistentData<>))
                .To(typeof(PersistentData<>))
                .AsSingle();
        }
    }
}
#endif