using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace USave.Storage
{
    public class PrefsStorage : IStorage
    {
        public UniTask<bool> Save(string key, ReadOnlyMemory<byte> data, CancellationToken ct = default)
        {
            string b64 = Convert.ToBase64String(data.Span);
            PlayerPrefs.SetString(key, b64);
            return UniTask.FromResult(true);
        }

        public UniTask<ReadOnlyMemory<byte>> Load<T>(string key, CancellationToken ct = default)
        {
            if (!PlayerPrefs.HasKey(key))
                return UniTask.FromResult(ReadOnlyMemory<byte>.Empty);

            string b64 = PlayerPrefs.GetString(key, null);
            if (string.IsNullOrEmpty(b64))
                return UniTask.FromResult(ReadOnlyMemory<byte>.Empty);

            try
            {
                byte[] bytes = Convert.FromBase64String(b64);
                return UniTask.FromResult((ReadOnlyMemory<byte>)bytes);
            }
            catch (FormatException e)
            {
                Debug.LogError($"{GetType().Name}: invalid Base64 for key '{key}'. {e.Message}");
                return UniTask.FromResult(ReadOnlyMemory<byte>.Empty);
            }
        }

        public UniTask<bool> Exist(string key, CancellationToken ct = default)
        {
            return UniTask.FromResult(PlayerPrefs.HasKey(key));
        }

        public UniTask<bool> Delete(string key, CancellationToken ct = default)
        {
            if (!PlayerPrefs.HasKey(key)) return UniTask.FromResult(false);

            PlayerPrefs.DeleteKey(key);
            return UniTask.FromResult(true);
        }
    }

    public static class Extensions
    {
        public static ModuleBuilder WithPrefsStorage(this ModuleBuilder builder)
            => builder.SetStorage(new PrefsStorage());
    }
}